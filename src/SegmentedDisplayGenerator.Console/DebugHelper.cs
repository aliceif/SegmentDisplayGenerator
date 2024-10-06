using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SegmentedDisplayGenerator.Console;

public static class DebugHelper
{
	public static void WriteToConsoleAscii(Image<Rgb24> image)
	{

		for (int y = 0; y < image.Height; ++y)
		{
			for (int x = 0; x < image.Width; ++x)
			{
				var pixel = image[x, y];
				System.Console.Write(pixel == new Rgb24(0x00, 0x00, 0x00)
				? '#'
				: pixel == new Rgb24(0xff, 0xff, 0xff)
				? ' '
				: '!');
			}
			System.Console.WriteLine();
		}

	}
}
