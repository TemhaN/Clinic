using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using WpfApp20.Models;
using System.Collections.Generic;

namespace WpfApp20
{
    public partial class Window7 : Window
    {
        private string currentUser;
        private AppDbContext entities;
        private static bool isFirstLoad = true;

        public bool IsAdmin => currentUser == "admin";

        /// Конструктор класса
        public Window7(string username)
        {
            InitializeComponent();
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            currentUser = username;
            DataContext = this;
            // Проверка наличия базы данных
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");
            // Если база данных не найдена, закрываем приложение
            if (!File.Exists(dbPath))
            {
                Application.Current.Shutdown();
                return;
            }
            // Инициализация контекста базы данных
            try
            {
                // Создание контекста базы данных с использованием SQLite
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                entities = new AppDbContext(optionsBuilder.Options);

                InitializeComboBoxes();
                CheckAccessRights();
            }
            catch (Exception ex)
            {
                Application.Current.Shutdown();
            }
        }
        // Загрузка данных в комбобоксы и инициализация статистики
        private void InitializeComboBoxes()
        {
            // Загрузка списка персонала в комбобокс
            monthComboBox.ItemsSource = new string[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

            // Загрузка уникальных годов из записей в базе данных
            var years = entities.Записи.Select(a => a.Дата.Year).Distinct().OrderBy(y => y).ToList();
            if (!years.Any())
            {
                years = new int[] { DateTime.Today.Year }.ToList();
            }
            // Установка источников данных для комбобоксов годов

            yearComboBox.ItemsSource = years;
            yearAttendanceComboBox.ItemsSource = years;
            yearDoctorComboBox.ItemsSource = years;

            monthComboBox.SelectedIndex = DateTime.Today.Month - 1;
            if (years.Contains(DateTime.Today.Year))
            {
                yearComboBox.SelectedItem = DateTime.Today.Year;
                yearAttendanceComboBox.SelectedItem = DateTime.Today.Year;
                yearDoctorComboBox.SelectedItem = DateTime.Today.Year;
            }
            else if (years.Any())
            {
                yearComboBox.SelectedItem = years.First();
                yearAttendanceComboBox.SelectedItem = years.First();
                yearDoctorComboBox.SelectedItem = years.First();
            }

            monthComboBox.SelectionChanged += (s, e) => UpdateStatistics();
            yearComboBox.SelectionChanged += (s, e) => UpdateStatistics();
            yearAttendanceComboBox.SelectionChanged += (s, e) => UpdateAttendance();
            yearDoctorComboBox.SelectionChanged += (s, e) => UpdateDoctorStatistics();

            UpdateStatistics();
            UpdateAttendance();
            UpdateDoctorStatistics();
        }

        // Проверка прав доступа пользователя
        private void CheckAccessRights()
        {
            if (!isFirstLoad) return;
            isFirstLoad = false;

            // Проверка, является ли текущий пользователь администратором, работником или отчетником
            if (currentUser != "admin" && currentUser != "rabotnik" && currentUser != "otchetnik")
            {
                MessageBox.Show("Ты не админ, работник или отчетник, доступ закрыт!", "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                monthComboBox.IsEnabled = false;
                yearComboBox.IsEnabled = false;
                yearAttendanceComboBox.IsEnabled = false;
                yearDoctorComboBox.IsEnabled = false;
                monthComboBox.Opacity = 0.5;
                yearComboBox.Opacity = 0.5;
                yearAttendanceComboBox.Opacity = 0.5;
                yearDoctorComboBox.Opacity = 0.5;
                AppointmentsButton.IsEnabled = false;
                TreatmentButton.IsEnabled = false;
                StaffButton.IsEnabled = false;
                PatientsButton.IsEnabled = false;
                ScheduleButton.IsEnabled = false;
                StatisticsButton.IsEnabled = false;
                AppointmentsButton.Opacity = 0.5;
                TreatmentButton.Opacity = 0.5;
                StaffButton.Opacity = 0.5;
                PatientsButton.Opacity = 0.5;
                ScheduleButton.Opacity = 0.5;
                StatisticsButton.Opacity = 0.5;
            }
        }
        // Обновление статистики заболеваний
        private void UpdateStatistics()
        {
            // Проверка выбранного месяца и года
            if (monthComboBox.SelectedIndex == -1 || yearComboBox.SelectedItem == null)
            {
                var emptyData = new ObservableCollection<DiseaseStat> { new DiseaseStat { Disease = "Нет данных", Count = 0, Percentage = 0 } };
                statsGrid.ItemsSource = emptyData;
                statsGrid.Items.Refresh();
                DrawDiseaseChart(null);
                return;
            }

            // Получение выбранного месяца и года из комбобоксов
            int month = monthComboBox.SelectedIndex + 1;
            int year = (int)yearComboBox.SelectedItem;

            // Получение записей за выбранный месяц и год из базы данных
            var records = entities.Записи
                .Include(a => a.Лечение)
                .Where(a => a.Дата.Year == year && a.Дата.Month == month)
                .ToList();

            // Подсчет общего количества записей за выбранный период
            var total = records.Count;
            var diseaseGroups = new ObservableCollection<DiseaseStat>(
                records
                    .GroupBy(a => a.Лечение?.Название ?? "Не указано")
                    .Select(g => new DiseaseStat
                    {
                        Disease = g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0 : (double)g.Count() / total * 100
                    })
                    .OrderByDescending(g => g.Count)
            );
            // Если нет записей за выбранный период, добавляем сообщение
            if (!diseaseGroups.Any())
            {
                diseaseGroups = new ObservableCollection<DiseaseStat> { new DiseaseStat { Disease = "Нет данных за выбранный период", Count = 0, Percentage = 0 } };
            }
            // Обновление источника данных для DataGrid и перерисовка диаграммы
            statsGrid.ItemsSource = diseaseGroups;
            statsGrid.Items.Refresh();
            DrawDiseaseChart(diseaseGroups.ToList());
        }

        // Обновление статистики посещаемости
        private void UpdateAttendance()
        {
            // Проверка выбранного года для посещаемости
            if (yearAttendanceComboBox.SelectedItem == null)
            {
                var emptyData = new ObservableCollection<AttendanceStat> { new AttendanceStat { Month = "Нет данных", Count = 0 } };
                attendanceGrid.ItemsSource = emptyData;
                attendanceGrid.Items.Refresh();
                DrawAttendanceChart(null);
                return;
            }
            // Получение выбранного года из комбобокса посещаемости
            int year = (int)yearAttendanceComboBox.SelectedItem;

            var records = entities.Записи
                .Where(a => a.Дата.Year == year)
                .ToList();
            // Подсчет посещаемости по месяцам
            var attendance = new ObservableCollection<AttendanceStat>(
                Enumerable.Range(1, 12)
                    .Select(m => new AttendanceStat
                    {
                        Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m),
                        Count = records.Count(a => a.Дата.Month == m)
                    })
                    .OrderBy(g => DateTime.ParseExact(g.Month, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month)
            );

            // Если нет записей за выбранный год, добавляем сообщение
            if (!records.Any())
            {
                attendance = new ObservableCollection<AttendanceStat> { new AttendanceStat { Month = "Нет данных за выбранный год", Count = 0 } };
            }

            // Обновление источника данных для DataGrid и перерисовка диаграммы посещаемости
            attendanceGrid.ItemsSource = attendance;
            attendanceGrid.Items.Refresh();
            DrawAttendanceChart(attendance.ToList());
        }

        // Обновление статистики врачей
        private void UpdateDoctorStatistics()
        {
            if (yearDoctorComboBox.SelectedItem == null)
            {
                var emptyData = new ObservableCollection<DoctorStat> { new DoctorStat { DoctorName = "Нет данных", AppointmentCount = 0, PatientCount = 0, AverageAppointmentsPerPatient = 0 } };
                doctorStatsGrid.ItemsSource = emptyData;
                doctorStatsGrid.Items.Refresh();
                DrawDoctorChart(null);
                return;
            }

            // Получение выбранного года из комбобокса врачей
            int year = (int)yearDoctorComboBox.SelectedItem;

            var records = entities.Записи
                .Include(a => a.Персонал)
                .Where(a => a.Дата.Year == year)
                .ToList();

            // Подсчет статистики по врачам
            var doctorStats = new ObservableCollection<DoctorStat>(
                // Группировка записей по врачам и подсчет количества приемов, пациентов и среднего количества приемов на пациента
                records
                    .GroupBy(a => a.Персонал != null ? $"{a.Персонал.ФИО}" : "Не указан")
                    .Select(g => new DoctorStat
                    {
                        DoctorName = g.Key,
                        AppointmentCount = g.Count(),
                        PatientCount = g.Select(a => a.id_pac).Distinct().Count(),
                        AverageAppointmentsPerPatient = g.Select(a => a.id_pac).Distinct().Count() == 0 ? 0 : (double)g.Count() / g.Select(a => a.id_pac).Distinct().Count()
                    })
                    .OrderByDescending(g => g.AppointmentCount)
            );

            // Если нет записей за выбранный год, добавляем сообщение
            if (!doctorStats.Any())
            {
                doctorStats = new ObservableCollection<DoctorStat> { new DoctorStat { DoctorName = "Нет данных за выбранный год", AppointmentCount = 0, PatientCount = 0, AverageAppointmentsPerPatient = 0 } };
            }
            // Обновление источника данных для DataGrid и перерисовка диаграммы врачей
            doctorStatsGrid.ItemsSource = doctorStats;
            doctorStatsGrid.Items.Refresh();
            DrawDoctorChart(doctorStats.ToList());
        }

        // Отрисовка диаграммы заболеваний
        private void DrawDiseaseChart(List<DiseaseStat> data)
        {
            diseaseChart.Series.Clear();
            // Если нет данных, отображаем сообщение
            if (data == null || !data.Any())
            {
                diseaseChart.Series = new SeriesCollection
                {
                    new PieSeries
                    {
                        Title = "Нет данных",
                        Values = new ChartValues<double> { 1 },
                        DataLabels = true
                    }
                };
                return;
            }

            // Создание коллекции серий для диаграммы
            var series = new SeriesCollection();
            foreach (var item in data)
            {
                series.Add(new PieSeries
                {
                    Title = item.Disease,
                    Values = new ChartValues<double> { item.Percentage },
                    DataLabels = true
                });
            }

            diseaseChart.Series = series;
        }

        // Отрисовка диаграммы посещаемости
        private void DrawAttendanceChart(List<AttendanceStat> data)
        {
            // Очистка текущих серий диаграммы посещаемости
            attendanceChart.Series.Clear();

            // Если нет данных, отображаем сообщение
            if (data == null || !data.Any())
            {
                attendanceChart.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Нет данных",
                        Values = new ChartValues<double> { 0 },
                        PointGeometrySize = 10
                    }
                };
                attendanceChart.AxisX.Clear();
                attendanceChart.AxisX.Add(new Axis { Title = "Месяцы", Labels = new[] { "Нет данных" } });
                attendanceChart.AxisY.Clear();
                attendanceChart.AxisY.Add(new Axis { Title = "Посещения", MinValue = 0 });
                return;
            }

            // Создание коллекции серий для диаграммы посещаемости
            var series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Количество посещений",
                    Values = new ChartValues<double>(data.Select(d => (double)d.Count)),
                    PointGeometrySize = 10
                }
            };

