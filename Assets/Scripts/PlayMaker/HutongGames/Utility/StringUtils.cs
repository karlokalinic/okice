using System;
using System.Text;
using System.Text.RegularExpressions;

namespace HutongGames.Utility
{
	public static class StringUtils
	{
		[ThreadStatic]
		private static StringBuilder escapeBuilder;

		public static bool forceAscii;

		internal static StringBuilder EscapeBuilder => escapeBuilder ?? (escapeBuilder = new StringBuilder());

		public static string IncrementStringCounter(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return "1";
			}
			string text = s.Split(' ')[^1];
			string text2 = s.Substring(0, s.Length - text.Length);
			if (int.TryParse(text, out var result))
			{
				return text2 + (result + 1).ToString(new string('0', text.Length));
			}
			return s + " 2";
		}

		public static string StripHtml(string input)
		{
			return Regex.Replace(input, "<.*?>", string.Empty);
		}

		public static string StripMarkdown(string input)
		{
			return input.Replace("{{", "").Replace("}}", "");
		}

		public static string StripHtmlAndMarkdown(string input)
		{
			return StripMarkdown(StripHtml(input));
		}

		public static string Escape(string aText)
		{
			StringBuilder stringBuilder = EscapeBuilder;
			stringBuilder.Length = 0;
			if (stringBuilder.Capacity < aText.Length + aText.Length / 10)
			{
				stringBuilder.Capacity = aText.Length + aText.Length / 10;
			}
			foreach (char c in aText)
			{
				switch (c)
				{
				case '\\':
					stringBuilder.Append("\\\\");
					continue;
				case '"':
					stringBuilder.Append("\\\"");
					continue;
				case '\n':
					stringBuilder.Append("\\n");
					continue;
				case '\r':
					stringBuilder.Append("\\r");
					continue;
				case '\t':
					stringBuilder.Append("\\t");
					continue;
				case '\b':
					stringBuilder.Append("\\b");
					continue;
				case '\f':
					stringBuilder.Append("\\f");
					continue;
				}
				if (c < ' ' || (forceAscii && c > '\u007f'))
				{
					ushort num = c;
					stringBuilder.Append("\\u").Append(num.ToString("X4"));
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			string result = stringBuilder.ToString();
			stringBuilder.Length = 0;
			return result;
		}
	}
}
