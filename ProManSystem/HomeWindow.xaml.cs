using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ProManSystem.Views;

namespace ProManSystem
{
    public partial class HomeWindow : Window
    {
        private bool _isFullScreen = false;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private DispatcherTimer _clockTimer;

        public HomeWindow()
        {
            InitializeComponent();
            InitializeDateAndTime();
            MainContent.Content = new DashboardView();
            SetActiveButton(DashboardButton);
        }

        private void InitializeDateAndTime()
        {
            var frenchCulture = new CultureInfo("fr-FR");
            TodayTextBlock.Text = DateTime.Now.ToString("dd MMM yyyy", frenchCulture);

            if (FooterTimeTextBlock != null)
            {
                FooterTimeTextBlock.Text = DateTime.Now.ToString("HH:mm");
                _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                _clockTimer.Tick += (s, e) => FooterTimeTextBlock.Text = DateTime.Now.ToString("HH:mm");
                _clockTimer.Start();
            }
        }

        private void SetActiveButton(Button activeButton)
        {
            var buttons = new[] { DashboardButton, ClientsButton, ProductsButton, SuppliersButton, RawMaterialsButton, PurchaseInvoicesButton, SalesInvoicesButton, SalesInvoicesListButton };
            foreach (var btn in buttons)
            {
                if (btn != null) btn.Style = (Style)FindResource("NavButton");
            }
            if (activeButton != null) activeButton.Style = (Style)FindResource("NavButtonActive");
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

        private void QuickNewSalesInvoice_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SalesInvoicesButton);
            MainContent.Content = new SalesInvoicesView();
        }

        private void QuickProducts_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ProductsButton);
            MainContent.Content = new ProductsView();
        }

        private void QuickClients_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(ClientsButton);
            MainContent.Content = new ClientsView();
        }

        private void SalesInvoicesListButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SalesInvoicesListButton);
            MainContent.Content = new SalesInvoicesListView();   
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullScreen)
            {
                _previousWindowState = this.WindowState;
                _previousWindowStyle = this.WindowStyle;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                _isFullScreen = true;
            }
            else
            {
                this.WindowStyle = _previousWindowStyle;
                this.WindowState = _previousWindowState;
                _isFullScreen = false;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Voulez-vous vraiment vous déconnecter ?",
                "Déconnexion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_clockTimer != null)
                {
                    _clockTimer.Stop();
                    _clockTimer = null;
                }
                Application.Current.Shutdown();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer = null;
            }
            base.OnClosed(e);
        }
    }
}
