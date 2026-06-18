using System;

namespace HutongGames.PlayMaker
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class ObjectTypeAttribute : Attribute
	{
		private readonly Type objectType;

		public Type ObjectType => objectType;

		public ObjectTypeAttribute(Type objectType)
		{
			this.objectType = objectType;
		}
	}
}
