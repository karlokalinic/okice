using System;
using System.Collections.Generic;
using HutongGames.Utility;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmVariables
	{
		private Dictionary<string, NamedVariable> _variableLookup;

		[NonSerialized]
		private NamedVariable[] _allVariables;

		[NonSerialized]
		private List<NamedVariable> _emptyVariables;

		[SerializeField]
		private FsmFloat[] floatVariables;

		[SerializeField]
		private FsmInt[] intVariables;

		[SerializeField]
		private FsmBool[] boolVariables;

		[SerializeField]
		private FsmString[] stringVariables;

		[SerializeField]
		private FsmVector2[] vector2Variables;

		[SerializeField]
		private FsmVector3[] vector3Variables;

		[SerializeField]
		private FsmColor[] colorVariables;

		[SerializeField]
		private FsmRect[] rectVariables;

		[SerializeField]
		private FsmQuaternion[] quaternionVariables;

		[SerializeField]
		private FsmGameObject[] gameObjectVariables;

		[SerializeField]
		private FsmObject[] objectVariables;

		[SerializeField]
		private FsmMaterial[] materialVariables;

		[SerializeField]
		private FsmTexture[] textureVariables;

		[SerializeField]
		private FsmArray[] arrayVariables;

		[SerializeField]
		private FsmEnum[] enumVariables;

		[SerializeField]
		private string[] categories = new string[1] { "" };

		[SerializeField]
		private int[] variableCategoryIDs = new int[0];

		public static PlayMakerGlobals GlobalsComponent => PlayMakerGlobals.Instance;

		public static FsmVariables GlobalVariables => PlayMakerGlobals.Instance.Variables;

		public static bool GlobalVariablesSynced { get; set; }

		private Dictionary<string, NamedVariable> variableLookup
		{
			get
			{
				if (_variableLookup == null)
				{
					Init();
				}
				return _variableLookup;
			}
		}

		private NamedVariable[] allVariables
		{
			get
			{
				if (_allVariables == null)
				{
					Init();
				}
				return _allVariables;
			}
		}

		private List<NamedVariable> emptyVariables
		{
			get
			{
				if (_emptyVariables == null)
				{
					Init();
				}
				return _emptyVariables;
			}
		}

		public string[] Categories
		{
			get
			{
				return categories;
			}
			set
			{
				categories = value;
			}
		}

		public int[] CategoryIDs
		{
			get
			{
				return variableCategoryIDs;
			}
			set
			{
				variableCategoryIDs = value;
			}
		}

		public int Count => allVariables.Length;

		public FsmFloat[] FloatVariables
		{
			get
			{
				return floatVariables ?? Arrays<FsmFloat>.Empty;
			}
			set
			{
				floatVariables = value;
			}
		}

		public FsmInt[] IntVariables
		{
			get
			{
				return intVariables ?? Arrays<FsmInt>.Empty;
			}
			set
			{
				intVariables = value;
			}
		}

		public FsmBool[] BoolVariables
		{
			get
			{
				return boolVariables ?? Arrays<FsmBool>.Empty;
			}
			set
			{
				boolVariables = value;
			}
		}

		public FsmString[] StringVariables
		{
			get
			{
				return stringVariables ?? Arrays<FsmString>.Empty;
			}
			set
			{
				stringVariables = value;
			}
		}

		public FsmVector2[] Vector2Variables
		{
			get
			{
				return vector2Variables ?? Arrays<FsmVector2>.Empty;
			}
			set
			{
				vector2Variables = value;
			}
		}

		public FsmVector3[] Vector3Variables
		{
			get
			{
				return vector3Variables ?? Arrays<FsmVector3>.Empty;
			}
			set
			{
				vector3Variables = value;
			}
		}

		public FsmRect[] RectVariables
		{
			get
			{
				return rectVariables ?? Arrays<FsmRect>.Empty;
			}
			set
			{
				rectVariables = value;
			}
		}

		public FsmQuaternion[] QuaternionVariables
		{
			get
			{
				return quaternionVariables ?? Arrays<FsmQuaternion>.Empty;
			}
			set
			{
				quaternionVariables = value;
			}
		}

		public FsmColor[] ColorVariables
		{
			get
			{
				return colorVariables ?? Arrays<FsmColor>.Empty;
			}
			set
			{
				colorVariables = value;
			}
		}

		public FsmGameObject[] GameObjectVariables
		{
			get
			{
				return gameObjectVariables ?? Arrays<FsmGameObject>.Empty;
			}
			set
			{
				gameObjectVariables = value;
			}
		}

		public FsmArray[] ArrayVariables
		{
			get
			{
				return arrayVariables ?? Arrays<FsmArray>.Empty;
			}
			set
			{
				arrayVariables = value;
			}
		}

		public FsmEnum[] EnumVariables
		{
			get
			{
				return enumVariables ?? Arrays<FsmEnum>.Empty;
			}
			set
			{
				enumVariables = value;
			}
		}

		public FsmObject[] ObjectVariables
		{
			get
			{
				return objectVariables ?? Arrays<FsmObject>.Empty;
			}
			set
			{
				objectVariables = value;
			}
		}

		public FsmMaterial[] MaterialVariables
		{
			get
			{
				return materialVariables ?? Arrays<FsmMaterial>.Empty;
			}
			set
			{
				materialVariables = value;
			}
		}

		public FsmTexture[] TextureVariables
		{
			get
			{
				return textureVariables ?? Arrays<FsmTexture>.Empty;
			}
			set
			{
				textureVariables = value;
			}
		}

		public FsmVariables()
		{
		}

		public FsmVariables(FsmVariables source)
		{
			if (source == null)
			{
				return;
			}
			categories = new string[source.categories.Length];
			Array.Copy(source.categories, categories, source.categories.Length);
			if (source.floatVariables != null)
			{
				floatVariables = new FsmFloat[source.floatVariables.Length];
				for (int i = 0; i < source.floatVariables.Length; i++)
				{
					floatVariables[i] = new FsmFloat(source.floatVariables[i]);
				}
			}
			if (source.intVariables != null)
			{
				intVariables = new FsmInt[source.intVariables.Length];
				for (int j = 0; j < source.intVariables.Length; j++)
				{
					intVariables[j] = new FsmInt(source.intVariables[j]);
				}
			}
			if (source.boolVariables != null)
			{
				boolVariables = new FsmBool[source.boolVariables.Length];
				for (int k = 0; k < source.boolVariables.Length; k++)
				{
					boolVariables[k] = new FsmBool(source.boolVariables[k]);
				}
			}
			if (source.gameObjectVariables != null)
			{
				gameObjectVariables = new FsmGameObject[source.gameObjectVariables.Length];
				for (int l = 0; l < source.gameObjectVariables.Length; l++)
				{
					gameObjectVariables[l] = new FsmGameObject(source.gameObjectVariables[l]);
				}
			}
			if (source.colorVariables != null)
			{
				colorVariables = new FsmColor[source.colorVariables.Length];
				for (int m = 0; m < source.colorVariables.Length; m++)
				{
					colorVariables[m] = new FsmColor(source.colorVariables[m]);
				}
			}
			if (source.vector2Variables != null)
			{
				vector2Variables = new FsmVector2[source.vector2Variables.Length];
				for (int n = 0; n < source.vector2Variables.Length; n++)
				{
					vector2Variables[n] = new FsmVector2(source.vector2Variables[n]);
				}
			}
			if (source.vector3Variables != null)
			{
				vector3Variables = new FsmVector3[source.vector3Variables.Length];
				for (int num = 0; num < source.vector3Variables.Length; num++)
				{
					vector3Variables[num] = new FsmVector3(source.vector3Variables[num]);
				}
			}
			if (source.rectVariables != null)
			{
				rectVariables = new FsmRect[source.rectVariables.Length];
				for (int num2 = 0; num2 < source.rectVariables.Length; num2++)
				{
					rectVariables[num2] = new FsmRect(source.rectVariables[num2]);
				}
			}
			if (source.quaternionVariables != null)
			{
				quaternionVariables = new FsmQuaternion[source.quaternionVariables.Length];
				for (int num3 = 0; num3 < source.quaternionVariables.Length; num3++)
				{
					quaternionVariables[num3] = new FsmQuaternion(source.quaternionVariables[num3]);
				}
			}
			if (source.objectVariables != null)
			{
				objectVariables = new FsmObject[source.objectVariables.Length];
				for (int num4 = 0; num4 < source.objectVariables.Length; num4++)
				{
					objectVariables[num4] = new FsmObject(source.objectVariables[num4]);
				}
			}
			if (source.materialVariables != null)
			{
				materialVariables = new FsmMaterial[source.materialVariables.Length];
				for (int num5 = 0; num5 < source.materialVariables.Length; num5++)
				{
					materialVariables[num5] = new FsmMaterial(source.materialVariables[num5]);
				}
			}
			if (source.textureVariables != null)
			{
				textureVariables = new FsmTexture[source.textureVariables.Length];
				for (int num6 = 0; num6 < source.textureVariables.Length; num6++)
				{
					textureVariables[num6] = new FsmTexture(source.textureVariables[num6]);
				}
			}
			if (source.stringVariables != null)
			{
				stringVariables = new FsmString[source.stringVariables.Length];
				for (int num7 = 0; num7 < source.stringVariables.Length; num7++)
				{
					stringVariables[num7] = new FsmString(source.stringVariables[num7]);
				}
			}
			if (source.arrayVariables != null)
			{
				arrayVariables = new FsmArray[source.arrayVariables.Length];
				for (int num8 = 0; num8 < source.arrayVariables.Length; num8++)
				{
					arrayVariables[num8] = new FsmArray(source.arrayVariables[num8]);
				}
			}
			if (source.enumVariables != null)
			{
				enumVariables = new FsmEnum[source.enumVariables.Length];
				for (int num9 = 0; num9 < source.enumVariables.Length; num9++)
				{
					enumVariables[num9] = new FsmEnum(source.enumVariables[num9]);
				}
			}
			if (source.categories != null)
			{
				categories = new string[source.categories.Length];
				for (int num10 = 0; num10 < source.categories.Length; num10++)
				{
					categories[num10] = source.categories[num10];
				}
			}
			if (source.CategoryIDs != null)
			{
				CategoryIDs = new int[source.CategoryIDs.Length];
				for (int num11 = 0; num11 < source.CategoryIDs.Length; num11++)
				{
					CategoryIDs[num11] = source.CategoryIDs[num11];
				}
			}
		}

		public void Init()
		{
			_emptyVariables = new List<NamedVariable>();
			_variableLookup = new Dictionary<string, NamedVariable>();
			FsmFloat[] array = FloatVariables;
			foreach (FsmFloat v in array)
			{
				AddVariableLookup(v);
			}
			FsmInt[] array2 = IntVariables;
			foreach (FsmInt v2 in array2)
			{
				AddVariableLookup(v2);
			}
			FsmBool[] array3 = BoolVariables;
			foreach (FsmBool v3 in array3)
			{
				AddVariableLookup(v3);
			}
			FsmString[] array4 = StringVariables;
			foreach (FsmString v4 in array4)
			{
				AddVariableLookup(v4);
			}
			FsmVector2[] array5 = Vector2Variables;
			foreach (FsmVector2 v5 in array5)
			{
				AddVariableLookup(v5);
			}
			FsmVector3[] array6 = Vector3Variables;
			foreach (FsmVector3 v6 in array6)
			{
				AddVariableLookup(v6);
			}
			FsmRect[] array7 = RectVariables;
			foreach (FsmRect v7 in array7)
			{
				AddVariableLookup(v7);
			}
			FsmQuaternion[] array8 = QuaternionVariables;
			foreach (FsmQuaternion v8 in array8)
			{
				AddVariableLookup(v8);
			}
			FsmGameObject[] array9 = GameObjectVariables;
			foreach (FsmGameObject v9 in array9)
			{
				AddVariableLookup(v9);
			}
			FsmObject[] array10 = ObjectVariables;
			foreach (FsmObject v10 in array10)
			{
				AddVariableLookup(v10);
			}
			FsmMaterial[] array11 = MaterialVariables;
			foreach (FsmMaterial v11 in array11)
			{
				AddVariableLookup(v11);
			}
			FsmTexture[] array12 = TextureVariables;
			foreach (FsmTexture v12 in array12)
			{
				AddVariableLookup(v12);
			}
			FsmColor[] array13 = ColorVariables;
			foreach (FsmColor v13 in array13)
			{
				AddVariableLookup(v13);
			}
			FsmArray[] array14 = ArrayVariables;
			foreach (FsmArray v14 in array14)
			{
				AddVariableLookup(v14);
			}
			FsmEnum[] array15 = EnumVariables;
			foreach (FsmEnum v15 in array15)
			{
				AddVariableLookup(v15);
			}
			Dictionary<string, NamedVariable>.ValueCollection values = variableLookup.Values;
			_allVariables = new NamedVariable[values.Count];
			int num = 0;
			foreach (NamedVariable item in values)
			{
				_allVariables[num] = item;
				num++;
			}
		}

		private void AddVariableLookup(NamedVariable v)
		{
			if (v == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(v.Name))
			{
				emptyVariables.Add(v);
			}
			else if (variableLookup.ContainsKey(v.Name))
			{
				NamedVariable namedVariable = variableLookup[v.Name];
				string text = "variableLookup already contains: " + v.Name;
				if (v.VariableType != namedVariable.VariableType)
				{
					text = text + "\nVariables are of different type: " + v.VariableType.ToString() + " " + namedVariable.VariableType;
					Debug.LogWarning(text);
				}
			}
			else
			{
				variableLookup.Add(v.Name, v);
			}
		}

		public void Reinitialize()
		{
			_emptyVariables = null;
			_variableLookup = null;
			_allVariables = null;
		}

		public NamedVariable[] GetAllNamedVariables()
		{
			return allVariables;
		}

		public NamedVariable[] GetAllNamedVariablesSorted()
		{
			List<NamedVariable> list = new List<NamedVariable>(allVariables);
			list.Sort();
			return list.ToArray();
		}

		public NamedVariable[] GetNamedVariables(VariableType type)
		{
			if (type == VariableType.Unknown)
			{
				return GetAllNamedVariables();
			}
			List<NamedVariable> list = new List<NamedVariable>();
			NamedVariable[] array = allVariables;
			foreach (NamedVariable namedVariable in array)
			{
				if (namedVariable.VariableType == type)
				{
					list.Add(namedVariable);
				}
			}
			return list.ToArray();
		}

		public NamedVariable[] GetNamedVariablesSorted(VariableType type)
		{
			List<NamedVariable> list = new List<NamedVariable>(GetNamedVariables(type));
			list.Sort();
			return list.ToArray();
		}

		public List<NamedVariable> GetEmptyVariables()
		{
			return emptyVariables;
		}

		public bool Contains(string variableName)
		{
			return variableLookup.ContainsKey(variableName);
		}

		public bool Contains(NamedVariable variable)
		{
			return variableLookup.ContainsValue(variable);
		}

		public NamedVariable[] GetNames(Type ofType)
		{
			return GetNamedVariables(FsmVar.GetVariableType(ofType));
		}

		public int GetVariableIndex(string variableName)
		{
			for (int i = 0; i < allVariables.Length; i++)
			{
				if (allVariables[i].Name == variableName)
				{
					return i;
				}
			}
			return -1;
		}

		public static bool AreCompatible(FsmVariables vars1, FsmVariables vars2)
		{
			if (vars1 == null || vars2 == null)
			{
				return false;
			}
			NamedVariable[] array = vars1.allVariables;
			NamedVariable[] array2 = vars2.allVariables;
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				NamedVariable namedVariable = array[i];
				NamedVariable namedVariable2 = array2[i];
				if (namedVariable.VariableType != namedVariable2.VariableType)
				{
					return false;
				}
				if (namedVariable.ObjectType != namedVariable2.ObjectType)
				{
					return false;
				}
				if (namedVariable.Name != namedVariable2.Name)
				{
					return false;
				}
			}
			return true;
		}

		public void OverrideVariableValues(FsmVariables source)
		{
			for (int i = 0; i < source.FloatVariables.Length; i++)
			{
				for (int j = 0; j < FloatVariables.Length; j++)
				{
					if (floatVariables[j].ShowInInspector && source.floatVariables[i].Name == floatVariables[j].Name)
					{
						floatVariables[j].Value = source.floatVariables[i].Value;
					}
				}
			}
			for (int k = 0; k < source.IntVariables.Length; k++)
			{
				for (int l = 0; l < IntVariables.Length; l++)
				{
					if (intVariables[l].ShowInInspector && source.intVariables[k].Name == intVariables[l].Name)
					{
						intVariables[l].Value = source.intVariables[k].Value;
					}
				}
			}
			for (int m = 0; m < source.BoolVariables.Length; m++)
			{
				for (int n = 0; n < BoolVariables.Length; n++)
				{
					if (boolVariables[n].ShowInInspector && source.boolVariables[m].Name == boolVariables[n].Name)
					{
						boolVariables[n].Value = source.boolVariables[m].Value;
					}
				}
			}
			for (int num = 0; num < source.GameObjectVariables.Length; num++)
			{
				for (int num2 = 0; num2 < GameObjectVariables.Length; num2++)
				{
					if (gameObjectVariables[num2].ShowInInspector && source.gameObjectVariables[num].Name == gameObjectVariables[num2].Name)
					{
						gameObjectVariables[num2].Value = source.gameObjectVariables[num].Value;
					}
				}
			}
			for (int num3 = 0; num3 < source.ColorVariables.Length; num3++)
			{
				for (int num4 = 0; num4 < ColorVariables.Length; num4++)
				{
					if (colorVariables[num4].ShowInInspector && source.colorVariables[num3].Name == colorVariables[num4].Name)
					{
						colorVariables[num4].Value = source.colorVariables[num3].Value;
					}
				}
			}
			for (int num5 = 0; num5 < source.Vector2Variables.Length; num5++)
			{
				for (int num6 = 0; num6 < Vector2Variables.Length; num6++)
				{
					if (vector2Variables[num6].ShowInInspector && source.vector2Variables[num5].Name == vector2Variables[num6].Name)
					{
						vector2Variables[num6].Value = source.vector2Variables[num5].Value;
					}
				}
			}
			for (int num7 = 0; num7 < source.Vector3Variables.Length; num7++)
			{
				for (int num8 = 0; num8 < Vector3Variables.Length; num8++)
				{
					if (vector3Variables[num8].ShowInInspector && source.vector3Variables[num7].Name == vector3Variables[num8].Name)
					{
						vector3Variables[num8].Value = source.vector3Variables[num7].Value;
					}
				}
			}
			for (int num9 = 0; num9 < source.RectVariables.Length; num9++)
			{
				for (int num10 = 0; num10 < RectVariables.Length; num10++)
				{
					if (rectVariables[num10].ShowInInspector && source.rectVariables[num9].Name == rectVariables[num10].Name)
					{
						rectVariables[num10].Value = source.rectVariables[num9].Value;
					}
				}
			}
			for (int num11 = 0; num11 < source.QuaternionVariables.Length; num11++)
			{
				for (int num12 = 0; num12 < QuaternionVariables.Length; num12++)
				{
					if (quaternionVariables[num12].ShowInInspector && source.quaternionVariables[num11].Name == quaternionVariables[num12].Name)
					{
						quaternionVariables[num12].Value = source.quaternionVariables[num11].Value;
					}
				}
			}
			for (int num13 = 0; num13 < source.ObjectVariables.Length; num13++)
			{
				for (int num14 = 0; num14 < ObjectVariables.Length; num14++)
				{
					if (objectVariables[num14].ShowInInspector && source.objectVariables[num13].Name == objectVariables[num14].Name)
					{
						objectVariables[num14].Value = source.objectVariables[num13].Value;
					}
				}
			}
			for (int num15 = 0; num15 < source.MaterialVariables.Length; num15++)
			{
				for (int num16 = 0; num16 < MaterialVariables.Length; num16++)
				{
					if (materialVariables[num16].ShowInInspector && source.materialVariables[num15].Name == materialVariables[num16].Name)
					{
						materialVariables[num16].Value = source.materialVariables[num15].Value;
					}
				}
			}
			for (int num17 = 0; num17 < source.TextureVariables.Length; num17++)
			{
				for (int num18 = 0; num18 < TextureVariables.Length; num18++)
				{
					if (textureVariables[num18].ShowInInspector && source.textureVariables[num17].Name == textureVariables[num18].Name)
					{
						textureVariables[num18].Value = source.textureVariables[num17].Value;
					}
				}
			}
			for (int num19 = 0; num19 < source.StringVariables.Length; num19++)
			{
				for (int num20 = 0; num20 < StringVariables.Length; num20++)
				{
					if (stringVariables[num20].ShowInInspector && source.stringVariables[num19].Name == stringVariables[num20].Name)
					{
						stringVariables[num20].Value = source.stringVariables[num19].Value;
					}
				}
			}
			for (int num21 = 0; num21 < source.ArrayVariables.Length; num21++)
			{
				for (int num22 = 0; num22 < ArrayVariables.Length; num22++)
				{
					if (arrayVariables[num22].ShowInInspector && source.arrayVariables[num21].Name == arrayVariables[num22].Name)
					{
						arrayVariables[num22].CopyValues(source.arrayVariables[num21]);
					}
				}
			}
			for (int num23 = 0; num23 < source.EnumVariables.Length; num23++)
			{
				for (int num24 = 0; num24 < EnumVariables.Length; num24++)
				{
					if (enumVariables[num24].ShowInInspector && source.enumVariables[num23].Name == enumVariables[num24].Name)
					{
						enumVariables[num24].Value = source.enumVariables[num23].Value;
					}
				}
			}
		}

		public void ApplyVariableValues(FsmVariables source)
		{
			if (source != null)
			{
				for (int i = 0; i < source.FloatVariables.Length; i++)
				{
					floatVariables[i].Value = source.floatVariables[i].Value;
				}
				for (int j = 0; j < source.IntVariables.Length; j++)
				{
					intVariables[j].Value = source.intVariables[j].Value;
				}
				for (int k = 0; k < source.BoolVariables.Length; k++)
				{
					boolVariables[k].Value = source.boolVariables[k].Value;
				}
				for (int l = 0; l < source.GameObjectVariables.Length; l++)
				{
					gameObjectVariables[l].Value = source.gameObjectVariables[l].Value;
				}
				for (int m = 0; m < source.ColorVariables.Length; m++)
				{
					colorVariables[m].Value = source.colorVariables[m].Value;
				}
				for (int n = 0; n < source.Vector2Variables.Length; n++)
				{
					vector2Variables[n].Value = source.vector2Variables[n].Value;
				}
				for (int num = 0; num < source.Vector3Variables.Length; num++)
				{
					vector3Variables[num].Value = source.vector3Variables[num].Value;
				}
				for (int num2 = 0; num2 < source.RectVariables.Length; num2++)
				{
					rectVariables[num2].Value = source.rectVariables[num2].Value;
				}
				for (int num3 = 0; num3 < source.QuaternionVariables.Length; num3++)
				{
					quaternionVariables[num3].Value = source.quaternionVariables[num3].Value;
				}
				for (int num4 = 0; num4 < source.ObjectVariables.Length; num4++)
				{
					objectVariables[num4].Value = source.objectVariables[num4].Value;
				}
				for (int num5 = 0; num5 < source.MaterialVariables.Length; num5++)
				{
					materialVariables[num5].Value = source.materialVariables[num5].Value;
				}
				for (int num6 = 0; num6 < source.TextureVariables.Length; num6++)
				{
					textureVariables[num6].Value = source.textureVariables[num6].Value;
				}
				for (int num7 = 0; num7 < source.StringVariables.Length; num7++)
				{
					stringVariables[num7].Value = source.stringVariables[num7].Value;
				}
				for (int num8 = 0; num8 < source.EnumVariables.Length; num8++)
				{
					enumVariables[num8].Value = source.enumVariables[num8].Value;
				}
				for (int num9 = 0; num9 < source.ArrayVariables.Length; num9++)
				{
					arrayVariables[num9].CopyValues(source.arrayVariables[num9]);
				}
			}
		}

		public void ApplyVariableValuesCareful(FsmVariables source)
		{
			if (source == null)
			{
				return;
			}
			for (int i = 0; i < source.FloatVariables.Length; i++)
			{
				FsmFloat fsmFloat = FindFsmFloat(source.floatVariables[i].Name);
				if (fsmFloat != null)
				{
					fsmFloat.Value = source.floatVariables[i].Value;
				}
			}
			for (int j = 0; j < source.IntVariables.Length; j++)
			{
				FsmInt fsmInt = FindFsmInt(source.IntVariables[j].Name);
				if (fsmInt != null)
				{
					fsmInt.Value = source.IntVariables[j].Value;
				}
			}
			for (int k = 0; k < source.BoolVariables.Length; k++)
			{
				FsmBool fsmBool = FindFsmBool(source.BoolVariables[k].Name);
				if (fsmBool != null)
				{
					fsmBool.Value = source.BoolVariables[k].Value;
				}
			}
			for (int l = 0; l < source.GameObjectVariables.Length; l++)
			{
				FsmBool fsmBool2 = FindFsmBool(source.BoolVariables[l].Name);
				if (fsmBool2 != null)
				{
					fsmBool2.Value = source.BoolVariables[l].Value;
				}
			}
			for (int m = 0; m < source.ColorVariables.Length; m++)
			{
				FsmBool fsmBool3 = FindFsmBool(source.BoolVariables[m].Name);
				if (fsmBool3 != null)
				{
					fsmBool3.Value = source.BoolVariables[m].Value;
				}
			}
			for (int n = 0; n < source.Vector2Variables.Length; n++)
			{
				FsmBool fsmBool4 = FindFsmBool(source.BoolVariables[n].Name);
				if (fsmBool4 != null)
				{
					fsmBool4.Value = source.BoolVariables[n].Value;
				}
			}
			for (int num = 0; num < source.Vector3Variables.Length; num++)
			{
				FsmBool fsmBool5 = FindFsmBool(source.BoolVariables[num].Name);
				if (fsmBool5 != null)
				{
					fsmBool5.Value = source.BoolVariables[num].Value;
				}
			}
			for (int num2 = 0; num2 < source.RectVariables.Length; num2++)
			{
				FsmRect fsmRect = FindFsmRect(source.RectVariables[num2].Name);
				if (fsmRect != null)
				{
					fsmRect.Value = source.RectVariables[num2].Value;
				}
			}
			for (int num3 = 0; num3 < source.QuaternionVariables.Length; num3++)
			{
				FsmQuaternion fsmQuaternion = FindFsmQuaternion(source.QuaternionVariables[num3].Name);
				if (fsmQuaternion != null)
				{
					fsmQuaternion.Value = source.QuaternionVariables[num3].Value;
				}
			}
			for (int num4 = 0; num4 < source.ObjectVariables.Length; num4++)
			{
				FsmObject fsmObject = FindFsmObject(source.ObjectVariables[num4].Name);
				if (fsmObject != null)
				{
					fsmObject.Value = source.ObjectVariables[num4].Value;
				}
			}
			for (int num5 = 0; num5 < source.MaterialVariables.Length; num5++)
			{
				FsmMaterial fsmMaterial = FindFsmMaterial(source.MaterialVariables[num5].Name);
				if (fsmMaterial != null)
				{
					fsmMaterial.Value = source.MaterialVariables[num5].Value;
				}
			}
			for (int num6 = 0; num6 < source.TextureVariables.Length; num6++)
			{
				FsmTexture fsmTexture = FindFsmTexture(source.TextureVariables[num6].Name);
				if (fsmTexture != null)
				{
					fsmTexture.Value = source.TextureVariables[num6].Value;
				}
			}
			for (int num7 = 0; num7 < source.StringVariables.Length; num7++)
			{
				FsmString fsmString = FindFsmString(source.StringVariables[num7].Name);
				if (fsmString != null)
				{
					fsmString.Value = source.StringVariables[num7].Value;
				}
			}
			for (int num8 = 0; num8 < source.EnumVariables.Length; num8++)
			{
				FsmEnum fsmEnum = FindFsmEnum(source.EnumVariables[num8].Name);
				if (fsmEnum != null)
				{
					fsmEnum.Value = source.EnumVariables[num8].Value;
				}
			}
			for (int num9 = 0; num9 < source.ArrayVariables.Length; num9++)
			{
				FindFsmArray(source.ArrayVariables[num9].Name)?.CopyValues(source.arrayVariables[num9]);
			}
		}

		public NamedVariable GetVariable(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			variableLookup.TryGetValue(name, out var value);
			if (value == null)
			{
				value = GlobalVariables.FindVariable(name);
			}
			if (value != null && !PlayMakerGlobals.IsPlaying)
			{
				return value.Copy();
			}
			return value;
		}

		public NamedVariable GetVariable(VariableType variableType, string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			NamedVariable variable = GetVariable(name);
			if (variable == null)
			{
				return null;
			}
			if (variable.VariableType == variableType)
			{
				return variable;
			}
			return variableType switch
			{
				VariableType.Float => new FsmFloat(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Int => new FsmInt(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Bool => new FsmBool(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.GameObject => new FsmGameObject(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.String => new FsmString(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Vector2 => new FsmVector2(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Vector3 => new FsmVector3(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Color => new FsmColor(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Rect => new FsmRect(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Material => new FsmMaterial(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Texture => new FsmTexture(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Quaternion => new FsmQuaternion(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Object => new FsmObject(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Unknown => null, 
				VariableType.Array => new FsmArray(name)
				{
					CastVariable = GetVariable(name)
				}, 
				VariableType.Enum => new FsmEnum(name)
				{
					CastVariable = GetVariable(name)
				}, 
				_ => throw new ArgumentOutOfRangeException("variableType", variableType, null), 
			};
		}

		public FsmFloat GetFsmFloat(string name)
		{
			return (GetVariable(name) as FsmFloat) ?? new FsmFloat(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmObject GetFsmObject(string name)
		{
			return (GetVariable(name) as FsmObject) ?? new FsmObject(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmMaterial GetFsmMaterial(string name)
		{
			return (GetVariable(name) as FsmMaterial) ?? new FsmMaterial(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmTexture GetFsmTexture(string name)
		{
			return (GetVariable(name) as FsmTexture) ?? new FsmTexture(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmInt GetFsmInt(string name)
		{
			return (GetVariable(name) as FsmInt) ?? new FsmInt(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmBool GetFsmBool(string name)
		{
			return (GetVariable(name) as FsmBool) ?? new FsmBool(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmString GetFsmString(string name)
		{
			return (GetVariable(name) as FsmString) ?? new FsmString(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmVector2 GetFsmVector2(string name)
		{
			return (GetVariable(name) as FsmVector2) ?? new FsmVector2(name);
		}

		public FsmVector3 GetFsmVector3(string name)
		{
			return (GetVariable(name) as FsmVector3) ?? new FsmVector3(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmRect GetFsmRect(string name)
		{
			return (GetVariable(name) as FsmRect) ?? new FsmRect(name);
		}

		public FsmQuaternion GetFsmQuaternion(string name)
		{
			return (GetVariable(name) as FsmQuaternion) ?? new FsmQuaternion(name);
		}

		public FsmColor GetFsmColor(string name)
		{
			return (GetVariable(name) as FsmColor) ?? new FsmColor(name);
		}

		public FsmGameObject GetFsmGameObject(string name)
		{
			return (GetVariable(name) as FsmGameObject) ?? new FsmGameObject(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public FsmArray GetFsmArray(string name)
		{
			return (GetVariable(name) as FsmArray) ?? new FsmArray(name);
		}

		public FsmEnum GetFsmEnum(string name)
		{
			return (GetVariable(name) as FsmEnum) ?? new FsmEnum(name)
			{
				CastVariable = GetVariable(name)
			};
		}

		public NamedVariable FindVariable(string name)
		{
			variableLookup.TryGetValue(name, out var value);
			return value;
		}

		public NamedVariable LoadGlobalVariable(string name)
		{
			variableLookup.TryGetValue(name, out var value);
			if (value != null && !PlayMakerGlobals.IsPlaying)
			{
				return value.Copy();
			}
			return value;
		}

		public NamedVariable FindVariable(VariableType type, string name)
		{
			if (variableLookup.TryGetValue(name, out var value))
			{
				if (value.VariableType != type)
				{
					return null;
				}
				return value;
			}
			return null;
		}

		public FsmFloat FindFsmFloat(string name)
		{
			return FindVariable(VariableType.Float, name) as FsmFloat;
		}

		public FsmObject FindFsmObject(string name)
		{
			return FindVariable(VariableType.Object, name) as FsmObject;
		}

		public FsmMaterial FindFsmMaterial(string name)
		{
			return FindVariable(VariableType.Material, name) as FsmMaterial;
		}

		public FsmTexture FindFsmTexture(string name)
		{
			return FindVariable(VariableType.Texture, name) as FsmTexture;
		}

		public FsmInt FindFsmInt(string name)
		{
			return FindVariable(VariableType.Int, name) as FsmInt;
		}

		public FsmBool FindFsmBool(string name)
		{
			return FindVariable(VariableType.Bool, name) as FsmBool;
		}

		public FsmString FindFsmString(string name)
		{
			return FindVariable(VariableType.String, name) as FsmString;
		}

		public FsmVector2 FindFsmVector2(string name)
		{
			return FindVariable(VariableType.Vector2, name) as FsmVector2;
		}

		public FsmVector3 FindFsmVector3(string name)
		{
			return FindVariable(VariableType.Vector3, name) as FsmVector3;
		}

		public FsmRect FindFsmRect(string name)
		{
			return FindVariable(VariableType.Rect, name) as FsmRect;
		}

		public FsmQuaternion FindFsmQuaternion(string name)
		{
			return FindVariable(VariableType.Quaternion, name) as FsmQuaternion;
		}

		public FsmColor FindFsmColor(string name)
		{
			return FindVariable(VariableType.Color, name) as FsmColor;
		}

		public FsmGameObject FindFsmGameObject(string name)
		{
			return FindVariable(VariableType.GameObject, name) as FsmGameObject;
		}

		public FsmEnum FindFsmEnum(string name)
		{
			return FindVariable(VariableType.Enum, name) as FsmEnum;
		}

		public FsmArray FindFsmArray(string name)
		{
			return FindVariable(VariableType.Array, name) as FsmArray;
		}
	}
}
