using System;
using HutongGames.Utility;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class TooltipAttribute : Attribute
	{
		private readonly string text;

		private readonly string codedText;

		public string Text => text;

		public string CodedText => codedText;

		public TooltipAttribute(string text)
		{
			codedText = StringUtils.Escape(text);
			this.text = StringUtils.StripHtmlAndMarkdown(text.Replace("</li>", "\n"));
		}
	}
}
