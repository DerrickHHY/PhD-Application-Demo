from variables_audio import input_directories, silence_threshold_db, add_suffix_to_dir_paths, base_directory
import os
import tkinter as tk
from louder import process_audio_directory
from FUN_backup import backup_file_if_exists
from FUN_tableview import EnhancedDataFrameViewer
#----------------------
backup_file = os.path.join(base_directory, "audio_process_backup")


auto_transcription = "DO_IT"
human_processing = "DO_IT"

##If the volume of certain participants' audio recordings are too low could use: human_processing_louder = 'TRUE'
# Otherwise, please use: human_processing_louder = 'False'
human_processing_louder = 'TRUE'
#human_processing_louder = 'False'

### If plan to separately execute the auto transcription and manual process, please conduct the following steps:
    ## --1-- First step is to auto transcript all audio file, then put them into an integrated data file.-------------------
    #  for this step, please use the following configuration
    #auto_transcription = "DO_IT"
    # human_processing = "NOT DO"

    ## --2-- Second step is to manual cross-check the content and reaction time (RT) in transcript.-------------------------
    #  for this step, please use the following configuration
    # auto_transcription = "DONE"
    #human_processing = "DO_IT"


total_idx = len (input_directories)
for idx, input_directory in enumerate(input_directories):

    print (f"___________________{idx+1}/{total_idx}______________________________")
    if os.path.exists(input_directory):
        if auto_transcription == "DO_IT":
            process_audio_directory(input_directory)

        output_csv_path = f"{input_directory}.csv"
        print(output_csv_path)
        parts = input_directory.split('/')
        ID = parts[-3]
        condition = parts[-2]
        input_directory = input_directory
        results = []
        if auto_transcription == "DO_IT":
            from FUN_transcript import AudioProcessor
            AudioProcessing = AudioProcessor (input_directory, silence_threshold_db, ID, condition, output_csv_path)
            transcription_df = AudioProcessing.audio_processing_from_directory()
            backup_file_if_exists(output_csv_path, backup_file)

        if human_processing == "DO_IT":

            if human_processing_louder == 'TRUE':
                input_directory = add_suffix_to_dir_paths(input_directory, suffix="_louder_40")
                # through adjust the amplitude_max_overall parameter to make the audio wave plotting fit your current volumn
                #amplitude_max_overall = 300 #(for _20)
                #amplitude_max_overall = 4000 #(for _40)
                amplitude_max_overall = 3000  # (for _40)
                amplitude_min_overall = -amplitude_max_overall

                # _______additional setting___________________
                root = tk.Tk()
                app = EnhancedDataFrameViewer(root, output_csv_path, input_directory, amplitude_min_overall, amplitude_max_overall)
                root.mainloop()
            else:
                root = tk.Tk()
                app = EnhancedDataFrameViewer(root, output_csv_path, input_directory)
                root.mainloop()

print ("All transcriptions have done")