using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SettingsMenuItemStateAttribute : Attribute
	{
		public string MenuItem { get; private set; }

		public SettingsMenuItemStateAttribute(string menuItem)
		{
			MenuItem = menuItem;
		}
	}
}
