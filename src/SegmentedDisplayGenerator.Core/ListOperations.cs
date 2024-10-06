namespace SegmentedDisplayGenerator.Core;

public static class ListOperations
{
	public static IEnumerable<IEnumerable<T>> CreateSubsets<T>(this ICollection<T> items){
		if (items.Count == 0) return [[]];
		IEnumerable<IEnumerable<T>> subsets = [ Enumerable.Empty<T>(), [items.First()]];
		foreach(var item in items.Skip(1)){
			subsets =  subsets.SelectMany(s => new IEnumerable<T>[]{s, s.Append(item)} );
		}
		return subsets;
	}
}
