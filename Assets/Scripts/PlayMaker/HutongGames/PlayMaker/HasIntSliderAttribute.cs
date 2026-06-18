using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class HasIntSliderAttribute : Attribute
	{
		private readonly int minValue;

		private readonly int maxValue;

		public int MinValue => minValue;

		public int MaxValue => maxValue;

		public HasIntSliderAttribute(int minValue, int maxValue)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
		}
	}
}
