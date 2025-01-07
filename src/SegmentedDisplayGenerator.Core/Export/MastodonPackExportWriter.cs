using System.Formats.Tar;

using SixLabors.ImageSharp;

namespace SegmentedDisplayGenerator.Core.Export;

public class MastodonPackExportWriter : IExportWriter
{
	private bool _disposed = false;
	private readonly FileStream _fileStream;
	private readonly string _fullPath;

	public MastodonPackExportWriter(string folderPath, string fileName)
	{
		_fullPath = Path.Join(folderPath, fileName + ".tar");
		_fileStream = File.Create(_fullPath);
	}

	public void Add(Image image, string name)
	{
		using var tarWriter = new TarWriter(_fileStream, leaveOpen: true);
		var entry = new PaxTarEntry(TarEntryType.RegularFile, name + ".png");
		using var imageStream = new MemoryStream();
		image.SaveAsPng(imageStream);
		imageStream.Position = 0;
		entry.DataStream = imageStream;
		tarWriter.WriteEntry(entry);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			_fileStream.Dispose();
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
