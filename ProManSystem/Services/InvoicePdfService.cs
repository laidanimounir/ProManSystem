using ProManSystem.Data;
using ProManSystem.Models;
using System;
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

        public string GeneratePurchaseInvoicePdf(int invoiceId)
        {
            // جلب الفاتورة
            PurchaseInvoice invoice = _db.PurchaseInvoices
                .Where(i => i.Id == invoiceId)
                .Select(i => new PurchaseInvoice
                {
                    Id = i.Id,
                    NumeroFacture = i.NumeroFacture,
                    DateFacture = i.DateFacture,
                    MontantHT = i.MontantHT,
                    MontantTVA = i.MontantTVA,
                    MontantTTC = i.MontantTTC,
                    Supplier = i.Supplier,
                    Lignes = i.Lignes.ToList()
                })
                .FirstOrDefault();

            if (invoice == null)
                throw new Exception("الفاتورة غير موجودة.");

            // إنشاء مستند MigraDoc
            MigraDoc.DocumentObjectModel.Document document =
                new MigraDoc.DocumentObjectModel.Document();

            MigraDoc.DocumentObjectModel.Section section =
                document.AddSection();

            section.PageSetup.TopMargin =
                new MigraDoc.DocumentObjectModel.Unit(1, MigraDoc.DocumentObjectModel.UnitType.Centimeter);
            section.PageSetup.LeftMargin =
                new MigraDoc.DocumentObjectModel.Unit(1.5, MigraDoc.DocumentObjectModel.UnitType.Centimeter);
            section.PageSetup.RightMargin =
                new MigraDoc.DocumentObjectModel.Unit(1.5, MigraDoc.DocumentObjectModel.UnitType.Centimeter);

            // رأس الفاتورة
            AddInvoiceHeader(section, invoice);

            // بيانات الفاتورة
            AddInvoiceInfo(section, invoice);

            // جدول البنود
            AddItemsTable(section, invoice);

            // الإجماليات
            AddTotals(section, invoice);

            // التذييل والتوقيع
            AddFooter(section);

            // حفظ الملف
            string fileName = $"FA_{invoice.NumeroFacture}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Invoices");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);

            MigraDoc.Rendering.PdfDocumentRenderer renderer =
                new MigraDoc.Rendering.PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();
            renderer.Save(filePath);

            return filePath;
        }

      

        private void AddInvoiceHeader(MigraDoc.DocumentObjectModel.Section section, PurchaseInvoice invoice)
        {
            MigraDoc.DocumentObjectModel.Tables.Table headerTable =
                section.AddTable();

            headerTable.Borders.Visible = false;
            headerTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(3, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            headerTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(9, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            headerTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(3, MigraDoc.DocumentObjectModel.UnitType.Centimeter));

            MigraDoc.DocumentObjectModel.Tables.Row row = headerTable.AddRow();

            // شعار (مربع بسيط حالياً)
            MigraDoc.DocumentObjectModel.Tables.Cell logoCell = row.Cells[0];
            MigraDoc.DocumentObjectModel.Paragraph logoParagraph =
                logoCell.AddParagraph("A");
            logoParagraph.Format.Font.Size = 32;
            logoParagraph.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;

            // اسم الشركة
            MigraDoc.DocumentObjectModel.Tables.Cell companyCell = row.Cells[1];

            MigraDoc.DocumentObjectModel.Paragraph companyName =
                companyCell.AddParagraph("بروكو سيستم");
            companyName.Format.Font.Bold = true;
            companyName.Format.Font.Size = 16;
            companyName.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Right;

            MigraDoc.DocumentObjectModel.Paragraph companyDesc =
                companyCell.AddParagraph("نظام إدارة المشتريات والإنتاج");
            companyDesc.Format.Font.Size = 10;
            companyDesc.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Right;

            // عنوان الفاتورة
            MigraDoc.DocumentObjectModel.Tables.Cell invoiceCell = row.Cells[2];

            MigraDoc.DocumentObjectModel.Paragraph invoiceTitle =
                invoiceCell.AddParagraph("فاتورة شراء");
            invoiceTitle.Format.Font.Bold = true;
            invoiceTitle.Format.Font.Size = 14;
            invoiceTitle.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;

            section.AddParagraph();
            MigraDoc.DocumentObjectModel.Paragraph separator =
                section.AddParagraph("____________________________________________________________");
            separator.Format.Font.Size = 8;
            separator.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
            section.AddParagraph();
        }

       

        private void AddInvoiceInfo(MigraDoc.DocumentObjectModel.Section section, PurchaseInvoice invoice)
        {
            MigraDoc.DocumentObjectModel.Tables.Table infoTable =
                section.AddTable();

            infoTable.Borders.Visible = true;
            infoTable.Borders.Width =
                new MigraDoc.DocumentObjectModel.Unit(0.5, MigraDoc.DocumentObjectModel.UnitType.Point);
            infoTable.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            infoTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(4, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            infoTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(5, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            infoTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(4, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            infoTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(3, MigraDoc.DocumentObjectModel.UnitType.Centimeter));

            MigraDoc.DocumentObjectModel.Tables.Row headerRow = infoTable.AddRow();
           // headerRow.Format.BackgroundColor = MigraDoc.DocumentObjectModel.Colors.DarkGray;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;

            headerRow.Cells[0].AddParagraph("البيان");
            headerRow.Cells[1].AddParagraph("التفاصيل");
            headerRow.Cells[2].AddParagraph("المورد");
            headerRow.Cells[3].AddParagraph("التاريخ");

            MigraDoc.DocumentObjectModel.Tables.Row row1 = infoTable.AddRow();
            row1.Cells[0].AddParagraph("رقم الفاتورة:");
            row1.Cells[1].AddParagraph(invoice.NumeroFacture);
            row1.Cells[2].AddParagraph(invoice.Supplier?.Designation ?? "غير محدد");
            row1.Cells[3].AddParagraph(invoice.DateFacture.ToString("dd/MM/yyyy"));

            section.AddParagraph();
        }

        // ================= جدول البنود =================

        private void AddItemsTable(MigraDoc.DocumentObjectModel.Section section, PurchaseInvoice invoice)
        {
            MigraDoc.DocumentObjectModel.Tables.Table itemsTable =
                section.AddTable();

            itemsTable.Borders.Visible = true;
            itemsTable.Borders.Width =
                new MigraDoc.DocumentObjectModel.Unit(0.75, MigraDoc.DocumentObjectModel.UnitType.Point);
            itemsTable.Borders.Color = MigraDoc.DocumentObjectModel.Colors.Black;

            itemsTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(7, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            itemsTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(2.5, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            itemsTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(3, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            itemsTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(3, MigraDoc.DocumentObjectModel.UnitType.Centimeter));

            MigraDoc.DocumentObjectModel.Tables.Row headerRow = itemsTable.AddRow();
           // headerRow.Format.BackgroundColor = MigraDoc.DocumentObjectModel.Colors.Black;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.White;
            headerRow.Format.Font.Size = 11;

            headerRow.Cells[0].AddParagraph("المادة");
            headerRow.Cells[1].AddParagraph("الكمية");
            headerRow.Cells[2].AddParagraph("السعر للوحدة");
            headerRow.Cells[3].AddParagraph("المجموع");

            foreach (PurchaseInvoiceLine line in invoice.Lignes)
            {
                MigraDoc.DocumentObjectModel.Tables.Row row = itemsTable.AddRow();
                row.Cells[0].AddParagraph(line.RawMaterial?.Designation ?? "غير محدد");
                row.Cells[1].AddParagraph(line.Quantite.ToString("N2"));
                row.Cells[2].AddParagraph(line.PrixUnitaire.ToString("N2"));
                row.Cells[3].AddParagraph(line.MontantLigne.ToString("N2"));
            }

            section.AddParagraph();
        }

       

        private void AddTotals(MigraDoc.DocumentObjectModel.Section section, PurchaseInvoice invoice)
        {
            MigraDoc.DocumentObjectModel.Tables.Table totalsTable =
                section.AddTable();

            totalsTable.Borders.Visible = false;
            totalsTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(9, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            totalsTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(4, MigraDoc.DocumentObjectModel.UnitType.Centimeter));

            MigraDoc.DocumentObjectModel.Tables.Row row1 = totalsTable.AddRow();
            row1.Cells[0].AddParagraph("الإجمالي بدون ضريبة (HT):")
                .Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Right;
            row1.Cells[1].AddParagraph(invoice.MontantHT.ToString("N2"))
                .Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;

            MigraDoc.DocumentObjectModel.Tables.Row row2 = totalsTable.AddRow();
            row2.Cells[0].AddParagraph("الضريبة (TVA 19%):")
                .Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Right;
            row2.Cells[1].AddParagraph(invoice.MontantTVA.ToString("N2"))
                .Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;

            MigraDoc.DocumentObjectModel.Tables.Row row3 = totalsTable.AddRow();
          //  row3.Format.BackgroundColor = MigraDoc.DocumentObjectModel.Colors.LightGray;
            row3.Format.Font.Bold = true;
            row3.Cells[0].AddParagraph("الإجمالي مع الضريبة (TTC):")
                .Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Right;
            row3.Cells[1].AddParagraph(invoice.MontantTTC.ToString("N2"))
                .Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;

            section.AddParagraph();
        }

       

        private void AddFooter(MigraDoc.DocumentObjectModel.Section section)
        {
            section.AddParagraph();

            MigraDoc.DocumentObjectModel.Paragraph footerTitle =
                section.AddParagraph("التوقيع والختم");
            footerTitle.Format.Font.Bold = true;
            footerTitle.Format.Font.Size = 11;

            MigraDoc.DocumentObjectModel.Tables.Table sigTable =
                section.AddTable();

            sigTable.Borders.Visible = true;
            sigTable.Borders.Width =
                new MigraDoc.DocumentObjectModel.Unit(0.5, MigraDoc.DocumentObjectModel.UnitType.Point);

            sigTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(5, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            sigTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(5, MigraDoc.DocumentObjectModel.UnitType.Centimeter));
            sigTable.AddColumn(new MigraDoc.DocumentObjectModel.Unit(5, MigraDoc.DocumentObjectModel.UnitType.Centimeter));

            MigraDoc.DocumentObjectModel.Tables.Row sigRow = sigTable.AddRow();
            sigRow.Height =
                new MigraDoc.DocumentObjectModel.Unit(2.2, MigraDoc.DocumentObjectModel.UnitType.Centimeter);

            sigRow.Cells[0].AddParagraph("المورد").Format.Font.Bold = true;
            sigRow.Cells[1].AddParagraph("الختم والتوقيع").Format.Font.Bold = true;
            sigRow.Cells[2].AddParagraph("المستقبل").Format.Font.Bold = true;

            section.AddParagraph();

            MigraDoc.DocumentObjectModel.Paragraph footerText =
                section.AddParagraph("شكراً لتعاملكم معنا");
            footerText.Format.Font.Size = 10;
            footerText.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
            footerText.Format.Font.Italic = true;
        }
    }
}
