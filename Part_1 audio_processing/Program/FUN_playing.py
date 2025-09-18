import tkinter as tk
from tkinter import ttk
import pyaudio
import wave
import threading
import time
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg


class AudioPlayerApp:
    def __init__(self, root, audio_file_path, rt, amplitude_min, amplitude_max):
        self.root = root
        self.original_rt = rt
        self.current_rt = rt
        self.root.title("Enhanced Audio Player with RT Editor")
        self.root.geometry("900x650")

        # Audio variables
        self.p = pyaudio.PyAudio()
        self.stream = None
        self.wf = None
        self.playing = False
        self.paused = False
        self.current_position = 0
        self.audio_file_path = audio_file_path
        self.audio_data = None
        self.sample_rate = None
        self.dragging_rt = False

        # Amplitude range settings
        self.amplitude_min = amplitude_min
        self.amplitude_max = amplitude_max

        # Create UI
        self.create_widgets()

        # Load the audio file
        self.load_file()

    def create_widgets(self):
        # Main container
        main_frame = tk.Frame(self.root)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        # File info frame
        file_frame = tk.Frame(main_frame)
        file_frame.pack(fill=tk.X, pady=5)

        self.file_label = tk.Label(file_frame, text=f"File: {self.audio_file_path}", anchor='w')
        self.file_label.pack(fill=tk.X)

        # RT control frame
        rt_frame = tk.Frame(main_frame)
        rt_frame.pack(fill=tk.X, pady=5)

        # Original RT display
        tk.Label(rt_frame, text="RT_from transcript:").pack(side=tk.LEFT, padx=5)
        self.original_rt_label = tk.Label(rt_frame, text=f"{self.original_rt:.3f}s")
        self.original_rt_label.pack(side=tk.LEFT, padx=5)

        # Adjusted RT display
        tk.Label(rt_frame, text="RT_after realigning:").pack(side=tk.LEFT, padx=5)
        self.current_rt_label = tk.Label(rt_frame, text=f"{self.current_rt:.3f}s")
        self.current_rt_label.pack(side=tk.LEFT, padx=5)



        # Waveform display
        self.figure = plt.Figure(figsize=(8, 3), dpi=100)
        self.ax = self.figure.add_subplot(111)
        self.canvas = FigureCanvasTkAgg(self.figure, master=main_frame)
        self.canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)

        # Bind mouse events for RT line dragging
        self.canvas.mpl_connect('button_press_event', self.on_press)
        self.canvas.mpl_connect('motion_notify_event', self.on_motion)
        self.canvas.mpl_connect('button_release_event', self.on_release)

        # Playback controls
        control_frame = tk.Frame(main_frame)
        control_frame.pack(fill=tk.X, pady=10)


        # play button
        self.play_button = tk.Button(control_frame, text="Play", command=self.toggle_play)
        self.play_button.pack(side=tk.LEFT, padx=5)

        # Refresh button
        self.refresh_button = tk.Button(control_frame, text="Refresh RT", command=self.confirm_rt_adjustment)
        self.refresh_button.pack(side=tk.LEFT, padx=5)

        # Progress bar
        self.progress = ttk.Progressbar(main_frame, orient="horizontal", length=600, mode="determinate")
        self.progress.pack(fill=tk.X, pady=5)

        # Time display with milliseconds precision
        self.time_label = tk.Label(main_frame, text="00:00.000 / 00:00.000")
        self.time_label.pack()
    def load_file(self):
        try:
            self.wf = wave.open(self.audio_file_path, 'rb')
            self.sample_rate = self.wf.getframerate()
            self.num_channels = self.wf.getnchannels()
            self.total_frames = self.wf.getnframes()

            self.wf.rewind()
            raw_data = self.wf.readframes(self.total_frames)

            if self.wf.getsampwidth() == 1:
                audio_data = np.frombuffer(raw_data, dtype=np.uint8) - 128
            else:
                audio_data = np.frombuffer(raw_data, dtype=np.int16)

            if self.num_channels > 1:
                audio_data = audio_data.reshape(-1, self.num_channels)[:, 0]  # 取左声道

            self.audio_data = audio_data

            self.frame_to_sample = np.linspace(
                0,
                len(self.audio_data) - 1,
                num=self.total_frames,
                dtype=int
            )

            # UI setting
            duration = self.total_frames / self.sample_rate
            self.progress["maximum"] = self.total_frames
            self.time_label.config(text=f"00:00.000 / {self.format_time(duration)}")
            self.plot_waveform()

        except Exception as e:
            print(f"Error loading file: {e}")
            self.file_label.config(text=f"Error loading: {self.audio_file_path}")
    def _convert_24bit_to_32bit(self, data):

        if len(data) % 3 != 0:
            data = data[:len(data) - (len(data) % 3)]

        dtype = np.dtype('b')
        reshaped = np.frombuffer(data, dtype=dtype).reshape(-1, 3)
        padded = np.pad(reshaped, ((0, 0), (1, 0)), mode='constant')
        return padded.view('<i4').flatten()

    def plot_waveform(self):
        self.ax.clear()

        # Wave plotting
        step = max(1, len(self.audio_data) // 5000)
        x = np.arange(0, len(self.audio_data), step)
        y = self.audio_data[::step]

        self.ax.plot(x, y, color='b', linewidth=0.5)
        self.ax.set_xlim(0, len(self.audio_data))
        self.ax.set_ylim(self.amplitude_min, self.amplitude_max)
        self.ax.set_title("Audio Waveform (Drag green line to adjust RT)")
        self.ax.set_xlabel("Samples")
        self.ax.set_ylabel("Amplitude")
        self.ax.grid(True, linestyle='--', alpha=0.5)

        self.current_line = self.ax.axvline(x=0, color='r', linewidth=1)

        rt_position = int(self.current_rt * self.sample_rate)
        self.rt_line = self.ax.axvline(x=rt_position, color='g', linestyle='-', linewidth=2, picker=5)

        self.canvas.draw()

    def on_press(self, event):
        if event.inaxes != self.ax:
            return

        if self.rt_line.contains(event)[0]:
            self.dragging_rt = True

    def on_motion(self, event):
        if not self.dragging_rt or event.inaxes != self.ax:
            return

        xdata = max(0, min(event.xdata, len(self.audio_data)))

        self.rt_line.set_xdata([xdata, xdata])

        self.current_rt = xdata / self.sample_rate
        self.current_rt_label.config(text=f"{self.current_rt:.3f}s")

        self.canvas.draw()

    def on_release(self, event):
        self.dragging_rt = False

    def confirm_rt_adjustment(self):
        return self.current_rt
    def update_waveform_position(self, position):
        """Update the position indicator on the waveform"""
        if hasattr(self, 'current_line'):
            self.current_line.set_xdata([position, position])
            self.canvas.draw()
    def toggle_play(self):
        if not self.wf:
            return

        if self.playing and not self.paused:
            self.paused = True
            self.play_button.config(text="Resume")
            if self.stream:
                self.stream.stop_stream()
        elif self.playing and self.paused:
            self.paused = False
            self.play_button.config(text="Pause")
            if self.stream:
                self.stream.start_stream()
            threading.Thread(target=self.update_progress, daemon=True).start()
        else:
            self.playing = True
            self.play_button.config(text="Pause")
            self.play_audio()
    def play_audio(self):
        if not self.wf:
            return

        self.wf.setpos(self.current_position)

        self.stream = self.p.open(
            format=self.p.get_format_from_width(self.wf.getsampwidth()),
            channels=self.wf.getnchannels(),
            rate=self.wf.getframerate(),
            output=True
        )

        threading.Thread(target=self.audio_stream_thread, daemon=True).start()
        threading.Thread(target=self.update_progress, daemon=True).start()
    def audio_stream_thread(self):
        chunk = 1024
        data = self.wf.readframes(chunk)

        while data and self.playing:
            if not self.paused:
                self.stream.write(data)
                data = self.wf.readframes(chunk)
            else:
                time.sleep(0.1)

        # Auto-reset when playback completes
        if not data and self.playing:
            self.root.after(100, self.reset_playback)
    def update_progress(self):
        while self.playing and not self.paused:
            if self.wf and self.stream:
                current_frame = self.wf.tell()

                self.progress["value"] = current_frame

                if current_frame < len(self.frame_to_sample):
                    sample_pos = self.frame_to_sample[current_frame]
                    self.update_waveform_position(sample_pos)

                current_time = current_frame / self.sample_rate
                total_time = self.total_frames / self.sample_rate
                self.time_label.config(text=f"{self.format_time(current_time)} / {self.format_time(total_time)}")

            time.sleep(0.01)
    def reset_playback(self):
        """Reset playback to beginning"""
        self.playing = False
        self.paused = False
        self.current_position = 0
        self.play_button.config(text="Play")

        if self.stream:
            self.stream.stop_stream()
            self.stream.close()
            self.stream = None

        if self.progress:
            self.progress["value"] = 0

        if self.time_label and self.wf:
            total_time = self.wf.getnframes() / float(self.sample_rate)
            self.time_label.config(text=f"00:00.000 / {self.format_time(total_time)}")

        # Reset waveform position indicator
        self.update_waveform_position(0)
    def format_time(self, seconds):
        """Format time with milliseconds precision (MM:SS.mmm)"""
        minutes = int(seconds // 60)
        seconds_remaining = seconds % 60
        milliseconds = int((seconds_remaining - int(seconds_remaining)) * 1000)
        return f"{minutes:02d}:{int(seconds_remaining):02d}.{milliseconds:03d}"
    def on_closing(self):
        self.reset_playback()
        self.p.terminate()
        self.root.destroy()

if __name__ == "__main__":
    root = tk.Tk()
    #audio_path = r"~\01_n_back\audio\a.wav"
    audio_path = r"~\data_flight\Data\N6\CC\N6_CC_P_NB_AU\recording_verval_resp_a_2025-01-09_19h16.05.604.wav"
    rt = 1.222
    app = AudioPlayerApp(root, audio_path, rt,amplitude_min=-100, amplitude_max=100)
    root.protocol("WM_DELETE_WINDOW", app.on_closing)
    root.mainloop()
    adjusted_rt = app.current_rt
    print(f"Original RT: {rt}, Adjusted RT: {adjusted_rt}")
