using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class SeeAlsoAttribute : Attribute
	{
		private readonly string text;

		public string Text => text;

		public SeeAlsoAttribute(string text)
		{
			this.text = text;
		}
	}
}
