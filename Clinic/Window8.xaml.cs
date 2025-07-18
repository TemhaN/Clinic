using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class Window8 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;
        public bool IsAdmin => currentUser == "admin";
        // Конструктор класса
        public Window8(string username)
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
                LoadUsers();
                CheckAccessRights();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        // Загрузка списка пользователей в DataGrid
        private void LoadUsers()
        {
            userDataGrid.ItemsSource = entities.Users.OrderBy(u => u.Логин).ToList();
        }
        // Проверка прав доступа
        private void CheckAccessRights()
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            if (currentUser != "admin")
            {
                MessageBox.Show("Доступ ограничен. Только администратор может управлять пользователями.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                var window = new MainWindow(currentUser);
                window.Show();
                Close();
            }
        }

        // Обработчик для кнопки "Сохранить пользователя"
        private void SaveUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения полей
                if (string.IsNullOrEmpty(loginTextBox.Text) || string.IsNullOrEmpty(passwordTextBox.Text) || roleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Проверка наличия пользователя с таким логином
                Users user;
                if (userDataGrid.SelectedItem != null)
                {
                    user = userDataGrid.SelectedItem as Users;
                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    if (entities.Users.Any(u => u.Логин == loginTextBox.Text))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    user = new Users();
                    entities.Users.Add(user);
                }

                user.Логин = loginTextBox.Text;
                user.Пароль = passwordTextBox.Text;
                user.Роль = (roleComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                // Сохранение изменений в базе данных
                entities.SaveChanges();
                LoadUsers();
                ClearUserFields();
                MessageBox.Show("Пользователь успешно сохранен!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик для кнопки "Очистить поля"
        private void ClearUserButton_Click(object sender, RoutedEventArgs e)
        {
            ClearUserFields();
        }

        private void ClearUserFields()
        {
            loginTextBox.Text = "";
            passwordTextBox.Text = "";
            roleComboBox.SelectedIndex = -1;
            userDataGrid.SelectedItem = null;
        }
        // Обработчик для изменения выбора в DataGrid
        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userDataGrid.SelectedItem != null)
            {
                var user = userDataGrid.SelectedItem as Users;
                loginTextBox.Text = user.Логин;
                passwordTextBox.Text = user.Пароль;
                roleComboBox.SelectedItem = roleComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == user.Роль);
            }
        }
        // Обработчик для кнопки "Удалить пользователя"
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userDataGrid.SelectedItem != null)
            {
                var user = userDataGrid.SelectedItem as Users;
                if (user.Логин == "admin")
                {
                    MessageBox.Show("Нельзя удалить учетную запись администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = MessageBox.Show("Вы уверены, что хотите удалить пользователя?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    entities.Users.Remove(user);
                    entities.SaveChanges();
                    LoadUsers();
                    ClearUserFields();
                    MessageBox.Show("Пользователь успешно удален!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Обработчики навигации по кнопкам
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
            var window = new Window6(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var window = new Window2(currentUser);
            window.Show();
            Close();
        }

        private void NavigateToUserManagement(object sender, RoutedEventArgs e)
        {
        }
        private void NavigateToStatistics(object sender, RoutedEventArgs e)
        {
            var window = new Window7(currentUser);
            window.Show();
            Close();
        }
        // Обработчик для кнопки "Выход"
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
        // Обработчик для кнопки "Закрыть приложение"
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}