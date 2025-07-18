using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using WpfApp20.Models;

namespace WpfApp20
{
    public partial class Window2 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;

        public bool IsAdmin => currentUser == "admin";
        // Конструктор класса Window2
        public Window2(string username)
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

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                entities = new AppDbContext(optionsBuilder.Options);

                LoadComboBoxes();
                LoadData();
                PopulateComboBoxWithYears();
                CheckAccessRights();
                textboxsearch.TextChanged += (sender, e) => UpdateData();
                comboboxsearch.SelectionChanged += (sender, e) => UpdateData();
                dataGrid.LoadingRow += (s, e) => UpdateRecordCount();
                dataGrid.UnloadingRow += (s, e) => UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        // Загрузка данных в комбобоксы
        private void LoadComboBoxes()
        {
            patientComboBox.ItemsSource = entities.Пациенты.OrderBy(p => p.ФИО).ToList();
            doctorComboBox.ItemsSource = entities.Персонал.OrderBy(p => p.ФИО).ToList();
            treatmentComboBox.ItemsSource = entities.Лечение.OrderBy(l => l.Название).ToList();
        }
        // Загрузка данных в DataGrid
        private void LoadData()
        {
            UpdateData();
        }
        // Проверка прав доступа пользователя
        private void CheckAccessRights()
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            if (currentUser == "admin" || currentUser == "otchetnik")
            {
            }
            else
            {
                buttonPrint.IsEnabled = false;
                buttonPrint.Opacity = 0.5;
                textboxsearch.IsEnabled = false;
                comboboxsearch.IsEnabled = false;
                SaveReportButton.IsEnabled = false;
                ClearReportButton.IsEnabled = false;
                patientComboBox.IsEnabled = false;
                doctorComboBox.IsEnabled = false;
                treatmentComboBox.IsEnabled = false;
                startDatePicker.IsEnabled = false;
                endDatePicker.IsEnabled = false;
                cabinetTextBox.IsEnabled = false;
                descriptionTextBox.IsEnabled = false;
                buttonDelete.IsEnabled = false;
                textboxsearch.Opacity = 0.5;
                comboboxsearch.Opacity = 0.5;
                SaveReportButton.Opacity = 0.5;
                ClearReportButton.Opacity = 0.5;
                patientComboBox.Opacity = 0.5;
                doctorComboBox.Opacity = 0.5;
                treatmentComboBox.Opacity = 0.5;
                startDatePicker.Opacity = 0.5;
                endDatePicker.Opacity = 0.5;
                cabinetTextBox.Opacity = 0.5;
                descriptionTextBox.Opacity = 0.5;
                buttonDelete.Opacity = 0.5;
            }
        }
        // Модель представления для отчета
        private class ReportViewModel
        {
            public int Id { get; set; }
            public DateTime Дата_начала { get; set; }
            public DateTime Дата_окончания { get; set; }
            public string Пациент { get; set; }
            public string Сотрудник { get; set; }
            public string Лечение { get; set; }
            public string Кабинет { get; set; }
            public string Описание { get; set; }
        }
        // Обновление данных в DataGrid на основе фильтров
        private void UpdateData()
        {
            var searchText = textboxsearch.Text?.Trim().ToLower() ?? "";
            var selectedYear = comboboxsearch.SelectedItem?.ToString();
            // Фильтрация данных по тексту и году
            var query = entities.Отчеты
                .Include(o => o.Пациенты)
                .Include(o => o.Персонал)
                .Include(o => o.Лечение)
                .AsEnumerable()
                .Where(o =>
                    (o.Пациенты?.ФИО?.ToLower().Contains(searchText) == true ||
                     o.Персонал?.ФИО?.ToLower().Contains(searchText) == true ||
                     o.Лечение?.Название?.ToLower().Contains(searchText) == true));
            // Применение фильтра по году, если выбран
            if (selectedYear != null && selectedYear != "Все годы")
            {
                int year;
                if (int.TryParse(selectedYear, out year))
                {
                    query = query.Where(o => o.Дата_начала.Year == year);
                }
            }
            // Преобразование данных в модель представления для отображения в DataGrid  

            var result = query.Select(o => new ReportViewModel
            {
                Id = o.Id,
                Дата_начала = o.Дата_начала,
                Дата_окончания = o.Дата_окончания,
                Пациент = o.Пациенты != null ? o.Пациенты.ФИО : "Не указан",
                Сотрудник = o.Персонал != null ? o.Персонал.ФИО : "Не указан",
                Лечение = o.Лечение != null ? o.Лечение.Название : "Не указано",
                Кабинет = o.Кабинет ?? "",
                Описание = o.Описание ?? ""
            }).ToList();

            dataGrid.ItemsSource = result;
            UpdateRecordCount();
        }
        // Заполнение комбобокса годами для фильтрации
        private void PopulateComboBoxWithYears()
        {
            var years = entities.Отчеты
                .Select(o => o.Дата_начала.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            years.Insert(0, 0);
            comboboxsearch.ItemsSource = years.Select(y => y == 0 ? "Все годы" : y.ToString());
            comboboxsearch.SelectedIndex = 0;
        }
        // Обработчик события для кнопки сохранения отчета
        private void SaveReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения всех полей
                if (patientComboBox.SelectedItem == null || doctorComboBox.SelectedItem == null ||
                    treatmentComboBox.SelectedItem == null || startDatePicker.SelectedDate == null ||
                    endDatePicker.SelectedDate == null || string.IsNullOrEmpty(cabinetTextBox.Text) ||
                    string.IsNullOrEmpty(descriptionTextBox.Text))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Отчеты report;
                if (dataGrid.SelectedItem != null)
                {
                    // Если отчет уже выбран, обновляем его
                    var selectedReport = dataGrid.SelectedItem as ReportViewModel;
                    report = entities.Отчеты.Find(selectedReport.Id);
                    if (report == null)
                    {
                        MessageBox.Show("Отчет не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    report = new Отчеты();
                    entities.Отчеты.Add(report);
                }
                // Заполнение полей отчета
                report.id_pac = (patientComboBox.SelectedItem as Пациенты).Id;
                report.id_perc = (doctorComboBox.SelectedItem as Персонал).Id;
                report.id_lec = (treatmentComboBox.SelectedItem as Лечение).Id;
                report.Дата_начала = startDatePicker.SelectedDate.Value;
                report.Дата_окончания = endDatePicker.SelectedDate.Value;
                report.Кабинет = cabinetTextBox.Text;
                report.Описание = descriptionTextBox.Text;
                // Сохранение изменений в базе данных
                entities.SaveChanges();
                UpdateData();
                ClearReportFields();
                MessageBox.Show("Отчет успешно сохранен!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Обработчик события для кнопки очистки полей отчета
        private void ClearReportButton_Click(object sender, RoutedEventArgs e)
        {
            ClearReportFields();
        }

        private void ClearReportFields()
        {
            patientComboBox.SelectedIndex = -1;
            doctorComboBox.SelectedIndex = -1;
            treatmentComboBox.SelectedIndex = -1;
            startDatePicker.SelectedDate = null;
            endDatePicker.SelectedDate = null;
            cabinetTextBox.Text = "";
            descriptionTextBox.Text = "";
            dataGrid.SelectedItem = null;
        }
        // Обработчик события для изменения выделения в DataGrid
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                // Если выбран отчет, заполняем поля формы данными из отчета
                var selectedReport = dataGrid.SelectedItem as ReportViewModel;
                var report = entities.Отчеты
                    .Include(o => o.Пациенты)
                    .Include(o => o.Персонал)
                    .Include(o => o.Лечение)
                    .FirstOrDefault(o => o.Id == selectedReport.Id);
                if (report != null)
                {
                    patientComboBox.SelectedItem = entities.Пациенты.FirstOrDefault(p => p.Id == report.id_pac);
                    doctorComboBox.SelectedItem = entities.Персонал.FirstOrDefault(p => p.Id == report.id_perc);
                    treatmentComboBox.SelectedItem = entities.Лечение.FirstOrDefault(l => l.Id == report.id_lec);
                    startDatePicker.SelectedDate = report.Дата_начала;
                    endDatePicker.SelectedDate = report.Дата_окончания;
                    cabinetTextBox.Text = report.Кабинет ?? "";
                    descriptionTextBox.Text = report.Описание ?? "";
                }
            }
        }
        // Обработчик события для кнопки удаления отчета
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                // Если выбран отчет, удаляем его
                var selectedReport = dataGrid.SelectedItem as ReportViewModel;
                var report = entities.Отчеты.Find(selectedReport.Id);
                if (report != null)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить отчет?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Удаление отчета из базы данных
                        entities.Отчеты.Remove(report);
                        entities.SaveChanges();
                        UpdateData();
                        ClearReportFields();
                        MessageBox.Show("Отчет успешно удален!", "Оповещение", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите отчет для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Сохранение данных в Excel файл
        private void SaveDataToExcel()
        {
            // Проверка наличия данных в DataGrid перед экспортом
            if (dataGrid.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!");
                return;
            }

            try
            {
                // Установка лицензии для EPPlus
                ExcelPackage.License.SetNonCommercialPersonal(Environment.GetEnvironmentVariable("EPPLUS_LICENSE") ?? "non-commercial-placeholder");
                // Открытие диалогового окна для сохранения файла Excel
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Сохранить отчет как",
                    FileName = $"Report_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                // Проверка, выбрал ли пользователь файл для сохранения
                if (saveFileDialog.ShowDialog() != true)
                    return;

                // Создание нового Excel файла и добавление данных из DataGrid
                using (var excelPackage = new ExcelPackage())
                {
                    // Добавление нового листа в Excel файл
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Отчеты");
                    var headers = new[] { "Дата начала", "Дата окончания", "Пациент", "Сотрудник", "Лечение", "Кабинет", "Описание" };

                    // Заполнение заголовков в первой строке
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(58, 173, 161));
                        worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Заполнение данными из DataGrid, начиная со второй строки
                    var items = dataGrid.ItemsSource as IEnumerable<ReportViewModel>;
                    if (items != null)
                    {
                        int row = 2;
                        foreach (var item in items)
                        {
                            worksheet.Cells[row, 1].Value = item.Дата_начала.ToString("dd.MM.yyyy");
                            worksheet.Cells[row, 2].Value = item.Дата_окончания.ToString("dd.MM.yyyy");
                            worksheet.Cells[row, 3].Value = item.Пациент;
                            worksheet.Cells[row, 4].Value = item.Сотрудник;
                            worksheet.Cells[row, 5].Value = item.Лечение;
                            worksheet.Cells[row, 6].Value = item.Кабинет;
                            worksheet.Cells[row, 7].Value = item.Описание;
                            row++;
                        }
                    }

                    // Автоматическая подгонка ширины столбцов
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Сохранение Excel файла
                    var file = new FileInfo(saveFileDialog.FileName);
                    excelPackage.SaveAs(file);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.FullName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление количества записей в текстовом поле
        private void UpdateRecordCount()
        {
            int count = dataGrid.Items.Count;
            textboxRecordCount.Text = $"Количество записей: {count}";
        }

        // Обработчики событий для кнопок навигации и закрытия приложения
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // Обработчик события для кнопки печати отчета
        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToExcel();
        }

        // Обработчик события для кнопки очистки поиска
        private void ButtonClearSearch_Click(object sender, RoutedEventArgs e)
        {
            textboxsearch.Text = "";
            comboboxsearch.SelectedIndex = 0;
            UpdateData();
        }

        // Обработчик события для кнопки поиска
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }
        // Обработчики навигации по кнопкам
        private void NavigateToAppointments(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new MainWindow(currentUser);
            targetWindow.Show();
            Close();
        }
        private void NavigateToTreatment(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new Window1(currentUser);
            targetWindow.Show();
            Close();
        }

        private void NavigateToStaff(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new Window3(currentUser);
            targetWindow.Show();
            Close();
        }

        private void NavigateToPatients(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new Window4(currentUser);
            targetWindow.Show();
            Close();
        }

        private void NavigateToSchedule(object sender, RoutedEventArgs e)
        {
            if (currentUser == "otchetnik")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new Window6(currentUser);
            targetWindow.Show();
            Close();
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
        }
        private void NavigateToStatistics(object sender, RoutedEventArgs e)
        {
            if (currentUser != "admin")
            {
                MessageBox.Show("Доступ ограничен. У вас есть доступ только к отчетам.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new Window7(currentUser); 
            targetWindow.Show();
            Close();
        }
        // Обработчик события для навигации к управлению пользователями
        private void NavigateToUserManagement(object sender, RoutedEventArgs e)
        {
            if (currentUser != "admin")
            {
                MessageBox.Show("Доступ ограничен. Только администратор может управлять пользователями.", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var targetWindow = new Window8(currentUser); 
            targetWindow.Show();
            Close();
        }
        // Обработчик события для кнопки выхода из приложения
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