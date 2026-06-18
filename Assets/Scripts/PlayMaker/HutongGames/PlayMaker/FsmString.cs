using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmString : NamedVariable
	{
		[SerializeField]
		private string value = "";

		public string Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return base.CastVariable.ToString();
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
				this.value = (string)value;
			}
		}

		public override VariableType VariableType => VariableType.String;

		public FsmString()
		{
		}

		public FsmString(string name)
			: base(name)
		{
		}

		public FsmString(FsmString source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmString(this);
		}

		public override string ToString()
		{
			return Value;
		}

		public override int ToInt()
		{
			float.TryParse(value, out var result);
			return (int)result;
		}

		public override void Clear()
		{
			value = "";
		}

		public override float ToFloat()
		{
			float.TryParse(value, out var result);
			return result;
		}

		public static implicit operator FsmString(string value)
		{
			return new FsmString(string.Empty)
			{
				value = value
			};
		}

		public static bool IsNullOrEmpty(FsmString fsmString)
		{
			if (fsmString == null)
			{
				return true;
			}
			if (fsmString.IsNone)
			{
				return true;
			}
			return string.IsNullOrEmpty(fsmString.value);
		}
	}
}
