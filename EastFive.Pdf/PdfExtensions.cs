using System.IO;
using System;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using HtmlAgilityPack;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

namespace EastFive.Pdf
{
    public static class PdfExtensions
    {
        public static Stream CreatePDFFromImages(this byte[][] images)
        {
            const double margin = 50d;

            PdfDocument document = new PdfDocument();

            foreach (var imageBytes in images)
            {
                PdfPage page = document.AddPage();

                XGraphics gfx = XGraphics.FromPdfPage(page);

                Stream imageStream = new MemoryStream(imageBytes);
                XImage image = XImage.FromStream(imageStream);
                gfx.DrawImage(image, margin, margin, page.Width - margin, page.Height - margin);
            }

            var pdfStream = new MemoryStream();
            document.Save(pdfStream);

            // For debugging ---
            //using (var fileStream = File.Create("C:\\temp\\outputpdf.pdf"))
            //{
            //    pdfStream.Seek(0, SeekOrigin.Begin);
            //    pdfStream.CopyTo(fileStream);
            //}
            pdfStream.Seek(0, SeekOrigin.Begin);

            return pdfStream;
        }

        public static Stream ConvertHtmlStringToPdf(this string htmlString)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            var pdfStream = document.ToPDF();

            // For debugging ---
            //File.WriteAllText("c:\\temp\\outputhtml.html", htmlString);
            //using (var fileStream = File.Create("C:\\temp\\outputpdf.pdf"))
            //{
            //    pdfStream.Seek(0, SeekOrigin.Begin);
            //    pdfStream.CopyTo(fileStream);
            //}
            pdfStream.Seek(0, SeekOrigin.Begin);

            return pdfStream;
        }

        public static Stream ToPDF(this HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            var config = new PdfGenerateConfig
            {
                PageSize = PdfSharp.PageSize.A4
            };
            config.SetMargins(20);

            PdfDocument doc = null;
            try
            {
                doc = PdfGenerator.GeneratePdf(htmlDocument.ParsedText, config);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            var output = new MemoryStream();
            doc.Save(output);
            return output;
        }

        public static byte[] Concat(this byte[] pdf1, byte[] pdf2)
        {
            PdfDocument outputDocument = new PdfDocument();

            var pdf1HasBytes = pdf1.Length > 0;
            var pdf2HasBytes = pdf2.Length > 0;

            if (!pdf1HasBytes && !pdf2HasBytes)
                return new byte[] { };

            if (!pdf1HasBytes)
                return pdf2;

            if (!pdf2HasBytes)
                return pdf1;

            var stream1 = new MemoryStream(pdf1);
            var stream2 = new MemoryStream(pdf2);
            var doc1 = PdfReader.Open(stream1, PdfDocumentOpenMode.Import);
            var doc2 = PdfReader.Open(stream2, PdfDocumentOpenMode.Import);

            foreach (var page in doc2.Pages)
            {
                doc1.AddPage(page);
            }

            var concatenatedStream = new MemoryStream();
            doc1.Save(concatenatedStream);
            return concatenatedStream.ToBytes();
        }

        public static byte[] ConcatPdfs(this byte[][] pdfs)
        {
            var concatenatedPdfs = new byte[] { };
            foreach (var pdf in pdfs)
            {
                concatenatedPdfs = concatenatedPdfs.Concat(pdf);
            }
            return concatenatedPdfs;
        }
    }
}
