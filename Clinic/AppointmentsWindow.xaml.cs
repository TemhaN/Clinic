using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class AppointmentsWindow : Window, IDisposable
    {
        private AppDbContext entities;
        private int staffId;
        private string currentUser;
        private bool disposed = false;

        public AppointmentsWindow(int staffId, string username)
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            this.staffId = staffId;
            currentUser = username;

            string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                entities = new AppDbContext(optionsBuilder.Options);

                LoadAppointments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке записей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadAppointments()
        {
            var staff = entities.Персонал.FirstOrDefault(s => s.Id == staffId);
            TitleTextBlock.Text = $"Записи врача {staff?.ФИО ?? "Неизвестно"}";

            var appointments = entities.Записи
                .Include(r => r.Пациенты)
                .Include(r => r.Лечение)
                .Where(r => r.id_perc == staffId)
                .ToList();
            appointmentsListView.ItemsSource = appointments
                .OrderBy(r => r.Дата)
                .ThenBy(r => r.Время);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                entities?.Dispose();
                disposed = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }
    }
}