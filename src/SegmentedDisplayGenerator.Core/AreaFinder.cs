using System;
using System.Collections.Immutable;

namespace SegmentedDisplayGenerator.Core;

/// <summary>
/// Module for area finding.
/// </summary>
public static class AreaFinder
{
	/// <summary>
	/// Finds all areas (contiguous regions of pixels (horizontal and vertical direct neighbors))
	/// </summary>
	/// <param name="targetPixels">The pixels to find areas in</param>
	/// <returns>The pixels, clustered into areas.</returns>
	public static IEnumerable<IList<PixelPosition>> FindAreas(ICollection<PixelPosition> targetPixels)
	{
		var searchSpace = targetPixels.ToList();
		searchSpace.Sort();

		while (searchSpace.Count != 0)
		{
			var currentList = new SortedSet<PixelPosition> { searchSpace.First() };

			int newlyAddedCount;
			do
			{
				newlyAddedCount = 0;
				foreach (var pixel in currentList.ToArray())
				{
					foreach(var neighbor in PixelPosition.OrthogonalUnitVectors.Select(v => v + pixel))
					if (searchSpace.BinarySearch(neighbor) >= 0)
					{
						if (currentList.Add(neighbor)) newlyAddedCount += 1;
					}
				}
			} while (newlyAddedCount > 0);

			foreach (var pixel in currentList)
				searchSpace.Remove(pixel);

			yield return currentList.ToArray();
		}
	}
}
