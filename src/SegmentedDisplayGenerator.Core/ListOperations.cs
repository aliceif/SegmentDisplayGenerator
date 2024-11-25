namespace SegmentedDisplayGenerator.Core;

public record class TaggedSubset<T>(string Tag, IEnumerable<T> Subset);

public static class ListOperations
{
	public static IEnumerable<TaggedSubset<T>> CreateSubsets<T>(this ICollection<T> items)
	{
		if (items.Count == 0) return [new TaggedSubset<T>("", [])];
		IEnumerable<TaggedSubset<T>> subsets = [new TaggedSubset<T>("0", []), new TaggedSubset<T>("1", [items.First()])];
		foreach (var item in items.Skip(1))
		{
			subsets = subsets.SelectMany(s => new TaggedSubset<T>[] { s with { Tag = s.Tag + "0" }, s with { Tag = s.Tag + "1", Subset = s.Subset.Append(item) } });
		}
		return subsets;
	}
}
