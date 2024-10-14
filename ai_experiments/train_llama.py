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
import json

dataset_name = "evgmaslov/cars"
text2text_model_name = "VityaVitalich/Llama3.1-8b-instruct"
text2code_model_name = "ajibawa-2023/Code-Llama-3-8B"
text2code_tuned_model_name = "evgmaslov/Code-Llama-3-8B-cars"

eos_token = "<|end_of_text|>"
b_inst_token = "<|start_header_id|>"
e_inst_token = "<|end_header_id|>"
prompt = """You are an experienced engineer who designs cars. You know everything about the shape and size of cars. You know many different types of car bodies and understand their geometric characteristics. You help people get the car they need. You receive a request to create a car, which describes its shape, size, and other characteristics. To create a car, you use a C# class of the following structure:
public Car()
{
    Length = ...;
    Width = ...;
    Height = ...;

    WheelWidth = ...;
    WheelRadius = ...;

    WheelRelativeBiasAlongWidth = ...;
    WheelRelativeBiasesAlongLength = ...;

    WheelBaseSegmentsSpans = ...;
    WheelBaseSegmentsBottomSurfaces = ...;
    WheelBaseTopSurface = ...;
    GapBetweenWheelAndBase = ...;

    BodySegmentsSpans = ...;
    BodySegmentsTopSurfaces = ...;
}.
Here is a description of its parameters. "Length" is the length of the car in centimeters, this is a float type parameter. "Width" is the width of the car in centimeters, this is a float type parameter. "Height" is the width of the car in centimeters, this is a float type parameter. "WheelWidth" is the width of each wheel in centimeters, this is a float type parameter. "WheelRadius" is the radius of each wheel in centimeters, this is a float type parameter. "WheelRelativeBiasAlongWidth" is a parameter that determines how deep the wheel is shifted into the car body. For example, if WheelRelativeBiasAlongWidth = 0.1, then the wheel is shifted into the car by a distance equal to 10% of its width. This parameter is of float type. "WheelRelativeBiasesAlongLength" is a parameter that contains a list of numbers in floating point format. The number of values in the list is equal to the number of wheel pairs the car has. Each value is equal to the relative offset of a wheel pair relative to the front of the car. For example, if WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f }, then the car has two wheel pairs, one offset by 20% of the length, and the other by 80%. The "WheelBaseSegmentsSpans" parameter describes the arrangement of the parts of the car's floor. The floor of a car is the frame on which its wheels are mounted. Usually a car has one frame, but a truck can have 2 or more frames. This parameter contains a list where each element is a range of lengths in which the floor of the car is located. The range is described by a list where the first element is the start of the range and the second element is the end of the range. The WheelBaseSegmentsBottomSurfaces parameter describes the shape of the bottom of the car. If the bottom of the car consists of several planes, this parameter will contain several planes. This parameter contains as many planes as there are ranges specified in the "WheelBaseSegmentsSpans" parameter, because each plane has its own range. This parameter can be used to adjust the ground clearance of the car. The "BodySegmentsSpans" parameter contains a list of length ranges in which the vehicle body components are located. The "BodySegmentsTopSurfaces" parameter contains a list of the components of the car body. The number of components of the car is equal to the number of ranges in the "BodySegmentsSpans" parameter, because each component has its own range. Each component of the car is described by the shape of its surface.
To fill parameters "WheelBaseSegmentsBottomSurfaces", "WheelBaseTopSurface" and "BodySegmentsTopSurfaces" you must use one of three classes: Constant, CornerRounded and TotalRounded. Using these classes you can describe the shape of the car. Here is a description of the classes and their parameters:
Constant is a class that describes a flat surface. It depends only on the "height" parameter, which is responsible for the absolute height of the plane.
CornerRounded is a class that generates a flat surface with rounded corners. It depends on the following parameters: "minHeight" - height of the bottom point of the corner rounding; "maxHeight" - height of the highest point of the surface; "cornerRelativeLength" - relative length of the rounded part of the surface; "surfaceAbsoluteLength" - absolute surface length; "leftCornerRounded" - presence of a rounded corner on the left; "rightCornerRounded" - presence of a rounded corner on the right.
TotalRounded is a class that generates a rounded surface of parabolic shape. It depends on the following parameters: "minHeight" - height of the bottom point of the rounding; "maxHeight" - height of the top point of the rounding; "leftRounded" indicates the direction of surface rounding, i.e. if the value is "true" then the surface has a left-hand rounding, rigth-hand otherwise.
The user will ask you to generate a car and will provide you with a description of its shape and dimensions. You need to extract from his request the information needed to fill in the C# class parameters described above. Write C# class to generate a car, fill in its parameters according to the user's request. If the user has not specified specific dimensions or has provided insufficient information, use all your knowledge of car shapes and sizes to fill in the missing values and generate the correct car class."""

