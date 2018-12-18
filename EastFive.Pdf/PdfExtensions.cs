﻿using System.IO;
using System;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using HtmlAgilityPack;

namespace EastFive.Pdf
{
    public static class PdfExtensions
    {
        public static Stream ConvertHtmlStringToPdf(this string htmlString)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            var pdfStream = document.ToPDF();


            // For debugging ---
            //using (var fileStream = File.Create("C:\\temp\\outputpdf.pdf"))
            //{
            //    pdfStream.Seek(0, SeekOrigin.Begin);
            //    pdfStream.CopyTo(fileStream);
            //}
            //pdfStream.Seek(0, SeekOrigin.Begin);


            return pdfStream;
        }

        public static Stream ToPDF(this HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            var config = new PdfGenerateConfig
            {
                PageSize = PdfSharp.PageSize.A4
            };
            config.SetMargins(20);

            var doc = PdfGenerator.GeneratePdf(htmlDocument.ParsedText, config);

            var output = new MemoryStream();
            doc.Save(output);
            return output;
        }

        
    }
}
