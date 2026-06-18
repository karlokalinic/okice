using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers
{
	public static class ListExtensions
	{
		public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
		{
			if (@this.Count == 0)
			{
				@this.Add(item);
				return;
			}
			if (@this[@this.Count - 1].CompareTo(item) <= 0)
			{
				@this.Add(item);
				return;
			}
			if (@this[0].CompareTo(item) >= 0)
			{
				@this.Insert(0, item);
				return;
			}
			int num = @this.BinarySearch(item);
			if (num < 0)
			{
				num = ~num;
			}
			@this.Insert(num, item);
		}

		public static void Shuffle<T>(this List<T> @this)
		{
			if (@this != null && @this.Count >= 2)
			{
				for (int i = 0; i < @this.Count - 1; i++)
				{
					int index = UnityEngine.Random.Range(i, @this.Count);
					T value = @this[i];
					@this[i] = @this[index];
					@this[index] = value;
				}
			}
		}

		public static T GetRandomElement<T>(this List<T> @this)
		{
			if (@this == null)
			{
				return default(T);
			}
			return @this[UnityEngine.Random.Range(0, @this.Count)];
		}
	}
}