def get_inference_text(text):
  input_format_without_prompt = """{b_inst}user{e_inst}
  {text}{eos}{b_inst}assistant{e_inst}
  """
  input_format_with_prompt = """{b_inst}system{e_inst}
  {prompt}{eos}""" + input_format_without_prompt
  input_text = ""
  if len(prompt) == 0:
    input_text = input_format_without_prompt.format(b_inst=b_inst_token, text=text, e_inst=e_inst_token, eos=eos_token)
  else:
    input_text = input_format_with_prompt.format(prompt=prompt, b_inst=b_inst_token, text=text, e_inst=e_inst_token, eos=eos_token)
  return input_text
def get_train_text(input_text, output_text):
  output_format = """{output}{eos}
  """
  inp = get_inference_text(input_text)
  out = inp + output_format.format(output=output_text, eos=eos_token)
  return out

def generate_train_text(row):
  row["text"] = get_train_text(row["natural_input"], row["output"])
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
            prompt = get_inference_text(example['natural_input'])
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

def clean_dataset(row):
    result = True
    row = {"input": row["input"].lower(), "natural_input": row["natural_input"].lower()}
    if "can't" in row["natural_input"] or "can not" in row["natural_input"]:
        result = False
    ints = ["length", "width", "heignt"]
    for param in ints:
        if param in row["input"]:
            value = re.findall("(?<=" + param + ": )\d+?(?= cm)", row["input"])
            if len(value) == 0:
                continue
            else:
                value = value[0]
            equals = [" "+value+" ", value[:-2] + " \w+? " + value[-2:], " "+str(float(value)/100)+" "]
            found = False
            for e in equals:
                if len(re.findall(e, row["natural_input"])) > 0:
                    found = True
                    break
            if not found:
                result = False
                break
    doubles = ["wheel radius", "wheel width"]
    for param in doubles:
        if param in row["input"]:
            value = re.findall("(?<=" + param + ": )\d+?(?= cm)", row["input"])
            if len(value) == 0:
                continue
            else:
                value = value[0]
            if len(re.findall(" "+value+" ", row["natural_input"])) == 0:
                result = False
                break
    body_types = ["sedan", "hatchback", "station wagon", "coupe", "limousine", "SUV", "pickup", "minivan", "van", "truck"]
    if "body type" in row["input"]:
        found = []
        for body in body_types:
            if body in row["natural_input"]:
                found.append(body)
        if len(found) > 1:
            result = False
        if len(found) == 1:
            right_body = re.findall("(?<=body type: )\w+?", row["input"])[0]
            if right_body != found[0]:
                result = False
    return result
    
login()
os.environ["WANDB_PROJECT"]="leap71"
os.environ["WANDB_LOG_MODEL"] = "checkpoint"
wandb.init("leap71")
def train():
    dataset = load_dataset(dataset_name)
    
    with open("train_nl.json", "r") as f:
        data = json.load(f)
    small_dataset = {"input":[], "natural_input":[], "output":[], "text": []}
    keys = [int(k) for k in data.keys()]
    sorted_keys = sorted(keys)
    for ind in tqdm(sorted_keys):
        natural_input = ""
        try:
            natural_input = json.loads(data[str(ind)])["rewritten request"]
        except:
            continue
        if natural_input != "":
            row = dataset["train"][ind]
            small_dataset["input"].append(row["input"])
            small_dataset["output"].append(row["output"])
            small_dataset["text"].append("")
            small_dataset["natural_input"].append(natural_input)
    small_dataset = Dataset.from_dict(small_dataset)
    small_dataset = small_dataset.train_test_split(test_size=0.05, seed=42)
    
    small_dataset = small_dataset.map(generate_train_text)
    
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                      bnb_4bit_compute_dtype=torch.float16,
                                      bnb_4bit_quant_type="nf4",
                                      bnb_4bit_use_double_quant=True)
    
    tokenizer = AutoTokenizer.from_pretrained(text2code_model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    peft_params = LoraConfig(
        lora_alpha=16, lora_dropout=0.1, r=64, bias="none", task_type="CAUSAL_LM")
    
    model = AutoModelForCausalLM.from_pretrained(text2code_model_name)
    
    resume_from_checkpoint=False
    training_params = TrainingArguments(
        output_dir=text2code_tuned_model_name,
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
      #trainer.train(resume_from_checkpoint=text2code_tuned_model_name + "/last-checkpoint")
      trainer.train(resume_from_checkpoint=text2code_tuned_model_name + "/checkpoint-10000")
    else:
      trainer.train()

notebook_launcher(train, args=(), num_processes=8)