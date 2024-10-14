from trl import SFTTrainer
import os
import torch
from datasets import load_dataset, Dataset
from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig, TrainingArguments, pipeline, logging, GenerationConfig
from peft import LoraConfig, prepare_model_for_kbit_training, PeftModel
import torch
import gc
import wandb
from tqdm.notebook import tqdm
import re
import random
from huggingface_hub import login
from transformers.integrations import WandbCallback
from torch.utils.data import DataLoader
import shutil
from accelerate import notebook_launcher

dataset_name = "evgmaslov/cars"
text2text_model_name = "VityaVitalich/Llama3.1-8b-instruct"
text2code_model_name = "Qwen/Qwen2.5-Coder-7B"
text2code_tuned_model_name = "evgmaslov/Qwen2.5-Coder-7B-cars"

eos_token = "<|end_of_text|>"
b_inst_token = "<|start_header_id|>"
e_inst_token = "<|end_header_id|>"
prompt = """You are an experienced engineer who designs cars. You help people get their dream car. You receive a request to create a car, which describes its shape, size, and other characteristics. To create a car, you use a C# class of the following type:
public Car()
{
    //length of the car
    Length = ...;
    //width of the car
    Width = ...;
    //height of the car
    Height = ...;

    //width of car wheels
    WheelWidth = ...;
    //radius of car wheels
    WheelRadius = ...;

    //displacement of wheels into the vehicle
    WheelRelativeBiasAlongWidth = ...;
    //arrangement of wheels along the body of the car
    WheelRelativeBiasesAlongLength = ...;

    //geometry of the bottom of the car
    WheelBaseSegmentsSpans = ...;
    WheelBaseSegmentsBottomSurfaces = ...;
    //geometry of the surface between the wheelbase and the body of the car
    WheelBaseTopSurface = ...;
    //the gap between the wheels of the car and the body
    GapBetweenWheelAndBase = ...;

    //car body shape
    BodySegmentsSpans = ...;
    BodySegmentsTopSurfaces = ...;
}.
Write C# class to generate a car, fill in its parameters according to the user's request."""

def get_inference_text(text, tokenizer):
  messages = [
      {"role": "system", "content": prompt},
      {"role": "user", "content": text}
  ]
  input_text = tokenizer.apply_chat_template(
      messages,
      tokenize=False,
      add_generation_prompt=True
  )
  return input_text
def get_train_text(input_text, output_text, tokenizer):
  messages = [
      {"role": "system", "content": prompt},
      {"role": "user", "content": input_text},
      {"role": "assistant", "content": output_text},
  ]
  out = tokenizer.apply_chat_template(
      messages,
      tokenize=False,
  )
  return out

def generate_train_text(row, tokenizer):
  row["text"] = get_train_text(row["natural_input"], row["output"], tokenizer)
  return row

class LLMSampleCB(WandbCallback):
    def __init__(self, trainer, test_dataset, num_samples=2, max_new_tokens=2048, log_model="checkpoint"):
        super().__init__()
        self.sample_dataset = test_dataset.select(random.choices(range(len(test_dataset)), k=num_samples))
        self.model, self.tokenizer = trainer.model, trainer.tokenizer
        self.inference_model = None
        self.gen_config = GenerationConfig.from_pretrained(trainer.model.name_or_path,
                                                           max_new_tokens=max_new_tokens)
    def generate(self, prompt):
        tokenized_prompt = self.tokenizer(prompt, return_tensors='pt')['input_ids'].cuda()
        with torch.inference_mode():
            output = self.inference_model.generate(**{"input_ids":tokenized_prompt,
                                                      "generation_config":self.gen_config})
        return self.tokenizer.decode(output[0][len(tokenized_prompt[0]):], skip_special_tokens=True)

    def samples_table(self, examples):
        records_table = wandb.Table(columns=["prompt", "generation"] + list(self.gen_config.to_dict().keys()))
        for example in tqdm(examples, leave=False):
            prompt = get_inference_text(example['natural_input'], self.tokenizer)
            generation = self.generate(prompt=prompt)
            records_table.add_data(prompt, generation, *list(self.gen_config.to_dict().values()))
        return records_table

    def on_evaluate(self, args, state, control,  **kwargs):
        super().on_evaluate(args, state, control, **kwargs)
        self.inference_model = self.model
        records_table = self.samples_table(self.sample_dataset)
        self._wandb.log({"sample_predictions":records_table})

