from datasets import load_dataset, Dataset
import json
from tqdm.auto import tqdm
from huggingface_hub import login
import argparse

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(f"--train_nl_inputs_path", default="", type=str)
    parser.add_argument(f"--test_nl_inputs_path", default="", type=str)
    parser.add_argument(f"--subset_name", default="new_subset", type=str)
    args = parser.parse_args()
    
    login()
    dataset_name = "evgmaslov/cars"
    dataset = load_dataset(dataset_name)
    
    paths = [args.train_nl_inputs_path, args.test_nl_inputs_path]
    splits = ["train", "test"]
    for path, split in zip(paths, splits):
        if path == "":
            continue
        with open(path, "r") as f:
            data = json.load(f)
        
        old_dataset = dataset[split]
        new_dataset = {"input":[], "nl_input":[], "output":[]}
        
        keys = [int(k) for k in data.keys()]
        sorted_keys = sorted(keys)
        for ind in tqdm(sorted_keys, desc=f"Processing {split} split"):
            nl_input = ""
            try:
                nl_input = json.loads(data[str(ind)])["rewritten request"]
            except:
                continue
            if nl_input != "":
                row = old_dataset[ind]
                new_dataset["input"].append(row["input"])
                new_dataset["output"].append(row["output"])
                new_dataset["nl_input"].append(nl_input)
        new_dataset = Dataset.from_dict(new_dataset)
        new_dataset.push_to_hub(dataset_name, args.subset_name, split=split)