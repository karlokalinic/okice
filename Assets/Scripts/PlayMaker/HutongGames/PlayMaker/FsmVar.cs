using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmVar
	{
		public string variableName;

		public string objectType;

		public bool useVariable;

		[NonSerialized]
		private NamedVariable namedVar;

		[NonSerialized]
		private Type namedVarType;

		private Type enumType;

		private Enum enumValue;

		[NonSerialized]
		private Type _objectType;

		[SerializeField]
		private VariableType type = VariableType.Unknown;

		public float floatValue;

		public int intValue;

		public bool boolValue;

		public string stringValue;

		public Vector4 vector4Value;

		public UnityEngine.Object objectReference;

		public FsmArray arrayValue;

		private Vector2 vector2;

		private Vector3 vector3;

		private Rect rect;

		public NamedVariable NamedVar
		{
			get
			{
				if (namedVar == null)
				{
					InitNamedVar();
				}
				return namedVar;
			}
			set
			{
				if (value != null)
				{
					UpdateType(value);
					namedVar = value;
					namedVarType = value.GetType();
					variableName = namedVar.Name;
					useVariable = value.UseVariable;
				}
				else
				{
					namedVar = null;
					namedVarType = null;
					variableName = null;
				}
				UpdateValue();
			}
		}

		public Type NamedVarType
		{
			get
			{
				if ((object)namedVarType == null && NamedVar != null)
				{
					namedVarType = NamedVar.GetType();
				}
				return namedVarType;
			}
		}

		public Type EnumType
		{
			get
			{
				if ((object)enumType == null)
				{
					InitEnumType();
				}
				return enumType;
			}
			set
			{
				if ((object)enumType != value)
				{
					enumType = value ?? typeof(None);
					ObjectType = enumType;
					intValue = 0;
					enumValue = null;
				}
				if (NamedVar is FsmEnum fsmEnum)
				{
					fsmEnum.EnumType = enumType;
				}
				if (NamedVar is FsmArray fsmArray)
				{
					fsmArray.ObjectType = enumType;
				}
			}
		}

		public Enum EnumValue
		{
			get
			{
				if (enumValue == null)
				{
					enumValue = (Enum)Enum.ToObject(EnumType, (object)intValue);
				}
				return enumValue;
			}
			set
			{
				if (value != null)
				{
					EnumType = value.GetType();
					enumValue = value;
					intValue = Convert.ToInt32(value);
				}
				else
				{
					enumValue = (Enum)Activator.CreateInstance(EnumType);
				}
			}
		}

		public Type ObjectType
		{
			get
			{
				if ((object)_objectType == null)
				{
					_objectType = ReflectionUtils.GetGlobalType(objectType);
				}
				if ((object)_objectType == null)
				{
					_objectType = typeof(UnityEngine.Object);
					objectType = _objectType.FullName;
				}
				return _objectType;
			}
			set
			{
				_objectType = value;
				if ((object)_objectType != null)
				{
					objectType = _objectType.FullName;
				}
				else
				{
					_objectType = typeof(UnityEngine.Object);
					objectType = _objectType.FullName;
				}
				if (namedVar != null)
				{
					NamedVar.ObjectType = _objectType;
				}
			}
		}

		public VariableType Type
		{
			get
			{
				return type;
			}
			set
			{
				if (value != type)
				{
					type = value;
					InitNamedVar();
				}
			}
		}

		public Type RealType => type switch
		{
			VariableType.Float => typeof(float), 
			VariableType.Int => typeof(int), 
			VariableType.Bool => typeof(bool), 
			VariableType.GameObject => typeof(GameObject), 
			VariableType.String => typeof(string), 
			VariableType.Vector2 => typeof(Vector2), 
			VariableType.Vector3 => typeof(Vector3), 
			VariableType.Color => typeof(Color), 
			VariableType.Rect => typeof(Rect), 
			VariableType.Material => typeof(Material), 
			VariableType.Texture => typeof(Texture), 
			VariableType.Quaternion => typeof(Quaternion), 
			VariableType.Object => ObjectType, 
			VariableType.Enum => EnumType, 
			VariableType.Array => arrayValue.RealType(), 
			VariableType.Unknown => null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};

		public bool IsNone
		{
			get
			{
				if (useVariable)
				{
					return string.IsNullOrEmpty(variableName);
				}
				return false;
			}
		}

		public Vector2 vector2Value
		{
			get
			{
				vector2.Set(vector4Value.x, vector4Value.y);
				return vector2;
			}
			set
			{
				vector4Value.Set(value.x, value.y, 0f, 0f);
			}
		}

		public Vector3 vector3Value
		{
			get
			{
				vector3.Set(vector4Value.x, vector4Value.y, vector4Value.z);
				return vector3;
			}
			set
			{
				vector4Value.Set(value.x, value.y, value.z, 0f);
			}
		}

		public Color colorValue
		{
			get
			{
				return new Color(vector4Value.x, vector4Value.y, vector4Value.z, vector4Value.w);
			}
			set
			{
				vector4Value.Set(value.r, value.g, value.b, value.a);
			}
		}

		public Rect rectValue
		{
			get
			{
				rect.Set(vector4Value.x, vector4Value.y, vector4Value.z, vector4Value.w);
				return rect;
			}
			set
			{
				vector4Value.Set(value.x, value.y, value.width, value.height);
			}
		}

		public Quaternion quaternionValue
		{
			get
			{
				return new Quaternion(vector4Value.x, vector4Value.y, vector4Value.z, vector4Value.w);
			}
			set
			{
				vector4Value.Set(value.x, value.y, value.z, value.w);
			}
		}

		public GameObject gameObjectValue
		{
			get
			{
				return objectReference as GameObject;
			}
			set
			{
				objectReference = value;
			}
		}

		public Material materialValue
		{
			get
			{
				return objectReference as Material;
			}
			set
			{
				objectReference = value;
			}
		}

		public Texture textureValue
		{
			get
			{
				return objectReference as Texture;
			}
			set
			{
				objectReference = value;
			}
		}

		public FsmVar()
		{
		}

		public FsmVar(Type type)
		{
			this.type = GetVariableType(type);
			if (type.IsEnum)
			{
				EnumType = type;
			}
			else if (type.IsArray)
			{
				Type elementType = type.GetElementType();
				arrayValue = new FsmArray
				{
					ElementType = GetVariableType(elementType)
				};
				if (elementType.IsEnum || typeof(UnityEngine.Object).IsAssignableFrom(elementType))
				{
					arrayValue.ObjectType = elementType;
				}
			}
			else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				ObjectType = type;
			}
		}

		public FsmVar(FsmVar source)
		{
			variableName = source.variableName;
			useVariable = source.useVariable;
			type = source.type;
			GetValueFrom(source.NamedVar);
		}

		public FsmVar(INamedVariable variable)
		{
			type = variable.VariableType;
			ObjectType = variable.ObjectType;
			variableName = variable.Name;
			GetValueFrom(variable);
		}

		public void Init(NamedVariable variable)
		{
			if (variable != null)
			{
				type = variable.VariableType;
				variableName = variable.Name;
			}
			else
			{
				variableName = "";
			}
			NamedVar = variable;
		}

		private void UpdateType(INamedVariable sourceVar)
		{
			if (sourceVar == null)
			{
				Type = VariableType.Unknown;
				return;
			}
			Type = sourceVar.VariableType;
			ObjectType = sourceVar.ObjectType;
		}

		private void InitNamedVar()
		{
			switch (type)
			{
			case VariableType.Float:
				namedVar = new FsmFloat(variableName)
				{
					Value = floatValue
				};
				break;
			case VariableType.Int:
				namedVar = new FsmInt(variableName)
				{
					Value = intValue
				};
				break;
			case VariableType.Bool:
				namedVar = new FsmBool(variableName)
				{
					Value = boolValue
				};
				break;
			case VariableType.GameObject:
				namedVar = new FsmGameObject(variableName)
				{
					Value = gameObjectValue
				};
				break;
			case VariableType.String:
				namedVar = new FsmString(variableName)
				{
					Value = stringValue
				};
				break;
			case VariableType.Vector2:
				namedVar = new FsmVector2(variableName)
				{
					Value = vector2Value
				};
				break;
			case VariableType.Vector3:
				namedVar = new FsmVector3(variableName)
				{
					Value = vector3Value
				};
				break;
			case VariableType.Color:
				namedVar = new FsmColor(variableName)
				{
					Value = colorValue
				};
				break;
			case VariableType.Rect:
				namedVar = new FsmRect(variableName)
				{
					Value = rectValue
				};
				break;
			case VariableType.Material:
				namedVar = new FsmMaterial(variableName)
				{
					Value = materialValue
				};
				break;
			case VariableType.Texture:
				namedVar = new FsmTexture(variableName)
				{
					Value = textureValue
				};
				break;
			case VariableType.Quaternion:
				namedVar = new FsmQuaternion(variableName)
				{
					Value = quaternionValue
				};
				break;
			case VariableType.Object:
				namedVar = new FsmObject(variableName)
				{
					ObjectType = ObjectType,
					Value = objectReference
				};
				break;
			case VariableType.Enum:
				namedVar = new FsmEnum(variableName)
				{
					EnumType = EnumType,
					Value = EnumValue
				};
				break;
			case VariableType.Array:
			{
				FsmArray fsmArray = new FsmArray(variableName)
				{
					ElementType = arrayValue.ElementType,
					ObjectType = arrayValue.ObjectType
				};
				fsmArray.CopyValues(arrayValue);
				fsmArray.SaveChanges();
				namedVar = fsmArray;
				break;
			}
			case VariableType.Unknown:
				namedVar = null;
				namedVarType = null;
				return;
			default:
				throw new ArgumentOutOfRangeException("Type");
			}
			if (namedVar != null)
			{
				namedVarType = namedVar.GetType();
				namedVar.UseVariable = useVariable;
			}
		}

		private void InitEnumType()
		{
			enumType = ReflectionUtils.GetGlobalType(objectType);
			if ((object)enumType == null || enumType.IsAbstract || !enumType.IsEnum)
			{
				enumType = typeof(None);
				objectType = enumType.FullName;
			}
		}

		public object GetValue()
		{
			if (namedVar == null)
			{
				InitNamedVar();
			}
			return type switch
			{
				VariableType.Float => floatValue, 
				VariableType.Int => intValue, 
				VariableType.Bool => boolValue, 
				VariableType.GameObject => gameObjectValue, 
				VariableType.String => stringValue, 
				VariableType.Vector2 => vector2Value, 
				VariableType.Vector3 => vector3Value, 
				VariableType.Color => colorValue, 
				VariableType.Rect => rectValue, 
				VariableType.Material => materialValue, 
				VariableType.Texture => textureValue, 
				VariableType.Quaternion => quaternionValue, 
				VariableType.Object => objectReference, 
				VariableType.Enum => enumValue, 
				VariableType.Array => arrayValue.Values, 
				VariableType.Unknown => null, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public void GetValueFrom(INamedVariable variable)
		{
			if (variable != null)
			{
				switch (type)
				{
				case VariableType.Float:
					floatValue = ((variable.VariableType == VariableType.Float) ? ((FsmFloat)variable).Value : ((NamedVariable)variable).ToFloat());
					break;
				case VariableType.Int:
					intValue = ((variable.VariableType == VariableType.Int) ? ((FsmInt)variable).Value : ((NamedVariable)variable).ToInt());
					break;
				case VariableType.Bool:
					boolValue = ((variable.VariableType == VariableType.Bool) ? ((FsmBool)variable).Value : (((NamedVariable)variable).ToInt() > 0));
					break;
				case VariableType.GameObject:
					objectReference = ((variable.VariableType == VariableType.GameObject) ? ((FsmGameObject)variable).Value : (variable.RawValue as GameObject));
					break;
				case VariableType.String:
					stringValue = ((variable.VariableType == VariableType.String) ? ((FsmString)variable).Value : ((NamedVariable)variable).ToString());
					break;
				case VariableType.Vector2:
					vector2Value = ((FsmVector2)variable).Value;
					break;
				case VariableType.Vector3:
					vector3Value = ((variable.VariableType == VariableType.Vector3) ? ((FsmVector3)variable).Value : ((Vector3)(Vector2)variable.RawValue));
					break;
				case VariableType.Color:
					colorValue = ((FsmColor)variable).Value;
					break;
				case VariableType.Rect:
					rectValue = ((FsmRect)variable).Value;
					break;
				case VariableType.Material:
					objectReference = ((FsmMaterial)variable).Value;
					break;
				case VariableType.Texture:
					objectReference = ((FsmTexture)variable).Value;
					break;
				case VariableType.Quaternion:
					quaternionValue = ((FsmQuaternion)variable).Value;
					break;
				case VariableType.Object:
					objectReference = ((FsmObject)variable).Value;
					break;
				case VariableType.Enum:
					EnumValue = ((FsmEnum)variable).Value;
					break;
				case VariableType.Array:
					arrayValue = new FsmArray((FsmArray)variable);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case VariableType.Unknown:
					break;
				}
			}
		}

		public void UpdateValue()
		{
			GetValueFrom(NamedVar);
		}

		public void ApplyValueTo(INamedVariable targetVariable)
		{
			if (targetVariable != null)
			{
				switch (targetVariable.VariableType)
				{
				case VariableType.Float:
					((FsmFloat)targetVariable).Value = floatValue;
					break;
				case VariableType.Int:
					((FsmInt)targetVariable).Value = intValue;
					break;
				case VariableType.Bool:
					((FsmBool)targetVariable).Value = boolValue;
					break;
				case VariableType.GameObject:
					((FsmGameObject)targetVariable).Value = objectReference as GameObject;
					break;
				case VariableType.String:
					((FsmString)targetVariable).Value = stringValue;
					break;
				case VariableType.Vector2:
					((FsmVector2)targetVariable).Value = vector2Value;
					break;
				case VariableType.Vector3:
					((FsmVector3)targetVariable).Value = vector3Value;
					break;
				case VariableType.Color:
					((FsmColor)targetVariable).Value = colorValue;
					break;
				case VariableType.Rect:
					((FsmRect)targetVariable).Value = rectValue;
					break;
				case VariableType.Material:
					((FsmMaterial)targetVariable).Value = objectReference as Material;
					break;
				case VariableType.Texture:
					((FsmTexture)targetVariable).Value = objectReference as Texture;
					break;
				case VariableType.Quaternion:
					((FsmQuaternion)targetVariable).Value = quaternionValue;
					break;
				case VariableType.Object:
					((FsmObject)targetVariable).Value = objectReference;
					break;
				case VariableType.Enum:
					((FsmEnum)targetVariable).Value = EnumValue;
					break;
				case VariableType.Array:
					((FsmArray)targetVariable).CopyValues(arrayValue);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case VariableType.Unknown:
					break;
				}
			}
		}

		public string DebugString()
		{
			if (string.IsNullOrEmpty(variableName))
			{
				return "None";
			}
			return variableName + ": " + NamedVar;
		}

		public override string ToString()
		{
			if (NamedVar != null)
			{
				return NamedVar.ToString();
			}
			return "None";
		}

		public void SetValue(object value)
		{
			switch (type)
			{
			case VariableType.Float:
				floatValue = ((value != null) ? ((float)value) : 0f);
				break;
			case VariableType.Int:
				intValue = ((value != null) ? ((int)value) : 0);
				break;
			case VariableType.Bool:
				boolValue = value != null && (bool)value;
				break;
			case VariableType.GameObject:
				gameObjectValue = value as GameObject;
				break;
			case VariableType.String:
				stringValue = value as string;
				break;
			case VariableType.Vector2:
				vector2Value = ((value != null) ? ((Vector2)value) : Vector2.zero);
				break;
			case VariableType.Vector3:
				vector3Value = ((value != null) ? ((Vector3)value) : Vector3.zero);
				break;
			case VariableType.Color:
				colorValue = ((value != null) ? ((Color)value) : Color.white);
				break;
			case VariableType.Rect:
				rectValue = ((value != null) ? ((Rect)value) : default(Rect));
				break;
			case VariableType.Material:
				materialValue = value as Material;
				break;
			case VariableType.Texture:
				textureValue = value as Texture;
				break;
			case VariableType.Quaternion:
				quaternionValue = ((value != null) ? ((Quaternion)value) : Quaternion.identity);
				break;
			case VariableType.Object:
				objectReference = value as UnityEngine.Object;
				break;
			case VariableType.Enum:
				EnumValue = value as Enum;
				break;
			case VariableType.Array:
				if (value is Array array)
				{
					object[] array2 = new object[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = array.GetValue(i);
					}
					arrayValue.Values = array2;
					arrayValue.SaveChanges();
				}
				break;
			case VariableType.Unknown:
				Debug.LogError("Unsupported type: " + ((value != null) ? value.GetType().ToString() : "null"));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			ApplyValueTo(namedVar);
		}

		private void DebugLog()
		{
			Debug.Log("Type: " + type);
			Debug.Log("UseVariable: " + useVariable);
		}

		public static VariableType GetVariableType(Type type)
		{
			if ((object)type == typeof(Material))
			{
				return VariableType.Material;
			}
			if ((object)type == typeof(Texture))
			{
				return VariableType.Texture;
			}
			if ((object)type == typeof(float))
			{
				return VariableType.Float;
			}
			if ((object)type == typeof(int))
			{
				return VariableType.Int;
			}
			if ((object)type == typeof(bool))
			{
				return VariableType.Bool;
			}
			if ((object)type == typeof(string))
			{
				return VariableType.String;
			}
			if ((object)type == typeof(GameObject))
			{
				return VariableType.GameObject;
			}
			if ((object)type == typeof(Vector2))
			{
				return VariableType.Vector2;
			}
			if ((object)type == typeof(Vector3))
			{
				return VariableType.Vector3;
			}
			if ((object)type == typeof(Rect))
			{
				return VariableType.Rect;
			}
			if ((object)type == typeof(Quaternion))
			{
				return VariableType.Quaternion;
			}
			if ((object)type == typeof(Color))
			{
				return VariableType.Color;
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return VariableType.Object;
			}
			if (type.IsEnum)
			{
				return VariableType.Enum;
			}
			if (type.IsArray)
			{
				return VariableType.Array;
			}
			return VariableType.Unknown;
		}
	}
}
