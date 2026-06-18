using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class HideIfAttribute : Attribute
	{
		public string Test { get; private set; }

		public HideIfAttribute(string test)
		{
			Test = test;
		}
	}
}
