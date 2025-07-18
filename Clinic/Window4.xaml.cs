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
    public partial class Window4 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;
        public bool IsAdmin => currentUser == "admin";
        public Window4(string username)
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

                LoadPatients();
                CheckAccessRights();
            }
            catch (Exception ex)
            {
                Application.Current.Shutdown();
            }
        }

        // Загрузка списка пациентов из базы данных
        private void LoadPatients()
        {
            listbox1.ItemsSource = entities.Пациенты.ToList();
        }
        // Проверка прав доступа для текущего пользователя
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
                button4.IsEnabled = false;
                button2.Opacity = 0.5;
                button3.Opacity = 0.5;
                button4.Opacity = 0.5;
            }
            else
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button4.IsEnabled = false;
                button2.Opacity = 0;
                button3.Opacity = 0;
                button4.Opacity = 0;
            }
        }
        // Обработчик для кнопки очистки полей поиска
        private void ClearSearchFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            searchFIO.Text = "";
            searchDate.SelectedDate = null;
            searchGenderComboBox.SelectedIndex = 0;
            LoadPatients();
        }
        // Обработчик для кнопки поиска пациентов
        private void SearchPatients(object sender, EventArgs e)
        {
            string fioFilter = searchFIO.Text?.Trim();
            DateTime? birthDate = searchDate.SelectedDate;
            // Получение выбранного пола из ComboBox
            var query = entities.Пациенты.AsQueryable();
            if (!string.IsNullOrEmpty(fioFilter))
            {
                query = query.Where(p => p.ФИО.Contains(fioFilter));
            }
            // Проверка даты рождения
            if (birthDate.HasValue)
            {
                query = query.Where(p => p.Дата_рождения == birthDate.Value.Date);
            }

            listbox1.ItemsSource = query.ToList();
        }
        // Обработчик для кнопки поиска пациентов
        private void SearchPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            string fioFilter = searchFIO.Text?.Trim();
            DateTime? birthDate = searchDate.SelectedDate;
            string genderFilter = (searchGenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            // Получение выбранного пола из ComboBox
            var query = entities.Пациенты.AsQueryable();
            if (!string.IsNullOrEmpty(fioFilter))
            {
                query = query.Where(p => p.ФИО.Contains(fioFilter));
            }
            if (birthDate.HasValue)
            {
                query = query.Where(p => p.Дата_рождения == birthDate.Value.Date);
            }
            // Проверка выбранного пола
            if (genderFilter != "Все" && !string.IsNullOrEmpty(genderFilter))
            {
                query = query.Where(p => p.Пол == genderFilter);
            }

            listbox1.ItemsSource = query.ToList();
        }
        // Обработчик для кнопки добавления нового пациента
        private void listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPatient = listbox1.SelectedItem as Пациенты;
            if (selectedPatient != null)
            {
                textbox1.Text = selectedPatient.ФИО;
                textbox2.SelectedDate = selectedPatient.Дата_рождения;
                textbox3.SelectedItem = textbox3.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == selectedPatient.Пол);
                textbox4.Text = selectedPatient.Номер_телефона;
                textbox5.Text = selectedPatient.Е_мейл;
                textbox6.Text = selectedPatient.Паспорт;
            }
            else
            {
                ClearFields();
            }
        }
        // Обработчик для кнопки сохранения или обновления пациента

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения обязательных полей и валидация данных
            if (string.IsNullOrEmpty(textbox1.Text) || textbox2.SelectedDate == null ||
                textbox3.SelectedItem == null || string.IsNullOrEmpty(textbox4.Text) ||
                string.IsNullOrEmpty(textbox5.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка формата данных

            if (!Regex.IsMatch(textbox4.Text, @"^\+?\d{10,12}$"))
            {
                MessageBox.Show("Неверный формат телефона! Пример: +79991234567", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка формата email
            if (!Regex.IsMatch(textbox5.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Неверный формат email! Пример: example@domain.com", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка формата паспорта
            if (!string.IsNullOrEmpty(textbox6.Text) && !Regex.IsMatch(textbox6.Text, @"^\d{4}\s\d{6}$"))
            {
                MessageBox.Show("Неверный формат паспорта! Пример: 1234 567890", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка на существование пациента с таким же ФИО
            var patient = listbox1.SelectedItem as Пациенты;
            try
            {
                var existingPatient = entities.Пациенты.FirstOrDefault(p => p.ФИО == textbox1.Text && (patient == null || p.Id != patient.Id));
                if (existingPatient != null)
                {
                    MessageBox.Show("Пациент с таким ФИО уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Создание или обновление пациента
                if (patient == null)
                {
                    patient = new Пациенты();
                    entities.Пациенты.Add(patient);
                }

                patient.ФИО = textbox1.Text;
                patient.Дата_рождения = textbox2.SelectedDate.Value;
                patient.Пол = (textbox3.SelectedItem as ComboBoxItem)?.Content.ToString();
                patient.Номер_телефона = textbox4.Text;
                patient.Е_мейл = textbox5.Text;
                patient.Паспорт = textbox6.Text;

                entities.SaveChanges();
                MessageBox.Show(patient.Id == 0 ? "Пациент успешно добавлен!" : "Пациент успешно обновлен!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPatients();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик для кнопки удаления пациента
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            var patient = listbox1.SelectedItem as Пациенты;
            if (patient != null)
            {
                // Подтверждение удаления пациента
                var result = MessageBox.Show("Вы уверены, что хотите удалить пациента?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        entities.Пациенты.Remove(patient);
                        entities.SaveChanges();
                        MessageBox.Show("Пациент успешно удален!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadPatients();
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
                MessageBox.Show("Выберите пациента для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Обработчик для кнопки очистки полей и перезагрузки списка пациентов
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            LoadPatients();
        }

        private void ClearFields()
        {
            textbox1.Text = "";
            textbox2.SelectedDate = null;
            textbox3.SelectedIndex = -1;
            textbox4.Text = "";
            textbox5.Text = "";
            textbox6.Text = "";
            listbox1.SelectedIndex = -1;
            textbox1.Focus();
        }

        // Обработчик для кнопки перехода к другим разделам
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
        // Обработчик для кнопки управления пользователями
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
        // Обработчик для кнопки отчетов
        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new Window2(currentUser);
            reportsWindow.Show();
            Close();
        }
        // Обработчик для кнопки выхода из системы
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

        // Обработчик для кнопки закрытия приложения
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}