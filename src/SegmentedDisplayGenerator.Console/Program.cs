using SegmentedDisplayGenerator.Console;
using SegmentedDisplayGenerator.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const string sampleImagePath = @"template.png";
var outFolder = $@".{Path.DirectorySeparatorChar}out{Path.DirectorySeparatorChar}";

var activeSegmentColor = new Rgb24(255, 0, 0);
var inactiveSegmentColor = new Rgb24(0x60, 0x60, 0x60);

Console.WriteLine("Hello, World!");

var image = await Image.LoadAsync<Rgb24>(sampleImagePath);

DebugHelper.WriteToConsoleAscii(image);

var areas = AreaFinder.FindAreas(ImageProcessing.ParsePixels(image).ToArray()).ToArray();

Console.WriteLine($"Areas detected: {areas.Length}");

image = ImageProcessing.CreateDyedImage(image, areas, inactiveSegmentColor);

var areaIndex = 1;
var subsets = ListOperations.CreateSubsets(areas).ToArray();
foreach (var permutation in subsets)
{
	Console.WriteLine($"Creating picture {areaIndex:000}");
	var activedImage = ImageProcessing.CreateDyedImage(image, permutation, activeSegmentColor);
	activedImage.SaveAsPng(Path.Join(outFolder, $"{areaIndex:000}.png"));
	++areaIndex;
}
