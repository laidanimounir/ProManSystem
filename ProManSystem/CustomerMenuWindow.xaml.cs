using System.Windows;

namespace ProManSystem
{
    public partial class CustomerMenuWindow : Window
    {
        public CustomerMenuWindow()
        {
            InitializeComponent();
        }

        private void ManageCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new MainWindow();   
            win.Show();
            this.Close();
        }

        private void SearchCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new CustomerSearchWindow(); 
            win.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var home = new HomeWindow();
            home.Show();
            this.Close();
        }
    }
}
