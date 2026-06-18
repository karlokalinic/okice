using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class PreviewFieldAttribute : Attribute
	{
		public string MethodName { get; private set; }

		public PreviewFieldAttribute(string methodName)
		{
			MethodName = methodName;
		}
	}
}
