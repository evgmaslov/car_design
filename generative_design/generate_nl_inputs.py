import os
import torch
from datasets import load_dataset
from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig, GenerationConfig
import torch
from tqdm.auto import tqdm
import random

from src.prompts import SYSTEM_PROMPTS
from src.prompt_utils import get_inference_llm_prompt_from_template
from src.generation_utils import generate
import argparse
import json
    
def main(dataset_name, model_name, system_prompt_key, batch_size, nl_inputs_folder):
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                  bnb_4bit_compute_dtype=torch.float16,
                                  bnb_4bit_quant_type="nf4",
                                  bnb_4bit_use_double_quant=True)

    tokenizer = AutoTokenizer.from_pretrained(model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    tokenizer.padding_side = "left"

    model = AutoModelForCausalLM.from_pretrained(model_name,
                                                 quantization_config=quant_config,
                                                 device_map='auto',
                                      low_cpu_mem_usage=True, offload_state_dict=True)

    gen_config = GenerationConfig.from_pretrained(model_name)
    gen_config.max_length = 4096
    prompt = SYSTEM_PROMPTS[system_prompt_key]
    
    dataset = load_dataset(dataset_name, "default")
    splits = ["train", "test"]
    file_names = ["train_nl.json", "test_nl.json"]
    for split, file_name in zip(splits, file_names):
        local_dataset = dataset[split]
        local_path = os.path.join(nl_inputs_folder, file_name)
        
        nl_inputs = {}
        if os.path.exists(local_path):
            with open(local_path, "r") as f:
              current = json.load(f)
            for key in current.keys():
                nl_inputs[key] = current[key]
        
        n_batches = int(len(local_dataset)/batch_size)
        if n_batches * batch_size < len(local_dataset):
            n_batches += 1
            
        for i in tqdm(range(n_batches), desc=f"Processing {split} split"):
            rest_inds = [ind for ind in range(len(local_dataset)) if str(ind) not in nl_inputs]
            if len(rest_inds) == 0:
                continue
            if len(rest_inds) < batch_size:
                inds = rest_inds
            else:
                inds = random.sample(rest_inds, batch_size)
            batch = local_dataset.select(inds)
            prompts = [get_inference_llm_prompt_from_template(prompt, inp, tokenizer) for inp in batch["input"]]
            generations = generate(model, tokenizer, gen_config, prompts)
            
            for ind, pos in enumerate(inds):
                nl_inputs[pos] = generations[ind]
            with open(local_path, "w+") as f:
                json.dump(nl_inputs, f)

if __name__ == "__main__":
    dataset_name = "evgmaslov/cars"
    
    parser = argparse.ArgumentParser()
    parser.add_argument(f"--model_name", default="mistralai/Mistral-Nemo-Instruct-2407", type=str)
    parser.add_argument(f"--system_prompt_key", default="generating_nl_inputs_mistral_nemo", type=str)
    parser.add_argument(f"--batch_size", default=24, type=int)
    parser.add_argument(f"--nl_inputs_folder", default="", type=str)
    args = parser.parse_args()
    
    main(args.dataset_name, args.model_name, args.system_prompt_key, args.batch_size, args.nl_inputs_folder)