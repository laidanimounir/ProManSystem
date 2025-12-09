using System.Windows;

namespace ProManSystem
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new MainWindow();  
            win.Show();
            this.Close();                
        }
    }
}
