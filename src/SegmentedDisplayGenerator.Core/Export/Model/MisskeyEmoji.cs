namespace SegmentedDisplayGenerator.Core.Export.Model;

public class MisskeyEmoji
{
    public bool Downloaded { get; init; }
    public required string FileName { get; init; }
    public required MisskeyEmojiData Emoji { get; init; }
}
