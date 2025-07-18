using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;
using System.IO;

namespace WpfApp20
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Лечение> Лечение { get; set; }
        public DbSet<Пациенты> Пациенты { get; set; }
        public DbSet<Персонал> Персонал { get; set; }
        public DbSet<Отчеты> Отчеты { get; set; }
        public DbSet<Записи> Записи { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
    }
}