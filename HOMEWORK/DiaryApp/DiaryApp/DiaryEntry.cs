using System;

namespace DiaryApp
{
    public class DiaryEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public DiaryEntry() { }

        public DiaryEntry(int id, DateTime date, string title, string content)
        {
            Id = id;
            Date = date;
            Title = title;
            Content = content;
        }
    }
}