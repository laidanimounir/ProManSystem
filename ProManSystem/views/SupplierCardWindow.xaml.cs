using ProManSystem.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QRCoder;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ProManSystem.Views
{
    public partial class SupplierCardWindow : Window
    {
        private readonly Supplier _supplier;

        public SupplierCardWindow(Supplier supplier)
        {
            InitializeComponent();
            _supplier = supplier;
            LoadSupplierData();
            GenerateQrCode();
        }

        private void LoadSupplierData()
        {
            CodeTextBlock.Text = _supplier.CodeFournisseur;
            DesignationTextBlock.Text = _supplier.Designation;
            ActiviteTextBlock.Text = _supplier.Activite ?? "N/A";
            AdresseTextBlock.Text = _supplier.Adresse ?? "N/A";
            RcTextBlock.Text = _supplier.NumeroRC ?? "N/A";
            MatriculeTextBlock.Text = _supplier.MatriculeFiscal ?? "N/A";
            IdTextBlock.Text = $"{_supplier.TypeIdentification} / {_supplier.NumeroIdentification}";
            TotalAchatsTextBlock.Text = (_supplier.TotalAchats?.ToString("F2") ?? "0.00") + " DA";
            DetteTextBlock.Text = (_supplier.Dette?.ToString("F2") ?? "0.00") + " DA";
            DateTextBlock.Text = _supplier.DateCreation.ToString("dd/MM/yyyy");

            if (_supplier.EstActif)
            {
                StatutBadge.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                StatutIcon.Text = "✓";
                StatutTextBlock.Text = "ACTIF";
            }
            else
            {
                StatutBadge.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                StatutIcon.Text = "✖";
                StatutTextBlock.Text = "INACTIF";
            }
        }

        private void GenerateQrCode()
        {
            try
            {
                string qrPayload = $"{_supplier.CodeFournisseur}|{_supplier.Designation}";

                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q))
                {
                    var pngByteQrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrBytes = pngByteQrCode.GetGraphic(8);

                    BitmapImage bi = new BitmapImage();
                    using (var ms = new MemoryStream(qrBytes))
                    {
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;
                        bi.EndInit();
                        bi.Freeze();
                    }

                    QrImage.Source = bi;
                    QrPlaceholder.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la génération du QR Code : " + ex.Message);
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(CardRootGrid, $"Carte fournisseur {_supplier.CodeFournisseur}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'impression : " + ex.Message);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Carte_{_supplier.CodeFournisseur}.pdf",
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    ExportCardToPdf(saveDialog.FileName);
                    MessageBox.Show("La carte a été exportée avec succès!", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'export : " + ex.Message);
            }
        }

        private void ExportCardToPdf(string filePath)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)CardRootGrid.ActualWidth,
                (int)CardRootGrid.ActualHeight,
                96, 96,
                PixelFormats.Pbgra32);
            rtb.Render(CardRootGrid);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Position = 0;

                using (PdfDocument document = new PdfDocument())
                {
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    using (XImage xImage = XImage.FromStream(ms))
                    {
                        double margin = 40;
                        double maxWidth = page.Width - 2 * margin;
                        double maxHeight = page.Height - 2 * margin;

                        double scale = Math.Min(maxWidth / xImage.PixelWidth,
                                                maxHeight / xImage.PixelHeight);
                        double drawWidth = xImage.PixelWidth * scale;
                        double drawHeight = xImage.PixelHeight * scale;

                        double x = (page.Width - drawWidth) / 2;
                        double y = (page.Height - drawHeight) / 2;

                        gfx.DrawImage(xImage, x, y, drawWidth, drawHeight);
                    }

                    document.Save(filePath);
                }
            }
        }
    }
}
