using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class MainWindow : Window, IDisposable
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;
        private bool disposed = false;

        public bool IsAdmin => currentUser == "admin";

        public MainWindow(string username)
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            currentUser = username;
            DataContext = this;

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");

            if (!File.Exists(dbPath))
            {
                MessageBox.Show($"Файл базы данных не найден по пути: {dbPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        // // Загрузка данных в элементы управления
        private void LoadData()
        {
            listbox1.ItemsSource = entities.Записи
                .Include(r => r.Пациенты)
                .Include(r => r.Лечение)
                .Include(r => r.Персонал)
                .ToList();
            combobox1.ItemsSource = entities.Лечение.ToList();
            combobox2.ItemsSource = entities.Пациенты.ToList();
            combobox3.ItemsSource = entities.Персонал.ToList();
            searchPatientComboBox.ItemsSource = entities.Пациенты.ToList();
            searchDoctorComboBox.ItemsSource = entities.Персонал.ToList();
        }
        // // Проверка прав доступа пользователя
        private void CheckAccessRights()
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                var reportsWindow = new Window2(currentUser);
                reportsWindow.Show();
                Close();
                return;
            }

            if (currentUser == "admin" || currentUser == "rabotnik")
            {
                MessageBox.Show($"Добро пожаловать, {currentUser}. Вам доступен полный функционал", "Приветствие", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Доступ ограничен. Ожидайте одобрения администратора.", "Приветствие", MessageBoxButton.OK, MessageBoxImage.Information);
                AddButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                ClearButton.IsEnabled = false;
                AddButton.Opacity = 0;
                UpdateButton.Opacity = 0;
                DeleteButton.Opacity = 0;
                ClearButton.Opacity = 0;
            }
        }
        // // Обработчик изменения выбора в списке записей
        private void AppointmentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRecord = listbox1.SelectedItem as Записи;
            if (selectedRecord != null)
            {
                datePicker.SelectedDate = selectedRecord.Дата;
                if (TimeSpan.TryParse(selectedRecord.Время, out TimeSpan time))
                {
                    timePicker.Value = DateTime.Today + time;
                }
                combobox1.SelectedItem = entities.Лечение.FirstOrDefault(l => l.Id == selectedRecord.id_lec);
                combobox2.SelectedItem = entities.Пациенты.FirstOrDefault(p => p.Id == selectedRecord.id_pac);
                combobox3.SelectedItem = entities.Персонал.FirstOrDefault(p => p.Id == selectedRecord.id_perc);
                cabinetTextBox.Text = selectedRecord.Кабинет;
            }
            else
            {
                ClearFields();
            }
        }
        // // Обработчик для кнопки "Добавить запись"
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (datePicker.SelectedDate == null || combobox1.SelectedIndex == -1 ||
                    combobox2.SelectedIndex == -1 || combobox3.SelectedIndex == -1 ||
                    string.IsNullOrEmpty(cabinetTextBox.Text))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedTime = timePicker.Value;
                if (selectedTime == null)
                {
                    MessageBox.Show("Выберите время!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var record = new Записи
                {
                    Дата = datePicker.SelectedDate.Value,
                    Время = selectedTime.Value.ToString("HH:mm"),
                    id_lec = (combobox1.SelectedItem as Лечение).Id,
                    id_pac = (combobox2.SelectedItem as Пациенты).Id,
                    id_perc = (combobox3.SelectedItem as Персонал).Id,
                    Кабинет = cabinetTextBox.Text
                };

                entities.Записи.Add(record);
                entities.SaveChanges();
                LoadData();
                MessageBox.Show("Запись успешно добавлена!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // // Обработчик для кнопки "Изменить запись"
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var record = listbox1.SelectedItem as Записи;
            if (record == null)
            {
                MessageBox.Show("Выберите запись для изменения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (datePicker.SelectedDate == null || combobox1.SelectedIndex == -1 ||
                combobox2.SelectedIndex == -1 || combobox3.SelectedIndex == -1 ||
                string.IsNullOrEmpty(cabinetTextBox.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedTime = timePicker.Value;
            if (selectedTime == null)
            {
                MessageBox.Show("Выберите время!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // // Обновление выбранной записи
            try
            {
                record.Дата = datePicker.SelectedDate.Value;
                record.Время = selectedTime.Value.ToString("HH:mm");
                record.id_lec = (combobox1.SelectedItem as Лечение).Id;
                record.id_pac = (combobox2.SelectedItem as Пациенты).Id;
                record.id_perc = (combobox3.SelectedItem as Персонал).Id;
                record.Кабинет = cabinetTextBox.Text;

                entities.SaveChanges();
                LoadData();
                MessageBox.Show("Запись успешно изменена!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // // Обработчик для кнопки "Удалить запись"
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var record = listbox1.SelectedItem as Записи;
            if (record != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    entities.Записи.Remove(record);
                    entities.SaveChanges();
                    LoadData();
                    MessageBox.Show("Запись успешно удалена!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }
        // // Очистка полей ввода
        private void ClearFields()
        {
            datePicker.SelectedDate = null;
            timePicker.Value = null;
            combobox1.SelectedIndex = -1;
            combobox2.SelectedIndex = -1;
            combobox3.SelectedIndex = -1;
            cabinetTextBox.Text = "";
            listbox1.SelectedIndex = -1;
        }
        // // Обработчик для кнопки "Поиск записей"
        private void SearchAppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создание запроса для поиска записей
                var query = entities.Записи
                    .Include(r => r.Пациенты)
                    .Include(r => r.Лечение)
                    .Include(r => r.Персонал)
                    .AsQueryable();
                // Применение фильтров поиска
                if (searchDatePicker.SelectedDate.HasValue)
                    query = query.Where(r => r.Дата == searchDatePicker.SelectedDate.Value);

                if (searchPatientComboBox.SelectedItem != null)
                    query = query.Where(r => r.id_pac == (searchPatientComboBox.SelectedItem as Пациенты).Id);

                if (searchDoctorComboBox.SelectedItem != null)
                    query = query.Where(r => r.id_perc == (searchDoctorComboBox.SelectedItem as Персонал).Id);

                if (!string.IsNullOrEmpty(searchCabinetTextBox.Text))
                    query = query.Where(r => r.Кабинет.Contains(searchCabinetTextBox.Text));
                // // Преобразование результата запроса в список
                var results = query.ToList();
                // // Применение фильтра по времени, если указано
                if (searchTimePicker.Value.HasValue)
                {
                    var searchTime = searchTimePicker.Value.Value.TimeOfDay;
                    results = results.Where(r => TimeSpan.TryParse(r.Время, out TimeSpan time) && time == searchTime).ToList();
                }

                if (results.Any())
                {
                    listbox1.ItemsSource = results;
                }
                else
                {
                    listbox1.ItemsSource = null;
                    MessageBox.Show("Записи не найдены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // // Обработчик для изменения выбора врача в комбобоксе
        private void DoctorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDoctor = combobox3.SelectedItem as Персонал;
            if (selectedDoctor == null) return;
        }
        // Обработчик для навигации к различным разделам приложения

        private void NavigateToAppointments(object sender, RoutedEventArgs e)
        {
        }

        private void NavigateToTreatment(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                var reportsWindow = new Window2(currentUser);
                reportsWindow.Show();
                Close();
                return;
            }
            var treatmentWindow = new Window1(currentUser);
            treatmentWindow.Show();
            Close();
        }

        private void NavigateToStaff(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                var reportsWindow = new Window2(currentUser);
                reportsWindow.Show();
                Close();
                return;
            }
            var staffWindow = new Window3(currentUser);
            staffWindow.Show();
            Close();
        }

        private void NavigateToPatients(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                var reportsWindow = new Window2(currentUser);
                reportsWindow.Show();
                Close();
                return;
            }
            var patientsWindow = new Window4(currentUser);
            patientsWindow.Show();
            Close();
        }

        private void NavigateToSchedule(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                var reportsWindow = new Window2(currentUser);
                reportsWindow.Show();
                Close();
                return;
            }
            var scheduleWindow = new Window6(currentUser);
            scheduleWindow.Show();
            Close();
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new Window2(currentUser);
            reportsWindow.Show();
            Close();
        }

        private void NavigateToStatistics(object sender, RoutedEventArgs e)
        {
            var window = new Window7(currentUser);
            window.Show();
            Close();
        }
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
        // // Обработчик для выхода из приложения
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
        public void Dispose()
        {
            if (!disposed)
            {
                entities?.Dispose();
                disposed = true;
            }
        }

        private void ClearSearchFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            searchDatePicker.SelectedDate = null;
            searchTimePicker.Value = null;
            searchPatientComboBox.SelectedIndex = -1;
            searchDoctorComboBox.SelectedIndex = -1;
            searchCabinetTextBox.Text = "";
            LoadData();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }
    }
}