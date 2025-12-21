using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ProManSystem.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProManSystem.Services
{
    public class InvoicePdfService
    {
        private readonly AppDbContext _db;

        public InvoicePdfService(AppDbContext db)
        {
            _db = db;
        }

        
        public string GeneratePurchaseInvoicePdf(int invoiceId, string outputPath = null)
        {
            var invoice = _db.PurchaseInvoices
                .Where(i => i.Id == invoiceId)
                .Select(i => new
                {
                    i.NumeroFacture,
                    i.DateFacture,
                    i.MontantHT,
                    i.TauxTVA,
                    i.MontantTTC,
                    SupplierName = i.Supplier.Designation
                })
                .FirstOrDefault();

            if (invoice == null)
                throw new Exception("الفاتورة غير موجودة.");

            if (outputPath == null)
            {
                outputPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Facture_Achat_{invoice.NumeroFacture}.pdf");
            }

           
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Facture achat";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
            XFont normalFont = new XFont("Arial", 12, XFontStyleEx.Regular);



            double y = 40;

            
            gfx.DrawString("FACTURE D'ACHAT", titleFont, XBrushes.Black,
                new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
            y += 40;

            // معلومات الفاتورة
            gfx.DrawString($"Numéro : {invoice.NumeroFacture}", normalFont, XBrushes.Black, 40, y); y += 20;
            gfx.DrawString($"Date   : {invoice.DateFacture:dd/MM/yyyy}", normalFont, XBrushes.Black, 40, y); y += 20;
            gfx.DrawString($"Fournisseur : {invoice.SupplierName}", normalFont, XBrushes.Black, 40, y); y += 40;

            // ملخص مالي بسيط
            gfx.DrawString($"Total HT : {invoice.MontantHT:F2}", normalFont, XBrushes.Black, 40, y); y += 20;
            gfx.DrawString($"TVA      : {invoice.TauxTVA:F2}", normalFont, XBrushes.Black, 40, y); y += 20;
            gfx.DrawString($"Total TTC: {invoice.MontantTTC:F2}", normalFont, XBrushes.Black, 40, y); y += 20;

            document.Save(outputPath);
            document.Close();

            // فتح الملف تلقائيًا
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputPath,
                    UseShellExecute = true
                });
            }
            catch { }

            return outputPath;
        }
    }
}
