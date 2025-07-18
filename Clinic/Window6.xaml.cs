using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class Window6 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;
        public bool IsAdmin => currentUser == "admin";

        /// Конструктор класса
        public Window6(string username)
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            currentUser = username;
            DataContext = this;

            // Проверка наличия базы данных
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");

            if (!File.Exists(dbPath))
            {
                MessageBox.Show("База данных не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            // Инициализация контекста базы данных
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                entities = new AppDbContext(optionsBuilder.Options);

                LoadStaff();
                CheckAccessRights();
            }
            // Обработка ошибок при инициализации
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        // Загрузка списка персонала в комбобокс
        private void LoadStaff()
        {
            var staffList = entities.Персонал.OrderBy(s => s.ФИО).ToList();
            staffComboBox.ItemsSource = staffList;
            staffComboBox.SelectedIndex = 0;
        }
        // Проверка прав доступа пользователя
        private void CheckAccessRights()
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            if (currentUser != "admin" && currentUser != "rabotnik" && currentUser != "otchetnik")
            {
                staffComboBox.IsEnabled = false;
                staffComboBox.Opacity = 0.5;
            }
        }
        // Обновление списка записей при выборе персонала
        private void UpdateAppointments()
        {
            var selectedStaff = staffComboBox.SelectedItem as Персонал;
            // Проверка выбранного персонала
            if (selectedStaff != null)
            {
                appointmentsList.ItemsSource = entities.Записи
                    .Include(a => a.Пациенты)
                    .Include(a => a.Лечение)
                    .Where(a => a.id_perc == selectedStaff.Id)
                    .OrderBy(a => a.Дата)
                    .ThenBy(a => a.Время)
                    .ToList();
            }
            else
            {
                appointmentsList.ItemsSource = null;
            }
        }
        // Обработчик события изменения выбора в комбобоксе персонала
        private void staffComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAppointments();
        }

        // Обработчик для навигации на другие окна
        private void NavigateToAppointments(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToTreatment(object sender, RoutedEventArgs e)
        {
            var window = new Window1(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToStaff(object sender, RoutedEventArgs e)
        {
            var window = new Window3(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToPatients(object sender, RoutedEventArgs e)
        {
            var window = new Window4(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToSchedule(object sender, RoutedEventArgs e)
        {
        }

        private void NavigateToStatistics(object sender, RoutedEventArgs e)
        {
            var window = new Window7(currentUser);
            window.Show();
            Close();
        }
        // Обработчик для навигации на окно управления пользователями с проверкой прав доступа
        private void NavigateToUserManagement(object sender, RoutedEventArgs e)
        {
            if (currentUser != "admin")
            {
                MessageBox.Show("Доступ ограничен. Только администратор может управлять пользователями.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var userManagementWindow = new Window8(currentUser);
            userManagementWindow.Show();
            Close();
        }
        // Обработчик для навигации на окно отчетов
        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new Window2(currentUser);
            reportsWindow.Show();
            Close();
        }
        // Обработчик для закрытия приложения
        private void Logout(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                isFirstLoad = true;
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
    }
}