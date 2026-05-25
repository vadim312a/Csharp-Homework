using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace MasterDetailApp
{
    public partial class Form1 : Form
    {
        private BindingList<Author> authors;
        private BindingSource authorsBindingSource;
        private BindingSource booksBindingSource;

        private DataGridView dgvAuthors;
        private DataGridView dgvBooks;
        private Label lblStatus;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeCustomUI();
            InitData();
        }

        private void InitData()
        {
            authors = new BindingList<Author>();

            var author1 = new Author(1, "Лев Толстой");
            author1.Books.Add(new Book(1, "Война и мир", 1869, 1200));
            author1.Books.Add(new Book(2, "Анна Каренина", 1877, 850));

            var author2 = new Author(2, "Фёдор Достоевский");
            author2.Books.Add(new Book(3, "Преступление и наказание", 1866, 700));
            author2.Books.Add(new Book(4, "Идиот", 1869, 750));

            authors.Add(author1);
            authors.Add(author2);

            authorsBindingSource = new BindingSource { DataSource = authors };
            dgvAuthors.DataSource = authorsBindingSource;

            booksBindingSource = new BindingSource { DataSource = authorsBindingSource, DataMember = "Books" };
            dgvBooks.DataSource = booksBindingSource;
        }

        private void SaveJson(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var dataToSave = authors.Select(a => new {
                a.Id,
                a.Name,
                Books = a.Books.Select(b => new { b.Id, b.Title, b.Year, b.Price }).ToList()
            }).ToList();

            string json = JsonSerializer.Serialize(dataToSave, options);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        private void LoadJson(string path)
        {
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                authors.Clear();
                foreach (var authorEl in doc.RootElement.EnumerateArray())
                {
                    int authId = authorEl.GetProperty("Id").GetInt32();
                    string authName = authorEl.GetProperty("Name").GetString();
                    var author = new Author(authId, authName);

                    if (authorEl.TryGetProperty("Books", out JsonElement booksEl))
                    {
                        foreach (var bookEl in booksEl.EnumerateArray())
                        {
                            int bId = bookEl.GetProperty("Id").GetInt32();
                            string bTitle = bookEl.GetProperty("Title").GetString();
                            int bYear = bookEl.GetProperty("Year").GetInt32();
                            decimal bPrice = bookEl.GetProperty("Price").GetDecimal();
                            author.Books.Add(new Book(bId, bTitle, bYear, bPrice));
                        }
                    }
                    authors.Add(author);
                }
            }
        }

        public class AuthorDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Book> Books { get; set; } = new List<Book>();
        }

        private void SaveXml(string path)
        {
            var list = authors.Select(a => new AuthorDto
            {
                Id = a.Id,
                Name = a.Name,
                Books = a.Books.ToList()
            }).ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<AuthorDto>));
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, list);
            }
        }

        private void LoadXml(string path)
        {
            if (!File.Exists(path)) return;
            XmlSerializer serializer = new XmlSerializer(typeof(List<AuthorDto>));
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                var list = (List<AuthorDto>)serializer.Deserialize(sr);
                authors.Clear();
                foreach (var dto in list)
                {
                    var author = new Author(dto.Id, dto.Name);
                    foreach (var b in dto.Books) author.Books.Add(b);
                    authors.Add(author);
                }
            }
        }

        private void ExportToExcel(string path)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dgvBooks.Columns.Count; i++)
            {
                sb.Append(dgvBooks.Columns[i].HeaderText + (i == dgvBooks.Columns.Count - 1 ? "" : "\t"));
            }
            sb.AppendLine();

            foreach (DataGridViewRow row in dgvBooks.Rows)
            {
                if (row.IsNewRow) continue;
                for (int i = 0; i < dgvBooks.Columns.Count; i++)
                {
                    sb.Append(row.Cells[i].Value?.ToString() + (i == dgvBooks.Columns.Count - 1 ? "" : "\t"));
                }
                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private void InitializeCustomUI()
        {
            this.Text = "Справочник Авторы-Книги (Мастер-Деталь с экспортом)";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.Controls.Add(mainLayout);

            FlowLayoutPanel filePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            Button btnSaveJson = new Button { Text = "Сохранить JSON", Width = 130, Height = 35 };
            Button btnLoadJson = new Button { Text = "Загрузить JSON", Width = 130, Height = 35 };
            Button btnSaveXml = new Button { Text = "Сохранить XML", Width = 130, Height = 35 };
            Button btnLoadXml = new Button { Text = "Загрузить XML", Width = 130, Height = 35 };
            Button btnExcel = new Button { Text = "Экспорт Книг в Excel", Width = 160, Height = 35 };
            filePanel.Controls.AddRange(new Control[] { btnSaveJson, btnLoadJson, btnSaveXml, btnLoadXml, btnExcel });
            mainLayout.Controls.Add(filePanel, 0, 0);

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 450 };
            mainLayout.Controls.Add(split, 0, 1);

            TableLayoutPanel leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));

            dgvAuthors = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = true };
            FlowLayoutPanel authorButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(3) };
            Button btnAddAuthor = new Button { Text = "+ Автор", Width = 100, Height = 30 };
            Button btnDelAuthor = new Button { Text = "- Удалить", Width = 100, Height = 30 };
            authorButtons.Controls.AddRange(new Control[] { btnAddAuthor, btnDelAuthor });
            leftLayout.Controls.Add(dgvAuthors, 0, 0);
            leftLayout.Controls.Add(authorButtons, 0, 1);
            split.Panel1.Controls.Add(leftLayout);

            TableLayoutPanel rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));

            dgvBooks = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = true };
            FlowLayoutPanel bookButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(3) };
            Button btnAddBook = new Button { Text = "+ Книга", Width = 100, Height = 30 };
            Button btnDelBook = new Button { Text = "- Удалить", Width = 100, Height = 30 };
            bookButtons.Controls.AddRange(new Control[] { btnAddBook, btnDelBook });
            rightLayout.Controls.Add(dgvBooks, 0, 0);
            rightLayout.Controls.Add(bookButtons, 0, 1);
            split.Panel2.Controls.Add(rightLayout);

            lblStatus = new Label { Text = " Справочник запущен. Выбирайте автора слева — книги изменятся справа.", Dock = DockStyle.Fill, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Italic), ForeColor = System.Drawing.Color.DarkBlue };
            mainLayout.Controls.Add(lblStatus, 0, 2);

            btnAddAuthor.Click += (s, e) => {
                int nextId = authors.Count > 0 ? authors.Max(a => a.Id) + 1 : 1;
                authors.Add(new Author(nextId, "Новый Автор"));
                authorsBindingSource.MoveLast();
            };

            btnDelAuthor.Click += (s, e) => {
                if (authorsBindingSource.Current is Author currentAuthor) authors.Remove(currentAuthor);
            };

            btnAddBook.Click += (s, e) => {
                if (authorsBindingSource.Current is Author currentAuthor)
                {
                    int nextId = currentAuthor.Books.Count > 0 ? currentAuthor.Books.Max(b => b.Id) + 1 : 1;
                    booksBindingSource.Add(new Book(nextId, "Новая Книга", DateTime.Now.Year, 500));
                }
                else
                {
                    MessageBox.Show("Сначала выберите или добавьте автора!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnDelBook.Click += (s, e) => {
                if (booksBindingSource.Current is Book currentBook) booksBindingSource.Remove(currentBook);
            };

            btnSaveJson.Click += (s, e) => {
                var sfd = new SaveFileDialog { Filter = "JSON|*.json" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SaveJson(sfd.FileName);
                    lblStatus.Text = $" Успешно сохранено в JSON!";
                }
            };

            btnLoadJson.Click += (s, e) => {
                var ofd = new OpenFileDialog { Filter = "JSON|*.json" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadJson(ofd.FileName);
                    lblStatus.Text = $" Успешно загружено из JSON!";
                }
            };

            btnSaveXml.Click += (s, e) => {
                var sfd = new SaveFileDialog { Filter = "XML|*.xml" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SaveXml(sfd.FileName);
                    lblStatus.Text = $" Успешно сохранено в XML!";
                }
            };

            btnLoadXml.Click += (s, e) => {
                var ofd = new OpenFileDialog { Filter = "XML|*.xml" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadXml(ofd.FileName);
                    lblStatus.Text = $" Успешно загружено из XML!";
                }
            };

            btnExcel.Click += (s, e) => {
                var sfd = new SaveFileDialog { Filter = "Excel|*.xls" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcel(sfd.FileName);
                    MessageBox.Show("Экспорт завершен! Вы можете открыть файл через Excel.", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
        }
    }
}