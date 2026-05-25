using System;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEmployeeDb
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; } 
    }

    public class EmployeeHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string OldDataJson { get; set; } 
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EmployeeHistory> HistoryRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=company_database.db");
        }
    }
}