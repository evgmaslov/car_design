from trl import SFTTrainer
import os
import torch
from datasets import load_dataset, Dataset
from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig, TrainingArguments, pipeline, logging, GenerationConfig, get_cosine_with_hard_restarts_schedule_with_warmup
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
import json

dataset_name = "evgmaslov/cars"
text2text_model_name = "mistralai/Mistral-Nemo-Instruct-2407"
text2code_model_name = "mistralai/Mistral-Nemo-Instruct-2407"
text2code_tuned_model_name = "evgmaslov/Mistral-Nemo-Instruct-2407-cars"

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

def transform_to_nl(dataset, batch_size, text2text_model, tokenizer, gen_config, prompt, path):
  natural_inputs = {}
  if os.path.exists(path):
      with open(path, "r") as f:
        current = json.load(f)
        for key in current.keys():
            natural_inputs[key] = current[key]
  n_batches = int(len(dataset)/batch_size)
  if n_batches * batch_size < len(dataset):
      n_batches += 1
  for i in range(n_batches):
      rest_inds = [ind for ind in range(len(dataset)) if str(ind) not in natural_inputs]
      if len(rest_inds) == 0:
          continue
      if len(rest_inds) < batch_size:
          inds = rest_inds
      else:
          inds = random.sample(rest_inds, batch_size)
      batch = dataset.select(inds)
      prompts = [get_inference_text(prompt, inp, tokenizer) for inp in batch["input"]]
      generations = generate(text2text_model, tokenizer, gen_config, prompts)
      
      for ind, pos in enumerate(inds):
          natural_inputs[pos] = generations[ind]
      with open(path, "w+") as f:
        json.dump(natural_inputs, f)
      print(f"Batch {i} is processed")
  return natural_inputs

def get_inference_text(prompt, text, tokenizer):
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
def get_train_text(prompt, input_text, output_text, tokenizer):
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

def generate_train_text(prompt, row, tokenizer):
  row["text"] = get_train_text(prompt, row["natural_input"], row["output"], tokenizer)
  return row
    
login()
def rewrite_dataset():
    dataset = load_dataset(dataset_name)
    
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                  bnb_4bit_compute_dtype=torch.float16,
                                  bnb_4bit_quant_type="nf4",
                                  bnb_4bit_use_double_quant=True)

    tokenizer = AutoTokenizer.from_pretrained(text2text_model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    tokenizer.padding_side = "left"
    peft_params = LoraConfig(
        lora_alpha=16, lora_dropout=0.1, r=64, bias="none", task_type="CAUSAL_LM")

    text2text_model = AutoModelForCausalLM.from_pretrained(text2text_model_name,
                                                 quantization_config=quant_config,
                                                 device_map='auto',
                                      low_cpu_mem_usage=True, offload_state_dict=True)

    train_dataloader = DataLoader(dataset["train"], batch_size=24, shuffle=False)
    test_dataloader = DataLoader(dataset["test"], batch_size=24, shuffle=False)

    gen_config = GenerationConfig.from_pretrained(text2text_model_name)
    gen_config.max_length = 4096
    prompt = """You receive a request to generate a car in the format "Generate a car with the following characteristics: name1:value1; name2:value2;...". This request contains a set of car characteristics. Rewrite this request into human language without losing information about the characteristics of the car. You must return a response in the following json format: {"rewritten request": here you must paste the rewritten request in human language, "explanation": here you should explain where in the human language query the information about a specific parameter is mentioned } Here are some examples:
Example 1
request: Generate a car with the following characteristics: length: 290 cm; width: 150 cm; height: 135 cm; wheel width: 27 cm; wheel radius: 36.6 cm; clearance height: 12 cm; bonnet size : small; bonnet shape : rounded; windscreen shape: rounded; boot shape: rounded;
answer: {"rewritten request":"Make me a car with a rounded hood and windshield, wheels with a radius of 36.6 cm and a width of 27 cm. The clearance should be 12 cm, the length 290 cm, the width 150 cm, the height 135 cm. Also let the hood be small and boot be rounded.",
"explanation":"I mentioned the clearance height, length, width and height here: 'clearance should be 12 cm, the length 290 cm, the width 150 cm, the height 135 cm'. I wrote about the wheel parameters here: 'wheels with a radius of 36.6 cm and a width of 27 cm'. Here I wrote about the hood: 'let the hood be small', 'a car with a rounded hood', about the windscreen: 'a rounded hood and windshield', and about the boot: 'boot be rounded'."}
Example 2
request: Generate a car with the following characteristics: length: 350 cm; width: 150 cm; height: 135 cm; wheel width: 15 cm; wheel radius: 35.15 cm; clearance height: 14 cm; bonnet size : large; bonnet shape : rounded; windscreen shape: rounded; boot shape: flat;
answer: {"rewritten request":"Generate a car with a length of 350 cm, a width of 150 cm and a height of 135 cm. Make a large rounded hood, a flat trunk and a rounded windshield. The wheels should have a radius of 35.15 cm and a width of 15 cm. The clearance should be 14 cm.",
"explanation":"I mentioned the length, width and height here: 'a length of 350 cm, a width of 150 cm and a height of 135 cm'. Wheel parameters have been mentioned here: 'The wheels should have a radius of 35.15 cm and a width of 15 cm'. Here I wrote about the clearance height: 'The clearance should be 14 cm', and here I wrote about the body shapes: 'Make a large rounded hood, a flat trunk and a rounded windshield'."}
Example 3
request: Generate a car with the following characteristics: 2 wheelsets;normal size boot; length: 250 cm; height: 135 cm; wheel radius: 25 cm;
answer: {"rewritten request":"Create a passenger car with a standard-sized trunk. It will be 2.5 meters long and 1 meter 35 centimeters high. The wheels should have a radius of 25 centimeters.",
"explanation":"I wrote 'Create a passenger car', because every passenger car has 2 wheelsets. 'standard-sized trunk' means normal size boot. Length and height are mentioned here: 'It will be 2.5 meters long and 1 meter 35 centimeters high'. And here I wrote about wheel radius: 'The wheels should have a radius of 25 centimeters'."}
Example 4
request: Generate a car with the following characteristics: body type: truck;
answer: {"rewritten request": "I want a random truck",
"explanation": "The request only mentions the body type, thats why I wrote 'random truck'."}
Example 5
request: Generate a car with the following characteristics: normal size bonnet; length: 250 cm; width is small; height is small; wheel radius: 25 cm;
answer: {"rewritten request":"I want you to create a car with a wheel radius of 25 cm. Make it compact, but make it 2.5 meters long. Leave the hood of standard size.",
"explanation":"I wrote 'Make it compact', because both width and height are small. I wrote about length here: 'make it 2.5 meters long'. Here I mentioned the wheel radius: 'wheel radius of 25 cm'. And here I wrote about the bonnet size: 'Leave the hood of standard size'."}
I ask you to be creative, use as many different words as possible. Use different words to start the sentence.
"""

    train_nl = transform_to_nl(dataset["train"], 24, text2text_model, tokenizer, gen_config, prompt, "train_nl.json")
    dataset["train"] = dataset["train"].add_column("natural_input_mistral", train_nl)
    test_nl = transform_to_nl(dataset["test"], 24, text2text_model, tokenizer, gen_config, prompt, "test_nl.json")
    dataset["test"] = dataset["test"].add_column("natural_input_mistral", test_nl)
    dataset.push_to_hub(dataset_name)

notebook_launcher(rewrite_dataset, args=(), num_processes=8)