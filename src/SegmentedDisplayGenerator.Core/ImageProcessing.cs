using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SegmentedDisplayGenerator.Core;

public static class ImageProcessing
{
	public static IEnumerable<PixelPosition> ParsePixels(Image<Rgb24> image)
	{
		for (int x = 0; x < image.Width; ++x)
		{
			for (int y = 0; y < image.Height; ++y)
			{
				if (image[x, y] == new Rgb24(0xff, 0xff, 0xff))
				{
					yield return new PixelPosition(x, y);
				}
			}
		}
	}

	public static Image<Rgb24> CreateDyedImage(Image<Rgb24> image, IEnumerable<IList<PixelPosition>> areas, Color color)
	{
		var pixelColor = color.ToPixel<Rgb24>();
		var output = image.Clone();
		foreach (var pixel in areas.SelectMany(x => x))
		{
			output[pixel.X, pixel.Y] = pixelColor;
		}
		return output;
	}
}
