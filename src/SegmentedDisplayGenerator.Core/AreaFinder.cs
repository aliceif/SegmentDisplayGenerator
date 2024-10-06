using System;

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
		targetPixels = targetPixels.ToList();
		while (targetPixels.Count != 0)
		{
			var currentList = new List<PixelPosition>(targetPixels.Count / 10);
			foreach (var pixel in targetPixels)
			{
				if (currentList is [] || currentList.Any(p => (p - pixel).MagnitudeManhattan() <= 1))
				{
					currentList.Add(pixel);
				}
			}
			currentList.ForEach(p => targetPixels.Remove(p));
			yield return currentList;
		}
	}
}
