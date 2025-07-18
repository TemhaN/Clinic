using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            UpdateLoginHintVisibility();
            UpdatePasswordHintVisibility();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
        }
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
        // Обработчик для кнопки "Закрыть"
        private void CloseApplication(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Обработчик для кнопки "Зарегистрироваться"
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string newLogin = txtEmail.Text;
            string newPassword = passwordBox.Password;
            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(newLogin) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                // Создание контекста базы данных и проверка на существование пользователя
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                // Проверка существования базы данных
                using (var context = new AppDbContext(optionsBuilder.Options))
                {
                    if (context.Users.Any(u => u.Логин == newLogin))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    // Создание нового пользователя
                    var newUser = new Users
                    {
                        Логин = newLogin,
                        Пароль = newPassword,
                        Роль = "user"
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();
                }

                MessageBox.Show("Регистрация прошла успешно!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}