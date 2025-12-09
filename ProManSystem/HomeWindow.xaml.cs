using System.Windows;
using System.Windows.Media;
using ProManSystem.Views;

namespace ProManSystem
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();

            // عند تشغيل البرنامج نعرض تبويب الزبائن مباشرة
            MainContent.Content = new ClientsView();
            HighlightTab("Clients");
        }

        // عند الضغط على زر الزبائن
        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClientsView();
            HighlightTab("Clients");
        }

        // دالة بسيطة لتلوين التبويب النشط
        private void HighlightTab(string tabName)
        {
            // تبويب الزبائن
            ClientsButton.Background = tabName == "Clients"
                ? new SolidColorBrush(Color.FromRgb(45, 55, 72)) // لون أغمق للتبويب النشط
                : Brushes.Transparent;

            // لاحقاً عندما نضيف تبويبات أخرى نحدّث ألوانها هنا أيضاً
        }
    }
}
