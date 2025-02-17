from trl import SFTTrainer
import os
import torch
from datasets import load_dataset
from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig, TrainingArguments
from peft import LoraConfig
import torch
import wandb
from tqdm.notebook import tqdm
from huggingface_hub import login
from accelerate import notebook_launcher
import json

import argparse
from src.prompts import SYSTEM_PROMPTS
from src.prompt_utils import (
    get_train_llm_prompt_from_tokens,
    get_inference_llm_prompt_from_tokens
)
from src.train_utils import LLMSampleCB

login()
os.environ["WANDB_PROJECT"]="leap71"
os.environ["WANDB_LOG_MODEL"] = "checkpoint"
wandb.init("leap71")

def train():
    dataset_name = "evgmaslov/cars"
    b_inst_token = "<|start_header_id|>"
    e_inst_token = "<|end_header_id|>"
    eos_token = "<|end_of_text|>"

    parser = argparse.ArgumentParser()
    parser.add_argument(f"--subset_name", default="mistral_nemo", type=str)
    parser.add_argument(f"--system_prompt_key", default="train_llama", type=str)
    parser.add_argument(f"--base_model_name", default="ajibawa-2023/Code-Llama-3-8B", type=str)
    parser.add_argument(f"--tuned_model_name", default="evgmaslov/Code-Llama-3-8B-cars", type=str)
    args = parser.parse_args()
    
    small_dataset = load_dataset(dataset_name, args.subset_name)
    if "test" not in small_dataset.keys():
        small_dataset = small_dataset.train_test_split(test_size=0.05, seed=42)
    train_text_col = [get_train_llm_prompt_from_tokens(SYSTEM_PROMPTS[args.system_prompt_key], row["nl_input"], row["output"], b_inst_token, e_inst_token, eos_token) for row in small_dataset["train"]]
    small_dataset["train"] = small_dataset["train"].add_column("text", train_text_col)
    test_text_col = [get_inference_llm_prompt_from_tokens(SYSTEM_PROMPTS[args.system_prompt_key], row["nl_input"], b_inst_token, e_inst_token, eos_token) for row in small_dataset["test"]]
    small_dataset["test"] = small_dataset["test"].add_column("text", test_text_col)
    
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                      bnb_4bit_compute_dtype=torch.float16,
                                      bnb_4bit_quant_type="nf4",
                                      bnb_4bit_use_double_quant=True)
    
    tokenizer = AutoTokenizer.from_pretrained(args.base_model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    peft_params = LoraConfig(
        lora_alpha=16, lora_dropout=0.1, r=64, bias="none", task_type="CAUSAL_LM")
    
    model = AutoModelForCausalLM.from_pretrained(args.base_model_name)
    
    resume_from_checkpoint=False
    training_params = TrainingArguments(
        output_dir=args.tuned_model_name,
        num_train_epochs=3,
        per_device_train_batch_size=4,
        per_device_eval_batch_size=4,
        gradient_accumulation_steps=1,
        optim="paged_adamw_32bit",
        save_steps=100, 
        learning_rate=3e-4, weight_decay=0.001,
        fp16=False, bf16=False,
        max_grad_norm=0.3,
        max_steps=-1, warmup_ratio=0.01,
        group_by_length=True, lr_scheduler_type='cosine',
        report_to="wandb",
        save_total_limit=4,
        eval_strategy="steps",
        eval_steps=100,
        push_to_hub=True,
        hub_strategy="checkpoint",
        resume_from_checkpoint=resume_from_checkpoint,
        run_name="natural_language_to_code_1")
    trainer = SFTTrainer(
        model=model,
        train_dataset=small_dataset["train"],
        eval_dataset=small_dataset["test"].select(range(32)),
        peft_config=peft_params,
        dataset_text_field="text",
        max_seq_length=None,
        tokenizer=tokenizer,
        args=training_params, packing=False)
    wandb_callback = LLMSampleCB(trainer, small_dataset["test"], num_samples=2, max_new_tokens=2048)
    trainer.add_callback(wandb_callback)

    if resume_from_checkpoint:
      trainer.train(resume_from_checkpoint=args.tuned_model_name + "/checkpoint-10000")
    else:
      trainer.train()

notebook_launcher(train, args=(), num_processes=8)