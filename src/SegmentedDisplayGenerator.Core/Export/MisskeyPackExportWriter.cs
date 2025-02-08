using System.Formats.Tar;
using System.IO.Compression;
using System.Text.Json;

using SixLabors.ImageSharp;

namespace SegmentedDisplayGenerator.Core.Export;

public class MisskeyPackExportWriter : IExportWriter
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	private bool _disposed = false;
	private readonly FileStream _fileStream;
	private readonly ZipArchive _zipArchive;
	private readonly string _fullPath;
	private readonly string _packageName;
	private readonly MisskeyMeta _metaObject;

	public MisskeyPackExportWriter(string folderPath, string fileName)
	{
		_fullPath = Path.Join(folderPath, fileName + ".zip");
		_packageName = fileName;
		_fileStream = File.Create(_fullPath);
		_zipArchive = new ZipArchive(_fileStream, ZipArchiveMode.Create);
		_metaObject = new MisskeyMeta()
		{
			// Host = "localhost",
			// ExportedAt = DateTimeOffset.Now,
			MetaVersion = 1,
		};
	}

	public void Add(Image image, string name)
	{
		var entry = _zipArchive.CreateEntry(name + ".png");
		using var imageStream = entry.Open();
		image.SaveAsPng(imageStream);
		_metaObject.Emojis.Add(new MisskeyEmoji
		{
			Downloaded = true,
			FileName = name + ".png",
			Emoji = new MisskeyEmojiData
			{
				// Id = null,
				// UpdatedAt = DateTimeOffset.Now,
				Name = name,
				// Host = null,
				Category = _packageName,
				// OriginalUrl = null,
				// PublicUrl = null,
				// Uri = null,
				Type = "image/png",
				Aliases = [],
			}
		});
	}

	private void WriteMetaFile()
	{
		var entry = _zipArchive.CreateEntry("meta.json");
		using var jsonStream = entry.Open();
		JsonSerializer.Serialize(jsonStream, _metaObject, JsonSerializerOptions);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			WriteMetaFile();
			_zipArchive.Dispose();
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
