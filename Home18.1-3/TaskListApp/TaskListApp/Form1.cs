using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace TaskListApp
{
    public class UserTask
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }
    }

    public partial class Form1 : Form
    {
        private BindingList<UserTask> tasks;
        private DataGridView dgvTasks;
        private TextBox txtDescription;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnToggleDone;
        private Button btnExportCsv;

        private readonly string jsonFilePath = "tasks.json";

        public Form1()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadTasksFromJson(); 
        }

        private void InitializeCustomUI()
        {
            this.Text = "Менеджер задач (Лекция 18.2 — Базовый уровень)";
            this.Size = new System.Drawing.Size(650, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += Form1_FormClosing; 

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            FlowLayoutPanel topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            topPanel.Controls.Add(new Label { Text = "Текст задачи:", AutoSize = true, Anchor = AnchorStyles.Left });
            txtDescription = new TextBox { Width = 320 };
            topPanel.Controls.Add(txtDescription);
            btnAdd = new Button { Text = "Добавить", Width = 90 };
            btnAdd.Click += BtnAdd_Click;
            topPanel.Controls.Add(btnAdd);

            dgvTasks = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AllowUserToAddRows = false };

            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            btnToggleDone = new Button { Text = "Сменить статус", Width = 140 };
            btnToggleDone.Click += BtnToggleDone_Click;
            btnDelete = new Button { Text = "Удалить", Width = 100 };
            btnDelete.Click += BtnDelete_Click;
            btnExportCsv = new Button { Text = "Экспорт в CSV", Width = 120, BackColor = System.Drawing.Color.LightGreen };
            btnExportCsv.Click += BtnExportCsv_Click;

            bottomPanel.Controls.AddRange(new Control[] { btnToggleDone, btnDelete, btnExportCsv });

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(dgvTasks, 0, 1);
            layout.Controls.Add(bottomPanel, 0, 2);
            this.Controls.Add(layout);
        }

        private void LoadTasksFromJson()
        {
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(jsonFilePath, Encoding.UTF8);
                    var list = JsonSerializer.Deserialize<System.Collections.Generic.List<UserTask>>(jsonString);
                    tasks = new BindingList<UserTask>(list ?? new System.Collections.Generic.List<UserTask>());
                }
                catch
                {
                    tasks = new BindingList<UserTask>();
                }
            }
            else
            {
                tasks = new BindingList<UserTask>();
            }

            dgvTasks.DataSource = tasks;
            dgvTasks.Columns["Id"].HeaderText = "№";
            dgvTasks.Columns["Description"].HeaderText = "Описание задачи";
            dgvTasks.Columns["IsDone"].HeaderText = "Выполнена";
            dgvTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void SaveTasksToJson()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonFilePath, jsonString, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка автосохранения: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text)) return;
            int nextId = tasks.Count > 0 ? tasks[tasks.Count - 1].Id + 1 : 1;
            tasks.Add(new UserTask { Id = nextId, Description = txtDescription.Text, IsDone = false });
            txtDescription.Clear();
            SaveTasksToJson();
        }

        private void BtnToggleDone_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is UserTask task)
            {
                task.IsDone = !task.IsDone;
                dgvTasks.Refresh();
                SaveTasksToJson();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTasks.CurrentRow?.DataBoundItem is UserTask task)
            {
                tasks.Remove(task);
                SaveTasksToJson();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) => SaveTasksToJson();

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV файлы (*.csv)|*.csv", FileName = "tasks.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder csv = new StringBuilder();
                    csv.AppendLine("ID;Описание;Статус");

                    foreach (var t in tasks)
                    {
                        string safeDescription = t.Description.Replace(";", " ").Replace("\"", "'");
                        string status = t.IsDone ? "Выполнена" : "В процессе";

                        csv.AppendLine($"{t.Id};\"{safeDescription}\";{status}");
                    }

                    File.WriteAllText(sfd.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show("Успешно экспортировано в CSV!", "Ок");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }
    }
}