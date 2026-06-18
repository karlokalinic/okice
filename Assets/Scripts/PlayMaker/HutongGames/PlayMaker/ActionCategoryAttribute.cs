using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ActionCategoryAttribute : Attribute
	{
		private readonly string category;

		public string Category => category;

		public ActionCategoryAttribute(string category)
		{
			this.category = category;
		}

		public ActionCategoryAttribute(ActionCategory category)
		{
			this.category = category.ToString();
		}
	}
}
