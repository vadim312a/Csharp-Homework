using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml; 

namespace EmployeeDirectoryBasic
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public string PhotoPath { get; set; } 
    }

    public partial class Form1 : Form
    {
        private BindingList<Employee> employees;
        private DataGridView dgvEmployees;
        private PictureBox pbPhoto;
        private Button btnUploadPhoto;
        private Button btnPrintCard;
        private Button btnExportExcel;
        private Button btnAdd;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomUI();
            InitializeData();
        }

        private void InitializeData()
        {
            employees = new BindingList<Employee>
            {
                new Employee { Id = 1, FullName = "Иванов Иван Иванович", Position = "Директор", Salary = 145000, PhotoPath = "" },
                new Employee { Id = 2, FullName = "Петров Петр Петрович", Position = "Главный разработчик", Salary = 115000, PhotoPath = "" },
                new Employee { Id = 3, FullName = "Сидорова Анна Сергеевна", Position = "HR-Менеджер", Salary = 75000, PhotoPath = "" }
            };

            dgvEmployees.DataSource = employees;
            dgvEmployees.Columns["Id"].HeaderText = "ID";
            dgvEmployees.Columns["FullName"].HeaderText = "ФИО Сотрудника";
            dgvEmployees.Columns["Position"].HeaderText = "Должность";
            dgvEmployees.Columns["Salary"].HeaderText = "Оклад";
            dgvEmployees.Columns["PhotoPath"].Visible = false;
            dgvEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void InitializeCustomUI()
        {
            this.Text = "Кадры (Лекция 18.3 — Базовый уровень)";
            this.Size = new System.Drawing.Size(950, 480);
            this.StartPosition = FormStartPosition.CenterScreen;

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 620 };
            this.Controls.Add(split);

            TableLayoutPanel leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            split.Panel1.Controls.Add(leftLayout);

            dgvEmployees = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false };
            dgvEmployees.SelectionChanged += DgvEmployees_SelectionChanged;
            leftLayout.Controls.Add(dgvEmployees, 0, 0);

            FlowLayoutPanel leftButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            btnAdd = new Button { Text = "+ Нанять сотрудника", Width = 150 };
            btnAdd.Click += (s, e) => employees.Add(new Employee { Id = employees.Count + 1, FullName = "Новый Стажер", Position = "Младший специалист", Salary = 25000 });

            btnExportExcel = new Button { Text = "Экспорт отчета в Excel", Width = 170, BackColor = Color.LightYellow };
            btnExportExcel.Click += BtnExportExcel_Click;

            leftButtons.Controls.AddRange(new Control[] { btnAdd, btnExportExcel });
            leftLayout.Controls.Add(leftButtons, 0, 1);

            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            split.Panel2.Controls.Add(rightPanel);

            rightPanel.Controls.Add(new Label { Text = "ИНФОРМАЦИОННАЯ КАРТОЧКА", Font = new Font("Arial", 10, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true });

            pbPhoto = new PictureBox { Location = new Point(15, 40), Size = new Size(160, 190), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            rightPanel.Controls.Add(pbPhoto);

            btnUploadPhoto = new Button { Text = "Загрузить фото", Location = new Point(15, 240), Width = 160 };
            btnUploadPhoto.Click += BtnUploadPhoto_Click;
            rightPanel.Controls.Add(btnUploadPhoto);

            btnPrintCard = new Button { Text = "🖨 Печать карточки", Location = new Point(15, 280), Width = 160, Height = 40, BackColor = Color.LightBlue };
            btnPrintCard.Click += BtnPrintCard_Click;
            rightPanel.Controls.Add(btnPrintCard);
        }

        private void DgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow?.DataBoundItem is Employee emp)
            {
                if (!string.IsNullOrEmpty(emp.PhotoPath) && File.Exists(emp.PhotoPath))
                    pbPhoto.Image = Image.FromFile(emp.PhotoPath);
                else
                    pbPhoto.Image = null;
            }
        }

        private void BtnUploadPhoto_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow?.DataBoundItem is Employee emp)
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Фотографии (*.jpg;*.png)|*.jpg;*.png" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    emp.PhotoPath = ofd.FileName;
                    if (File.Exists(emp.PhotoPath)) pbPhoto.Image = Image.FromFile(emp.PhotoPath);
                    MessageBox.Show("Фото успешно привязано!", "Ок");
                }
            }
        }

        private void BtnPrintCard_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow?.DataBoundItem is Employee emp)
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += (s, ev) =>
                {
                    Graphics g = ev.Graphics;
                    g.DrawString("ЛИЧНОЕ ДЕЛО СОТРУДНИКА", new Font("Arial", 16, FontStyle.Bold), Brushes.Black, 80, 40);
                    g.DrawString($"Табельный номер: {emp.Id}", new Font("Arial", 12), Brushes.Black, 80, 90);
                    g.DrawString($"ФИО: {emp.FullName}", new Font("Arial", 12), Brushes.Black, 80, 120);
                    g.DrawString($"Должность: {emp.Position}", new Font("Arial", 12), Brushes.Black, 80, 150);
                    g.DrawString($"Окладная часть: {emp.Salary} руб.", new Font("Arial", 12), Brushes.Black, 80, 180);

                    if (pbPhoto.Image != null)
                        g.DrawImage(pbPhoto.Image, new Rectangle(80, 220, 140, 170));
                };

                PrintPreviewDialog pvd = new PrintPreviewDialog { Document = pd };
                pvd.ShowDialog();
            }
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel книги (*.xlsx)|*.xlsx", FileName = "StaffReport.xlsx" };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (ExcelPackage pack = new ExcelPackage())
                    {
                        ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Штат");
                        ws.Cells[1, 1].Value = "ID";
                        ws.Cells[1, 2].Value = "ФИО";
                        ws.Cells[1, 3].Value = "Должность";
                        ws.Cells[1, 4].Value = "Оклад";

                        using (var r = ws.Cells[1, 1, 1, 4]) { r.Style.Font.Bold = true; r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid; r.Style.Fill.BackgroundColor.SetColor(Color.LightGray); }

                        int row = 2;
                        foreach (var emp in employees)
                        {
                            ws.Cells[row, 1].Value = emp.Id;
                            ws.Cells[row, 2].Value = emp.FullName;
                            ws.Cells[row, 3].Value = emp.Position;
                            ws.Cells[row, 4].Value = emp.Salary;
                            row++;
                        }
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        pack.SaveAs(new FileInfo(sfd.FileName));
                    }
                    MessageBox.Show("Данные успешно выгружены в Excel файл через EPPlus!", "Готово");
                }
                catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
            }
        }
    }
}