using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SettingsMenuItemAttribute : Attribute
	{
		public string MenuItem { get; private set; }

		public SettingsMenuItemAttribute(string menuItem)
		{
			MenuItem = menuItem;
		}
	}
}
