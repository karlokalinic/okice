using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmVector3 : NamedVariable
	{
		[SerializeField]
		private Vector3 value;

		public Vector3 Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return (Vector2)base.CastVariable.RawValue;
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
				this.value = (Vector3)value;
			}
		}

		public override VariableType VariableType => VariableType.Vector3;

		public FsmVector3()
		{
		}

		public FsmVector3(string name)
			: base(name)
		{
		}

		public FsmVector3(FsmVector3 source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmVector3(this);
		}

		public override void Clear()
		{
			value = default(Vector3);
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static implicit operator FsmVector3(Vector3 value)
		{
			return new FsmVector3(string.Empty)
			{
				value = value
			};
		}
	}
}
