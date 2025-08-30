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
		rootCommand.Subcommands.Add(previewCommand);

		var templateOption = new Option<FileInfo?>(name: "--template", "-t") { Recursive = true, Required = true, Description = "the template file for generating the graphics", };
		var outputOption = new Option<DirectoryInfo?>(name: "--output", "-o") { Required = true, Description = "the directory to output the generated graphics in", };
		var packageTypeOption = new Option<PackageType>(name: "--packageType", "-p") { Required = false, Description = "the package type to use for the output", DefaultValueFactory = _ => (PackageType.Folder), };
		var packageNameOption = new Option<string>(name: "--packageName", "-n") { Required = true, Description = "the package name to use for the output" };
		var litOption = new Option<Color?>(name: "--lit-segment-color", "-l")
		{
			Description = "the color to fill lit segments",
			CustomParser = result =>
			{
				if (!result.Tokens.Any()) return Color.Red;
				if (!Color.TryParse(result.Tokens.Single().Value, out var litColor))
				{
					result.AddError("The color was not recgonized. Try using hex notation like in HTML.");
					return Color.Red;
				}

				return litColor;
			}
		};
		var unlitOption = new Option<Color?>(name: "--unlit-segment-color", "-u")
		{
			Description = "the color to fill unlit segments",
			CustomParser = result =>
			{
				if (!result.Tokens.Any())
				{
					return null;
				}

				if (!Color.TryParse(result.Tokens.Single().Value, out var litColor))
				{
					result.AddError("The color was not recognized. Try using hex notation like in HTML.");
					return null;
				}
				else
				{
					return litColor;
				}
			}
		};

		rootCommand.Options.Add(templateOption);
		rootCommand.Options.Add(outputOption);
		rootCommand.Options.Add(packageTypeOption);
		rootCommand.Options.Add(packageNameOption);
		rootCommand.Options.Add(litOption);
		rootCommand.Options.Add(unlitOption);

		rootCommand.SetAction((parseResult, cancellationToken) => Generate(
			parseResult.GetRequiredValue(templateOption),
			parseResult.GetRequiredValue(outputOption),
			parseResult.GetRequiredValue(packageTypeOption),
			parseResult.GetRequiredValue(packageNameOption),
			parseResult.GetValue(litOption),
			parseResult.GetValue(unlitOption),
			cancellationToken));

		previewCommand.SetAction((parseResult, cancellationToken) => Preview(
			parseResult.GetRequiredValue(templateOption),
			cancellationToken));

		var parseResult = rootCommand.Parse(args);
		return await parseResult.InvokeAsync();
	}

	private static async Task Generate(FileInfo? template, DirectoryInfo? output, PackageType packageType, string packageName, Color? lit, Color? unlit, CancellationToken cancellationToken)
	{
		System.Console.WriteLine("Processing...");
		ArgumentNullException.ThrowIfNull(template);
		ArgumentNullException.ThrowIfNull(output);
		ArgumentNullException.ThrowIfNull(packageName);

		lit ??= Color.Red;
		unlit ??= Color.DimGray;
		using var templateImage = await Image.LoadAsync<Rgb24>(template.FullName, cancellationToken);

		var areas = AreaFinder.FindAreas(ImageProcessing.ParsePixels(templateImage).ToArray()).ToArray();

		System.Console.WriteLine($"Areas detected: {areas.Length}");

		using var baseImage = ImageProcessing.CreateDyedImage(templateImage, areas, unlit.Value);

		var areaIndex = 1;
		var subsets = areas.CreateSubsets().ToArray();

		if (subsets.Length > 1)
		{
			output.Create();
		}

		using (IExportWriter exportWriter = CreateExportWriter(packageType, packageName, output))
		{
			foreach (var permutation in subsets)
			{
				System.Console.WriteLine($"Creating picture {areaIndex:000}");
				using var activeImage = ImageProcessing.CreateDyedImage(baseImage, permutation.Subset, lit.Value);
				exportWriter.Add(activeImage, permutation.Tag);
				++areaIndex;
			}
		}

		System.Console.WriteLine("done.");
	}

	private static IExportWriter CreateExportWriter(PackageType packageType, string packageName, DirectoryInfo outputFolder)
	{
		return packageType switch
		{
			PackageType.Folder => new FolderExportWriter(outputFolder.FullName, packageName),
			PackageType.Misskey => new MisskeyPackExportWriter(outputFolder.FullName, packageName),
			PackageType.Mastodon => new MastodonPackExportWriter(outputFolder.FullName, packageName),
			_ => throw new ArgumentOutOfRangeException(nameof(packageType), packageType, "The package type is not supported"),
		};
	}

	private static async Task Preview(FileInfo? template, CancellationToken cancellationToken)
	{
		System.Console.WriteLine("Processing...");
		ArgumentNullException.ThrowIfNull(template);
		using var templateImage = await Image.LoadAsync<Rgb24>(template.FullName, cancellationToken);

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
