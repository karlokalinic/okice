using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmObject : NamedVariable
	{
		[SerializeField]
		private string typeName;

		[SerializeField]
		private UnityEngine.Object value;

		private Type objectType;

		public override Type ObjectType
		{
			get
			{
				if ((object)objectType == null)
				{
					if (string.IsNullOrEmpty(typeName))
					{
						typeName = typeof(UnityEngine.Object).FullName;
					}
					objectType = ReflectionUtils.GetGlobalType(typeName);
				}
				return objectType;
			}
			set
			{
				objectType = value;
				if ((object)objectType == null)
				{
					objectType = typeof(UnityEngine.Object);
				}
				if ((object)this.value != null)
				{
					Type type = this.value.GetType();
					if (!type.IsAssignableFrom(objectType) && !type.IsSubclassOf(objectType))
					{
						this.value = null;
					}
				}
				typeName = objectType.FullName;
			}
		}

		public string TypeName => typeName;

		public UnityEngine.Object Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return ((FsmObject)base.CastVariable).Value;
			}
			set
			{
				this.value = value;
			}
		}

		public override object RawValue
		{
			get
			{
				return value;
			}
			set
			{
				this.value = (UnityEngine.Object)value;
			}
		}

		public override VariableType VariableType => VariableType.Object;

		public FsmObject()
		{
		}

		public FsmObject(string name)
			: base(name)
		{
			typeName = typeof(UnityEngine.Object).FullName;
			objectType = typeof(UnityEngine.Object);
		}

		public FsmObject(FsmObject source)
			: base(source)
		{
			value = source.value;
			typeName = source.typeName;
			objectType = source.objectType;
		}

		public override NamedVariable Clone()
		{
			return new FsmObject(this);
		}

		public override void Clear()
		{
			typeName = null;
			value = null;
		}

		public override string ToString()
		{
			if (!(Value == null))
			{
				return Value.ToString();
			}
			return "None";
		}

		public static implicit operator FsmObject(UnityEngine.Object value)
		{
			return new FsmObject
			{
				value = value
			};
		}

		public override bool TestTypeConstraint(VariableType variableType, Type _objectType = null)
		{
			if (variableType == VariableType.Unknown)
			{
				return true;
			}
			if (base.TestTypeConstraint(variableType, objectType))
			{
				if ((object)_objectType != null && (object)_objectType != typeof(UnityEngine.Object) && (object)ObjectType != _objectType)
				{
					return _objectType.IsAssignableFrom(ObjectType);
				}
				return true;
			}
			return false;
		}
	}
}
