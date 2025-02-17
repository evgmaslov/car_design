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

import argparse
from src.prompts import SYSTEM_PROMPTS
from src.prompt_utils import (
    get_train_llm_prompt_from_template,
    get_inference_llm_prompt_from_template
)
from src.train_utils import LLMSampleCB
   
login()
os.environ["WANDB_PROJECT"]="leap71"
os.environ["WANDB_LOG_MODEL"] = "checkpoint"
wandb.init("leap71")
def train():
    dataset_name = "evgmaslov/cars"

    parser = argparse.ArgumentParser()
    parser.add_argument(f"--subset_name", default="mistral_nemo", type=str)
    parser.add_argument(f"--system_prompt_key", default="train_mistral_nemo", type=str)
    parser.add_argument(f"--base_model_name", default="mistralai/Mistral-Nemo-Instruct-2407", type=str)
    parser.add_argument(f"--tuned_model_name", default="evgmaslov/Mistral-Nemo-Instruct-2407-cars", type=str)
    args = parser.parse_args()
    
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                      bnb_4bit_compute_dtype=torch.float16,
                                      bnb_4bit_quant_type="nf4",
                                      bnb_4bit_use_double_quant=True)
    
    tokenizer = AutoTokenizer.from_pretrained(args.base_model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    tokenizer.padding_side = "right"
    tokenizer.chat_template = """{%- if messages[0][\"role\"] == \"system\" %}\n    {%- set system_message = messages[0][\"content\"] %}\n    {%- set loop_messages = messages[1:] %}\n{%- else %}\n    {%- set loop_messages = messages %}\n{%- endif %}\n{%- if not tools is defined %}\n    {%- set tools = none %}\n{%- endif %}\n{%- set user_messages = loop_messages | selectattr(\"role\", \"equalto\", \"user\") | list %}\n\n{#- This block checks for alternating user/assistant messages, skipping tool calling messages #}\n{%- set ns = namespace() %}\n{%- set ns.index = 0 %}\n{%- for message in loop_messages %}\n    {%- if not (message.role == \"tool\" or message.role == \"tool_results\" or (message.tool_calls is defined and message.tool_calls is not none)) %}\n        {%- if (message[\"role\"] == \"user\") != (ns.index % 2 == 0) %}\n            {{- raise_exception(\"After the optional system message, conversation roles must alternate user/assistant/user/assistant/...\") }}\n        {%- endif %}\n        {%- set ns.index = ns.index + 1 %}\n    {%- endif %}\n{%- endfor %}\n\n{{- bos_token }}\n{%- for message in loop_messages %}\n    {%- if message[\"role\"] == \"user\" %}\n        {%- if tools is not none and (message == user_messages[-1]) %}\n            {{- \"[AVAILABLE_TOOLS][\" }}\n            {%- for tool in tools %}\n                {%- set tool = tool.function %}\n                {{- '{\"type\": \"function\", \"function\": {' }}\n                {%- for key, val in tool.items() if key != \"return\" %}\n                    {%- if val is string %}\n                        {{- '\"' + key + '\": \"' + val + '\"' }}\n                    {%- else %}\n                        {{- '\"' + key + '\": ' + val|tojson }}\n                    {%- endif %}\n                    {%- if not loop.last %}\n                        {{- \", \" }}\n                    {%- endif %}\n                {%- endfor %}\n                {{- \"}}\" }}\n                {%- if not loop.last %}\n                    {{- \", \" }}\n                {%- else %}\n                    {{- \"]\" }}\n                {%- endif %}\n            {%- endfor %}\n            {{- \"[/AVAILABLE_TOOLS]\" }}\n            {%- endif %}\n        {%- if system_message is defined %}\n            {{- \"[INST]\" + system_message + \"\\n\\n\" + message[\"content\"] + \"[/INST]\" }}\n        {%- else %}\n            {{- \"[INST]\" + message[\"content\"] + \"[/INST]\" }}\n        {%- endif %}\n    {%- elif (message.tool_calls is defined and message.tool_calls is not none) %}\n        {{- \"[TOOL_CALLS][\" }}\n        {%- for tool_call in message.tool_calls %}\n            {%- set out = tool_call.function|tojson %}\n            {{- out[:-1] }}\n            {%- if not tool_call.id is defined or tool_call.id|length != 9 %}\n                {{- raise_exception(\"Tool call IDs should be alphanumeric strings with length 9!\") }}\n            {%- endif %}\n            {{- ', \"id\": \"' + tool_call.id + '\"}' }}\n            {%- if not loop.last %}\n                {{- \", \" }}\n            {%- else %}\n                {{- \"]\" + eos_token }}\n            {%- endif %}\n        {%- endfor %}\n    {%- elif message[\"role\"] == \"assistant\" %}\n        {{- message[\"content\"] + eos_token}}\n    {%- elif message[\"role\"] == \"tool_results\" or message[\"role\"] == \"tool\" %}\n        {%- if message.content is defined and message.content.content is defined %}\n            {%- set content = message.content.content %}\n        {%- else %}\n            {%- set content = message.content %}\n        {%- endif %}\n        {{- '[TOOL_RESULTS]{\"content\": ' + content|string + \", \" }}\n        {%- if not message.tool_call_id is defined or message.tool_call_id|length != 9 %}\n            {{- raise_exception(\"Tool call IDs should be alphanumeric strings with length 9!\") }}\n        {%- endif %}\n        {{- '\"call_id\": \"' + message.tool_call_id + '\"}[/TOOL_RESULTS]' }}\n    {%- else %}\n        {{- raise_exception(\"Only user and assistant roles are supported, with the exception of an initial optional system message!\") }}\n    {%- endif %}\n{%- endfor %}\n"""
    peft_params = LoraConfig(
        lora_alpha=16, lora_dropout=0.1, r=64, bias="none", task_type="CAUSAL_LM")
    
    small_dataset = load_dataset(dataset_name, args.subset_name)
    if "test" not in small_dataset.keys():
        small_dataset = small_dataset.train_test_split(test_size=0.05, seed=42)
    train_text_col = [get_train_llm_prompt_from_template(SYSTEM_PROMPTS[args.system_prompt_key], row["nl_input"], row["output"], tokenizer) for row in small_dataset["train"]]
    small_dataset["train"] = small_dataset["train"].add_column("text", train_text_col)
    test_text_col = [get_inference_llm_prompt_from_template(SYSTEM_PROMPTS[args.system_prompt_key], row["nl_input"], tokenizer) for row in small_dataset["test"]]
    small_dataset["test"] = small_dataset["test"].add_column("text", test_text_col)
    
    model = AutoModelForCausalLM.from_pretrained(args.base_model_name)
    
    resume_from_checkpoint=False
    training_params = TrainingArguments(
        output_dir=args.tuned_model_name,
        num_train_epochs=3,
        per_device_train_batch_size=2,
        per_device_eval_batch_size=2,
        gradient_accumulation_steps=1,
        optim="paged_adamw_32bit",
        save_steps=100, 
        learning_rate=3e-4, weight_decay=0.001,
        fp16=False, bf16=False,
        max_steps=-1, warmup_ratio=0.01,
        group_by_length=True, lr_scheduler_type='cosine',
        report_to="wandb",
        save_total_limit=2,
        eval_strategy="steps",
        eval_steps=100,
        push_to_hub=True,
        hub_strategy="checkpoint",
        resume_from_checkpoint=resume_from_checkpoint,
        run_name="natural_language_to_code_1",
        load_best_model_at_end=True)
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
        pass
    else:
      trainer.train()

notebook_launcher(train, args=(), num_processes=8)