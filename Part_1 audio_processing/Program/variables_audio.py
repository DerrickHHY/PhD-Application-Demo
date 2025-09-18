import os


ids = {"N6"}
conditions = {"CS"}

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
base_directory = os.path.join(BASE_DIR, "sample_data", "original_data")

input_directories = []
silence_threshold_db = -5


def add_suffix_to_dir_paths(path, suffix):

    dirname, basename = os.path.split(path)

    new_basename = f"{basename}{suffix}"

    new_path = os.path.join(path, new_basename)
    new_path = new_path.replace("\\", "/")

    return new_path


for ID in ids:
    for condition in conditions:
        input_directory = f"{base_directory}/{ID}/{condition}/{ID}_{condition}_P_NB_AU"
        input_directories.append(input_directory)