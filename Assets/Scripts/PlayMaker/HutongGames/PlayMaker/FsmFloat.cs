using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmFloat : NamedVariable
	{
		[SerializeField]
		private float value;

		public float Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return base.CastVariable.ToFloat();
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
				this.value = (float)value;
			}
		}

		public override VariableType VariableType => VariableType.Float;

		public override void SafeAssign(object val)
		{
			if (val is float)
			{
				value = (float)val;
			}
			if (val is int)
			{
				value = (int)val;
			}
		}

		public FsmFloat()
		{
		}

		public FsmFloat(string name)
			: base(name)
		{
		}

		public FsmFloat(FsmFloat source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmFloat(this);
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public override string DebugString()
		{
			return $"{Value:0.0###########}";
		}

		public override int ToInt()
		{
			return (int)value;
		}

		public override void Clear()
		{
			value = 0f;
		}

		public static implicit operator FsmFloat(float value)
		{
			return new FsmFloat(string.Empty)
			{
				value = value
			};
		}
	}
}
