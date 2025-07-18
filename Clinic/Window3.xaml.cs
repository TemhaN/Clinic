using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class Window3 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;
        public bool IsAdmin => currentUser == "admin";
        
        public Window3(string username)
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            currentUser = username;
            DataContext = this;
            // Проверка существования базы данных
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");

            if (!File.Exists(dbPath))
            {
                Application.Current.Shutdown();
                return;
            }

            try
            {
                // Инициализация контекста базы данных
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

        private void LoadData()
        {
            listbox1.ItemsSource = entities.Персонал.ToList();
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
                button1.IsEnabled = false;
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button5.IsEnabled = false;
                button1.Opacity = 0.5;
                button2.Opacity = 0.5;
                button3.Opacity = 0.5;
                button5.Opacity = 0.5;
            }
            else
            {
                button1.IsEnabled = false;
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button5.IsEnabled = false;
                button1.Opacity = 0;
                button2.Opacity = 0;
                button3.Opacity = 0;
                button5.Opacity = 0;
            }
        }
        // Обработчик события загрузки окна
        private void listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedStaff = listbox1.SelectedItem as Персонал;
            if (selectedStaff != null)
            {
                textbox1.Text = selectedStaff.ФИО;
                textbox2.Text = selectedStaff.Должность;
                textbox3.Text = selectedStaff.Телефон;
            }
            else
            {
                ClearFields();
            }
        }
        // Обработчик события нажатия кнопки "Сохранить" или "Обновить"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textbox1.Text) || string.IsNullOrEmpty(textbox2.Text) || string.IsNullOrEmpty(textbox3.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка формата телефона
            if (!Regex.IsMatch(textbox3.Text, @"^\+?\d{10,12}$"))
            {
                MessageBox.Show("Неверный формат телефона! Пример: +79991234567", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка на существование сотрудника с таким ФИО

            var staff = listbox1.SelectedItem as Персонал;
            try
            {
                var existingStaff = entities.Персонал.FirstOrDefault(s => s.ФИО == textbox1.Text && (staff == null || s.Id != staff.Id));
                if (existingStaff != null)
                {
                    MessageBox.Show("Сотрудник с таким ФИО уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Создание или обновление сотрудника
                if (staff == null)
                {
                    staff = new Персонал();
                    entities.Персонал.Add(staff);
                }
                // Заполнение данных сотрудника

                staff.ФИО = textbox1.Text;
                staff.Должность = textbox2.Text;
                staff.Телефон = textbox3.Text;

                // Сохранение изменений в базе данных
                entities.SaveChanges();
                MessageBox.Show(staff.Id == 0 ? "Сотрудник успешно добавлен!" : "Сотрудник успешно обновлен!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик события нажатия кнопки "Удалить"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var staff = listbox1.SelectedItem as Персонал;
            if (staff != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить сотрудника?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Проверка наличия записей, связанных с сотрудником
                    try
                    {
                        entities.Персонал.Remove(staff);
                        entities.SaveChanges();
                        MessageBox.Show("Сотрудник успешно удален!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ClearFields();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Обработчик события нажатия кнопки "Очистить поля"
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        // Метод для очистки полей ввода и списка
        private void ClearFields()
        {
            textbox1.Text = "";
            textbox2.Text = "";
            textbox3.Text = "";
            searchTextBox.Text = "";
            listbox1.SelectedIndex = -1;
            LoadData();
            textbox1.Focus();
        }
        // Обработчик события нажатия кнопки "Поиск"
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение текста поиска и фильтрация списка персонала
                string searchText = searchTextBox.Text.Trim();
                var query = entities.Персонал.AsQueryable();

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(s => s.ФИО.Contains(searchText) || s.Должность.Contains(searchText) || s.Телефон.Contains(searchText));
                }

                listbox1.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик события нажатия кнопки "Показать записи"
        private void ShowAppointments_Click(object sender, RoutedEventArgs e)
        {
            var selectedStaff = listbox1.SelectedItem as Персонал;
            if (selectedStaff == null)
            {
                MessageBox.Show("Выберите сотрудника!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка наличия записей для выбранного сотрудника
            try
            {
                var appointmentsWindow = new AppointmentsWindow(selectedStaff.Id, currentUser);
                appointmentsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке записей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик события нажатия кнопки "Закрыть"
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Обработчик нажатия кнопок для навигации по окнам
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
        // Обработчик нажатия кнопки для навигации к управлению пользователями
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
        // Обработчик нажатия кнопки для навигации к отчетам
        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new Window2(currentUser);
            reportsWindow.Show();
            Close();
        }
        // Обработчик нажатия кнопки "Выход"
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