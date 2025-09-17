import os
import shutil

def backup_file_if_exists(original_path, backup_path):

    try:
        if not os.path.exists(original_path):
            print(f"original file does not exist: {original_path}")
            return False


        if not os.path.isfile(original_path):
            print(f"The path is not a file: {original_path}")
            return False

        backup_dir = os.path.dirname(backup_path)
        if backup_dir and not os.path.exists(backup_dir):
            os.makedirs(backup_dir)

        shutil.copy2(original_path, backup_path)
        print(f"The file backup was successful: {original_path} -> {backup_path}")
        return True

    except Exception as e:
        print(f"An error occurred during the backup process: {e}")
        return False


if __name__ == "__main__":
    original_file = r"~data_flight\Data\N6\CC\N6_CC_P_NB_AU.csv"

    backup_file = r"~data_flight\audio_process_backup"

    backup_file_if_exists(original_file, backup_file)