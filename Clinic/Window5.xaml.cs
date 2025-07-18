using System.Windows;

namespace WpfApp20
{
    public partial class Window5 : Window
    {
        private string currentUser;
        private static bool isFirstLoad = true;
        public bool IsAdmin => currentUser == "admin";
        public Window5()
        {
            InitializeComponent();
            DataContext = this;
            MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
            if (isFirstLoad)
            {
                MessageBox.Show("Доступ ограничен. Ожидайте одобрения администратора.", "Приветствие", MessageBoxButton.OK, MessageBoxImage.Information);
                isFirstLoad = false;
            }
        }
        // Обработчик для закрытия приложения
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // Обработчик для кнопки "Выйти"
        private void Logout(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Закрытие текущего окна и открытие окна входа
                isFirstLoad = true;
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
    }
}