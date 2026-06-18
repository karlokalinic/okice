using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmBool : NamedVariable
	{
		[SerializeField]
		private bool value;

		public bool Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return base.CastVariable.ToInt() > 0;
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
				this.value = (bool)value;
			}
		}

		public override VariableType VariableType => VariableType.Bool;

		public FsmBool()
		{
		}

		public FsmBool(string name)
			: base(name)
		{
		}

		public FsmBool(FsmBool source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmBool(this);
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public override int ToInt()
		{
			return value ? 1 : 0;
		}

		public override void Clear()
		{
			value = false;
		}

		public static implicit operator FsmBool(bool value)
		{
			return new FsmBool(string.Empty)
			{
				value = value
			};
		}
	}
}
