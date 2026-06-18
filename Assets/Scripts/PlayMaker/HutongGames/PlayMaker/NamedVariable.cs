using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class NamedVariable : INameable, INamedVariable, IComparable
	{
		[SerializeField]
		private bool useVariable;

		[SerializeField]
		private string name;

		[SerializeField]
		[TextArea(0, 10)]
		private string tooltip = "";

		[SerializeField]
		private bool showInInspector;

		[SerializeField]
		private bool networkSync;

		[NonSerialized]
		protected object obj;

		public NamedVariable CastVariable
		{
			get
			{
				return obj as NamedVariable;
			}
			set
			{
				obj = value;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				Debug.LogWarning("Trying to set variable name directly: " + value + "\nNormally you should create a new variable with that name! Otherwise you might overwrite the name of the current variable this points to: " + name + "\nIf you definitely mean to rename the variable (e.g., in an editor tool) use SetName instead.");
			}
		}

		public virtual VariableType VariableType
		{
			get
			{
				throw new Exception("VariableType not implemented: " + GetType().FullName);
			}
		}

		public virtual Type ObjectType
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual VariableType TypeConstraint => VariableType;

		public virtual object RawValue
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string Tooltip
		{
			get
			{
				return tooltip;
			}
			set
			{
				tooltip = value;
			}
		}

		public bool UseVariable
		{
			get
			{
				return useVariable;
			}
			set
			{
				useVariable = value;
			}
		}

		public bool ShowInInspector
		{
			get
			{
				return showInInspector;
			}
			set
			{
				showInInspector = value;
			}
		}

		public bool NetworkSync
		{
			get
			{
				return networkSync;
			}
			set
			{
				networkSync = value;
			}
		}

		public bool IsNone
		{
			get
			{
				if (useVariable)
				{
					return string.IsNullOrEmpty(name);
				}
				return false;
			}
		}

		public bool UsesVariable
		{
			get
			{
				if (useVariable)
				{
					return !string.IsNullOrEmpty(name);
				}
				return false;
			}
		}

		public void SetName(string newName)
		{
			name = newName;
		}

		public static bool IsNullOrNone(NamedVariable variable)
		{
			return variable?.IsNone ?? true;
		}

		public NamedVariable()
		{
			name = "";
			tooltip = "";
		}

		public NamedVariable(string name)
		{
			this.name = name;
			if (!string.IsNullOrEmpty(name))
			{
				useVariable = true;
			}
		}

		public NamedVariable(NamedVariable source)
		{
			if (source != null)
			{
				useVariable = source.useVariable;
				name = source.name;
				showInInspector = source.showInInspector;
				tooltip = source.tooltip;
				networkSync = source.networkSync;
			}
		}

		public virtual void Init()
		{
		}

		public virtual bool TestTypeConstraint(VariableType variableType, Type objectType = null)
		{
			if (variableType == VariableType.Unknown)
			{
				return true;
			}
			return TypeConstraint == variableType;
		}

		public virtual void SafeAssign(object val)
		{
			throw new NotImplementedException();
		}

		public virtual NamedVariable Clone()
		{
			throw new NotImplementedException();
		}

		public NamedVariable Copy()
		{
			Type type = GetType();
			if (type == typeof(FsmMaterial))
			{
				return new FsmMaterial((FsmMaterial)this);
			}
			if (type == typeof(FsmTexture))
			{
				return new FsmTexture((FsmTexture)this);
			}
			if (type == typeof(FsmFloat))
			{
				return new FsmFloat((FsmFloat)this);
			}
			if (type == typeof(FsmInt))
			{
				return new FsmInt((FsmInt)this);
			}
			if (type == typeof(FsmBool))
			{
				return new FsmBool((FsmBool)this);
			}
			if (type == typeof(FsmString))
			{
				return new FsmString((FsmString)this);
			}
			if (type == typeof(FsmGameObject))
			{
				return new FsmGameObject((FsmGameObject)this);
			}
			if (type == typeof(FsmVector2))
			{
				return new FsmVector2((FsmVector2)this);
			}
			if (type == typeof(FsmVector3))
			{
				return new FsmVector3((FsmVector3)this);
			}
			if (type == typeof(FsmRect))
			{
				return new FsmRect((FsmRect)this);
			}
			if (type == typeof(FsmQuaternion))
			{
				return new FsmQuaternion((FsmQuaternion)this);
			}
			if (type == typeof(FsmColor))
			{
				return new FsmColor((FsmColor)this);
			}
			if (type == typeof(FsmArray))
			{
				return new FsmArray((FsmArray)this);
			}
			if (type == typeof(FsmEnum))
			{
				return new FsmEnum((FsmEnum)this);
			}
			if (type == typeof(FsmObject))
			{
				return new FsmObject((FsmObject)this);
			}
			Debug.LogError("Unknown variable type!");
			return null;
		}

		public string GetDisplayName()
		{
			if (string.IsNullOrEmpty(Name))
			{
				return "None";
			}
			return Name;
		}

		public virtual float ToFloat()
		{
			return 0f;
		}

		public virtual int ToInt()
		{
			return 0;
		}

		public virtual string DebugString()
		{
			return ToString();
		}

		public virtual void Clear()
		{
			throw new NotImplementedException();
		}

		public int CompareTo(object target)
		{
			if (!(target is NamedVariable namedVariable))
			{
				return 0;
			}
			return string.CompareOrdinal(name, namedVariable.name);
		}
	}
}
