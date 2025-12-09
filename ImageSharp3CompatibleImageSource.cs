using System;
using System.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace EastFive.Pdf;

// see: https://github.com/ststeiger/PdfSharpCore/issues/426
public class ImageSharp3CompatibleImageSource<TPixel> : ImageSource where TPixel : unmanaged, IPixel<TPixel> {
	public static IImageSource FromImageSharpImage(
		Image<TPixel> image,
		IImageFormat imgFormat,
		int? quality = 75) =>
		new ImageSharpImageSourceImpl<TPixel>("*" + Guid.NewGuid().ToString("B"), image, quality ?? 75, imgFormat is PngFormat);

	protected override IImageSource FromBinaryImpl(
		string name,
		Func<byte[]> imageSource,
		int? quality = 75) {
		Image<TPixel> image = Image.Load<TPixel>(imageSource());
		return new ImageSharpImageSourceImpl<TPixel>(name, image, quality ?? 75, image.Metadata.DecodedImageFormat is PngFormat);
	}

	protected override IImageSource FromFileImpl(string path, int? quality = 75) {
		Image<TPixel> image = Image.Load<TPixel>(path);
		return new ImageSharpImageSourceImpl<TPixel>(path, image, quality ?? 75, image.Metadata.DecodedImageFormat is PngFormat);
	}

	protected override IImageSource FromStreamImpl(
		string name,
		Func<Stream> imageStream,
		int? quality = 75) {
		using (Stream stream = imageStream()) {
			Image<TPixel> image = Image.Load<TPixel>(stream);
			return new ImageSharpImageSourceImpl<TPixel>(name, image, quality ?? 75, image.Metadata.DecodedImageFormat is PngFormat);
		}
	}

	private class ImageSharpImageSourceImpl<TPixel2>(
		string name,
		Image<TPixel2> image,
		int quality,
		bool isTransparent)
		: IImageSource
		where TPixel2 : unmanaged, IPixel<TPixel2> {
		private Image<TPixel2> Image { get; } = image;

		public int Width => Image.Width;

		public int Height => Image.Height;

		public string Name { get; } = name;

		public bool Transparent { get; internal set; } = isTransparent;

		public void SaveAsJpeg(MemoryStream ms) =>
			Image.SaveAsJpeg(ms, new JpegEncoder() {
				Quality = quality
			});

		public void SaveAsPdfBitmap(MemoryStream ms) {
			BmpEncoder encoder = new BmpEncoder() {
				BitsPerPixel = BmpBitsPerPixel.Pixel32
			};
			Image.Save(ms, encoder);
		}
	}
}
