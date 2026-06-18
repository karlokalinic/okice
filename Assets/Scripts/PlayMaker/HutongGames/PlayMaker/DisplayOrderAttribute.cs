using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DisplayOrderAttribute : Attribute
	{
		private readonly int index;

		public int Index => index;

		public DisplayOrderAttribute(int orderIndex)
		{
			index = orderIndex;
		}
	}
}
