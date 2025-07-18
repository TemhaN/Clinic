using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class Window1 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;
        public bool IsAdmin => currentUser == "admin";
        public Window1(string username)
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            currentUser = username;
            DataContext = this;

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");

            if (!File.Exists(dbPath))
            {
                Application.Current.Shutdown();
                return;
            }

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                entities = new AppDbContext(optionsBuilder.Options);

                LoadData();
                CheckAccessRights();
            }
            catch (Exception ex)
            {
                Application.Current.Shutdown();
            }
        }
        // Загрузка данных в ListBox
        private void LoadData()
        {
            listbox1.ItemsSource = entities.Лечение.ToList();
        }
        // Проверка прав доступа
        private void CheckAccessRights()
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            if (currentUser == "admin" || currentUser == "rabotnik")
            {
            }
            else if (currentUser == "otchetnik")
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button5.IsEnabled = false;
                button2.Opacity = 0.5;
                button3.Opacity = 0.5;
                button5.Opacity = 0.5;
            }
            else
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button5.IsEnabled = false;
                button2.Opacity = 0;
                button3.Opacity = 0;
                button5.Opacity = 0;
            }
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTreatment = listbox1.SelectedItem as Лечение;
            if (selectedTreatment != null)
            {
                textbox1.Text = selectedTreatment.Название;
                textbox2.Text = selectedTreatment.Описание;
                textbox3.Text = selectedTreatment.Продолжительность.ToString();
                textbox4.Text = selectedTreatment.Стоимость.ToString();
            }
            else
            {
                ClearFields(null, null);
            }
        }
        // Обработчик нажатия кнопки "Сохранить" или "Обновить"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textbox1.Text) || string.IsNullOrEmpty(textbox2.Text) ||
                string.IsNullOrEmpty(textbox3.Text) || string.IsNullOrEmpty(textbox4.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(textbox3.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Продолжительность должна быть положительным числом (в минутах)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(textbox4.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Стоимость должна быть неотрицательным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка на существование услуги с таким названием
            var treatment = listbox1.SelectedItem as Лечение;
            try
            {
                var existingTreatment = entities.Лечение.FirstOrDefault(t => t.Название == textbox1.Text && (treatment == null || t.Id != treatment.Id));
                if (existingTreatment != null)
                {
                    MessageBox.Show("Услуга с таким названием уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Создание или обновление услуги
                if (treatment == null)
                {
                    treatment = new Лечение();
                    entities.Лечение.Add(treatment);
                }

                treatment.Название = textbox1.Text;
                treatment.Описание = textbox2.Text;
                treatment.Продолжительность = duration;
                treatment.Стоимость = cost;

                entities.SaveChanges();
                MessageBox.Show(treatment.Id == 0 ? "Услуга успешно добавлена!" : "Услуга успешно обновлена!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearFields(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик нажатия кнопки "Удалить"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var treatment = listbox1.SelectedItem as Лечение;
            if (treatment != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить услугу?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        entities.Лечение.Remove(treatment);
                        entities.SaveChanges();
                        MessageBox.Show("Услуга успешно удалена!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ClearFields(null, null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Обработчик нажатия кнопки "Поиск"
        private void SearchTreatmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = searchNameTextBox.Text.Trim();
                var query = entities.Лечение.AsQueryable();

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(t => t.Название.Contains(name));

                listbox1.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик нажатия кнопки "Очистить поля"
        private void ClearFields(object sender, RoutedEventArgs e)
        {
            textbox1.Text = "";
            textbox2.Text = "";
            textbox3.Text = "";
            textbox4.Text = "";
            searchNameTextBox.Text = "";
            listbox1.SelectedIndex = -1;
            LoadData();
            textbox1.Focus();
        }
        // Обработчики для кнопок навигации
        private void NavigateToAppointments(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToTreatment(object sender, RoutedEventArgs e)
        {
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
            var window = new Window6(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToStatistics(object sender, RoutedEventArgs e)
        {
            var window = new Window7(currentUser);
            window.Show();
            Close();
        }
        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new Window2(currentUser);
            reportsWindow.Show();
            Close();
        }
        // Обработчик для навигации к управлению пользователями
        private void NavigateToUserManagement(object sender, RoutedEventArgs e)
        {
            // Проверка прав доступа для управления пользователями
            if (currentUser != "admin")
            {
                MessageBox.Show("Доступ ограничен. Только администратор может управлять пользователями.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var userManagementWindow = new Window8(currentUser);
            userManagementWindow.Show();
            Close();
        }
        // Обработчик для выхода из приложения
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
        // Обработчик для закрытия приложения
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}