# Semi-automated manual audio cross-checking

One of the my experiment assessments, contained a dual N-back assessment, which required participants to complete the task using both keyboard responses and verbal responses. As a result, each participant generates a large number of short audio files (each condition produces 6 clocks × 40 trials of audio data, with 40 participants and 2 conditions per participant). Even though, before the task begins, I repeatedly remind participants to provide clear and loud verbal responses for each trial, some may forget this requirement during the high-cognitive-load task, leading to poor audio quality.

When using Python's _SpeechRecognition_ package for automatic audio transcription, many low-quality audio files cannot be recognized and are therefore labeled as "_UNKNOWN_". To ensure data quality, this program was developed to facilitate manual cross-checking of the automated transcription results.

Program demonstration:

[https://github.com/user-attachments/assets/eb29b1bf-8d09-470f-b6d3-a8bf9cafb79a](https://github.com/user-attachments/assets/eb29b1bf-8d09-470f-b6d3-a8bf9cafb79a)

Note: Not all code was produced entirely by myself! LLM Deepseek provided significant assistance, including recommendations for suitable packages, necessary double-checking, debugging support, and interpretation of fundamental code.

## ***Requirements:***
The corresponding Python version and package requirements for the compatible version are provided in the _requirements.txt_ file.

## ***Sample Data:***
For those who wish to run the relevant code, I have provided real suitable data in the _Sample_data/N6/CS_ directory. All audio data comes from myself (HUANG Hongyi) and replicates the poor audio quality scenarios encountered in the actual experimental data (though sometimes it can be even worse in practice).

## ***Processing Steps:***

All procedures can be completed within the ***__mian__transcript.py*** file. The entire workflow can be customized by adjusting the _auto_transcription_ and _human_processing_ parameters. For demonstration purposes, the current default parameter settings in the file will immediately enter the manual checking process after automatic transcription. In previous data processing, I would complete these steps sequentially: first, performing automatic transcription on all data, then conducting manual verification altogether. This workflow arrangement is because the automatic transcription process for each participant's audio files (336 files) requires considerable time, approximately 10 minutes.

The manual checking process can be seen in the demo video. When opening the integrated audio data table for a specific condition of a participant, the first 1/4 of files are displayed in green, indicating they are from visual condition trials with no audio recording. The remaining 3/4 of the files appear in either white or red. White represents audio files with satisfactory transcription quality that require no manual verification, while red indicates files with poor transcription quality that need manual processing.

For processing specific files, users can click on the filename (second column) to open both a visualization window (right side) and a content input window (left side). The visualization window displays the waveform of the audio file along with automatically detected reaction time (RT) (shown as green vertical lines in the waveform). Users can drag the green line to recalibrate the RT measurement. The audio can also be played by clicking the play button to facilitate manual checking of the audio content. After confirming the audio content, users can click the corresponding option in the input window to complete the manual processing for that file.

Once all files requiring processing have been addressed, the interface will show only green and white files. At this point, users can click the "_Export Full Data_" button in the upper left corner to export all audio data for one condition of one participant.

