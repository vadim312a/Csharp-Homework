using System;
using System.ComponentModel;

namespace MasterDetailApp
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public BindingList<Book> Books { get; set; } = new BindingList<Book>();

        public Author() { }

        public Author(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}