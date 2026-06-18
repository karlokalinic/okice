using System;

namespace PixelCrushers.DialogueSystem
{
	public static class ConditionPriorityUtility
	{
		public static ConditionPriority StringToConditionPriority(string s)
		{
			if (!Enum.TryParse<ConditionPriority>(s, out var result))
			{
				return ConditionPriority.Normal;
			}
			return result;
		}
	}
}
