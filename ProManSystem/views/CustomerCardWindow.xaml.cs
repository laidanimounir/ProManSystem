using ProManSystem.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

using PdfSharp.Pdf;          // لو بقيت PdfSharp-gdi
using PdfSharp.Drawing;

using QRCoder;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;

using DrawingBitmap = System.Drawing.Bitmap;
using DrawingImage = System.Drawing.Image;

namespace ProManSystem.Views
{
    public partial class CustomerCardWindow : Window
    {
        private readonly Customer _customer;

        public CustomerCardWindow(Customer customer)
        {
            InitializeComponent();
            _customer = customer;
            LoadCustomerData();
            GenerateQrCode();
        }

        private void LoadCustomerData()
        {
            CodeTextBlock.Text = _customer.CodeClient;
            NomTextBlock.Text = _customer.NomComplet;
            ActiviteTextBlock.Text = _customer.Activite ?? "N/A";
            AdresseTextBlock.Text = _customer.Adresse ?? "N/A";
            RcTextBlock.Text = _customer.NumeroRC ?? "N/A";
            MatriculeTextBlock.Text = _customer.MatriculeFiscal ?? "N/A";
            IdTextBlock.Text = $"{_customer.TypeIdentification} / {_customer.NumeroIdentification}";
            CategorieTextBlock.Text = _customer.Categorie ?? "Standard";
            DateTextBlock.Text = _customer.DateCreation.ToString("dd/MM/yyyy");

            if (_customer.EstRadie)
            {
                StatutBadge.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                StatutTextBlock.Text = "RADIÉ";
                RemarquesTextBlock.Text = "Ce client est radié et inactif.";
            }
            else
            {
                StatutBadge.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                StatutTextBlock.Text = "ACTIF";
                RemarquesTextBlock.Text = "Client actif et en règle.";
            }
        }

        private void GenerateQrCode()
        {
            try
            {
                string qrPayload =
                    $"{_customer.CodeClient}|{_customer.NomComplet}|{_customer.Categorie}|{(_customer.EstRadie ? "Radié" : "Actif")}";

                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q))
                {
                   
                    var pngByteQrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrBytes = pngByteQrCode.GetGraphic(20);   

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
                MessageBox.Show("خطأ في إنشاء QR Code: " + ex.Message);
            }
        }



        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(CardRootGrid, $"Carte client {_customer.CodeClient}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الطباعة: " + ex.Message);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Carte_{_customer.CodeClient}.pdf",
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    ExportCardToPdf(saveDialog.FileName);
                    MessageBox.Show("البطاقة تم تصديرها بنجاح!", "نجاح",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في التصدير: " + ex.Message);
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
