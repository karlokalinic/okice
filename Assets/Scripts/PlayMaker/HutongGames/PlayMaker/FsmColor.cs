using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmColor : NamedVariable
	{
		[SerializeField]
		private Color value = Color.black;

		public Color Value
		{
			get
			{
				return value;
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
				this.value = (Color)value;
			}
		}

		public override VariableType VariableType => VariableType.Color;

		public FsmColor()
		{
		}

		public FsmColor(string name)
			: base(name)
		{
		}

		public FsmColor(FsmColor source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmColor(this);
		}

		public override void Clear()
		{
			value = Color.white;
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static implicit operator FsmColor(Color value)
		{
			return new FsmColor(string.Empty)
			{
				value = value
			};
		}
	}
}
