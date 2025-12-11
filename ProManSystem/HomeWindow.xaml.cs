using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProManSystem.Views;


namespace ProManSystem
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
            MainContent.Content = new ClientsView();
            SetActiveButton(ClientsButton);
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ClientsButton);
            MainContent.Content = new ClientsView();
        }

        private void SuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SuppliersButton);
            MainContent.Content = new SuppliersEditorView();
        }

        private void SetActiveButton(Button activeButton)
        {
            ClientsButton.Background = Brushes.Transparent;
            ProductsButton.Background = Brushes.Transparent;
            SuppliersButton.Background = Brushes.Transparent;
            RawMaterialsButton.Background = Brushes.Transparent;


            activeButton.Background = new SolidColorBrush(Color.FromRgb(55, 71, 79));
        }

        private void RawMaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(RawMaterialsButton);
            MainContent.Content = new RawMaterialsEditorView();
        }

    }
}
