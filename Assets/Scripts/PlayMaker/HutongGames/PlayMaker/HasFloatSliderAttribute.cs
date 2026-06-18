using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class HasFloatSliderAttribute : Attribute
	{
		private readonly float minValue;

		private readonly float maxValue;

		public float MinValue => minValue;

		public float MaxValue => maxValue;

		public HasFloatSliderAttribute(float minValue, float maxValue)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
		}
	}
}
