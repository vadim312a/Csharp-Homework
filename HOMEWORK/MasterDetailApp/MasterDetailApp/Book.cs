using System;

namespace MasterDetailApp
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }

        public Book() { }

        public Book(int id, string title, int year, decimal price)
        {
            Id = id;
            Title = title;
            Year = year;
            Price = price;
        }
    }
}