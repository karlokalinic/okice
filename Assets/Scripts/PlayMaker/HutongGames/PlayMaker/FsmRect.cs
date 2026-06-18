using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmRect : NamedVariable
	{
		[SerializeField]
		private Rect value;

		public Rect Value
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
				this.value = (Rect)value;
			}
		}

		public override VariableType VariableType => VariableType.Rect;

		public FsmRect()
		{
		}

		public FsmRect(string name)
			: base(name)
		{
		}

		public FsmRect(FsmRect source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmRect(this);
		}

		public override void Clear()
		{
			value = default(Rect);
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static implicit operator FsmRect(Rect value)
		{
			return new FsmRect(string.Empty)
			{
				value = value
			};
		}
	}
}
