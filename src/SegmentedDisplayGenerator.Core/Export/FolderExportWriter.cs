using SixLabors.ImageSharp;

namespace SegmentedDisplayGenerator.Core.Export;

public class FolderExportWriter : IExportWriter
{
	private bool _disposed = false;
	private readonly string _folderPath;

	public FolderExportWriter(string folderPath, string packageName)
	{
		_folderPath = Path.Join(folderPath, packageName);
		Directory.CreateDirectory(_folderPath);
	}

	public void Add(Image image, string name)
	{
		image.SaveAsPng(Path.Join(_folderPath, name + ".png"));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
		}

		_disposed = true;
	}
	public void Dispose()
	{
		// Dispose of unmanaged resources.
		Dispose(true);
		// Suppress finalization.
		GC.SuppressFinalize(this);
	}
}
