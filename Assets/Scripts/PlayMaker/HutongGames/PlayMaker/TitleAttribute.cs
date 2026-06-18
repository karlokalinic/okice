using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class TitleAttribute : Attribute
	{
		private readonly string text;

		public string Text => text;

		public TitleAttribute(string text)
		{
			this.text = text;
		}
	}
}
