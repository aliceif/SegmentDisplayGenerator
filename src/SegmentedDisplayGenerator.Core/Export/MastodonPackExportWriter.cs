using System.Formats.Tar;
using System.IO.Compression;

using SixLabors.ImageSharp;

namespace SegmentedDisplayGenerator.Core.Export;

public class MastodonPackExportWriter : IExportWriter
{
	private bool _disposed = false;
	private readonly FileStream _fileStream;
	private readonly GZipStream _gZipStream;
	private readonly TarWriter _tarWriter;
	private readonly string _fullPath;

	public MastodonPackExportWriter(string folderPath, string fileName)
	{
		_fullPath = Path.Join(folderPath, fileName + ".tar.gz");
		_fileStream = File.Create(_fullPath);
		_gZipStream = new GZipStream(_fileStream, CompressionMode.Compress);
		_tarWriter = new TarWriter(_gZipStream);
	}

	public void Add(Image image, string name)
	{
		var entry = new PaxTarEntry(TarEntryType.RegularFile, name + ".png");
		using var imageStream = new MemoryStream();
		image.SaveAsPng(imageStream);
		imageStream.Position = 0;
		entry.DataStream = imageStream;
		_tarWriter.WriteEntry(entry);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			_tarWriter.Dispose();
			_gZipStream.Dispose();
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
