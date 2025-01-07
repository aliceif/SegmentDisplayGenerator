using System.CommandLine;
using System.Text;

using SegmentedDisplayGenerator.Core;
using SegmentedDisplayGenerator.Core.Export;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
		rootCommand.AddOption(outputOption);
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

		using (FolderExportWriter folderExportWriter = new(output.FullName))
		{
			foreach (var permutation in subsets)
			{
				System.Console.WriteLine($"Creating picture {areaIndex:000}");
				using var activeImage = ImageProcessing.CreateDyedImage(baseImage, permutation.Subset, lit.Value);
				folderExportWriter.Add(activeImage, permutation.Tag);
				++areaIndex;
			}
		}

		System.Console.WriteLine("done.");
	}

	private static async Task Preview(FileInfo? template, DirectoryInfo? output)
	{
		System.Console.WriteLine("Processing...");
		ArgumentNullException.ThrowIfNull(template);
		using var templateImage = await Image.LoadAsync<Rgb24>(template.FullName);

		PixelPosition[] targetPixels = ImageProcessing.ParsePixels(templateImage).ToArray();
		var areas = AreaFinder.FindAreas(targetPixels).ToArray();

		System.Console.WriteLine($"Areas detected: {areas.Length}");

		var plainPreviewBuilder = new StringBuilder(templateImage.Width * templateImage.Height);

		for (int y = 0; y < templateImage.Height; ++y)
		{
			for (int x = 0; x < templateImage.Width; ++x)
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

		var areaPreviewBuilder = new StringBuilder(templateImage.Width * templateImage.Height);

		if (areas.Length <= 36)
		{
			var areaPixels = areas.SelectMany((area, areaNumber) => area.Select(position => (position, areaNumber: areaNumber + 1))).ToDictionary(t => t.position, t => t.areaNumber);

			for (int y = 0; y < templateImage.Height; ++y)
			{
				for (int x = 0; x < templateImage.Width; ++x)
				{

					if (areaPixels.TryGetValue(new PixelPosition(x, y), out var area))
					{
						areaPreviewBuilder.Append(area < 10 ? area.ToString() : area - 10 + 'A');
					}
					else
					{
						areaPreviewBuilder.Append(".");
					}
				}
				areaPreviewBuilder.AppendLine();
			}
		}

		System.Console.WriteLine(areaPreviewBuilder);

		System.Console.WriteLine("done.");
	}
}