def generate(model, tokenizer, generation_config, inp):
  tokenized_prompt = tokenizer(inp, return_tensors='pt', padding=True, truncation=True)
  tokenized_prompt_ids = tokenized_prompt["input_ids"].cuda()
  tokenized_prompt_mask = tokenized_prompt["attention_mask"].cuda()
  with torch.inference_mode():
      output = model.generate(**{"input_ids":tokenized_prompt_ids, "attention_mask":tokenized_prompt_mask, "generation_config":generation_config}).detach().cpu()
  decoded = []
  for i in range(output.shape[0]):
    ans = tokenizer.decode(output[i][len(tokenized_prompt[0]):], skip_special_tokens=True)
    decoded.append(ans)
  return decoded

def format_prompt(prompt_format, args):
  prompt = prompt_format.format(**args)
  return prompt

def transform_to_nl(dataloader, text2text_model, tokenizer, gen_config, prompt, path):
  natural_inputs = []
  for batch in tqdm(dataloader):
    formated_inputs = ["characteristics:{text}\n".format(text=inp.replace("Generate a car with the following characteristics: ", "")) for inp in batch["input"]]
    prompts = [format_prompt(llama3_format_prompt_inference, {"prompt":prompt, "input":inp}) for inp in batch["input"]]
    generations = generate(text2text_model, tokenizer, gen_config, prompts)
    natural_inputs.extend(generations)

    existed = ""
    if os.path.exists(path):
      with open(path, "r") as f:
        existed = f.read()
    with open(path, "w+") as f:
      f.write(existed + "###SAMPLE###" + "###SAMPLE###".join(generations))
  return natural_inputs
    
login()
os.environ["WANDB_PROJECT"]="leap71"
os.environ["WANDB_LOG_MODEL"] = "checkpoint"
wandb.init("leap71")
def train():
    dataset = load_dataset(dataset_name)
    
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                      bnb_4bit_compute_dtype=torch.float16,
                                      bnb_4bit_quant_type="nf4",
                                      bnb_4bit_use_double_quant=True)
    
    tokenizer = AutoTokenizer.from_pretrained(text2code_model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    tokenizer.padding_side = "right"
    peft_params = LoraConfig(
        lora_alpha=16, lora_dropout=0.1, r=64, bias="none", task_type="CAUSAL_LM")

    dataset = dataset.map(lambda row: generate_train_text(row, tokenizer))
    
    model = AutoModelForCausalLM.from_pretrained(text2code_model_name)
    
    resume_from_checkpoint=False
    training_params = TrainingArguments(
        output_dir=text2code_tuned_model_name,
        num_train_epochs=1,
        per_device_train_batch_size=4,
        per_device_eval_batch_size=4,
        gradient_accumulation_steps=1,
        optim="paged_adamw_32bit",
        save_steps=100, 
        learning_rate=3e-5, weight_decay=0.001,
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
        run_name="natural_language_to_code_1")
    trainer = SFTTrainer(
        model=model,
        train_dataset=dataset["train"],
        eval_dataset=dataset["test"].select(range(32)),
        peft_config=peft_params,
        dataset_text_field="text",
        max_seq_length=None,
        tokenizer=tokenizer,
        args=training_params, packing=False)
    wandb_callback = LLMSampleCB(trainer, dataset["test"], num_samples=2, max_new_tokens=2048)
    trainer.add_callback(wandb_callback)

    if resume_from_checkpoint:
      #trainer.train(resume_from_checkpoint=text2code_tuned_model_name + "/last-checkpoint")
      trainer.train(resume_from_checkpoint=text2code_tuned_model_name + "/checkpoint-10000")
    else:
      trainer.train()

notebook_launcher(train, args=(), num_processes=8)