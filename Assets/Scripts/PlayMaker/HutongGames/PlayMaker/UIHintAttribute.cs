using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class UIHintAttribute : Attribute
	{
		private readonly UIHint hint;

		public UIHint Hint => hint;

		public UIHintAttribute(UIHint hint)
		{
			this.hint = hint;
		}
	}
}
