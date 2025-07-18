using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace WpfApp20
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");
            if (!File.Exists(dbPath))
            {
                MessageBox.Show($"Файл базы данных не найден по пути: {dbPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            UpdateLoginHintVisibility();
            UpdatePasswordHintVisibility();
        }
        // // Проверка логина и пароля в базе данных
        private bool IsLoginValidFromDatabase(string login, string password)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            using (var context = new AppDbContext(optionsBuilder.Options))
            {
                return context.Users.Any(u => u.Логин == login && u.Пароль == password);
            }
        }
        // // Обработчик для кнопки "Регистрация"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var window = new RegistrationWindow();
            window.Show();
            this.Close();
        }
        // // Обработчик для кнопки "Войти как гость"
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var window = new Window5();
            window.Show();
            this.Close();
        }
        // // Обработчик для кнопки "Закрыть приложение"
        private void CloseApplication(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // // Обработчики событий для управления видимостью подсказок
        private void TxtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateLoginHintVisibility();
        }

        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateLoginHintVisibility();
        }

        private void TxtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLoginHintVisibility();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordHintVisibility();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePasswordHintVisibility();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordHintVisibility();
        }

        // // Обновление видимости подсказок для логина и пароля
        private void UpdateLoginHintVisibility()
        {
            textlogin.Visibility = string.IsNullOrEmpty(txtEmail.Text) && !txtEmail.IsFocused
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void UpdatePasswordHintVisibility()
        {
            textPassword.Visibility = string.IsNullOrEmpty(passwordBox.Password) && !passwordBox.IsFocused
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        // // Обработчик для кнопки "Войти"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = txtEmail.Text;
            string password = passwordBox.Password;
            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // // Проверка логина и пароля в базе данных
            if (IsLoginValidFromDatabase(login, password))
            {
                MessageBox.Show("Успешный вход!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (login.ToLower() == "otchetnik")
                {
                    MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    var reportsWindow = new Window2(login);
                    reportsWindow.Show();
                }
                else
                {
                    var mainWindow = new MainWindow(login);
                    mainWindow.Show();
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}