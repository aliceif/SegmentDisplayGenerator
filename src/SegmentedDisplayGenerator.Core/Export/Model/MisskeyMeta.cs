namespace SegmentedDisplayGenerator.Core.Export.Model;

public class MisskeyMeta
{
	public long MetaVersion { get; init;}
	public string? Host {get;init;}
	/// <summary>
	/// ECMAScript Date.prototype.toString
	/// </summary>
	public DateTimeOffset? ExportedAt {get;init;}
	public List<MisskeyEmoji?> Emojis {get;init;} = [];
}
