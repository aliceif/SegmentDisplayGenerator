using SixLabors.ImageSharp;

namespace SegmentedDisplayGenerator.Core.Export;

public interface IExportWriter : IDisposable
{
	public void Add(Image image, string name);
}
