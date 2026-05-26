using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace BooksDirectory
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
    }

    public partial class Form1 : Form
    {
        private BindingList<Book> booksList;
        private BindingSource bindingSource;

        private DataGridView dgvBooks;
        private TextBox txtFilterAuthor;
        private TextBox txtFilterYear;
        private Button btnFilter;
        private Button btnResetFilter;
        private Button btnSortTitle;
        private Button btnSortPrice;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomUI();
            InitializeData();
        }

        private void InitializeData()
        {
            booksList = new BindingList<Book>
            {
                new Book { Id = 1, Title = "Преступление и наказание", Author = "Фёдор Достоевский", Year = 1866, Price = 450 },
                new Book { Id = 2, Title = "Мастер и Маргарита", Author = "Михаил Булгаков", Year = 1967, Price = 600 },
                new Book { Id = 3, Title = "Война и мир", Author = "Лев Толстой", Year = 1869, Price = 900 },
                new Book { Id = 4, Title = "Братья Карамазовы", Author = "Фёдор Достоевский", Year = 1880, Price = 550 },
                new Book { Id = 5, Title = "Собачье сердце", Author = "Михаил Булгаков", Year = 1925, Price = 380 }
            };

            bindingSource = new BindingSource();
            bindingSource.DataSource = booksList;
            dgvBooks.DataSource = bindingSource;

            dgvBooks.Columns["Id"].HeaderText = "ID";
            dgvBooks.Columns["Title"].HeaderText = "Название книги";
            dgvBooks.Columns["Author"].HeaderText = "Автор";
            dgvBooks.Columns["Year"].HeaderText = "Год издания";
            dgvBooks.Columns["Price"].HeaderText = "Цена (руб.)";
            dgvBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void InitializeCustomUI()
        {
            this.Text = "Справочник книг (Лекция 18.1 — Базовый уровень)";
            this.Size = new System.Drawing.Size(850, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            topPanel.Controls.Add(new Label { Text = "Автор:", AutoSize = true, Anchor = AnchorStyles.Left });
            txtFilterAuthor = new TextBox { Width = 120 };
            topPanel.Controls.Add(txtFilterAuthor);

            topPanel.Controls.Add(new Label { Text = "Год:", AutoSize = true, Anchor = AnchorStyles.Left });
            txtFilterYear = new TextBox { Width = 60 };
            topPanel.Controls.Add(txtFilterYear);

            btnFilter = new Button { Text = "Поиск", Width = 80 };
            btnFilter.Click += BtnFilter_Click;
            topPanel.Controls.Add(btnFilter);

            btnResetFilter = new Button { Text = "Сбросить", Width = 80 };
            btnResetFilter.Click += BtnResetFilter_Click;
            topPanel.Controls.Add(btnResetFilter);

            topPanel.Controls.Add(new Label { Text = "  |  Сортировка: ", AutoSize = true, Anchor = AnchorStyles.Left });

            btnSortTitle = new Button { Text = "По алфавиту", Width = 110 };
            btnSortTitle.Click += (s, e) => { bindingSource.DataSource = new BindingList<Book>(booksList.OrderBy(b => b.Title).ToList()); };
            topPanel.Controls.Add(btnSortTitle);

            btnSortPrice = new Button { Text = "По цене", Width = 90 };
            btnSortPrice.Click += (s, e) => { bindingSource.DataSource = new BindingList<Book>(booksList.OrderBy(b => b.Price).ToList()); };
            topPanel.Controls.Add(btnSortPrice);

            dgvBooks = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false };

            mainLayout.Controls.Add(topPanel, 0, 0);
            mainLayout.Controls.Add(dgvBooks, 0, 1);
            this.Controls.Add(mainLayout);
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            var filtered = booksList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtFilterAuthor.Text))
            {
                filtered = filtered.Where(b => b.Author.IndexOf(txtFilterAuthor.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(txtFilterYear.Text) && int.TryParse(txtFilterYear.Text, out int year))
            {
                filtered = filtered.Where(b => b.Year == year);
            }

            bindingSource.DataSource = new BindingList<Book>(filtered.ToList());
        }

        private void BtnResetFilter_Click(object sender, EventArgs e)
        {
            txtFilterAuthor.Clear();
            txtFilterYear.Clear();
            bindingSource.DataSource = booksList;
        }
    }
}