using System;
using System.Globalization;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmEnum : NamedVariable
	{
		[SerializeField]
		private string enumName;

		[SerializeField]
		private int intValue;

		private Enum value;

		private int parsedIntValue = -1;

		private Type enumType;

		public override object RawValue
		{
			get
			{
				return Value;
			}
			set
			{
				Value = (Enum)value;
			}
		}

		public Type EnumType
		{
			get
			{
				if ((object)enumType == null || enumType.IsAbstract || !enumType.IsEnum)
				{
					InitEnumType();
				}
				return enumType;
			}
			set
			{
				if ((object)enumType == null || enumType.IsAbstract || !enumType.IsEnum)
				{
					InitEnumType();
				}
				if ((object)enumType != value)
				{
					enumType = value ?? typeof(None);
					enumName = enumType.FullName;
					InitEnumType();
					Value = (Enum)Activator.CreateInstance(enumType);
				}
			}
		}

		public string EnumName
		{
			get
			{
				return enumName;
			}
			set
			{
				enumName = value;
			}
		}

		public Enum Value
		{
			get
			{
				if (parsedIntValue != intValue)
				{
					value = null;
				}
				if (value == null)
				{
					value = (Enum)Enum.Parse(EnumType, intValue.ToString(CultureInfo.InvariantCulture));
					parsedIntValue = intValue;
				}
				return value;
			}
			set
			{
				this.value = value;
				intValue = Convert.ToInt32(value);
			}
		}

		public override VariableType VariableType => VariableType.Enum;

		public override Type ObjectType
		{
			get
			{
				return EnumType;
			}
			set
			{
				EnumType = value;
			}
		}

		public override void Init()
		{
			InitEnumType();
		}

		private void InitEnumType()
		{
			enumType = ReflectionUtils.GetGlobalType(enumName);
			if ((object)enumType == null || enumType.IsAbstract || !enumType.IsEnum)
			{
				enumType = typeof(None);
				EnumName = enumType.FullName;
			}
		}

		public void ResetValue()
		{
			value = (Enum)Enum.Parse(EnumType, intValue.ToString(CultureInfo.InvariantCulture));
		}

		public FsmEnum()
		{
		}

		public FsmEnum(string name, Type enumType, int intValue)
			: base(name)
		{
			EnumType = enumType;
			Value = (Enum)Enum.Parse(EnumType, intValue.ToString(CultureInfo.InvariantCulture));
		}

		public FsmEnum(string name)
			: base(name)
		{
			enumName = typeof(Enum).FullName;
			enumType = typeof(Enum);
		}

		public FsmEnum(FsmEnum source)
			: base(source)
		{
			EnumType = source.EnumType;
			Value = source.Value;
		}

		public override NamedVariable Clone()
		{
			return new FsmEnum(this);
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public override int ToInt()
		{
			return intValue;
		}

		public override void Clear()
		{
		}

		public override bool TestTypeConstraint(VariableType variableType, Type _enumType = null)
		{
			if (variableType == VariableType.Unknown)
			{
				return true;
			}
			if (base.TestTypeConstraint(variableType, enumType))
			{
				if ((object)enumType != typeof(Enum) && (object)_enumType != EnumType)
				{
					return (object)_enumType == null;
				}
				return true;
			}
			return false;
		}

		public static implicit operator FsmEnum(Enum value)
		{
			return new FsmEnum
			{
				Value = value
			};
		}
	}
}
