using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ProManSystem.Views;

namespace ProManSystem
{
    public partial class HomeWindow : Window
    {
        //  VARIABLES 
        private bool _isFullScreen = false;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private DispatcherTimer _clockTimer;
        private bool _isSidebarCollapsed = false;
        private const double SIDEBAR_EXPANDED = 260;
        private const double SIDEBAR_COLLAPSED = 70;

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
            var buttons = new[]
            {
                DashboardButton,
                ClientsButton,
                ProductsButton,
                SuppliersButton,
                RawMaterialsButton,
                PurchaseInvoicesButton,
                SalesInvoicesButton,
                SalesInvoicesListButton
            };

            foreach (var btn in buttons)
            {
                if (btn != null)
                    btn.Style = (Style)FindResource("NavButton");
            }

            if (activeButton != null)
                activeButton.Style = (Style)FindResource("NavButtonActive");
        }

        // LEFT SIDEBAR NAVIGATION
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

        private void SalesInvoicesListButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(SalesInvoicesListButton);
            MainContent.Content = new SalesInvoicesListView();
        }

        //  RIGHT PANEL INVOICE TYPES 
        private void SalesInvoiceType_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(SalesInvoicesButton);
            MainContent.Content = new SalesInvoicesView();
        }

        private void PurchaseInvoiceType_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(PurchaseInvoicesButton);
            MainContent.Content = new PurchaseInvoicesView();
        }

        private void Proforma_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(null);

            MessageBox.Show(
                "Vue Proforma à créer.\nCréez ProManSystem.Views.ProformaView.xaml",
                "En développement",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BonCommande_Click(object sender, MouseButtonEventArgs e)
        {
            SetActiveButton(null);

            MessageBox.Show(
                "Vue Bon de Commande à créer.\nCréez ProManSystem.Views.BonCommandeView.xaml",
                "En développement",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CreateNewInvoice_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Choisir le type de facture",
                Width = 700,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent
            };

            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
                CornerRadius = new CornerRadius(16)
            };

            var mainGrid = new Grid { Background = Brushes.White };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(70) });

            // Header
            var header = new Border
            {
                Background = (LinearGradientBrush)FindResource("PrimaryGradient"),
                Padding = new Thickness(32, 24, 32, 24)  
            };

            var headerStack = new StackPanel();
            headerStack.Children.Add(new TextBlock
            {
                Text = "Nouvelle Facture",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });
            headerStack.Children.Add(new TextBlock
            {
                Text = "Sélectionnez le type de facture à créer",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Margin = new Thickness(0, 4, 0, 0)
            });
            header.Child = headerStack;

            var closeBtn = new Button
            {
                Content = "✕",
                FontSize = 20,
                Width = 40,
                Height = 40,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 20, 0)
            };
            closeBtn.Click += (s, ev) => dialog.Close();

            var headerGrid = new Grid();
            headerGrid.Children.Add(header);
            headerGrid.Children.Add(closeBtn);
            Grid.SetRow(headerGrid, 0);

           
            var contentGrid = new Grid { Margin = new Thickness(32) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
            contentGrid.RowDefinitions.Add(new RowDefinition());
            contentGrid.RowDefinitions.Add(new RowDefinition());

            var card1 = CreateInvoiceTypeCard("💰", "Facture de Vente", "Créer une facture de vente", "#10B981");
            card1.MouseLeftButtonDown += (s, ev) => { dialog.Close(); SalesInvoicesButton_Click(s, new RoutedEventArgs()); };
            Grid.SetRow(card1, 0);
            Grid.SetColumn(card1, 0);

            var card2 = CreateInvoiceTypeCard("🛒", "Facture d'Achat", "Créer une facture d'achat", "#F59E0B");
            card2.MouseLeftButtonDown += (s, ev) => { dialog.Close(); PurchaseInvoicesButton_Click(s, new RoutedEventArgs()); };
            Grid.SetRow(card2, 0);
            Grid.SetColumn(card2, 1);

            var card3 = CreateInvoiceTypeCard("📄", "Facture Proforma", "Créer une facture proforma", "#3B82F6");
            card3.MouseLeftButtonDown += (s, ev) => { dialog.Close(); };
            Grid.SetRow(card3, 1);
            Grid.SetColumn(card3, 0);

            var card4 = CreateInvoiceTypeCard("📝", "Bon de Commande", "Créer un bon de commande", "#8B5CF6");
            card4.MouseLeftButtonDown += (s, ev) => { dialog.Close(); };
            Grid.SetRow(card4, 1);
            Grid.SetColumn(card4, 1);

            contentGrid.Children.Add(card1);
            contentGrid.Children.Add(card2);
            contentGrid.Children.Add(card3);
            contentGrid.Children.Add(card4);
            Grid.SetRow(contentGrid, 1);

            // Footer
            var footer = new Border
            {
                Background = (SolidColorBrush)FindResource("Gray50"),
                BorderBrush = (SolidColorBrush)FindResource("Border"),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(32, 16, 32, 16) 
            };

            var cancelBtn = new Button
            {
                Content = "Annuler",
                Padding = new Thickness(24, 12, 24, 12),  
                Background = Brushes.Transparent,
                BorderBrush = (SolidColorBrush)FindResource("Border"),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            cancelBtn.Click += (s, ev) => dialog.Close();
            footer.Child = cancelBtn;
            Grid.SetRow(footer, 2);

            mainGrid.Children.Add(headerGrid);
            mainGrid.Children.Add(contentGrid);
            mainGrid.Children.Add(footer);

            overlay.Child = mainGrid;
            dialog.Content = overlay;

            dialog.Opacity = 0;
            dialog.Loaded += (s, ev) =>
            {
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                dialog.BeginAnimation(OpacityProperty, fadeIn);
            };

            dialog.ShowDialog();
        }

        private Border CreateInvoiceTypeCard(string emoji, string title, string description, string color)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = (SolidColorBrush)FindResource("Border"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(8),
                Padding = new Thickness(24),
                Cursor = Cursors.Hand
            };

            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            stack.Children.Add(new TextBlock
            {
                Text = emoji,
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("TextPrimary"),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 13,
                Foreground = (SolidColorBrush)FindResource("TextSecondary"),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            });

            card.Child = stack;

            card.MouseEnter += (s, e) =>
            {
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                var anim = new ThicknessAnimation
                {
                    From = new Thickness(8),
                    To = new Thickness(8, 4, 8, 12),
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                card.BeginAnimation(MarginProperty, anim);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BorderBrush = (SolidColorBrush)FindResource("Border");
                var anim = new ThicknessAnimation
                {
                    From = new Thickness(8, 4, 8, 12),
                    To = new Thickness(8),
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                card.BeginAnimation(MarginProperty, anim);
            };

            return card;
        }

      
        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            var duration = TimeSpan.FromMilliseconds(300);

            if (!_isSidebarCollapsed)
            {
               
                AnimateColumnWidth(LeftSidebarColumn, SIDEBAR_EXPANDED, SIDEBAR_COLLAPSED, duration);
                _isSidebarCollapsed = true;

                AnimateOpacity(SearchBoxContainer, 1, 0, 200);
                AnimateOpacity(UserTextInfo, 1, 0, 200);
                AnimateOpacity(NavSectionHeader, 1, 0, 200);
                AnimateOpacity(FacturationSectionHeader, 1, 0, 200);
                AnimateOpacity(SeparatorLine, 1, 0, 200);

                HideButtonText(NavigationPanel);
                RotateIcon(CollapseIcon, 0, 180);
            }
            else
            {
              
                AnimateColumnWidth(LeftSidebarColumn, SIDEBAR_COLLAPSED, SIDEBAR_EXPANDED, duration);
                _isSidebarCollapsed = false;

                AnimateOpacity(SearchBoxContainer, 0, 1, 300);
                AnimateOpacity(UserTextInfo, 0, 1, 300);
                AnimateOpacity(NavSectionHeader, 0, 1, 300);
                AnimateOpacity(FacturationSectionHeader, 0, 1, 300);
                AnimateOpacity(SeparatorLine, 0, 1, 300);

                ShowButtonText(NavigationPanel);
                RotateIcon(CollapseIcon, 180, 0);
            }
        }

        private void AnimateColumnWidth(ColumnDefinition column, double from, double to, TimeSpan duration)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, column);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ColumnDefinition.WidthProperty));

            column.Width = new GridLength(from);
            storyboard.Children.Add(animation);
            storyboard.Begin();

            animation.Completed += (s, e) =>
            {
                column.Width = new GridLength(to);
            };
        }

        private void AnimateOpacity(UIElement element, double from, double to, double milliseconds)
        {
            if (element == null) return;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(milliseconds)
            };

            if (to == 0)
            {
                animation.Completed += (s, e) => element.Visibility = Visibility.Collapsed;
            }
            else
            {
                element.Visibility = Visibility.Visible;
            }

            element.BeginAnimation(OpacityProperty, animation);
        }

        private void RotateIcon(System.Windows.Shapes.Path icon, double from, double to)
        {
            if (icon == null) return;

            var rotateTransform = new RotateTransform(from);
            icon.RenderTransform = rotateTransform;
            icon.RenderTransformOrigin = new Point(0.5, 0.5);

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void HideButtonText(Panel panel)
        {
            if (panel == null) return;

            foreach (var child in panel.Children)
            {
                if (child is Button button)
                {
                    var content = button.Content as StackPanel;
                    if (content != null && content.Children.Count > 1)
                    {
                        for (int i = 1; i < content.Children.Count; i++)
                        {
                            if (content.Children[i] is TextBlock tb)
                            {
                                AnimateOpacity(tb, 1, 0, 200);
                            }
                        }
                    }

                    var textBlock = content?.Children.OfType<TextBlock>().LastOrDefault();
                    if (textBlock != null)
                    {
                        button.ToolTip = textBlock.Text;
                    }
                }
            }
        }

        private void ShowButtonText(Panel panel)
        {
            if (panel == null) return;

            foreach (var child in panel.Children)
            {
                if (child is Button button)
                {
                    var content = button.Content as StackPanel;
                    if (content != null && content.Children.Count > 1)
                    {
                        for (int i = 1; i < content.Children.Count; i++)
                        {
                            if (content.Children[i] is TextBlock tb)
                            {
                                AnimateOpacity(tb, 0, 1, 300);
                            }
                        }
                    }

                    button.ToolTip = null;
                }
            }
        }

       
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                ShowAllNavigationButtons();
                return;
            }

            var buttons = NavigationPanel.Children.OfType<Button>();
            foreach (var button in buttons)
            {
                var stackPanel = button.Content as StackPanel;
                if (stackPanel != null)
                {
                    var textBlocks = stackPanel.Children.OfType<TextBlock>().ToList();
                    if (textBlocks.Count > 1)
                    {
                        string buttonText = textBlocks[1].Text.ToLower();
                        button.Visibility = buttonText.Contains(searchText)
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    }
                }
            }
        }

        private void ShowAllNavigationButtons()
        {
            var buttons = NavigationPanel.Children.OfType<Button>();
            foreach (var button in buttons)
            {
                button.Visibility = Visibility.Visible;
            }
        }

       
        private void MainSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = MainSearchBox.Text.ToLower().Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                System.Diagnostics.Debug.WriteLine($"Global search: {searchText}");
            }
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Vous avez 5 notifications:\n\n" +
                "• 3 factures en attente de validation\n" +
                "• 1 paiement reçu\n" +
                "• 1 nouveau client ajouté",
                "Notifications",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Paramètres à implémenter:\n\n" +
                "• Préférences utilisateur\n" +
                "• Configuration système\n" +
                "• Gestion des droits\n" +
                "• Paramètres d'impression",
                "Paramètres",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Profil utilisateur:\n\n" +
                "Nom: ADMINISTRATEUR\n" +
                "Email: admin@atelio.com\n" +
                "Rôle: Administrateur\n" +
                "Dernière connexion: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                "Mon Profil",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
