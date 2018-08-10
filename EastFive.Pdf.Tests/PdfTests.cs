using System;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EastFive.Pdf;
using System.IO;

namespace EastFive.Pdf.Tests
{
    [TestClass]
    public class PdfTests
    {
        [TestMethod]
        public void ConvertHtmlStringToPdf(string htmlString)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            var pdfStream = document.ToPDF();

            using (var fileStream = File.Create("C:\\temp\\outputpdf.pdf"))
            {
                pdfStream.Seek(0, SeekOrigin.Begin);
                pdfStream.CopyTo(fileStream);
            }
        }
    }
}
