import os
import pandas as pd
import librosa
import numpy as np
import re
import speech_recognition as sr
#import speech_recognition as sr "put this part in each loop, otherwise always show the r and sr are not identified"
class AudioProcessor:
    def __init__(self, input_directory: str, silence_threshold_db, ID, condition, output_csv_path):
        self.input_directory = input_directory
        self.results = []
        self.silence_threshold_db = silence_threshold_db
        self.ID = ID
        self.condition = condition
        self.output_csv_path = output_csv_path


    def _detect_reaction_time(self, y, sr, min_silence_duration=0.2, energy_ratio_threshold=5.0):

        frame_length = int(0.03 * sr)
        hop_length = frame_length // 2
        energy = librosa.feature.rms(y=y, frame_length=frame_length, hop_length=hop_length)[0]
        energy_db = librosa.amplitude_to_db(energy, ref=np.max)


        background = energy_db[:int(0.1 * len(energy_db))]
        if len(background) > 0:
            threshold_db = np.median(background) + energy_ratio_threshold
        else:
            threshold_db = -40


        above_threshold = energy_db > threshold_db


        min_frames = int(min_silence_duration * sr / hop_length)
        reaction_frame = None

        for i in range(len(above_threshold) - min_frames):
            if sum(above_threshold[i:i + min_frames]) > min_frames * 0.8:
                reaction_frame = i
                break

        return (reaction_frame * hop_length) / sr if reaction_frame is not None else None
    def audio_processing_from_directory (self):
        for filename in os.listdir(self.input_directory):
            if filename.endswith('.wav'):
                # Create the full path to the audio file
                audio_file_path = os.path.join(self.input_directory, filename)

                # Load audio file using librosa
                y, sr = librosa.load(audio_file_path, sr=None)

                # Check if the audio data is empty
                if len(y) == 0:
                    # Append results with None values for empty audio
                    self.results.append({
                        'File Name': filename,
                        'Reaction Time (s)': None,
                        'Min Volume (dB)': None,
                        'Max Volume (dB)': None,
                        'Silence Threshold (dB)': self.silence_threshold_db,
                        'Length (s)': 0  # Length is zero for empty audio
                    })
                    continue  # Skip to the next file


                min_volume = np.min(librosa.amplitude_to_db(np.abs(y)))
                max_volume = np.max(librosa.amplitude_to_db(np.abs(y)))

                reaction_time = self._detect_reaction_time(y, sr)


                length = librosa.get_duration(y=y, sr=sr)


                self.results.append({
                    'File Name': filename,
                    'Reaction Time (s)': reaction_time,
                    'Min Volume (dB)': min_volume,
                    'Max Volume (dB)': max_volume,
                    'Silence Threshold (dB)': self.silence_threshold_db,
                    'Length (s)': length
                })


        df = self._following_transcript()
        df.to_csv(self.output_csv_path, index=False)

        print(f"the {self.ID}_{self.condition}'s Transcriptions have been written to '{self.output_csv_path}'")

        return df
#-----------------------------------------------------------
    def _following_transcript (self):
        file_transcriptions = {}
        output_directories = self.generate_output_directories()
        for index, audio_base_path in enumerate(output_directories):
            transcription_key = f'transcription_{index * 20}'  # Will generate: 0, 20, 40, 60

            for filename in os.listdir(audio_base_path):
                if filename.endswith('.wav'):

                    audio_data = os.path.join(audio_base_path, filename)

                    transcription = self.transcribe_yes_no(audio_data)

                    if filename not in file_transcriptions:
                        file_transcriptions[filename] = {
                            'transcription_0': '',
                            'transcription_20': '',
                            'transcription_40': '',
                            'transcription_60': ''
                        }

                    file_transcriptions[filename][transcription_key] = transcription


        transcriptions_columns = []
        for filename, transcriptions in file_transcriptions.items():
            transcriptions_columns.append((
                filename,
                transcriptions['transcription_0'],
                transcriptions['transcription_20'],
                transcriptions['transcription_40'],
                transcriptions['transcription_60']
            ))

        df = pd.DataFrame(transcriptions_columns,
                          columns=['File Name', 'transcription_0', 'transcription_20', 'transcription_40',
                                   'transcription_60'])

        # Write the DataFrame to a CSV file
        results_df = pd.DataFrame(self.results)
        df = df.merge(results_df, on='File Name', how='left')

        segments = [
            (0, 41, "NV_1"),
            (42, 83, "NV_2"),
            (84, 125, "NA_1"),
            (126, 167, "NA_2"),
            (168, 209, "NDU_1"),
            (210, 251, "NDU_2"),
            (252, 293, "NDU_3"),
            (294, 335, "NDU_4")
        ]
        segment_list = []
        for start, end, label in segments:
            segment_list.extend([label] * (end - start + 1))

        segment_df = pd.DataFrame({'Segment': segment_list})
        df = pd.concat([segment_df, df], axis=1)
        return df



    def transcribe_yes_no(self, audio_path):

        r = sr.Recognizer()
        r.energy_threshold = 2000
        r.pause_threshold = 0.5
        try:
            with sr.AudioFile(audio_path) as source:

                r.adjust_for_ambient_noise(source, duration=0.5)
                audio_data = r.record(source, duration=5)


                raw_text = r.recognize_google(audio_data).upper()


                yes_pattern = r'\b(YES|YEAH|YEP|YUP|YE|Y)\b'
                no_pattern = r'\b(NO|NOPE|NAH|N|O)\b'


                yes_matches = [(m.group(), m.start()) for m in re.finditer(yes_pattern, raw_text)]
                no_matches = [(m.group(), m.start()) for m in re.finditer(no_pattern, raw_text)]


                all_matches = yes_matches + no_matches
                all_matches_sorted = sorted(all_matches, key=lambda x: x[1])


                if all_matches_sorted:
                    return " → ".join([match[0] for match in all_matches_sorted])
                else:
                    return "UNCERTAIN"

        except sr.UnknownValueError:
            return "UNKNOWN"
        except sr.RequestError as e:
            return f"API_ERROR: {str(e)}"
        except Exception as e:
            return f"ERROR: {str(e)}"

    # , then start the transcriptions
    def generate_output_directories(self):# prossor.generate_output_directories() usage example
        output_directories = []
        output_directories.insert(0, self.input_directory)
        for volume in ['_20', '_40', '_60']:
            output_directory = f"{self.input_directory}/{self.ID}_{self.condition}_P_NB_AU_louder{volume}"
            output_directories.append(output_directory)
        return output_directories



