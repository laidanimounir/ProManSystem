using System.Windows;
using System.Windows.Controls;
using ProManSystem.Views;

namespace ProManSystem
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();

           
            MainContent.Content = new DashboardView();
            SetActiveButton(DashboardButton);
        }

        private void SetActiveButton(Button activeButton)
        {
         
            var buttons = new[]
            {
                DashboardButton,
                ClientsButton,
                ProductsButton,
                SuppliersButton,
                RawMaterialsButton,
                PurchaseInvoicesButton,
                SalesInvoicesButton
            };

          
            foreach (var btn in buttons)
            {
                if (btn != null)
                    btn.Style = (Style)FindResource("NavButton");
            }

            
            if (activeButton != null)
                activeButton.Style = (Style)FindResource("NavButtonActive");
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(DashboardButton);
            MainContent.Content = new DashboardView();
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ClientsButton);
            MainContent.Content = new ClientsView();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ProductsButton);
            MainContent.Content = new ProductsView();
        }

        private void SuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SuppliersButton);
            MainContent.Content = new SuppliersEditorView();
        }

        private void RawMaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(RawMaterialsButton);
            MainContent.Content = new RawMaterialsEditorView();
        }

        private void PurchaseInvoicesButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(PurchaseInvoicesButton);
            MainContent.Content = new PurchaseInvoicesView();
        }

        private void SalesInvoicesButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SalesInvoicesButton);
            MainContent.Content = new SalesInvoicesView();
        }
    }
}
