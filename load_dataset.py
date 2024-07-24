from datasets import load_dataset
from tqdm.auto import tqdm
import os
import argparse
parser = argparse.ArgumentParser(description='Loading dataset')
parser.add_argument('path', type=str, help='Path to save dataset')
args = parser.parse_args()
dataset_path = args.path
if not os.path.exists(dataset_path):
  os.mkdir(dataset_path)

dataset = load_dataset("evgmaslov/gen_cars")
codes = dataset["train"]["codes"]
codes_path = os.path.join(dataset_path, "OutputCode")
if not os.path.exists(codes_path):
  os.mkdir(codes_path)
for i, code in tqdm(enumerate(codes), total=len(codes)):
  path = os.path.join(codes_path, f"Code_{i}.cs")
  with open(path, "w+") as f:
    f.write(code)
texts = dataset["train"]["texts"]
with open(os.path.join(dataset_path, "InputTexts.txt"), "w+") as f:
  f.writelines(texts)