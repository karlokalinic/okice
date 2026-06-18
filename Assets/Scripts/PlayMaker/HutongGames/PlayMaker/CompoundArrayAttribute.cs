using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class CompoundArrayAttribute : Attribute
	{
		private readonly string name;

		private readonly string firstArrayName;

		private readonly string secondArrayName;

		public string Name => name;

		public string FirstArrayName => firstArrayName;

		public string SecondArrayName => secondArrayName;

		public CompoundArrayAttribute(string name, string firstArrayName, string secondArrayName)
		{
			this.name = name;
			this.firstArrayName = firstArrayName;
			this.secondArrayName = secondArrayName;
		}
	}
}
