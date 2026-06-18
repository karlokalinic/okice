using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmInt : NamedVariable
	{
		[SerializeField]
		private int value;

		public int Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return base.CastVariable.ToInt();
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
				this.value = (int)value;
			}
		}

		public override VariableType VariableType => VariableType.Int;

		public override void SafeAssign(object val)
		{
			if (val is int)
			{
				value = (int)val;
			}
			if (val is float)
			{
				value = Mathf.FloorToInt((float)val);
			}
		}

		public FsmInt()
		{
		}

		public FsmInt(string name)
			: base(name)
		{
		}

		public FsmInt(FsmInt source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmInt(this);
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public override float ToFloat()
		{
			return value;
		}

		public override int ToInt()
		{
			return value;
		}

		public override void Clear()
		{
			value = 0;
		}

		public static implicit operator FsmInt(int value)
		{
			return new FsmInt(string.Empty)
			{
				value = value
			};
		}
	}
}
