using System;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEmployeeDb
{
    public partial class Form1 : Form
    {
        private AppDbContext db;
        private User currentUser;

        private Panel loginPanel;
        private ComboBox cmbUsers;
        private Panel mainPanel;
        private DataGridView dgvEmployees;
        private DataGridView dgvHistory;
        private Label lblStatus;

        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRollback;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeCustomUI();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            db = new AppDbContext();

            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                db.Users.Add(new User { Login = "Директор (Admin)", Role = "Admin" });
                db.Users.Add(new User { Login = "Стажер (Viewer)", Role = "Viewer" });
                db.SaveChanges();
            }

            if (!db.Employees.Any())
            {
                db.Employees.Add(new Employee { FullName = "Иванов Иван", Position = "Разработчик", Salary = 100000 });
                db.SaveChanges();
            }

            cmbUsers.DataSource = db.Users.ToList();
            cmbUsers.DisplayMember = "Login";
        }

        private void PerformLogin()
        {
            currentUser = cmbUsers.SelectedItem as User;
            if (currentUser != null)
            {
                loginPanel.Visible = false;
                mainPanel.Visible = true;
                this.Text = $"Справочник HR — Пользователь: {currentUser.Login} ({currentUser.Role})";

                ApplyRolePermissions();
                LoadEmployees();
            }
        }

        private void ApplyRolePermissions()
        {
            bool isAdmin = currentUser.Role == "Admin";
            btnAdd.Enabled = isAdmin;
            btnEdit.Enabled = isAdmin;
            btnDelete.Enabled = isAdmin;
            btnRollback.Enabled = isAdmin;

            lblStatus.Text = isAdmin ? " У вас полные права доступа." : " Режим только для чтения.";
            dgvEmployees.ReadOnly = !isAdmin;
        }

        private void LoadEmployees()
        {
            db.Employees.Load();
            dgvEmployees.DataSource = db.Employees.Local.ToBindingList();
        }

        private void LoadHistory(int employeeId)
        {
            var history = db.HistoryRecords.Where(h => h.EmployeeId == employeeId).OrderByDescending(h => h.ChangedAt).ToList();
            dgvHistory.DataSource = history;
        }

        private void AddEmployee()
        {
            var newEmp = new Employee { FullName = "Новый Сотрудник", Position = "Стажер", Salary = 30000 };
            db.Employees.Add(newEmp);
            db.SaveChanges();
            dgvEmployees.Refresh();
        }

        private void EditEmployee(Employee emp)
        {
            var historyRecord = new EmployeeHistory
            {
                EmployeeId = emp.Id,
                OldDataJson = JsonSerializer.Serialize(emp),
                ChangedAt = DateTime.Now,
                ChangedBy = currentUser.Login
            };
            db.HistoryRecords.Add(historyRecord);

            emp.Salary += 5000;
            emp.Position = "Повышен в должности";

            db.SaveChanges();
            dgvEmployees.Refresh();
            LoadHistory(emp.Id);
            MessageBox.Show("Сотрудник изменен. Старая версия сохранена в историю!", "Успех");
        }

        private void DeleteEmployee(Employee emp)
        {
            if (MessageBox.Show($"Удалить {emp.FullName}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                db.Employees.Remove(emp);
                db.SaveChanges();
            }
        }

        private void RollbackEmployee(EmployeeHistory historyRecord, Employee currentEmp)
        {
            var restoredData = JsonSerializer.Deserialize<Employee>(historyRecord.OldDataJson);

            currentEmp.FullName = restoredData.FullName;
            currentEmp.Position = restoredData.Position;
            currentEmp.Salary = restoredData.Salary;

            db.HistoryRecords.Remove(historyRecord);
            db.SaveChanges();

            dgvEmployees.Refresh();
            LoadHistory(currentEmp.Id);
            MessageBox.Show("Данные успешно возвращены к предыдущему состоянию!", "Откат выполнен");
        }

        private void InitializeCustomUI()
        {
            this.Size = new System.Drawing.Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система HR (Авторизация и Откаты)";

            loginPanel = new Panel { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.WhiteSmoke };
            Label lblTitle = new Label { Text = "ВХОД В СИСТЕМУ", Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold), AutoSize = true, Location = new System.Drawing.Point(380, 200) };
            cmbUsers = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new System.Drawing.Point(380, 240), Width = 200, Font = new System.Drawing.Font("Arial", 12) };
            Button btnLogin = new Button { Text = "Войти", Location = new System.Drawing.Point(380, 280), Width = 200, Height = 40, BackColor = System.Drawing.Color.LightBlue, Font = new System.Drawing.Font("Arial", 12) };

            btnLogin.Click += (s, e) => PerformLogin();

            loginPanel.Controls.AddRange(new Control[] { lblTitle, cmbUsers, btnLogin });
            this.Controls.Add(loginPanel);

            mainPanel = new Panel { Dock = DockStyle.Fill, Visible = false };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainPanel.Controls.Add(layout);
            this.Controls.Add(mainPanel);

            FlowLayoutPanel topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            btnAdd = new Button { Text = "+ Добавить", Width = 100, Height = 35 };
            btnEdit = new Button { Text = "Редактировать", Width = 120, Height = 35 };
            btnDelete = new Button { Text = "Удалить", Width = 100, Height = 35 };
            btnRollback = new Button { Text = "Откатить изменение (Rollback)", Width = 220, Height = 35, BackColor = System.Drawing.Color.MistyRose };
            Button btnLogout = new Button { Text = "Выйти", Width = 100, Height = 35, Margin = new Padding(50, 5, 5, 5) };

            topPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRollback, btnLogout });
            layout.Controls.Add(topPanel, 0, 0);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 500 };
            layout.Controls.Add(split, 0, 1);

            TableLayoutPanel leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            dgvEmployees = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = true, ReadOnly = false };
            leftLayout.Controls.Add(new Label { Text = "Сотрудники:", Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) }, 0, 0);
            leftLayout.Controls.Add(dgvEmployees, 0, 1);
            split.Panel1.Controls.Add(leftLayout);

            TableLayoutPanel rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            dgvHistory = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = true, ReadOnly = true };
            rightLayout.Controls.Add(new Label { Text = "История изменений выбранного сотрудника:", Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) }, 0, 0);
            rightLayout.Controls.Add(dgvHistory, 0, 1);
            split.Panel2.Controls.Add(rightLayout);

            lblStatus = new Label { Dock = DockStyle.Fill, ForeColor = System.Drawing.Color.DarkBlue };
            layout.Controls.Add(lblStatus, 0, 2);

            dgvEmployees.SelectionChanged += (s, e) => {
                if (dgvEmployees.CurrentRow != null && dgvEmployees.CurrentRow.DataBoundItem is Employee emp)
                    LoadHistory(emp.Id);
                dgvEmployees.CellEndEdit += (s, e) => {
                    db.SaveChanges();
                    dgvEmployees.Refresh();
                };
            };

            btnAdd.Click += (s, e) => AddEmployee();

            btnEdit.Click += (s, e) => {
                if (dgvEmployees.CurrentRow?.DataBoundItem is Employee emp) EditEmployee(emp);
            };

            btnDelete.Click += (s, e) => {
                if (dgvEmployees.CurrentRow?.DataBoundItem is Employee emp) DeleteEmployee(emp);
            };

            btnRollback.Click += (s, e) => {
                if (dgvHistory.CurrentRow?.DataBoundItem is EmployeeHistory hist && dgvEmployees.CurrentRow?.DataBoundItem is Employee emp)
                {
                    if (MessageBox.Show("Откатить сотрудника к этой старой версии?", "Откат", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        RollbackEmployee(hist, emp);
                }
                else MessageBox.Show("Выберите запись в истории справа для отката.");
            };

            btnLogout.Click += (s, e) => {
                mainPanel.Visible = false;
                loginPanel.Visible = true;
                this.Text = "Система HR (Авторизация и Откаты)";
            };
        }
    }
}