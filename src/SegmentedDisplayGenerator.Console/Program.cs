using System.CommandLine;
using System.Text;

using SegmentedDisplayGenerator.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SegmentedDisplayGenerator.Console;

class Program
{
	static async Task<int> Main(string[] args)
	{
		var rootCommand = new RootCommand("Program for generating segment display graphics");

		var previewCommand = new Command(name: "preview", description: "Prints out a preview of the segments");
		rootCommand.AddCommand(previewCommand);

		var templateOption = new Option<FileInfo?>(name: "--template", description: "the template file for generating the graphics");
		templateOption.AddAlias("-t");
		var outputOption = new Option<DirectoryInfo?>(name: "--output", description: "the directory to output the generated graphics in");
		outputOption.AddAlias("-o");
		var litOption = new Option<Color?>(name: "--lit-segment-color", description: "the color to fill lit segments",
		parseArgument: result =>
		{
			if (!result.Tokens.Any()) return Color.Red;
			if (!Color.TryParse(result.Tokens.Single().Value, out var litColor))
			{
				result.ErrorMessage = "The color was not recgonized. Try using hex notation like in HTML.";
				return Color.Red;
			}
			else { return litColor; };
		});
		litOption.AddAlias("-l");
		var unlitOption = new Option<Color?>(name: "--unlit-segment-color", description: "the color to fill unlit segments",
		parseArgument: result =>
		{
			if (!result.Tokens.Any())
			{
				return null;
			}
			if (!Color.TryParse(result.Tokens.Single().Value, out var litColor))
			{
				result.ErrorMessage = "The color was not recognized. Try using hex notation like in HTML.";
				return null;
			}
			else
			{
				return litColor;
			};
		});
		unlitOption.AddAlias("-u");

		rootCommand.AddGlobalOption(templateOption);
		rootCommand.AddGlobalOption(outputOption);
		rootCommand.AddOption(litOption);
		rootCommand.AddOption(unlitOption);

		rootCommand.SetHandler(Generate, templateOption, outputOption, litOption, unlitOption);

		previewCommand.SetHandler(Preview, templateOption, outputOption);

		return await rootCommand.InvokeAsync(args);
	}

	private static async Task Generate(FileInfo? template, DirectoryInfo? output, Color? lit, Color? unlit)
	{
		System.Console.WriteLine("Processing...");
		ArgumentNullException.ThrowIfNull(template);
		ArgumentNullException.ThrowIfNull(output);
		lit ??= Color.Red;
		unlit ??= Color.DimGray;
		using var templateImage = await Image.LoadAsync<Rgb24>(template.FullName);

		var areas = AreaFinder.FindAreas(ImageProcessing.ParsePixels(templateImage).ToArray()).ToArray();

		System.Console.WriteLine($"Areas detected: {areas.Length}");

		using var baseImage = ImageProcessing.CreateDyedImage(templateImage, areas, unlit.Value);

		var areaIndex = 1;
		var subsets = ListOperations.CreateSubsets(areas).ToArray();

		if (subsets.Length > 1)
		{
			output.Create();
		}

		foreach (var permutation in subsets)
		{
			System.Console.WriteLine($"Creating picture {areaIndex:000}");
			using var activeImage = ImageProcessing.CreateDyedImage(baseImage, permutation.Subset, lit.Value);
			activeImage.SaveAsPng(Path.Join(output.FullName, $"{permutation.Tag}.png"));
			++areaIndex;
		}

		System.Console.WriteLine("done.");
	}

	private static async Task Preview(FileInfo? template, DirectoryInfo? output)
	{
		System.Console.WriteLine("Processing...");
		ArgumentNullException.ThrowIfNull(template);
		ArgumentNullException.ThrowIfNull(output);
		var image = await Image.LoadAsync<Rgb24>(template.FullName);

		PixelPosition[] targetPixels = ImageProcessing.ParsePixels(image).ToArray();
		var areas = AreaFinder.FindAreas(targetPixels).ToArray();

		System.Console.WriteLine($"Areas detected: {areas.Length}");

		var plainPreviewBuilder = new StringBuilder(image.Width * image.Height);

		for (int y = 0; y < image.Height; ++y)
		{
			for (int x = 0; x < image.Width; ++x)
			{
				if (targetPixels.Contains(new PixelPosition(x, y)))
				{
					plainPreviewBuilder.Append("#");
				}
				else
				{
					plainPreviewBuilder.Append(".");
				}
			}
			plainPreviewBuilder.AppendLine();
		}

		System.Console.WriteLine(plainPreviewBuilder);

		System.Console.WriteLine("done.");
	}
}