            // Установка серий и осей для диаграммы посещаемости
            attendanceChart.Series = series;
            attendanceChart.AxisX.Clear();
            attendanceChart.AxisX.Add(new Axis
            {
                Title = "Месяцы",
                Labels = data.Select(d => d.Month).ToList()
            });
            attendanceChart.AxisY.Clear();
            attendanceChart.AxisY.Add(new Axis
            {
                Title = "Посещения",
                MinValue = 0
            });
        }

        // Отрисовка диаграммы врачей
        private void DrawDoctorChart(List<DoctorStat> data)
        {
            // Очистка текущих серий диаграммы врачей
            doctorChart.Series.Clear();

            // Если нет данных, отображаем сообщение
            if (data == null || !data.Any())
            {
                doctorChart.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Нет данных",
                        Values = new ChartValues<double> { 0 }
                    }
                };
                doctorChart.AxisX.Clear();
                doctorChart.AxisX.Add(new Axis { Title = "Врачи", Labels = new[] { "Нет данных" } });
                doctorChart.AxisY.Clear();
                doctorChart.AxisY.Add(new Axis { Title = "Количество приёмов", MinValue = 0 });
                return;
            }

            // Создание серии для диаграммы врачей
            var series = new ColumnSeries
            {
                Title = "Приёмы",
                Values = new ChartValues<double>(data.Select(d => (double)d.AppointmentCount)),
                DataLabels = true
            };

            doctorChart.Series = new SeriesCollection { series };
            doctorChart.AxisX.Clear();
            doctorChart.AxisX.Add(new Axis
            {
                Title = "Врачи",
                Labels = data.Select(d => d.DoctorName).ToList()
            });
            doctorChart.AxisY.Clear();
            doctorChart.AxisY.Add(new Axis
            {
                Title = "Количество приёмов",
                MinValue = 0
            });
        }

        // Обработчики событий для кнопок экспорта данных в Excel
        private void ExportDiseaseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToExcel(statsGrid, "Заболевания", new[] { "Заболевание", "Количество", "Процент" });
        }

        private void ExportAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToExcel(attendanceGrid, "Посещаемость", new[] { "Месяц", "Количество посещений" });
        }

        private void ExportDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToExcel(doctorStatsGrid, "Статистика_врачей", new[] { "Врач", "Количество приемов", "Количество пациентов", "Среднее кол-во приемов на пациента" });
        }

        // Сохранение данных в Excel файл
        private void SaveDataToExcel(DataGrid dataGrid, string sheetName, string[] headers)
        {
            // Проверка наличия данных в DataGrid перед экспортом
            if (dataGrid.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Установка лицензии для EPPlus
                ExcelPackage.License.SetNonCommercialPersonal(Environment.GetEnvironmentVariable("EPPLUS_LICENSE") ?? "non-commercial-placeholder");
                // Открытие диалога сохранения файла
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Сохранить статистику как",
                    FileName = $"{sheetName}_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };
                // Если пользователь отменяет диалог сохранения, выходим из метода
                if (saveFileDialog.ShowDialog() != true)
                    return;

                // Создание нового Excel файла и добавление данных
                using (var excelPackage = new ExcelPackage())
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(58, 173, 161));
                        worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Заполнение данных в Excel из DataGrid
                    if (dataGrid == statsGrid)
                    {
                        var items = dataGrid.ItemsSource as IEnumerable<DiseaseStat>;
                        if (items != null)
                        {
                            int row = 2;
                            foreach (var item in items)
                            {
                                worksheet.Cells[row, 1].Value = item.Disease;
                                worksheet.Cells[row, 2].Value = item.Count;
                                worksheet.Cells[row, 3].Value = $"{item.Percentage:F2}%";
                                row++;
                            }
                        }
                    }
                    // Если DataGrid - это attendanceGrid, заполняем данные посещаемости
                    else if (dataGrid == attendanceGrid)
                    {
                        var items = dataGrid.ItemsSource as IEnumerable<AttendanceStat>;
                        if (items != null)
                        {
                            int row = 2;
                            foreach (var item in items)
                            {
                                worksheet.Cells[row, 1].Value = item.Month;
                                worksheet.Cells[row, 2].Value = item.Count;
                                row++;
                            }
                        }
                    }
                    // Если DataGrid - это doctorStatsGrid, заполняем данные статистики врачей
                    else if (dataGrid == doctorStatsGrid)
                    {
                        var items = dataGrid.ItemsSource as IEnumerable<DoctorStat>;
                        if (items != null)
                        {
                            int row = 2;
                            foreach (var item in items)
                            {
                                worksheet.Cells[row, 1].Value = item.DoctorName;
                                worksheet.Cells[row, 2].Value = item.AppointmentCount;
                                worksheet.Cells[row, 3].Value = item.PatientCount;
                                worksheet.Cells[row, 4].Value = item.AverageAppointmentsPerPatient.ToString("F2");
                                row++;
                            }
                        }
                    }

                    // Автоматическая подгонка ширины столбцов
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

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

        // Обработчики событий для навигации по окнам приложения
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

        private void NavigateToStatistics(object sender, RoutedEventArgs e)
        {
            // Переход к текущему окну, так как это окно уже является окном статистики
        }

        // Обработчик для навигации к управлению пользователями
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
        // Обработчик для навигации к отчетам
        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new Window2(currentUser);
            reportsWindow.Show();
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