using System.IO;
using HtmlAgilityPack;
using EastFive.Linq.Async;
using EastFive.Extensions;
using System.Threading.Tasks;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using VetCV.HtmlRendererCore.PdfSharpCore;
using PdfSharpCore.Pdf.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SixLabors.ImageSharp.PixelFormats;
using PdfSharpCore;

namespace EastFive.Pdf
{
    public static class PdfExtensions
    {
        static PdfExtensions()
        {
            if (ImageSource.ImageSourceImpl == null)
                    ImageSource.ImageSourceImpl = new ImageSharp3CompatibleImageSource<Rgba32>();
        }

        public static async Task<byte[]> AggregateImagesAsync(this IEnumerableAsync<byte[]> images, int margin = 0)
        {
            var composedPdf = await images.AggregateAsync(
                new PdfDocument(),
                (outputPdf, imageBytes) =>
                {
                    if (imageBytes.IsDefaultNullOrEmpty())
                        return outputPdf;
                    
                    PdfPage page = outputPdf.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    Stream imageStream = new MemoryStream(imageBytes);
                    XImage image = XImage.FromStream(() => imageStream);
                    gfx.DrawImage(image, margin, margin, page.Width - margin, page.Height - margin);
                    return outputPdf;
                });

            var concatenatedStream = new MemoryStream();
            composedPdf.Save(concatenatedStream);
            return await concatenatedStream.ToBytesAsync();
        }

        public static Stream ConvertHtmlStringToPdf(this string htmlString, 
            PageSize pageSize = PdfSharpCore.PageSize.Letter,
            int margin = 0)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            var pdfStream = document.ToPDF(pageSize, margin);

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

        public static Stream ToPDF(this HtmlAgilityPack.HtmlDocument htmlDocument, 
            PageSize pageSize = PdfSharpCore.PageSize.Letter, 
            int margin = 0)
        {
            var config = new PdfGenerateConfig
            {
                PageSize = pageSize,
            };
            config.SetMargins(margin);

            PdfDocument doc = PdfGenerator.GeneratePdf(htmlDocument.ParsedText, config);
            var output = new MemoryStream();
            doc.Save(output);
            return output;
        }

        public static async Task<byte[]> AggregatePdfsAsync(this IEnumerableAsync<byte[]> pdfs)
        {
            var composedPdf = await pdfs.AggregateAsync(
                new PdfDocument(),
                (outputPdf, pdfBytes) =>
                {
                    if (pdfBytes.IsDefaultNullOrEmpty())
                        return outputPdf;
                    
                    var stream = new MemoryStream(pdfBytes);
                    var doc = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                    foreach (var page in doc.Pages)
                    {
                        outputPdf.AddPage(page);
                    }
                    return outputPdf;
                });
            var concatenatedStream = new MemoryStream();
            composedPdf.Save(concatenatedStream);
            return await concatenatedStream.ToBytesAsync();
        }

        public static async Task<byte[]> ConcatAsync(this byte[] pdf1, byte[] pdf2)
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
            return await concatenatedStream.ToBytesAsync();
        }

        public static async Task<byte[]> ConcatPdfs(this byte[][] pdfs)
        {
            var concatenatedPdfs = new byte[] { };
            foreach (var pdf in pdfs)
            {
                concatenatedPdfs = await concatenatedPdfs.ConcatAsync(pdf);
            }
            return concatenatedPdfs;
        }
    }
}
