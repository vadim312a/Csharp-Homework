using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace DiaryApp
{
    public partial class Form1 : Form
    {
        private BindingList<DiaryEntry> diaryEntries;
        private BindingSource diaryBindingSource;

        private DataGridView dgvEntries;
        private TextBox txtTitle;
        private TextBox txtContent;
        private DateTimePicker dtpDate;
        private Label lblStatus;
        private System.Windows.Forms.Timer autoSaveTimer;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load; 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeCustomUI();
            InitData();
            SetupAutoSave();
        }

        private void InitData()
        {
            diaryEntries = new BindingList<DiaryEntry>();

            diaryEntries.Add(new DiaryEntry(1, DateTime.Now, "Мой первый день", "Сегодня я начал писать свой продвинутый дневник!"));

            diaryBindingSource = new BindingSource { DataSource = diaryEntries };
            dgvEntries.DataSource = diaryBindingSource;

            txtTitle.DataBindings.Add("Text", diaryBindingSource, "Title", true, DataSourceUpdateMode.OnPropertyChanged);
            txtContent.DataBindings.Add("Text", diaryBindingSource, "Content", true, DataSourceUpdateMode.OnPropertyChanged);
            dtpDate.DataBindings.Add("Value", diaryBindingSource, "Date", true, DataSourceUpdateMode.OnPropertyChanged);
        }


        private void SaveJson(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(diaryEntries.ToList(), options);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        private List<DiaryEntry> LoadJson(string path)
        {
            if (!File.Exists(path)) return new List<DiaryEntry>();
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new List<DiaryEntry>();
            return JsonSerializer.Deserialize<List<DiaryEntry>>(json);
        }

        private void SaveXml(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<DiaryEntry>));
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, diaryEntries.ToList());
            }
        }

        private List<DiaryEntry> LoadXml(string path)
        {
            if (!File.Exists(path) || File.ReadAllText(path).Length == 0) return new List<DiaryEntry>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<DiaryEntry>));
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                return (List<DiaryEntry>)serializer.Deserialize(sr);
            }
        }

        private void SaveCsv(string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine("Id;Date;Title;Content");
                foreach (var entry in diaryEntries)
                {
                    string safeTitle = entry.Title?.Replace(";", ",").Replace("\r\n", " ").Replace("\n", " ") ?? "";
                    string safeContent = entry.Content?.Replace(";", ",").Replace("\r\n", " ").Replace("\n", " ") ?? "";
                    sw.WriteLine($"{entry.Id};{entry.Date:yyyy-MM-dd HH:mm:ss};{safeTitle};{safeContent}");
                }
            }
        }

        private List<DiaryEntry> LoadCsv(string path)
        {
            var list = new List<DiaryEntry>();
            if (!File.Exists(path)) return list;

            var lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var parts = lines[i].Split(';');
                if (parts.Length >= 4)
                {
                    int id = int.Parse(parts[0]);
                    DateTime date = DateTime.Parse(parts[1]);
                    string title = parts[2];
                    string content = parts[3];
                    list.Add(new DiaryEntry(id, date, title, content));
                }
            }
            return list;
        }

        private void SetupAutoSave()
        {
            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 5 * 60 * 1000; 
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
            autoSaveTimer.Start();
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string backupPath = Path.Combine(Application.StartupPath, "diary_autosave_backup.json");
                SaveJson(backupPath);
                lblStatus.Text = $" Фоновое автосохранение успешно выполнено в {DateTime.Now:HH:mm:ss}";
            }
            catch
            {
                lblStatus.Text = " Ошибка при автоматическом сохранении данных.";
            }
        }

        private void InitializeCustomUI()
        {
            this.Text = "Личный Дневник (Поддержка XML, JSON, CSV и Автосохранение)";
            this.Size = new System.Drawing.Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.Controls.Add(mainLayout);

            FlowLayoutPanel filePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            Button btnJson = new Button { Text = "Опции JSON", Width = 120, Height = 35 };
            Button btnXml = new Button { Text = "Опции XML", Width = 120, Height = 35 };
            Button btnCsv = new Button { Text = "Опции CSV", Width = 120, Height = 35 };
            Label lblInfo = new Label { Text = "<- Конвертируйте форматы: загрузите один файл, а сохраните в другом!", AutoSize = true, Padding = new Padding(0, 10, 0, 0) };
            filePanel.Controls.AddRange(new Control[] { btnJson, btnXml, btnCsv, lblInfo });
            mainLayout.Controls.Add(filePanel, 0, 0);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 400 };
            mainLayout.Controls.Add(split, 0, 1);

            TableLayoutPanel leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));

            dgvEntries = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = true };
            FlowLayoutPanel entryButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(3) };
            Button btnAdd = new Button { Text = "+ Новая запись", Height = 30, Width = 120 };
            Button btnDel = new Button { Text = "- Удалить", Height = 30, Width = 120 };
            entryButtons.Controls.AddRange(new Control[] { btnAdd, btnDel });

            leftLayout.Controls.Add(dgvEntries, 0, 0);
            leftLayout.Controls.Add(entryButtons, 0, 1);
            split.Panel1.Controls.Add(leftLayout);

            TableLayoutPanel rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, Padding = new Padding(10) };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            dtpDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            txtTitle = new TextBox { Dock = DockStyle.Fill };
            txtContent = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };

            rightLayout.Controls.Add(new Label { Text = "Дата создания:" }, 0, 0);
            rightLayout.Controls.Add(dtpDate, 0, 1);
            rightLayout.Controls.Add(new Label { Text = "Заголовок записи:" }, 0, 2);
            rightLayout.Controls.Add(txtTitle, 0, 3);
            rightLayout.Controls.Add(new Label { Text = "Текст в дневнике:" }, 0, 4);
            rightLayout.Controls.Add(txtContent, 0, 5);
            split.Panel2.Controls.Add(rightLayout);

            lblStatus = new Label { Text = " Ожидание действий... Автосохранение активно (каждые 5 минут).", Dock = DockStyle.Fill, ForeColor = System.Drawing.Color.DarkGreen, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Italic) };
            mainLayout.Controls.Add(lblStatus, 0, 2);

            btnAdd.Click += (s, e) => {
                int nextId = diaryEntries.Count > 0 ? diaryEntries.Max(x => x.Id) + 1 : 1;
                var newEntry = new DiaryEntry(nextId, DateTime.Now, "Новая заметка", "Текст...");
                diaryEntries.Add(newEntry);
                diaryBindingSource.MoveLast();
            };

            btnDel.Click += (s, e) => {
                if (diaryBindingSource.Current is DiaryEntry entry) diaryEntries.Remove(entry);
            };

            btnJson.Click += (s, e) => {
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Импорт из JSON").Click += (s2, e2) => {
                    OpenFileDialog ofd = new OpenFileDialog { Filter = "JSON|*.json" };
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var list = LoadJson(ofd.FileName);
                        diaryEntries.Clear();
                        foreach (var x in list) diaryEntries.Add(x);
                        lblStatus.Text = " Данные успешно загружены из JSON!";
                    }
                };
                menu.Items.Add("Экспорт в JSON").Click += (s2, e2) => {
                    SaveFileDialog sfd = new SaveFileDialog { Filter = "JSON|*.json" };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        SaveJson(sfd.FileName);
                        lblStatus.Text = " Данные успешно сохранены в JSON!";
                    }
                };
                menu.Show(btnJson, new System.Drawing.Point(0, btnJson.Height));
            };

            btnXml.Click += (s, e) => {
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Импорт из XML").Click += (s2, e2) => {
                    OpenFileDialog ofd = new OpenFileDialog { Filter = "XML|*.xml" };
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var list = LoadXml(ofd.FileName);
                        diaryEntries.Clear();
                        foreach (var x in list) diaryEntries.Add(x);
                        lblStatus.Text = " Данные успешно загружены из XML!";
                    }
                };
                menu.Items.Add("Экспорт в XML").Click += (s2, e2) => {
                    SaveFileDialog sfd = new SaveFileDialog { Filter = "XML|*.xml" };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        SaveXml(sfd.FileName);
                        lblStatus.Text = " Данные успешно сохранены в XML!";
                    }
                };
                menu.Show(btnXml, new System.Drawing.Point(0, btnXml.Height));
            };

            btnCsv.Click += (s, e) => {
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Импорт из CSV").Click += (s2, e2) => {
                    OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV|*.csv" };
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var list = LoadCsv(ofd.FileName);
                        diaryEntries.Clear();
                        foreach (var x in list) diaryEntries.Add(x);
                        lblStatus.Text = " Данные успешно загружены из CSV!";
                    }
                };
                menu.Items.Add("Экспорт в CSV").Click += (s2, e2) => {
                    SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV|*.csv" };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        SaveCsv(sfd.FileName);
                        lblStatus.Text = " Данные успешно сохранены в CSV!";
                    }
                };
                menu.Show(btnCsv, new System.Drawing.Point(0, btnCsv.Height));
            };
        }
    }
}