import os
from pydub import AudioSegment
from variables_audio import input_directories


def process_audio_directory(input_directory):
    if os.path.exists(input_directory):
        # Loop through all CSV files in the target directory
        output_csv_path = f"{input_directory}.csv"
        #extract ID AND CONDITION

        parts = input_directory.split('/')

        ID = parts[-3]
        condition = parts[-2]

        def generate_output_directories(ID, condition):
            output_directories = []
            for volume in ['_20', '_40', '_60']:
                output_directory = f"{input_directory}/{ID}_{condition}_P_NB_AU_louder{volume}"
                output_directories.append(output_directory)

            return output_directories


        output_directories = generate_output_directories(ID, condition)
        #print(f"output_directories: {output_directories}")
        print (f'{ID:^10}:')
        print(f'{condition:^10}:')
        print(f"{'_________please wait for the louder processing of ' + input_directory:>80}")


        for output_directory in output_directories:
            os.makedirs(output_directory, exist_ok=True)

            if "_20" in output_directory:
                volume_increase = 20
            elif "_40" in output_directory:
                volume_increase = 40
            elif "_60" in output_directory:
                volume_increase = 60
            else:
                volume_increase = 0


            for filename in os.listdir(input_directory):
                if filename.endswith('.wav'):

                    audio_file_path = os.path.join(input_directory, filename)

                    audio = AudioSegment.from_wav(audio_file_path)

                    louder_audio = audio + volume_increase

                    output_file_path = os.path.join(output_directory, filename)
                    louder_audio.export(output_file_path, format="wav")

        print(f"{'the louder files of ' + f'{ID}_{condition}' + ' has done':>50}")
        output_directories.insert(0, input_directory)
        audio_base_paths = output_directories
