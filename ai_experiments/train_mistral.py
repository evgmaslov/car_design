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
text2code_model_name = "mistralai/Mistral-Nemo-Instruct-2407"
text2code_tuned_model_name = "evgmaslov/Mistral-Nemo-Instruct-2407-cars"

eos_token = "<|end_of_text|>"
b_inst_token = "<|start_header_id|>"
e_inst_token = "<|end_header_id|>"
prompt = """You are an experienced engineer who designs cars. You help people get their dream car. You receive a request to create a car, which describes its shape, size, and other characteristics. To create a car, you use a C# class of the following structure:
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
Here is a description of its parameters. "Length" is the length of the car in centimeters, this is a float type parameter. "Width" is the length of the car in centimeters, this is a float type parameter. "Height" is the length of the car in centimeters, this is a float type parameter. "WheelWidth" is the width of each wheel in centimeters, this is a float type parameter. "WheelRadius" is the radius of each wheel in centimeters, this is a float type parameter. "WheelRelativeBiasAlongWidth" is a parameter that determines how far the wheel is shifted into the car body. For example, if WheelRelativeBiasAlongWidth = 0.1, then the wheel is shifted into the car by a distance equal to 10% of its width. This parameter is of float type. "WheelRelativeBiasesAlongLength" is a parameter that contains a list of numbers in floating point format. The number of values in the list is equal to the number of wheel pairs the car has. Each value is equal to the relative offset of a wheel pair relative to the front of the car. For example, if WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f }, then the car has two wheel pairs, one offset by 20% of the length, and the other by 80%. The "WheelBaseSegmentsSpans" parameter describes the arrangement of the parts of the car's floor. The floor of a car is the frame on which its wheels are mounted. Usually a car has one frame, but a truck can have 2 or more frames. This parameter contains a list where each element is a range of lengths in which the floor of the car is located. The range is described by a list where the first element is the start of the range and the second element is the end of the range. The WheelBaseSegmentsBottomSurfaces parameter describes the shape of the bottom of the car. If the bottom of the car consists of several planes, this parameter will contain several planes. This parameter contains as many planes as there are ranges specified in the "WheelBaseSegmentsSpans" parameter, because each plane has its own range. This parameter can be used to adjust the ground clearance of the car. The BodySegmentsSpans parameter contains a list of length ranges in which the vehicle body components are located. The "BodySegmentsTopSurfaces" parameter contains a list of the components of the car body. The number of components of the car is equal to the number of ranges in the "BodySegmentsSpans" parameter, because each component has its own range. Each component of the car is described by the shape of its surface. The shape can be specified by one of three classes: Constant (plain shape), CornerRounded (plain shape with rounded corners) and TotalRounded (rounded shape).
The user will ask you to generate a car and will provide you with a description of its shape and dimensions. You need to extract from his request the information needed to fill in the C# class parameters described above. Write C# class to generate a car, fill in its parameters according to the user's request. Here are some examples of what you need to do:
Example 1
user request: Design a car with a standard-sized hood and trunk. It should be 1.67 meters wide and have wheels with a radius of 40 cm.
car: using System.Collections.Generic;
using System;
using System.Collections;
namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 250f;
            Width = 167f;
            Height = 150f;

            WheelWidth = 20f;
            WheelRadius = 40f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 13f) };
            WheelBaseTopSurface = new Constant(height: 90f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 0.3f }, new List<float>(){ 0.3f, 0.8f }, new List<float>(){ 0.8f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() { new Constant(height: 105f), new Constant(height: 150f), new CornerRounded(minHeight: 90f, maxHeight: 105f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 49.999996f, leftCornerRounded: false, rightCornerRounded: true) };
        }
    }

}
Example 2:
user request: Design a car with two wheelsets and a standard-sized hood. It should be 1.67 meters wide.
car: using System.Collections.Generic;
using System;
using System.Collections;
namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 250f;
            Width = 167f;
            Height = 150f;

            WheelWidth = 20f;
            WheelRadius = 25f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 18f) };
            WheelBaseTopSurface = new Constant(height: 75f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 0.3f }, new List<float>(){ 0.3f, 0.9f }, new List<float>(){ 0.9f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() { new TotalRounded(minHeight: 75f, maxHeight: 90f, leftRounded: true), new Constant(height: 150f), new CornerRounded(minHeight: 75f, maxHeight: 90f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 25.000006f, leftCornerRounded: false, rightCornerRounded: true) };
        }
    }

}
Example 3: 
user request: Design a compact sedan. It should be short and narrow, with a small height.
car: using System.Collections.Generic;
using System;
using System.Collections;
namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 250f;
            Width = 167f;
            Height = 135f;

            WheelWidth = 20f;
            WheelRadius = 25f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 13f) };
            WheelBaseTopSurface = new Constant(height: 67.5f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 0.2f }, new List<float>(){ 0.2f, 0.8f }, new List<float>(){ 0.8f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() { new CornerRounded(minHeight: 67.5f, maxHeight: 94.5f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 50f, leftCornerRounded: true, rightCornerRounded: false), new CornerRounded(minHeight: 94.5f, maxHeight: 135f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 150f, leftCornerRounded: true, rightCornerRounded: true), new CornerRounded(minHeight: 67.5f, maxHeight: 94.5f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 49.999996f, leftCornerRounded: false, rightCornerRounded: true) };
        }
    }

}
Example 4:
user request: Design a car with two wheelsets, a standard-sized hood, and a height of 1.35 meters. Ensure the wheel radius is large.
car: using System.Collections.Generic;
using System;
using System.Collections;
namespace GenerativeDesign.Cars
{
    public class Car : CarBase
    {
        public Car()
        {
            Length = 250f;
            Width = 167f;
            Height = 135f;

            WheelWidth = 20f;
            WheelRadius = 40f;
            WheelRelativeBiasAlongWidth = 0.1f;
            WheelRelativeBiasesAlongLength = new List<float>() { 0.2f, 0.8f };

            WheelBaseSegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 1f } };
            WheelBaseSegmentsBottomSurfaces = new List<Line>() { new Constant(height: 18f) };
            WheelBaseTopSurface = new Constant(height: 94.5f);
            GapBetweenWheelAndBase = 3f;

            BodySegmentsSpans = new List<List<float>>() { new List<float>(){ 0f, 0.2f }, new List<float>(){ 0.2f, 0.8f }, new List<float>(){ 0.8f, 1f } };
            BodySegmentsTopSurfaces = new List<Line>() { new TotalRounded(minHeight: 94.5f, maxHeight: 108f, leftRounded: true), new CornerRounded(minHeight: 108f, maxHeight: 135f, cornerRelativeLength: 0.2f, surfaceAbsoluteLength: 150f, leftCornerRounded: true, rightCornerRounded: true), new TotalRounded(minHeight: 94.5f, maxHeight: 108f, leftRounded: false) };
        }
    }

}"""

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
    print(f"Dataset length: {len(small_dataset)}")
    small_dataset = small_dataset.train_test_split(test_size=0.05, seed=42)
    
    quant_config = BitsAndBytesConfig(load_in_4bit=True,
                                      bnb_4bit_compute_dtype=torch.float16,
                                      bnb_4bit_quant_type="nf4",
                                      bnb_4bit_use_double_quant=True)
    
    tokenizer = AutoTokenizer.from_pretrained(text2code_model_name, trust_remote_code=True)
    tokenizer.pad_token = tokenizer.eos_token
    tokenizer.padding_side = "right"
    tokenizer.chat_template = """{%- if messages[0][\"role\"] == \"system\" %}\n    {%- set system_message = messages[0][\"content\"] %}\n    {%- set loop_messages = messages[1:] %}\n{%- else %}\n    {%- set loop_messages = messages %}\n{%- endif %}\n{%- if not tools is defined %}\n    {%- set tools = none %}\n{%- endif %}\n{%- set user_messages = loop_messages | selectattr(\"role\", \"equalto\", \"user\") | list %}\n\n{#- This block checks for alternating user/assistant messages, skipping tool calling messages #}\n{%- set ns = namespace() %}\n{%- set ns.index = 0 %}\n{%- for message in loop_messages %}\n    {%- if not (message.role == \"tool\" or message.role == \"tool_results\" or (message.tool_calls is defined and message.tool_calls is not none)) %}\n        {%- if (message[\"role\"] == \"user\") != (ns.index % 2 == 0) %}\n            {{- raise_exception(\"After the optional system message, conversation roles must alternate user/assistant/user/assistant/...\") }}\n        {%- endif %}\n        {%- set ns.index = ns.index + 1 %}\n    {%- endif %}\n{%- endfor %}\n\n{{- bos_token }}\n{%- for message in loop_messages %}\n    {%- if message[\"role\"] == \"user\" %}\n        {%- if tools is not none and (message == user_messages[-1]) %}\n            {{- \"[AVAILABLE_TOOLS][\" }}\n            {%- for tool in tools %}\n                {%- set tool = tool.function %}\n                {{- '{\"type\": \"function\", \"function\": {' }}\n                {%- for key, val in tool.items() if key != \"return\" %}\n                    {%- if val is string %}\n                        {{- '\"' + key + '\": \"' + val + '\"' }}\n                    {%- else %}\n                        {{- '\"' + key + '\": ' + val|tojson }}\n                    {%- endif %}\n                    {%- if not loop.last %}\n                        {{- \", \" }}\n                    {%- endif %}\n                {%- endfor %}\n                {{- \"}}\" }}\n                {%- if not loop.last %}\n                    {{- \", \" }}\n                {%- else %}\n                    {{- \"]\" }}\n                {%- endif %}\n            {%- endfor %}\n            {{- \"[/AVAILABLE_TOOLS]\" }}\n            {%- endif %}\n        {%- if system_message is defined %}\n            {{- \"[INST]\" + system_message + \"\\n\\n\" + message[\"content\"] + \"[/INST]\" }}\n        {%- else %}\n            {{- \"[INST]\" + message[\"content\"] + \"[/INST]\" }}\n        {%- endif %}\n    {%- elif (message.tool_calls is defined and message.tool_calls is not none) %}\n        {{- \"[TOOL_CALLS][\" }}\n        {%- for tool_call in message.tool_calls %}\n            {%- set out = tool_call.function|tojson %}\n            {{- out[:-1] }}\n            {%- if not tool_call.id is defined or tool_call.id|length != 9 %}\n                {{- raise_exception(\"Tool call IDs should be alphanumeric strings with length 9!\") }}\n            {%- endif %}\n            {{- ', \"id\": \"' + tool_call.id + '\"}' }}\n            {%- if not loop.last %}\n                {{- \", \" }}\n            {%- else %}\n                {{- \"]\" + eos_token }}\n            {%- endif %}\n        {%- endfor %}\n    {%- elif message[\"role\"] == \"assistant\" %}\n        {{- message[\"content\"] + eos_token}}\n    {%- elif message[\"role\"] == \"tool_results\" or message[\"role\"] == \"tool\" %}\n        {%- if message.content is defined and message.content.content is defined %}\n            {%- set content = message.content.content %}\n        {%- else %}\n            {%- set content = message.content %}\n        {%- endif %}\n        {{- '[TOOL_RESULTS]{\"content\": ' + content|string + \", \" }}\n        {%- if not message.tool_call_id is defined or message.tool_call_id|length != 9 %}\n            {{- raise_exception(\"Tool call IDs should be alphanumeric strings with length 9!\") }}\n        {%- endif %}\n        {{- '\"call_id\": \"' + message.tool_call_id + '\"}[/TOOL_RESULTS]' }}\n    {%- else %}\n        {{- raise_exception(\"Only user and assistant roles are supported, with the exception of an initial optional system message!\") }}\n    {%- endif %}\n{%- endfor %}\n"""
    peft_params = LoraConfig(
        lora_alpha=16, lora_dropout=0.1, r=64, bias="none", task_type="CAUSAL_LM")
    
    small_dataset = small_dataset.map(lambda row: generate_train_text(row, tokenizer))
    
    model = AutoModelForCausalLM.from_pretrained(text2code_model_name)
    
    resume_from_checkpoint=False
    training_params = TrainingArguments(
        output_dir=text2code_tuned_model_name,
        num_train_epochs=2,
        per_device_train_batch_size=2,
        per_device_eval_batch_size=2,
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