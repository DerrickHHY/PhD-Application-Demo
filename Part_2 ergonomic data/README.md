# Ergonomic Data sample

## ***数据说明:***

这些数据是我在XREAL作为人体工学工程师收集并处理的Ergonomic Data的样本。原始数据由扫描上百名志愿者的扫描电子模型组成，包括亚洲人与非亚洲人。数据收集的初衷为AR眼镜的外形设计提供Ergonomic参考，例如鼻梁范围，鼻托活动范围，耳眼距离，两耳距离等。原始数据最终全部通过标点与数据提取（by Python）目标数据，最终生成目标数据报告与平局模型。这些数据与报告仍然被用于XREAL的设计参考。出于数据归属权和版权原因，此岩本仅会展示部分数据展示的图片以及全部由我个人完成的数据提取脚本。展示过程中出现的任何个人模型或者人像，均是我本人的人像资料，不会涉及任何数据志愿者的隐私。注：若存在任何侵权或法律问题，请联系我以删除此展示。

When using Python's *SpeechRecognition* package for automatic audio transcription, many low-quality audio files cannot be recognized and are therefore labeled as "*UNKNOWN*". To ensure data quality, this program was developed to facilitate manual cross-checking of the automated transcription results.

Program demonstration:

[https://github.com/user-attachments/assets/9087f3be-edc5-4b72-9897-dcf9da0fdd9d](https://github.com/user-attachments/assets/9087f3be-edc5-4b72-9897-dcf9da0fdd9d)

Note: Not all code was produced entirely by myself! LLM Deepseek provided significant assistance, including recommendations for suitable packages, necessary double-checking, debugging support, and interpretation of fundamental code.

## ***Requirements:***

The corresponding Python version and package requirements for the compatible version are provided in the *requirements.txt* file.

## ***Sample Data:***

For those who wish to run the relevant code, I have provided real suitable data in the *Sample\_data/N6/CS* directory. All audio data comes from myself (HUANG Hongyi) and replicates the poor audio quality scenarios encountered in the actual experimental data (though sometimes it can be even worse in practice).

## ***Processing Steps:***

All procedures can be completed within the ***\_\_mian\_\_transcript.py*** file. The entire workflow can be customized by adjusting the *auto\_transcription* and *human\_processing* parameters. For demonstration purposes, the current default parameter settings in the file will immediately enter the manual checking process after automatic transcription. In previous data processing, I would complete these steps sequentially: first performing automatic transcription on all data, then conducting manual verification altogether. This workflow arrangement is because the automatic transcription process for each participant's audio files (336 files) requires considerable time, approximately 10 minutes.

The manual checking process can be seen in the demo video. When opening the integrated audio data table for a specific condition of a participant, the first 1/4 of files are displayed in green, indicating they are from visual condition trials with no audio recording. The remaining 3/4 of files appear in either white or red. White represents audio files with satisfactory transcription quality that require no manual verification, while red indicates files with poor transcription quality that need manual processing.

For processing specific files, users can click on the filename (second column) to open both a visualization window (right side) and a content input window (left side). The visualization window displays the waveform of the audio file along with automatically detected reaction time (RT) (shown as green vertical lines in the waveform). Users can drag the green line to recalibrate the RT measurement. The audio can also be played by clicking the play button to facilitate manual checking of the audio content. After confirming the audio content, users can click the corresponding option in the input window to complete the manual processing for that file.

Once all files requiring processing have been addressed, the interface will show only green and white files. At this point, users can click the "*Export Full Data*" button in the upper left corner to export all audio data for one condition of one participant.

