using PdfSharpCore.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace EastFive.Pdf
{
    //03/08/2019, KDH
    //Totally and completely lifted this class from https://stackoverflow.com/questions/27606877/pdfsharp-private-fonts-for-azure-1-50
    //Had to do this because the call to PdfGenerator.GeneratePdf(htmlDocument.ParsedText, config); in pdf extensions returned this 
    //error when in an Azure Function:
    //"System.InvalidOperationException: 'Microsoft Azure returns STATUS_ACCESS_DENIED ((NTSTATUS)0xC0000022L) from GetFontData.
    //This is a bug in Azure. You must implement a FontResolver to circumvent this issue."
    public class PDFFontResolver : IFontResolver
    {
        private Assembly projectAssembly;

        public PDFFontResolver(Assembly projectAssembly)
        {
            this.projectAssembly = projectAssembly;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Ignore case of font names.
            var name = familyName.ToLower().TrimEnd('#');

            // Deal with the fonts we know.
            switch (name)
            {
                case "arial":
                case "segoe ui":
                default:
                    if (isBold)
                    {
                        if (isItalic)
                            return new FontResolverInfo("Arial#bi");
                        return new FontResolverInfo("Arial#b");
                    }
                    if (isItalic)
                        return new FontResolverInfo("Arial#i");
                    return new FontResolverInfo("Arial#");
            }

            // We pass all other font requests to the default handler.
            // When running on a web server without sufficient permission, you can return a default font at this stage.
            //return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case "Arial#":
                    return LoadFontData($"{projectAssembly.GetName().Name}.Fonts.Arial.arial.ttf"); ;

                case "Arial#b":
                    return LoadFontData($"{projectAssembly.GetName().Name}.Fonts.Arial.arialbd.ttf"); ;

                case "Arial#i":
                    return LoadFontData($"{projectAssembly.GetName().Name}.Fonts.Arial.ariali.ttf");

                case "Arial#bi":
                    return LoadFontData($"{projectAssembly.GetName().Name}.Fonts.Arial.arialbi.ttf");
            }

            return null;
        }

        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        private byte[] LoadFontData(string name)
        {
            // Test code to find the names of embedded fonts - put a watch on "ourResources"
            //var ourResources = projectAssembly.GetManifestResourceNames();

            using (Stream stream = projectAssembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }

        internal static PDFFontResolver OurGlobalFontResolver = null;

        public string DefaultFontName => throw new NotImplementedException();

        /// <summary>
        /// Ensure the font resolver is only applied once (or an exception is thrown)
        /// </summary>
        public static void Apply(Assembly projectAssembly)
        {
            if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
            {
                if (OurGlobalFontResolver == null)
                    OurGlobalFontResolver = new PDFFontResolver(projectAssembly);

                GlobalFontSettings.FontResolver = OurGlobalFontResolver;
            }
        }
    }
}



