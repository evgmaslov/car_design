import json
import argparse
from datasets import load_dataset, Dataset
from tqdm.auto import tqdm
import os
from src.eval_utils import (
    extract_codes,
    extraction_score,
    compilation_score,
    transform_codes_to_jsons, 
    code_to_json,
    reconstruction_score
)

def main(dataset_name, subset_name, submit_path, code_by_types_path):
    with open(submit_path, "r") as f:
        generations = json.load(f)
    
    test_dataset = load_dataset(dataset_name, subset_name, split="test")
    eval_dataset = {"input":[], "output":[], "natural_input":[], "generation":[]}
    for key in generations.keys():
        ind = int(key)
        row = test_dataset[ind]
        gen = generations[key]
        eval_dataset["generation"].append(gen)
        eval_dataset["input"].append(row["input"])
        eval_dataset["output"].append(row["output"])
        eval_dataset["natural_input"].append(row["natural_input"])
    eval_dataset = Dataset.from_dict(eval_dataset)
    
    extraction = extraction_score(eval_dataset["generation"])
    
    codes = extract_codes(eval_dataset["generation"])
    compilation = compilation_score(codes)
    
    inds, jsons = transform_codes_to_jsons(codes)
    descriptions = [eval_dataset[ind]["input"] for ind in inds]
    path = code_by_types_path
    codes_dict = {}
    for d in tqdm(os.listdir(path), desc = "Preparing data"):
        codes_path = os.path.join(path, d)
        codes_dict[d] = []
        for name in os.listdir(codes_path):
            with open(os.path.join(codes_path, name), "r") as f:
                code = f.read()
            code_json = code_to_json(code)
            codes_dict[d].append(code_json)
    type_mapping = {
        "Coupe": "coupe",
        "Hatchback":"hatchback",
        "Limousine":"limousine",
        "Minivan":"minivan",
        "Pickup":"pickup",
        "Sedan":"sedan",
        "StationWagon":"station wagon",
        "SUV":"SUV",
        "Truck":"truck",
        "Van":"van",
    }
    codes_by_types = {}
    for key in codes_dict.keys():
        codes_by_types[type_mapping[key]] = codes_dict[key]
    reconstruction = reconstruction_score(jsons, descriptions, codes_by_types)
    
    print(f"""Extraction score: {extraction}
          Compilation score: {compilation}
          Reconstruction score: {reconstruction}""")

if __name__ == "__main__":
    dataset_name = "evgmaslov/cars"
    
    parser = argparse.ArgumentParser()
    parser.add_argument(f"--subset_name", default="mistral_nemo", type=str)
    parser.add_argument(f"--submit_path", default="generations.json", type=str)
    parser.add_argument(f"--code_by_types_path", default="code_by_types", type=str)
    args = parser.parse_args()
    main(dataset_name, args.subset_name, args.submit_path, args.code_by_types_path)