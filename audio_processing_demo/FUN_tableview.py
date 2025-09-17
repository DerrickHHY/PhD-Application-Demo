import tkinter as tk
from tkinter import ttk, messagebox, simpledialog
import pandas as pd
import os
import shutil
from FUN_playing import AudioPlayerApp

class EnhancedDataFrameViewer:
    def __init__(self, root, csv_file_path, input_dir=None, amplitude_min_overall= -100, amplitude_max_overall= 100):
        self.root = root
        self.root.title("Enhanced CSV Viewer")
        self.amplitude_min = amplitude_min_overall
        self.amplitude_max = amplitude_max_overall
        # self.root.geometry("1200x800")
        window_width = 1200
        window_height = 800


        screen_width = self.root.winfo_screenwidth()
        screen_height = self.root.winfo_screenheight()
        x = (screen_width - window_width) // 2 - 180
        y = (screen_height - window_height) // 2


        self.root.geometry(f"{window_width}x{window_height}+{x}+{y}")
        self.csv_file_path = csv_file_path
        self.input_dir = input_dir
        self.audio_player = None



        self.main_frame = tk.Frame(self.root)
        self.main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)


        self.load_and_process_csv()

        self.create_widgets()

        self.apply_highlight_rules()

        self.tree.bind("<Button-1>", self.on_cell_click)

    def load_and_process_csv(self):
        try:
            self.df = pd.read_csv(self.csv_file_path)


            self.original_file_name = os.path.basename(self.csv_file_path)
            self.file_label_text = (f"Loaded: {self.original_file_name} | "
                                    f"Rows: {len(self.df)} | "
                                    f"Columns: {len(self.df.columns)}")
        except Exception as e:
            self.df = pd.DataFrame({"Error": [f"Failed to load CSV: {str(e)}"]})
            self.file_label_text = f"Error loading: {self.csv_file_path}"

    def create_widgets(self):

        self.file_label = tk.Label(self.main_frame, text=self.file_label_text, anchor='w')
        self.file_label.pack(fill=tk.X, pady=5)

        control_frame = tk.Frame(self.main_frame)
        control_frame.pack(fill=tk.X, pady=5)

        export_btn = tk.Button(control_frame, text="Export Full Data",
                               command=self.export_full_data)
        export_btn.pack(side=tk.LEFT, padx=5)

        search_frame = tk.Frame(control_frame)
        search_frame.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=10)

        tk.Label(search_frame, text="Search:").pack(side=tk.LEFT)

        self.search_var = tk.StringVar()
        self.search_entry = tk.Entry(search_frame, textvariable=self.search_var)
        self.search_entry.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=5)

        search_btn = tk.Button(search_frame, text="Search", command=self.search_data)
        search_btn.pack(side=tk.LEFT)

        self.create_table_with_scroll()

    def create_table_with_scroll(self):
        container = tk.Frame(self.main_frame)
        container.pack(fill=tk.BOTH, expand=True)

        self.tree = ttk.Treeview(container, show="headings")

        self.style = ttk.Style()
        self.style.configure("Highlight.Treeview", background="#ffdddd")
        self.style.configure("Normal.Treeview", background="white")
        self.style.configure("Grayed.Treeview", background="#708069")

        yscroll = ttk.Scrollbar(container, orient="vertical", command=self.tree.yview)
        xscroll = ttk.Scrollbar(container, orient="horizontal", command=self.tree.xview)
        self.tree.configure(yscrollcommand=yscroll.set, xscrollcommand=xscroll.set)

        self.tree.grid(row=0, column=0, sticky="nsew")
        yscroll.grid(row=0, column=1, sticky="ns")
        xscroll.grid(row=1, column=0, sticky="ew")

        container.grid_rowconfigure(0, weight=1)
        container.grid_columnconfigure(0, weight=1)

        self.populate_treeview()

    def populate_treeview(self):
        self.tree["columns"] = list(self.df.columns)

        for col in self.df.columns:
            self.tree.column(col, width=120, anchor='w')
            self.tree.heading(col, text=col,
                              command=lambda c=col: self.treeview_sort_column(c, False))

        for i, row in self.df.iterrows():
            if i >= 10000:
                self.tree.insert("", "end", values=["..."] * len(self.df.columns))
                break
            self.tree.insert("", "end", values=list(row), tags=("normal",))

    def on_cell_click(self, event):
        region = self.tree.identify("region", event.x, event.y)
        if region != "cell":
            return

        column = self.tree.identify_column(event.x)
        row_id = self.tree.identify_row(event.y)

        if column == "#2":
            if hasattr(self, 'edit_window') and self.edit_window.winfo_exists():
                self.edit_window.destroy()
                self.close_player()
            item = self.tree.item(row_id)
            row_values = item["values"]
            row_index = self.tree.index(row_id)

            cell_text = row_values[1]
            self.create_edit_window(row_index, cell_text)
            self.on_play_click()
            row_values = self.df.iloc[self.current_edit_row].values
            rt_value = float(row_values[6]) if len(row_values) > 6 and pd.notna(row_values[6]) else 0.01
            self.RT = rt_value

    def create_edit_window(self, row_index, cell_text):
        self.edit_window = tk.Toplevel(self.root)
        self.edit_window.title("Edit Cell")

        main_x = self.root.winfo_x()
        main_y = self.root.winfo_y()
        main_width = self.root.winfo_width()
        edit_window_width = 300
        edit_window_height = 200
        x = main_x - 150
        y = main_y + (self.root.winfo_height() - edit_window_height) // 2

        self.edit_window.geometry(f"{edit_window_width}x{edit_window_height}+{x}+{y}")

        self.current_edit_row = row_index
        self.current_cell_text = cell_text

        play_btn = tk.Button(self.edit_window, text="Play",
                             command=self.on_play_click)
        play_btn.pack(pady=10)

        btn_frame = tk.Frame(self.edit_window)
        btn_frame.pack(pady=20)

        # YES Button
        yes_btn = tk.Button(btn_frame, text="YES", width=10,
                            command=lambda: self.on_option_click("YES"))
        yes_btn.pack(side=tk.LEFT, padx=5)

        # NO Button
        no_btn = tk.Button(btn_frame, text="NO", width=10,
                           command=lambda: self.on_option_click("NO"))
        no_btn.pack(side=tk.LEFT, padx=5)

        # NO_RESPOND Button
        no_resp_btn = tk.Button(btn_frame, text="NO_RESPOND", width=10,
                                command=lambda: self.on_option_click("NO_RESPOND"))
        no_resp_btn.pack(side=tk.LEFT, padx=5)
    def close_player(self):
        if self.audio_player:
            row_values = self.df.iloc[self.current_edit_row].values
            rt_value = float(row_values[6]) if len(row_values) > 6 and pd.notna(row_values[6]) else 0.01
            self.RT = round(float(self.audio_player.current_rt), 3)

            if self.RT != rt_value:

                print(f"RT has been updated! {self.RT = } ______ {rt_value = }")
            else:

                print ("No RT updating")

        if self.audio_player:
            self.audio_player.on_closing()
            self.audio_player = None

    def on_play_click(self):
        if not self.input_dir:
            messagebox.showwarning("Warning", "No input directory specified")
            return

        row_values = self.df.iloc[self.current_edit_row].values
        audio_file = row_values[1]
        rt_value = float(row_values[6]) if len(row_values) > 6 and pd.notna(row_values[6]) else 0.01


        audio_path = os.path.join(self.input_dir, audio_file)


        if self.audio_player:
            self.audio_player.on_closing()


        player_window = tk.Toplevel(self.root)
        player_window.title("Audio Player")

        main_x = self.root.winfo_x()
        main_width = self.root.winfo_width()
        x = main_x + main_width - 400
        y = self.root.winfo_y()
        player_window.geometry(f"800x600+{x}+{y}")

        self.audio_player = AudioPlayerApp(
            player_window,
            audio_path,
            rt_value,
            amplitude_min=self.amplitude_min,
            amplitude_max=self.amplitude_max
        )

        player_window.protocol("WM_DELETE_WINDOW", self.close_player)

    def on_option_click(self, option):
        self.close_player()
        for i in range(2, 6):
            self.df.iloc[self.current_edit_row, i] = option

        self.df.iloc[self.current_edit_row, 6] = self.RT
        self.df.to_csv(self.csv_file_path, index=False)

        self.edit_window.destroy()
        self.refresh_table()

        self.apply_highlight_rules()

    def refresh_table(self):

        for item in self.tree.get_children():
            self.tree.delete(item)

        self.populate_treeview()

    def apply_highlight_rules(self):

        for item in self.tree.get_children():
            self.tree.item(item, tags=("normal",))
        for i in range(0, 84):
            item = self.tree.get_children()[i]
            self.tree.item(item, tags=("Grayed",))
            self.tree.tag_configure("Grayed", background="#708069")


        for i in range(84, len(self.df)):
            row = self.df.iloc[i]


            cols_to_check = row[2:6]


            inconsistent = len(set(cols_to_check)) > 1


            unknown_count = sum(str(x).lower() == "unknown" for x in cols_to_check)

            valid_responses = {"yes", "no", "no_respond"}
            contains_valid = any(str(x).lower() in valid_responses for x in cols_to_check)
            # self.style.configure("Grayed.Treeview", background="#f0f0f0")

            if inconsistent or unknown_count >= 2 or not contains_valid:
                item = self.tree.get_children()[i]
                self.tree.item(item, tags=("highlight",))
                self.tree.tag_configure("highlight", background="#ffdddd")

    def export_full_data(self):
        try:
            dir_name = os.path.dirname(self.csv_file_path)
            base_name = os.path.splitext(self.original_file_name)[0]
            export_path = os.path.join(dir_name, f"{base_name}_FULL.csv")

            self.df.to_csv(export_path, index=False)
            messagebox.showinfo("Export Success",
                                f"Data exported to:/n{export_path}")
        except Exception as e:
            messagebox.showerror("Export Error", f"Failed to export: {str(e)}")

    def search_data(self):
        query = self.search_var.get().lower()
        if not query:
            return

        for item in self.tree.selection():
            self.tree.selection_remove(item)

        for item in self.tree.get_children():
            values = [str(v).lower() for v in self.tree.item(item)["values"]]
            if any(query in val for val in values):
                self.tree.selection_add(item)
                self.tree.see(item)

    def treeview_sort_column(self, col, reverse):
        data = [(self.tree.set(child, col), child)
                for child in self.tree.get_children()]

        try:
            data.sort(key=lambda x: float(x[0]), reverse=reverse)
        except ValueError:
            data.sort(reverse=reverse)

        for idx, (val, child) in enumerate(data):
            self.tree.move(child, "", idx)

        self.tree.heading(col, command=lambda: self.treeview_sort_column(col, not reverse))
if __name__ == "__main__":
    root = tk.Tk()
    csv_file = r"~/data_flight/Data/N6/CC/N6_CC_P_NB_AU.csv"
    input_dir = "~/data_flight/Data/N6/CC/N6_CC_P_NB_AU"
    # amplitude_min_overall = -100
    # amplitude_max_overall = 100
    # this two value are default value
    app = EnhancedDataFrameViewer(root, csv_file, input_dir)
    root.mainloop()

