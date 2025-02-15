namespace SegmentedDisplayGenerator.Core.Export.Model;

public class MisskeyEmojiData
{
	public string? Id { get; init; }
		/// <summary>
	/// ECMAScript Date.prototype.toString
	/// </summary>
	public DateTimeOffset? UpdatedAt { get; init; }
	public string? Name { get; init; }
	public object? Host { get; init; }
	public string? Category { get; init; }
	public string? OriginalUrl { get; init; }
	public string? PublicUrl { get; init; }
	public object? Uri { get; init; }
	public string? Type { get; init; }
	public string?[]? Aliases { get; init; }
}
