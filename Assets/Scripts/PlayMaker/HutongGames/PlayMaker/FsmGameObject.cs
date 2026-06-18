using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmGameObject : NamedVariable
	{
		[SerializeField]
		private GameObject value;

		public GameObject Value
		{
			get
			{
				if (base.CastVariable == null)
				{
					return value;
				}
				return base.CastVariable.RawValue as GameObject;
			}
			set
			{
				if ((object)value != this.value)
				{
					this.value = value;
					if (this.OnChange != null)
					{
						this.OnChange();
					}
				}
			}
		}

		public override Type ObjectType => typeof(GameObject);

		public override object RawValue
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value as GameObject;
			}
		}

		public override VariableType VariableType => VariableType.GameObject;

		public event Action OnChange;

		public override void SafeAssign(object val)
		{
			value = val as GameObject;
		}

		public FsmGameObject()
		{
		}

		public FsmGameObject(string name)
			: base(name)
		{
		}

		public FsmGameObject(FsmGameObject source)
			: base(source)
		{
			if (source != null)
			{
				value = source.value;
			}
		}

		public override NamedVariable Clone()
		{
			return new FsmGameObject(this);
		}

		public override void Clear()
		{
			value = null;
		}

		public override string ToString()
		{
			if (!(Value == null))
			{
				return Value.name;
			}
			return "None";
		}

		public static implicit operator FsmGameObject(GameObject value)
		{
			return new FsmGameObject(string.Empty)
			{
				value = value
			};
		}
	}
}
