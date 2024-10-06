namespace SegmentedDisplayGenerator.Core;


public record PixelPosition(int X, int Y) : IComparable<PixelPosition>
{
	public int CompareTo(PixelPosition? other)
	{
		var xcomp = X.CompareTo(other?.X);

		if (xcomp != 0)
		{
			return xcomp;
		}

		return Y.CompareTo(other?.Y);
	}

	public static PixelPosition operator +(PixelPosition a, PixelPosition b)
	{
		return new(a.X + b.X, a.Y + b.Y);
	}

	public static PixelPosition operator -(PixelPosition a, PixelPosition b)
	{
		return new(a.X - b.X, a.Y - b.Y);
	}

	public static PixelPosition operator -(PixelPosition a)
	{
		return new(-a.X, -a.Y);
	}

	public int MagnitudeManhattan()
	{
		return Math.Abs(X) + Math.Abs(Y);
	}

	public int MagnitudeGeometric()
	{
		return (int)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
	}
}
