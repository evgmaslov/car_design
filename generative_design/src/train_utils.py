import torch
import random
from tqdm.auto import tqdm
import wandb
from transformers import GenerationConfig
from transformers.integrations import WandbCallback
from generation_utils import generate
import re

class LLMSampleCB(WandbCallback):
    def __init__(self, trainer, test_dataset, num_samples=2, max_new_tokens=2048):
        super().__init__()
        self.sample_dataset = test_dataset.select(random.choices(range(len(test_dataset)), k=num_samples))
        self.model, self.tokenizer = trainer.model, trainer.tokenizer
        self.inference_model = None
        self.gen_config = GenerationConfig.from_pretrained(trainer.model.name_or_path,
                                                           max_new_tokens=max_new_tokens)

    def samples_table(self, examples):
        records_table = wandb.Table(columns=["prompt", "generation"])
        prompts = examples["text"]
        generations = generate(self.inference_model, self.tokenizer, self.gen_config, prompts)
        for i in range(len(prompts)):
            records_table.add_data(prompts[i], generations[i])
        return records_table

    def on_evaluate(self, args, state, control,  **kwargs):
        super().on_evaluate(args, state, control, **kwargs)
        self.inference_model = self.model
        records_table = self.samples_table(self.sample_dataset)
        self._wandb.log({"sample_predictions":records_table})

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