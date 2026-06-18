using UnityEngine;

namespace HutongGames.Extensions
{
	public static class RectExtensions
	{
		public static string Debug(this Rect rect)
		{
			return "(" + (int)rect.x + ", " + (int)rect.y + ", " + (int)rect.width + ", " + (int)rect.height + ")";
		}

		public static Rect BottomRight(this Rect rect, float size)
		{
			return new Rect(rect.xMax - size, rect.yMax - size, size, size);
		}

		public static Rect RoundToInt(this Rect rect)
		{
			return new Rect((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
		}

		public static bool IsDifferent(this Rect rect1, Rect rect2)
		{
			if (!Mathf.Approximately(rect1.x, rect2.x))
			{
				return true;
			}
			if (!Mathf.Approximately(rect1.y, rect2.y))
			{
				return true;
			}
			if (!Mathf.Approximately(rect1.width, rect2.width))
			{
				return true;
			}
			if (!Mathf.Approximately(rect1.height, rect2.height))
			{
				return true;
			}
			return false;
		}

		public static bool AreEqual(this Rect rect1, Rect rect2)
		{
			return !rect1.IsDifferent(rect2);
		}

		public static bool Contains(this Rect rect, float x, float y)
		{
			if (x > rect.xMin && x < rect.xMax && y > rect.yMin)
			{
				return y < rect.yMax;
			}
			return false;
		}

		public static bool Contains(this Rect rect1, Rect rect2)
		{
			if (rect1.xMin <= rect2.xMin && rect1.yMin <= rect2.yMin && rect1.xMax >= rect2.xMax)
			{
				return rect1.yMax >= rect2.yMax;
			}
			return false;
		}

		public static bool IntersectsWith(this Rect rect1, Rect rect2)
		{
			if (rect2.xMin <= rect1.xMax && rect2.xMax >= rect1.xMin && rect2.yMin <= rect1.yMax)
			{
				return rect2.yMax >= rect1.yMin;
			}
			return false;
		}

		public static Rect Union(this Rect rect1, Rect rect2)
		{
			return Rect.MinMaxRect(Mathf.Min(rect1.xMin, rect2.xMin), Mathf.Min(rect1.yMin, rect2.yMin), Mathf.Max(rect1.xMax, rect2.xMax), Mathf.Max(rect1.yMax, rect2.yMax));
		}

		public static Rect Move(this Rect rect, float x, float y)
		{
			return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
		}

		public static Rect Move(this Rect rect, Vector2 delta)
		{
			return new Rect(rect.x + delta.x, rect.y + delta.y, rect.width, rect.height);
		}

		public static Rect Scale(this Rect rect, float scale)
		{
			return new Rect(rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale);
		}

		public static Rect ScaleToInt(this Rect rect, float scale)
		{
			return new Rect((int)(rect.x * scale), (int)(rect.y * scale), (int)(rect.width * scale), (int)(rect.height * scale));
		}

		public static Rect MinSize(this Rect rect, float minWidth, float minHeight)
		{
			return new Rect(rect.x, rect.y, Mathf.Max(rect.width, minWidth), Mathf.Max(rect.height, minHeight));
		}

		public static Rect MinSize(this Rect rect, Vector2 minSize)
		{
			return new Rect(rect.x, rect.y, Mathf.Max(rect.width, minSize.x), Mathf.Max(rect.height, minSize.y));
		}

		public static Rect Expand(this Rect rect, float amount)
		{
			return new Rect(rect.x - amount, rect.y - amount, rect.width + amount * 2f, rect.height + amount * 2f);
		}

		public static Rect ExpandToFit(this Rect rect, Rect rect2)
		{
			float num = Mathf.Min(rect.x, rect2.x);
			float num2 = Mathf.Min(rect.y, rect2.y);
			float width = Mathf.Max(rect.xMax - num, rect2.xMax - num);
			float height = Mathf.Max(rect.yMax - num2, rect2.yMax - num2);
			return new Rect(num, num2, width, height);
		}

		public static Rect FitPoints(this Rect rect, params Vector3[] points)
		{
			if (points == null || points.Length == 0)
			{
				return default(Rect);
			}
			float num = float.PositiveInfinity;
			float num2 = float.NegativeInfinity;
			float num3 = float.PositiveInfinity;
			float num4 = float.NegativeInfinity;
			for (int i = 0; i < points.Length; i++)
			{
				Vector3 vector = points[i];
				num = Mathf.Min(num, vector.x);
				num2 = Mathf.Max(num2, vector.x);
				num3 = Mathf.Min(num3, vector.y);
				num4 = Mathf.Max(num4, vector.y);
			}
			return new Rect(num, num3, num2 - num, num4 - num3);
		}

		public static Vector2 TopLeft(this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMin);
		}

		public static Vector2 Center(this Rect rect)
		{
			return new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}

		public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}

		public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale.x;
			result.xMax *= scale.x;
			result.yMin *= scale.y;
			result.yMax *= scale.y;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}
	}
}
