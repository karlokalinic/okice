using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class ActionData
	{
		public class Context
		{
			public Fsm currentFsm;

			public FsmState currentState;

			public FsmStateAction currentAction;

			public int currentActionIndex;

			public string currentParameter;

			public override string ToString()
			{
				string fullFsmLabel = FsmUtility.GetFullFsmLabel(currentFsm);
				fullFsmLabel = fullFsmLabel + ": " + ((currentState != null) ? currentState.Name : "Missing State");
				fullFsmLabel = fullFsmLabel + ": " + ((currentAction != null) ? currentAction.Name : "Missing Action");
				return fullFsmLabel + ": " + currentActionIndex + ": " + currentParameter;
			}
		}

		private const string autoNameString = "~AutoName";

		private static readonly Dictionary<string, Type> ActionTypeLookup = new Dictionary<string, Type>();

		public static readonly Dictionary<Type, FieldInfo[]> ActionFieldsLookup = new Dictionary<Type, FieldInfo[]>();

		public static readonly Dictionary<Type, int> ActionHashCodeLookup = new Dictionary<Type, int>();

		private static bool resaveActionData;

		private static readonly List<int> UsedIndices = new List<int>();

		private static readonly List<FieldInfo> LoadedFields = new List<FieldInfo>();

		[SerializeField]
		private List<string> actionNames = new List<string>();

		[SerializeField]
		private List<string> customNames = new List<string>();

		[SerializeField]
		private List<bool> actionEnabled = new List<bool>();

		[SerializeField]
		private List<bool> actionIsOpen = new List<bool>();

		[SerializeField]
		private List<int> actionStartIndex = new List<int>();

		[SerializeField]
		private List<int> actionHashCodes = new List<int>();

		[SerializeField]
		private List<UnityEngine.Object> unityObjectParams;

		[SerializeField]
		private List<FsmGameObject> fsmGameObjectParams;

		[SerializeField]
		private List<FsmOwnerDefault> fsmOwnerDefaultParams;

		[SerializeField]
		private List<FsmAnimationCurve> animationCurveParams;

		[SerializeField]
		private List<FunctionCall> functionCallParams;

		[SerializeField]
		private List<FsmTemplateControl> fsmTemplateControlParams;

		[SerializeField]
		private List<FsmEventTarget> fsmEventTargetParams;

		[SerializeField]
		private List<FsmProperty> fsmPropertyParams;

		[SerializeField]
		private List<LayoutOption> layoutOptionParams;

		[SerializeField]
		private List<FsmString> fsmStringParams;

		[SerializeField]
		private List<FsmObject> fsmObjectParams;

		[SerializeField]
		private List<FsmVar> fsmVarParams;

		[SerializeField]
		private List<FsmArray> fsmArrayParams;

		[SerializeField]
		private List<FsmEnum> fsmEnumParams;

		[SerializeField]
		private List<FsmFloat> fsmFloatParams;

		[SerializeField]
		private List<FsmInt> fsmIntParams;

		[SerializeField]
		private List<FsmBool> fsmBoolParams;

		[SerializeField]
		private List<FsmVector2> fsmVector2Params;

		[SerializeField]
		private List<FsmVector3> fsmVector3Params;

		[SerializeField]
		private List<FsmColor> fsmColorParams;

		[SerializeField]
		private List<FsmRect> fsmRectParams;

		[SerializeField]
		private List<FsmQuaternion> fsmQuaternionParams;

		[SerializeField]
		private List<string> stringParams;

		[SerializeField]
		private List<byte> byteData = new List<byte>();

		[NonSerialized]
		private byte[] byteDataAsArray;

		[SerializeField]
		private List<int> arrayParamSizes;

		[SerializeField]
		private List<string> arrayParamTypes;

		[SerializeField]
		private List<int> customTypeSizes;

		[SerializeField]
		private List<string> customTypeNames;

		[SerializeField]
		private List<ParamDataType> paramDataType = new List<ParamDataType>();

		[SerializeField]
		private List<string> paramName = new List<string>();

		[SerializeField]
		private List<int> paramDataPos = new List<int>();

		[SerializeField]
		private List<int> paramByteDataSize = new List<int>();

		private int nextParamIndex;

		private const int MUST_BE_LESS_THAN = 100000000;

		public int ActionCount => actionNames.Count;

		public List<string> ActionNames => actionNames;

		public ActionData Copy()
		{
			return new ActionData
			{
				actionNames = new List<string>(actionNames),
				customNames = new List<string>(customNames),
				actionEnabled = new List<bool>(actionEnabled),
				actionIsOpen = new List<bool>(actionIsOpen),
				actionStartIndex = new List<int>(actionStartIndex),
				actionHashCodes = new List<int>(actionHashCodes),
				fsmFloatParams = CopyFsmFloatParams(),
				fsmIntParams = CopyFsmIntParams(),
				fsmBoolParams = CopyFsmBoolParams(),
				fsmColorParams = CopyFsmColorParams(),
				fsmVector2Params = CopyFsmVector2Params(),
				fsmVector3Params = CopyFsmVector3Params(),
				fsmRectParams = CopyFsmRectParams(),
				fsmQuaternionParams = CopyFsmQuaternionParams(),
				stringParams = CopyStringParams(),
				byteData = new List<byte>(byteData),
				unityObjectParams = ((unityObjectParams != null) ? new List<UnityEngine.Object>(unityObjectParams) : null),
				fsmStringParams = CopyFsmStringParams(),
				fsmObjectParams = CopyFsmObjectParams(),
				fsmGameObjectParams = CopyFsmGameObjectParams(),
				fsmOwnerDefaultParams = CopyFsmOwnerDefaultParams(),
				animationCurveParams = CopyAnimationCurveParams(),
				functionCallParams = CopyFunctionCallParams(),
				fsmTemplateControlParams = CopyFsmTemplateControlParams(),
				fsmVarParams = CopyFsmVarParams(),
				fsmArrayParams = CopyFsmArrayParams(),
				fsmEnumParams = CopyFsmEnumParams(),
				fsmPropertyParams = CopyFsmPropertyParams(),
				fsmEventTargetParams = CopyFsmEventTargetParams(),
				layoutOptionParams = CopyLayoutOptionParams(),
				arrayParamSizes = ((arrayParamSizes != null) ? new List<int>(arrayParamSizes) : null),
				arrayParamTypes = ((arrayParamTypes != null) ? new List<string>(arrayParamTypes) : null),
				customTypeSizes = ((customTypeSizes != null) ? new List<int>(customTypeSizes) : null),
				customTypeNames = ((customTypeNames != null) ? new List<string>(customTypeNames) : null),
				paramName = new List<string>(paramName),
				paramDataPos = new List<int>(paramDataPos),
				paramByteDataSize = new List<int>(paramByteDataSize),
				paramDataType = new List<ParamDataType>(paramDataType)
			};
		}

		private List<string> CopyStringParams()
		{
			if (stringParams == null)
			{
				return null;
			}
			return new List<string>(stringParams);
		}

		private List<FsmFloat> CopyFsmFloatParams()
		{
			if (fsmFloatParams == null)
			{
				return null;
			}
			List<FsmFloat> list = new List<FsmFloat>();
			foreach (FsmFloat fsmFloatParam in fsmFloatParams)
			{
				list.Add(new FsmFloat(fsmFloatParam));
			}
			return list;
		}

		private List<FsmInt> CopyFsmIntParams()
		{
			if (fsmIntParams == null)
			{
				return null;
			}
			List<FsmInt> list = new List<FsmInt>();
			foreach (FsmInt fsmIntParam in fsmIntParams)
			{
				list.Add(new FsmInt(fsmIntParam));
			}
			return list;
		}

		private List<FsmBool> CopyFsmBoolParams()
		{
			if (fsmBoolParams == null)
			{
				return null;
			}
			List<FsmBool> list = new List<FsmBool>();
			foreach (FsmBool fsmBoolParam in fsmBoolParams)
			{
				list.Add(new FsmBool(fsmBoolParam));
			}
			return list;
		}

		private List<FsmVector2> CopyFsmVector2Params()
		{
			if (fsmVector2Params == null)
			{
				return null;
			}
			List<FsmVector2> list = new List<FsmVector2>();
			foreach (FsmVector2 fsmVector2Param in fsmVector2Params)
			{
				list.Add(new FsmVector2(fsmVector2Param));
			}
			return list;
		}

		private List<FsmVector3> CopyFsmVector3Params()
		{
			if (fsmVector3Params == null)
			{
				return null;
			}
			List<FsmVector3> list = new List<FsmVector3>();
			foreach (FsmVector3 fsmVector3Param in fsmVector3Params)
			{
				list.Add(new FsmVector3(fsmVector3Param));
			}
			return list;
		}

		private List<FsmColor> CopyFsmColorParams()
		{
			if (fsmColorParams == null)
			{
				return null;
			}
			List<FsmColor> list = new List<FsmColor>();
			foreach (FsmColor fsmColorParam in fsmColorParams)
			{
				list.Add(new FsmColor(fsmColorParam));
			}
			return list;
		}

		private List<FsmRect> CopyFsmRectParams()
		{
			if (fsmRectParams == null)
			{
				return null;
			}
			List<FsmRect> list = new List<FsmRect>();
			foreach (FsmRect fsmRectParam in fsmRectParams)
			{
				list.Add(new FsmRect(fsmRectParam));
			}
			return list;
		}

		private List<FsmQuaternion> CopyFsmQuaternionParams()
		{
			if (fsmQuaternionParams == null)
			{
				return null;
			}
			List<FsmQuaternion> list = new List<FsmQuaternion>();
			foreach (FsmQuaternion fsmQuaternionParam in fsmQuaternionParams)
			{
				list.Add(new FsmQuaternion(fsmQuaternionParam));
			}
			return list;
		}

		private List<FsmString> CopyFsmStringParams()
		{
			if (fsmStringParams == null)
			{
				return null;
			}
			List<FsmString> list = new List<FsmString>();
			foreach (FsmString fsmStringParam in fsmStringParams)
			{
				list.Add(new FsmString(fsmStringParam));
			}
			return list;
		}

		private List<FsmObject> CopyFsmObjectParams()
		{
			if (fsmObjectParams == null)
			{
				return null;
			}
			List<FsmObject> list = new List<FsmObject>();
			foreach (FsmObject fsmObjectParam in fsmObjectParams)
			{
				list.Add(new FsmObject(fsmObjectParam));
			}
			return list;
		}

		private List<FsmGameObject> CopyFsmGameObjectParams()
		{
			if (fsmGameObjectParams == null)
			{
				return null;
			}
			List<FsmGameObject> list = new List<FsmGameObject>();
			foreach (FsmGameObject fsmGameObjectParam in fsmGameObjectParams)
			{
				list.Add(new FsmGameObject(fsmGameObjectParam));
			}
			return list;
		}

		private List<FsmOwnerDefault> CopyFsmOwnerDefaultParams()
		{
			if (fsmOwnerDefaultParams == null)
			{
				return null;
			}
			List<FsmOwnerDefault> list = new List<FsmOwnerDefault>();
			foreach (FsmOwnerDefault fsmOwnerDefaultParam in fsmOwnerDefaultParams)
			{
				list.Add(new FsmOwnerDefault(fsmOwnerDefaultParam));
			}
			return list;
		}

		private List<FsmAnimationCurve> CopyAnimationCurveParams()
		{
			if (animationCurveParams == null)
			{
				return null;
			}
			List<FsmAnimationCurve> list = new List<FsmAnimationCurve>();
			foreach (FsmAnimationCurve animationCurveParam in animationCurveParams)
			{
				FsmAnimationCurve fsmAnimationCurve = new FsmAnimationCurve();
				if (animationCurveParam != null && animationCurveParam.curve != null)
				{
					fsmAnimationCurve.curve.keys = animationCurveParam.curve.keys;
					fsmAnimationCurve.curve.preWrapMode = animationCurveParam.curve.preWrapMode;
					fsmAnimationCurve.curve.postWrapMode = animationCurveParam.curve.postWrapMode;
				}
				list.Add(fsmAnimationCurve);
			}
			return list;
		}

		private List<FunctionCall> CopyFunctionCallParams()
		{
			if (functionCallParams == null)
			{
				return null;
			}
			List<FunctionCall> list = new List<FunctionCall>();
			foreach (FunctionCall functionCallParam in functionCallParams)
			{
				list.Add(new FunctionCall(functionCallParam));
			}
			return list;
		}

		private List<FsmTemplateControl> CopyFsmTemplateControlParams()
		{
			if (fsmTemplateControlParams == null)
			{
				return null;
			}
			List<FsmTemplateControl> list = new List<FsmTemplateControl>();
			foreach (FsmTemplateControl fsmTemplateControlParam in fsmTemplateControlParams)
			{
				list.Add(new FsmTemplateControl(fsmTemplateControlParam));
			}
			return list;
		}

		private List<FsmVar> CopyFsmVarParams()
		{
			if (fsmVarParams == null)
			{
				return null;
			}
			List<FsmVar> list = new List<FsmVar>();
			foreach (FsmVar fsmVarParam in fsmVarParams)
			{
				list.Add(new FsmVar(fsmVarParam));
			}
			return list;
		}

		private List<FsmArray> CopyFsmArrayParams()
		{
			if (fsmArrayParams == null)
			{
				return null;
			}
			List<FsmArray> list = new List<FsmArray>();
			foreach (FsmArray fsmArrayParam in fsmArrayParams)
			{
				list.Add(new FsmArray(fsmArrayParam));
			}
			return list;
		}

		private List<FsmEnum> CopyFsmEnumParams()
		{
			if (fsmEnumParams == null)
			{
				return null;
			}
			List<FsmEnum> list = new List<FsmEnum>();
			foreach (FsmEnum fsmEnumParam in fsmEnumParams)
			{
				list.Add(new FsmEnum(fsmEnumParam));
			}
			return list;
		}

		private List<FsmProperty> CopyFsmPropertyParams()
		{
			if (fsmPropertyParams == null)
			{
				return null;
			}
			List<FsmProperty> list = new List<FsmProperty>();
			foreach (FsmProperty fsmPropertyParam in fsmPropertyParams)
			{
				list.Add(new FsmProperty(fsmPropertyParam));
			}
			return list;
		}

		private List<FsmEventTarget> CopyFsmEventTargetParams()
		{
			if (fsmEventTargetParams == null)
			{
				return null;
			}
			List<FsmEventTarget> list = new List<FsmEventTarget>();
			foreach (FsmEventTarget fsmEventTargetParam in fsmEventTargetParams)
			{
				list.Add(new FsmEventTarget(fsmEventTargetParam));
			}
			return list;
		}

		private List<LayoutOption> CopyLayoutOptionParams()
		{
			if (layoutOptionParams == null)
			{
				return null;
			}
			List<LayoutOption> list = new List<LayoutOption>();
			foreach (LayoutOption layoutOptionParam in layoutOptionParams)
			{
				list.Add(new LayoutOption(layoutOptionParam));
			}
			return list;
		}

		private void ClearActionData()
		{
			actionNames.Clear();
			customNames.Clear();
			actionEnabled.Clear();
			actionIsOpen.Clear();
			actionStartIndex.Clear();
			actionHashCodes.Clear();
			byteData.Clear();
			unityObjectParams = null;
			fsmStringParams = null;
			fsmObjectParams = null;
			fsmVarParams = null;
			fsmArrayParams = null;
			fsmEnumParams = null;
			fsmGameObjectParams = null;
			fsmOwnerDefaultParams = null;
			animationCurveParams = null;
			functionCallParams = null;
			fsmTemplateControlParams = null;
			fsmPropertyParams = null;
			fsmEventTargetParams = null;
			layoutOptionParams = null;
			arrayParamSizes = null;
			arrayParamTypes = null;
			customTypeNames = null;
			customTypeSizes = null;
			fsmFloatParams = null;
			fsmIntParams = null;
			fsmBoolParams = null;
			fsmVector2Params = null;
			fsmVector3Params = null;
			fsmColorParams = null;
			fsmRectParams = null;
			fsmQuaternionParams = null;
			stringParams = null;
			paramDataPos.Clear();
			paramByteDataSize.Clear();
			paramDataType.Clear();
			paramName.Clear();
			nextParamIndex = 0;
		}

		public static Type GetActionType(string actionName)
		{
			if (ActionTypeLookup.TryGetValue(actionName, out var value))
			{
				return value;
			}
			value = ReflectionUtils.GetGlobalType(actionName);
			if ((object)value == null)
			{
				return null;
			}
			ActionTypeLookup[actionName] = value;
			return value;
		}

		public static FieldInfo[] GetFields(Type actionType)
		{
			if (ActionFieldsLookup.TryGetValue(actionType, out var value))
			{
				return value;
			}
			value = actionType.GetPublicFields();
			ActionFieldsLookup[actionType] = value;
			return value;
		}

		private static int GetActionTypeHashCode(Type actionType)
		{
			if (ActionHashCodeLookup.TryGetValue(actionType, out var value))
			{
				return value;
			}
			string text = "";
			FieldInfo[] fields = GetFields(actionType);
			foreach (FieldInfo fieldInfo in fields)
			{
				text = text + fieldInfo.FieldType?.ToString() + "|";
			}
			value = GetStableHash(text);
			ActionHashCodeLookup[actionType] = value;
			return value;
		}

		private static int GetStableHash(string s)
		{
			uint num = 0u;
			byte[] bytes = Encoding.Unicode.GetBytes(s);
			foreach (byte b in bytes)
			{
				num += b;
				num += num << 10;
				num ^= num >> 6;
			}
			num += num << 3;
			num ^= num >> 11;
			num += num << 15;
			return (int)(num % 100000000);
		}

		public FsmStateAction[] LoadActions(FsmState state)
		{
			Context context = new Context
			{
				currentState = state,
				currentFsm = state.Fsm
			};
			int count = actionNames.Count;
			FsmStateAction[] array = new FsmStateAction[count];
			byteDataAsArray = byteData.ToArray();
			resaveActionData = false;
			for (int i = 0; i < count; i++)
			{
				FsmStateAction fsmStateAction = CreateAction(context, i);
				if (fsmStateAction != null)
				{
					array[i] = fsmStateAction;
				}
			}
			if (resaveActionData && !PlayMakerGlobals.IsBuilding)
			{
				SaveActions(state, array);
				array = LoadActions(state);
				state.Fsm.setDirty = true;
			}
			return array;
		}

		public FsmStateAction CreateAction(FsmState state, int actionIndex)
		{
			return CreateAction(new Context
			{
				currentState = state,
				currentFsm = state.Fsm
			}, actionIndex);
		}

		public FsmStateAction CreateAction(Context context, int actionIndex)
		{
			string text = string.Empty;
			context.currentActionIndex = actionIndex;
			FsmState currentState = context.currentState;
			if (currentState.Fsm == null)
			{
				UnityEngine.Debug.LogError("state.Fsm == null");
			}
			string text2 = actionNames[actionIndex];
			Type actionType = GetActionType(text2);
			if ((object)actionType == null)
			{
				string text3 = TryFixActionName(text2);
				actionType = GetActionType(text3);
				if ((object)actionType == null)
				{
					MissingAction missingAction = (MissingAction)Activator.CreateInstance(typeof(MissingAction));
					string text4 = FsmUtility.StripNamespace(text2);
					missingAction.actionName = text2;
					context.currentAction = missingAction;
					LogError(context, "Could Not Create Action: " + text4 + " (Maybe the script was removed?)");
					UnityEngine.Debug.LogWarning("Could Not Create Action: " + FsmUtility.GetPath(currentState) + text4 + " (Maybe the script was removed?)");
					return missingAction;
				}
				text = "Action : " + text2 + " Updated To: " + text3;
				text2 = text3;
				resaveActionData = true;
			}
			FsmStateAction fsmStateAction = Activator.CreateInstance(actionType) as FsmStateAction;
			if (fsmStateAction == null)
			{
				MissingAction missingAction2 = (MissingAction)Activator.CreateInstance(typeof(MissingAction));
				string text5 = (missingAction2.actionName = FsmUtility.StripNamespace(text2));
				context.currentAction = missingAction2;
				LogError(context, "Could Not Create Action: " + text5 + " (Maybe the script was removed?)");
				UnityEngine.Debug.LogError("Could Not Create Action: " + FsmUtility.GetPath(currentState) + text5 + " (Maybe the script was removed?)");
				return missingAction2;
			}
			context.currentAction = fsmStateAction;
			if (!string.IsNullOrEmpty(text))
			{
				LogInfo(context, text);
			}
			bool flag = true;
			if (paramDataType.Count != paramDataPos.Count || paramName.Count != paramDataPos.Count)
			{
				resaveActionData = true;
				flag = false;
			}
			if (actionHashCodes[actionIndex] != GetActionTypeHashCode(actionType))
			{
				fsmStateAction.Reset();
				resaveActionData = true;
				flag = false;
				if (paramDataType.Count != paramDataPos.Count)
				{
					LogError(context, "Action has changed since FSM was saved. Could not recover parameters. Parameters reset to default values.");
					UnityEngine.Debug.LogError("Action script has changed since Fsm was saved: " + FsmUtility.GetPath(currentState) + FsmUtility.StripNamespace(text2) + ". Parameters reset to default values...");
				}
				else
				{
					try
					{
						fsmStateAction = TryRecoverAction(context, actionType, fsmStateAction, actionIndex);
					}
					catch
					{
						LogError(context, "Action has changed since FSM was saved. Could not recover parameters. Parameters reset to default values.");
						throw;
					}
				}
			}
			nextParamIndex = actionStartIndex[actionIndex];
			if (flag)
			{
				FieldInfo[] fields = GetFields(actionType);
				foreach (FieldInfo fieldInfo in fields)
				{
					try
					{
						context.currentParameter = fieldInfo.Name;
						LoadActionField(context.currentFsm, fsmStateAction, fieldInfo, nextParamIndex);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError("Error Loading Action: " + currentState.Name + " : " + text2 + " : " + fieldInfo.Name + "\n" + ex);
					}
					nextParamIndex++;
				}
			}
			if (actionEnabled.Count > actionIndex)
			{
				fsmStateAction.Enabled = actionEnabled[actionIndex];
			}
			if (PlayMakerGlobals.IsEditor)
			{
				InitEditorData(fsmStateAction, actionIndex);
			}
			return fsmStateAction;
		}

		public void InitEditorData(FsmState state)
		{
			for (int i = 0; i < state.Actions.Length; i++)
			{
				InitEditorData(state.Actions[i], i);
			}
		}

		private void InitEditorData(FsmStateAction action, int actionIndex)
		{
			if (action.GetType() == typeof(MissingAction))
			{
				action.IsOpen = true;
				return;
			}
			if (customNames.Count > actionIndex)
			{
				action.Name = customNames[actionIndex];
				if (!PlayMakerGlobals.IsBuilding && !PlayMakerFSM.NotMainThread && action.Name == "~AutoName")
				{
					action.Name = action.AutoName();
					action.IsAutoNamed = true;
				}
			}
			action.IsOpen = actionIsOpen.Count <= actionIndex || actionIsOpen[actionIndex];
		}

		private string GetStringParam(int paramIndex)
		{
			int num = ((paramIndex >= 0 && paramIndex < paramDataPos.Count) ? paramDataPos[paramIndex] : (-1));
			if (num < 0 || stringParams == null || num >= stringParams.Count)
			{
				return "";
			}
			return stringParams[num];
		}

		private void LoadActionField(Fsm fsm, object obj, FieldInfo field, int paramIndex)
		{
			Type fieldType = field.FieldType;
			object value = null;
			if ((object)fieldType == typeof(FsmGameObject))
			{
				value = GetFsmGameObject(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmEvent))
			{
				if (fsm.DataVersion > 1)
				{
					string stringParam = GetStringParam(paramIndex);
					value = (string.IsNullOrEmpty(stringParam) ? null : FsmEvent.GetFsmEvent(stringParam));
				}
				else
				{
					try
					{
						value = FsmUtility.ByteArrayToFsmEvent(byteDataAsArray, paramDataPos[paramIndex], paramByteDataSize[paramIndex]);
					}
					catch
					{
						value = null;
					}
				}
			}
			else if ((object)fieldType == typeof(FsmFloat))
			{
				value = GetFsmFloat(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmInt))
			{
				value = GetFsmInt(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmBool))
			{
				value = GetFsmBool(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmVector2))
			{
				value = GetFsmVector2(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmVector3))
			{
				value = GetFsmVector3(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmRect))
			{
				value = GetFsmRect(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmQuaternion))
			{
				value = GetFsmQuaternion(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmColor))
			{
				value = GetFsmColor(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmObject))
			{
				value = GetFsmObject(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmMaterial))
			{
				value = GetFsmMaterial(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmTexture))
			{
				value = GetFsmTexture(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FunctionCall))
			{
				value = GetFunctionCall(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmTemplateControl))
			{
				value = GetFsmTemplateControl(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmVar))
			{
				value = GetFsmVar(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmEnum))
			{
				value = GetFsmEnum(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmArray))
			{
				value = GetFsmArray(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmProperty))
			{
				value = GetFsmProperty(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmEventTarget))
			{
				value = GetFsmEventTarget(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(LayoutOption))
			{
				value = GetLayoutOption(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmOwnerDefault))
			{
				value = GetFsmOwnerDefault(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(FsmAnimationCurve))
			{
				value = GetFsmAnimationCurve(paramIndex);
			}
			else if ((object)fieldType == typeof(FsmString))
			{
				value = GetFsmString(fsm, paramIndex);
			}
			else if ((object)fieldType == typeof(float))
			{
				try
				{
					value = FsmUtility.BitConverter.ToSingle(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = 0f;
				}
			}
			else if ((object)fieldType == typeof(int))
			{
				try
				{
					value = FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = 0;
				}
			}
			else if ((object)fieldType == typeof(bool))
			{
				try
				{
					value = FsmUtility.BitConverter.ToBoolean(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = false;
				}
			}
			else if ((object)fieldType == typeof(Color))
			{
				try
				{
					value = FsmUtility.ByteArrayToColor(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = default(Color);
				}
			}
			else if ((object)fieldType == typeof(Vector2))
			{
				try
				{
					value = FsmUtility.ByteArrayToVector2(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = default(Vector2);
				}
			}
			else if ((object)fieldType == typeof(Vector3))
			{
				try
				{
					value = FsmUtility.ByteArrayToVector3(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = default(Vector3);
				}
			}
			else if ((object)fieldType == typeof(Vector4))
			{
				try
				{
					value = FsmUtility.ByteArrayToVector4(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = default(Vector4);
				}
			}
			else if ((object)fieldType == typeof(Rect))
			{
				try
				{
					value = FsmUtility.ByteArrayToRect(byteDataAsArray, paramDataPos[paramIndex]);
				}
				catch
				{
					value = default(Rect);
				}
			}
			else if ((object)fieldType == typeof(string))
			{
				if (fsm.DataVersion > 1 && stringParams != null && stringParams.Count > 0)
				{
					value = GetStringParam(paramIndex);
				}
				else
				{
					try
					{
						value = FsmUtility.ByteArrayToString(byteDataAsArray, paramDataPos[paramIndex], paramByteDataSize[paramIndex]);
					}
					catch
					{
						value = "";
					}
				}
			}
			else if (fieldType.IsEnum)
			{
				try
				{
					value = Enum.ToObject(fieldType, (object)FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex]));
				}
				catch
				{
					value = Activator.CreateInstance(fieldType);
				}
			}
			else if (typeof(FsmObject).IsAssignableFrom(fieldType))
			{
				field.SetValue(obj, GetFsmObject(fsm, paramIndex));
			}
			else
			{
				if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
				{
					if (fieldType.IsArray)
					{
						try
						{
							Type globalType = ReflectionUtils.GetGlobalType(arrayParamTypes[paramDataPos[paramIndex]]);
							int num = arrayParamSizes[paramDataPos[paramIndex]];
							Array array = Array.CreateInstance(globalType, num);
							for (int i = 0; i < num; i++)
							{
								nextParamIndex++;
								LoadArrayElement(fsm, array, globalType, i, nextParamIndex);
							}
							field.SetValue(obj, array);
							return;
						}
						catch
						{
							field.SetValue(obj, null);
							return;
						}
					}
					if (fieldType.IsClass)
					{
						try
						{
							Type globalType2 = ReflectionUtils.GetGlobalType(customTypeNames[paramDataPos[paramIndex]]);
							object obj14 = Activator.CreateInstance(globalType2);
							int num2 = customTypeSizes[paramDataPos[paramIndex]];
							for (int j = 0; j < num2; j++)
							{
								nextParamIndex++;
								FieldInfo field2 = globalType2.GetField(paramName[nextParamIndex]);
								if ((object)field2 != null)
								{
									LoadActionField(fsm, obj14, field2, nextParamIndex);
								}
							}
							field.SetValue(obj, obj14);
							return;
						}
						catch
						{
							field.SetValue(obj, null);
							return;
						}
					}
					UnityEngine.Debug.LogError("ActionData: Missing LoadActionField for type: " + fieldType);
					field.SetValue(obj, null);
					return;
				}
				try
				{
					UnityEngine.Object obj16 = unityObjectParams[paramDataPos[paramIndex]];
					if ((object)obj16 != null)
					{
						if ((object)obj16.GetType() != typeof(UnityEngine.Object))
						{
							field.SetValue(obj, obj16);
						}
						return;
					}
				}
				catch
				{
					field.SetValue(obj, null);
					return;
				}
			}
			field.SetValue(obj, value);
		}

		private void LoadArrayElement(Fsm fsm, Array field, Type fieldType, int elementIndex, int paramIndex)
		{
			if (elementIndex >= field.GetLength(0) || paramIndex >= paramDataPos.Count)
			{
				return;
			}
			if ((object)fieldType == typeof(FsmGameObject))
			{
				field.SetValue(GetFsmGameObject(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FunctionCall))
			{
				field.SetValue(GetFunctionCall(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmProperty))
			{
				field.SetValue(GetFsmProperty(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(LayoutOption))
			{
				field.SetValue(GetLayoutOption(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmOwnerDefault))
			{
				field.SetValue(GetFsmOwnerDefault(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmAnimationCurve))
			{
				field.SetValue(GetFsmAnimationCurve(paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmVar))
			{
				field.SetValue(GetFsmVar(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmArray))
			{
				field.SetValue(GetFsmArray(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmString))
			{
				field.SetValue(GetFsmString(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmObject))
			{
				field.SetValue(GetFsmObject(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmMaterial))
			{
				field.SetValue(GetFsmMaterial(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmTexture))
			{
				field.SetValue(GetFsmTexture(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmEnum))
			{
				field.SetValue(GetFsmEnum(fsm, paramIndex), elementIndex);
				return;
			}
			if (fieldType.IsArray)
			{
				UnityEngine.Debug.LogError("Nested arrays are not supported!");
				return;
			}
			if ((object)fieldType == typeof(FsmEvent))
			{
				if (fsm.DataVersion > 1)
				{
					string stringParam = GetStringParam(paramIndex);
					field.SetValue(string.IsNullOrEmpty(stringParam) ? null : FsmEvent.GetFsmEvent(stringParam), elementIndex);
					return;
				}
				try
				{
					field.SetValue(FsmUtility.ByteArrayToFsmEvent(byteDataAsArray, paramDataPos[paramIndex], paramByteDataSize[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(null, elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(FsmFloat))
			{
				field.SetValue(GetFsmFloat(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmInt))
			{
				field.SetValue(GetFsmInt(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmBool))
			{
				field.SetValue(GetFsmBool(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmVector2))
			{
				field.SetValue(GetFsmVector2(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmVector3))
			{
				field.SetValue(GetFsmVector3(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmRect))
			{
				field.SetValue(GetFsmRect(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmQuaternion))
			{
				field.SetValue(GetFsmQuaternion(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(FsmColor))
			{
				field.SetValue(GetFsmColor(fsm, paramIndex), elementIndex);
				return;
			}
			if ((object)fieldType == typeof(float))
			{
				try
				{
					field.SetValue(FsmUtility.BitConverter.ToSingle(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(0f, elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(int))
			{
				try
				{
					field.SetValue(FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(0, elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(bool))
			{
				try
				{
					field.SetValue(FsmUtility.BitConverter.ToBoolean(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(false, elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(Color))
			{
				try
				{
					field.SetValue(FsmUtility.ByteArrayToColor(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(default(Color), elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(Vector2))
			{
				try
				{
					field.SetValue(FsmUtility.ByteArrayToVector2(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(default(Vector2), elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(Vector3))
			{
				try
				{
					field.SetValue(FsmUtility.ByteArrayToVector3(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(default(Vector3), elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(Vector4))
			{
				try
				{
					field.SetValue(FsmUtility.ByteArrayToVector4(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(default(Vector4), elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(Rect))
			{
				try
				{
					field.SetValue(FsmUtility.ByteArrayToRect(byteDataAsArray, paramDataPos[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue(default(Rect), elementIndex);
					return;
				}
			}
			if ((object)fieldType == typeof(string))
			{
				if (fsm.DataVersion > 1)
				{
					field.SetValue(GetStringParam(paramIndex), elementIndex);
					return;
				}
				try
				{
					field.SetValue(FsmUtility.ByteArrayToString(byteDataAsArray, paramDataPos[paramIndex], paramByteDataSize[paramIndex]), elementIndex);
					return;
				}
				catch
				{
					field.SetValue("", elementIndex);
					return;
				}
			}
			if (fieldType.IsEnum)
			{
				try
				{
					object value = Enum.ToObject(fieldType, (object)FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex]));
					field.SetValue(value, elementIndex);
					return;
				}
				catch
				{
					field.SetValue(Activator.CreateInstance(fieldType), elementIndex);
					return;
				}
			}
			if (typeof(FsmObject).IsAssignableFrom(fieldType))
			{
				field.SetValue(GetFsmObject(fsm, paramIndex), elementIndex);
				return;
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
			{
				try
				{
					UnityEngine.Object obj12 = unityObjectParams[paramDataPos[paramIndex]];
					if ((object)obj12 != null && (object)obj12.GetType() != typeof(UnityEngine.Object))
					{
						field.SetValue(obj12, elementIndex);
					}
					return;
				}
				catch
				{
					field.SetValue(null, elementIndex);
					return;
				}
			}
			if (fieldType.IsClass)
			{
				try
				{
					Type globalType = ReflectionUtils.GetGlobalType(customTypeNames[paramDataPos[paramIndex]]);
					object obj14 = Activator.CreateInstance(globalType);
					int num = customTypeSizes[paramDataPos[paramIndex]];
					for (int i = 0; i < num; i++)
					{
						nextParamIndex++;
						FieldInfo field2 = globalType.GetField(paramName[nextParamIndex]);
						if ((object)field2 != null)
						{
							LoadActionField(fsm, obj14, field2, nextParamIndex);
						}
					}
					field.SetValue(obj14, elementIndex);
					return;
				}
				catch
				{
					field.SetValue(null, elementIndex);
					return;
				}
			}
			field.SetValue(null, elementIndex);
		}

		public static void LogError(Context context, string error)
		{
			if (context.currentState != null && context.currentAction != null && context.currentFsm != null && (object)context.currentFsm.Owner != null)
			{
				ActionReport.LogError(context.currentFsm, context.currentState, context.currentAction, context.currentActionIndex, context.currentParameter, error);
			}
		}

		private static void LogInfo(Context context, string info)
		{
			if (context.currentFsm == null || context.currentState == null || context.currentAction == null)
			{
				UnityEngine.Debug.LogWarning("Missing context!");
			}
			else
			{
				ActionReport.Log(context.currentFsm, context.currentState, context.currentAction, context.currentActionIndex, context.currentParameter, info);
			}
		}

		private FsmFloat GetFsmFloat(Fsm fsm, int paramIndex)
		{
			if (fsm == null)
			{
				UnityEngine.Debug.LogError("fsm == null!");
				return new FsmFloat();
			}
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmFloat();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmFloatParams != null && fsmFloatParams.Count > num)
				{
					FsmFloat fsmFloat = fsmFloatParams[num];
					if (fsmFloat == null)
					{
						return new FsmFloat();
					}
					if (string.IsNullOrEmpty(fsmFloat.Name))
					{
						return fsmFloat;
					}
					return fsm.GetFsmFloat(fsmFloat.Name);
				}
				return new FsmFloat();
			}
			if (paramByteDataSize == null)
			{
				UnityEngine.Debug.LogError("paramByteDataSize == null! Data Version: " + fsm.DataVersion);
				return new FsmFloat();
			}
			return FsmUtility.ByteArrayToFsmFloat(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmInt GetFsmInt(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmInt();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmIntParams != null && fsmIntParams.Count > num)
				{
					FsmInt fsmInt = fsmIntParams[num];
					if (fsmInt == null)
					{
						return new FsmInt();
					}
					if (string.IsNullOrEmpty(fsmInt.Name))
					{
						return fsmInt;
					}
					return fsm.GetFsmInt(fsmInt.Name);
				}
				return new FsmInt();
			}
			return FsmUtility.ByteArrayToFsmInt(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmBool GetFsmBool(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmBool();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmBoolParams != null && fsmBoolParams.Count > num)
				{
					FsmBool fsmBool = fsmBoolParams[num];
					if (fsmBool == null)
					{
						return new FsmBool();
					}
					if (string.IsNullOrEmpty(fsmBool.Name))
					{
						return fsmBool;
					}
					return fsm.GetFsmBool(fsmBool.Name);
				}
				return new FsmBool();
			}
			return FsmUtility.ByteArrayToFsmBool(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmVector2 GetFsmVector2(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmVector2();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmVector2Params != null && fsmVector2Params.Count > num)
				{
					FsmVector2 fsmVector = fsmVector2Params[num];
					if (fsmVector == null)
					{
						return new FsmVector2();
					}
					if (string.IsNullOrEmpty(fsmVector.Name))
					{
						return fsmVector;
					}
					return fsm.GetFsmVector2(fsmVector.Name);
				}
				return new FsmVector2();
			}
			return FsmUtility.ByteArrayToFsmVector2(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmVector3 GetFsmVector3(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmVector3();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmVector3Params != null && fsmVector3Params.Count > num)
				{
					FsmVector3 fsmVector = fsmVector3Params[num];
					if (fsmVector == null)
					{
						return new FsmVector3();
					}
					if (string.IsNullOrEmpty(fsmVector.Name))
					{
						return fsmVector;
					}
					return fsm.GetFsmVector3(fsmVector.Name);
				}
				return new FsmVector3();
			}
			return FsmUtility.ByteArrayToFsmVector3(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmColor GetFsmColor(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmColor();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmColorParams != null && fsmColorParams.Count > num)
				{
					FsmColor fsmColor = fsmColorParams[num];
					if (fsmColor == null)
					{
						return new FsmColor();
					}
					if (string.IsNullOrEmpty(fsmColor.Name))
					{
						return fsmColor;
					}
					return fsm.GetFsmColor(fsmColor.Name);
				}
				return new FsmColor();
			}
			return FsmUtility.ByteArrayToFsmColor(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmAnimationCurve GetFsmAnimationCurve(int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmAnimationCurve();
			}
			int num = paramDataPos[paramIndex];
			if (num >= 0 && animationCurveParams != null && animationCurveParams.Count > num)
			{
				return animationCurveParams[num] ?? new FsmAnimationCurve();
			}
			return new FsmAnimationCurve();
		}

		private FsmRect GetFsmRect(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmRect();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmRectParams != null && fsmRectParams.Count > num)
				{
					FsmRect fsmRect = fsmRectParams[num];
					if (fsmRect == null)
					{
						return new FsmRect();
					}
					if (string.IsNullOrEmpty(fsmRect.Name))
					{
						return fsmRect;
					}
					return fsm.GetFsmRect(fsmRect.Name);
				}
				return new FsmRect();
			}
			return FsmUtility.ByteArrayToFsmRect(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmQuaternion GetFsmQuaternion(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmQuaternion();
			}
			int num = paramDataPos[paramIndex];
			if (fsm.DataVersion > 1)
			{
				if (num >= 0 && fsmQuaternionParams != null && fsmQuaternionParams.Count > num)
				{
					FsmQuaternion fsmQuaternion = fsmQuaternionParams[num];
					if (fsmQuaternion == null)
					{
						return new FsmQuaternion();
					}
					if (string.IsNullOrEmpty(fsmQuaternion.Name))
					{
						return fsmQuaternion;
					}
					return fsm.GetFsmQuaternion(fsmQuaternion.Name);
				}
				return new FsmQuaternion();
			}
			return FsmUtility.ByteArrayToFsmQuaternion(fsm, byteDataAsArray, num, paramByteDataSize[paramIndex]);
		}

		private FsmGameObject GetFsmGameObject(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmGameObject();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmGameObjectParams == null || fsmGameObjectParams.Count <= num)
			{
				return new FsmGameObject();
			}
			FsmGameObject fsmGameObject = fsmGameObjectParams[num];
			if (fsmGameObject == null)
			{
				return new FsmGameObject();
			}
			if (string.IsNullOrEmpty(fsmGameObject.Name))
			{
				return fsmGameObject;
			}
			return fsm.GetFsmGameObject(fsmGameObject.Name);
		}

		private FsmTemplateControl GetFsmTemplateControl(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmTemplateControl();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmTemplateControlParams == null || fsmTemplateControlParams.Count <= num)
			{
				return new FsmTemplateControl();
			}
			FsmTemplateControl fsmTemplateControl = fsmTemplateControlParams[num];
			if (fsmTemplateControl == null)
			{
				return new FsmTemplateControl();
			}
			if (fsmTemplateControl.inputVariables != null)
			{
				FsmVarOverride[] inputVariables = fsmTemplateControl.inputVariables;
				foreach (FsmVarOverride fsmVarOverride in inputVariables)
				{
					if ((object)fsmTemplateControl.fsmTemplate != null && fsmTemplateControl.fsmTemplate.fsm != null && fsmVarOverride.variable.UsesVariable)
					{
						fsmVarOverride.variable = fsmTemplateControl.fsmTemplate.fsm.Variables.GetVariable(fsmVarOverride.variable.Name);
					}
					if (fsmVarOverride.fsmVar.NamedVar.UsesVariable)
					{
						fsmVarOverride.fsmVar.NamedVar = fsm.Variables.GetVariable(fsmVarOverride.fsmVar.Type, fsmVarOverride.fsmVar.variableName);
					}
				}
			}
			return fsmTemplateControl;
		}

		private FsmVar GetFsmVar(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmVar();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmVarParams == null || fsmVarParams.Count <= num)
			{
				return new FsmVar();
			}
			FsmVar fsmVar = fsmVarParams[num] ?? new FsmVar();
			if (!string.IsNullOrEmpty(fsmVar.variableName))
			{
				fsmVar.NamedVar = fsm.Variables.GetVariable(fsmVar.variableName);
			}
			return fsmVar;
		}

		private FsmArray GetFsmArray(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmArray();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmArrayParams == null || fsmArrayParams.Count <= num)
			{
				return new FsmArray();
			}
			FsmArray fsmArray = fsmArrayParams[num] ?? new FsmArray();
			if (string.IsNullOrEmpty(fsmArray.Name))
			{
				return fsmArray;
			}
			return fsm.GetFsmArray(fsmArray.Name);
		}

		private FsmEnum GetFsmEnum(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmEnum();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmEnumParams == null || fsmEnumParams.Count <= num)
			{
				return new FsmEnum();
			}
			FsmEnum fsmEnum = fsmEnumParams[num] ?? new FsmEnum();
			if (string.IsNullOrEmpty(fsmEnum.Name))
			{
				return fsmEnum;
			}
			return fsm.GetFsmEnum(fsmEnum.Name);
		}

		private FunctionCall GetFunctionCall(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FunctionCall();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || functionCallParams == null || functionCallParams.Count <= num)
			{
				return new FunctionCall();
			}
			FunctionCall functionCall = functionCallParams[num];
			if (functionCall == null)
			{
				return new FunctionCall();
			}
			if (!string.IsNullOrEmpty(functionCall.BoolParameter.Name))
			{
				functionCall.BoolParameter = fsm.GetFsmBool(functionCall.BoolParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.FloatParameter.Name))
			{
				functionCall.FloatParameter = fsm.GetFsmFloat(functionCall.FloatParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.GameObjectParameter.Name))
			{
				functionCall.GameObjectParameter = fsm.GetFsmGameObject(functionCall.GameObjectParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.IntParameter.Name))
			{
				functionCall.IntParameter = fsm.GetFsmInt(functionCall.IntParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.MaterialParameter.Name))
			{
				functionCall.MaterialParameter = fsm.GetFsmMaterial(functionCall.MaterialParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.ObjectParameter.Name))
			{
				functionCall.ObjectParameter = fsm.GetFsmObject(functionCall.ObjectParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.QuaternionParameter.Name))
			{
				functionCall.QuaternionParameter = fsm.GetFsmQuaternion(functionCall.QuaternionParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.RectParamater.Name))
			{
				functionCall.RectParamater = fsm.GetFsmRect(functionCall.RectParamater.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.StringParameter.Name))
			{
				functionCall.StringParameter = fsm.GetFsmString(functionCall.StringParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.TextureParameter.Name))
			{
				functionCall.TextureParameter = fsm.GetFsmTexture(functionCall.TextureParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.Vector2Parameter.Name))
			{
				functionCall.Vector2Parameter = fsm.GetFsmVector2(functionCall.Vector2Parameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.Vector3Parameter.Name))
			{
				functionCall.Vector3Parameter = fsm.GetFsmVector3(functionCall.Vector3Parameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.EnumParameter.Name))
			{
				functionCall.EnumParameter = fsm.GetFsmEnum(functionCall.EnumParameter.Name);
			}
			if (!string.IsNullOrEmpty(functionCall.ArrayParameter.Name))
			{
				functionCall.ArrayParameter = fsm.GetFsmArray(functionCall.ArrayParameter.Name);
			}
			return functionCall;
		}

		private FsmProperty GetFsmProperty(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmProperty();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmPropertyParams == null || fsmPropertyParams.Count <= num)
			{
				return new FsmProperty();
			}
			FsmProperty fsmProperty = fsmPropertyParams[num];
			if (fsmProperty == null)
			{
				return new FsmProperty();
			}
			if (!string.IsNullOrEmpty(fsmProperty.TargetObject.Name))
			{
				fsmProperty.TargetObject = fsm.GetFsmObject(fsmProperty.TargetObject.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.BoolParameter.Name))
			{
				fsmProperty.BoolParameter = fsm.GetFsmBool(fsmProperty.BoolParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.FloatParameter.Name))
			{
				fsmProperty.FloatParameter = fsm.GetFsmFloat(fsmProperty.FloatParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.GameObjectParameter.Name))
			{
				fsmProperty.GameObjectParameter = fsm.GetFsmGameObject(fsmProperty.GameObjectParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.IntParameter.Name))
			{
				fsmProperty.IntParameter = fsm.GetFsmInt(fsmProperty.IntParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.MaterialParameter.Name))
			{
				fsmProperty.MaterialParameter = fsm.GetFsmMaterial(fsmProperty.MaterialParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.ObjectParameter.Name))
			{
				fsmProperty.ObjectParameter = fsm.GetFsmObject(fsmProperty.ObjectParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.QuaternionParameter.Name))
			{
				fsmProperty.QuaternionParameter = fsm.GetFsmQuaternion(fsmProperty.QuaternionParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.RectParamater.Name))
			{
				fsmProperty.RectParamater = fsm.GetFsmRect(fsmProperty.RectParamater.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.StringParameter.Name))
			{
				fsmProperty.StringParameter = fsm.GetFsmString(fsmProperty.StringParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.TextureParameter.Name))
			{
				fsmProperty.TextureParameter = fsm.GetFsmTexture(fsmProperty.TextureParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.ColorParameter.Name))
			{
				fsmProperty.ColorParameter = fsm.GetFsmColor(fsmProperty.ColorParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.Vector2Parameter.Name))
			{
				fsmProperty.Vector2Parameter = fsm.GetFsmVector2(fsmProperty.Vector2Parameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.Vector3Parameter.Name))
			{
				fsmProperty.Vector3Parameter = fsm.GetFsmVector3(fsmProperty.Vector3Parameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.EnumParameter.Name))
			{
				fsmProperty.EnumParameter = fsm.GetFsmEnum(fsmProperty.EnumParameter.Name);
			}
			if (!string.IsNullOrEmpty(fsmProperty.ArrayParameter.Name))
			{
				fsmProperty.ArrayParameter = fsm.GetFsmArray(fsmProperty.ArrayParameter.Name);
			}
			return fsmProperty;
		}

		private FsmEventTarget GetFsmEventTarget(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmEventTarget();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmEventTargetParams == null || fsmEventTargetParams.Count <= num)
			{
				return new FsmEventTarget();
			}
			FsmEventTarget fsmEventTarget = fsmEventTargetParams[num];
			if (fsmEventTarget == null)
			{
				return new FsmEventTarget();
			}
			if (!string.IsNullOrEmpty(fsmEventTarget.excludeSelf.Name))
			{
				fsmEventTarget.excludeSelf = fsm.GetFsmBool(fsmEventTarget.excludeSelf.Name);
			}
			string name = fsmEventTarget.gameObject.GameObject.Name;
			if (!string.IsNullOrEmpty(name))
			{
				fsmEventTarget.gameObject.GameObject = fsm.GetFsmGameObject(name);
			}
			if (!string.IsNullOrEmpty(fsmEventTarget.fsmName.Name))
			{
				fsmEventTarget.fsmName = fsm.GetFsmString(fsmEventTarget.fsmName.Name);
			}
			if (!string.IsNullOrEmpty(fsmEventTarget.sendToChildren.Name))
			{
				fsmEventTarget.sendToChildren = fsm.GetFsmBool(fsmEventTarget.sendToChildren.Name);
			}
			return fsmEventTarget;
		}

		private LayoutOption GetLayoutOption(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new LayoutOption();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || layoutOptionParams == null || layoutOptionParams.Count <= num)
			{
				return new LayoutOption();
			}
			LayoutOption layoutOption = layoutOptionParams[num];
			if (layoutOption == null)
			{
				return new LayoutOption();
			}
			if (!string.IsNullOrEmpty(layoutOption.boolParam.Name))
			{
				layoutOption.boolParam = fsm.GetFsmBool(layoutOption.boolParam.Name);
			}
			if (!string.IsNullOrEmpty(layoutOption.floatParam.Name))
			{
				layoutOption.floatParam = fsm.GetFsmFloat(layoutOption.floatParam.Name);
			}
			return layoutOption;
		}

		private FsmOwnerDefault GetFsmOwnerDefault(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmOwnerDefault();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmOwnerDefaultParams == null || fsmOwnerDefaultParams.Count <= num)
			{
				return new FsmOwnerDefault();
			}
			FsmOwnerDefault fsmOwnerDefault = fsmOwnerDefaultParams[num];
			if (fsmOwnerDefault == null)
			{
				return new FsmOwnerDefault();
			}
			if (fsmOwnerDefault.OwnerOption != OwnerDefaultOption.UseOwner)
			{
				string name = fsmOwnerDefault.GameObject.Name;
				if (!string.IsNullOrEmpty(name))
				{
					fsmOwnerDefault.GameObject = fsm.GetFsmGameObject(name);
				}
			}
			return fsmOwnerDefault;
		}

		private FsmString GetFsmString(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmString();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmStringParams == null || fsmStringParams.Count <= num)
			{
				return new FsmString();
			}
			FsmString fsmString = fsmStringParams[paramDataPos[paramIndex]];
			if (fsmString == null)
			{
				return new FsmString();
			}
			if (string.IsNullOrEmpty(fsmString.Name))
			{
				return fsmString;
			}
			return fsm.GetFsmString(fsmString.Name);
		}

		private FsmObject GetFsmObject(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmObject();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmObjectParams == null || fsmObjectParams.Count <= num)
			{
				return new FsmObject();
			}
			FsmObject fsmObject = fsmObjectParams[num];
			if (fsmObject == null)
			{
				return new FsmObject();
			}
			if (string.IsNullOrEmpty(fsmObject.Name))
			{
				return fsmObject;
			}
			return fsm.GetFsmObject(fsmObject.Name);
		}

		private FsmMaterial GetFsmMaterial(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmMaterial();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmObjectParams == null || fsmObjectParams.Count <= num)
			{
				return new FsmMaterial();
			}
			FsmObject fsmObject = fsmObjectParams[num];
			if (fsmObject == null)
			{
				return new FsmMaterial();
			}
			if (string.IsNullOrEmpty(fsmObject.Name))
			{
				return new FsmMaterial(fsmObject);
			}
			return fsm.GetFsmMaterial(fsmObject.Name);
		}

		private FsmTexture GetFsmTexture(Fsm fsm, int paramIndex)
		{
			if (paramIndex < 0 || paramIndex >= paramDataPos.Count)
			{
				return new FsmTexture();
			}
			int num = paramDataPos[paramIndex];
			if (num < 0 || fsmObjectParams == null || fsmObjectParams.Count <= num)
			{
				return new FsmTexture();
			}
			FsmObject fsmObject = fsmObjectParams[paramDataPos[paramIndex]];
			if (fsmObject == null)
			{
				return new FsmTexture();
			}
			if (string.IsNullOrEmpty(fsmObject.Name))
			{
				return new FsmTexture(fsmObject);
			}
			return fsm.GetFsmTexture(fsmObject.Name);
		}

		public bool UsesDataVersion2()
		{
			if ((fsmArrayParams == null || fsmArrayParams.Count <= 0) && (fsmEnumParams == null || fsmEnumParams.Count <= 0) && (fsmFloatParams == null || fsmFloatParams.Count <= 0) && (fsmIntParams == null || fsmIntParams.Count <= 0) && (fsmBoolParams == null || fsmBoolParams.Count <= 0) && (fsmVector2Params == null || fsmVector2Params.Count <= 0) && (fsmVector3Params == null || fsmVector3Params.Count <= 0) && (fsmColorParams == null || fsmColorParams.Count <= 0) && (fsmRectParams == null || fsmRectParams.Count <= 0) && (fsmQuaternionParams == null || fsmQuaternionParams.Count <= 0))
			{
				if (stringParams != null)
				{
					return stringParams.Count > 0;
				}
				return false;
			}
			return true;
		}

		private static string TryFixActionName(string actionName)
		{
			if (actionName == "HutongGames.PlayMaker.Actions.ScreenWrap2d")
			{
				return "HutongGames.PlayMaker.Actions.ScreenWrap";
			}
			if (actionName == "HutongGames.PlayMaker.Actions.FloatAddMutiple")
			{
				return "HutongGames.PlayMaker.Actions.FloatAddMultiple";
			}
			return "HutongGames.PlayMaker.Actions." + actionName;
		}

		private FsmStateAction TryRecoverAction(Context context, Type actionType, FsmStateAction action, int actionIndex)
		{
			UsedIndices.Clear();
			LoadedFields.Clear();
			int num = actionStartIndex[actionIndex];
			int num2 = ((actionIndex < actionNames.Count - 1) ? actionStartIndex[actionIndex + 1] : paramDataPos.Count);
			if (paramName.Count == paramDataPos.Count)
			{
				for (int i = num; i < num2; i++)
				{
					FieldInfo fieldInfo = FindField(actionType, i);
					if ((object)fieldInfo != null)
					{
						nextParamIndex = i;
						LoadActionField(context.currentFsm, action, fieldInfo, i);
						UsedIndices.Add(i);
						LoadedFields.Add(fieldInfo);
					}
				}
				for (int j = num; j < num2; j++)
				{
					if (UsedIndices.Contains(j))
					{
						continue;
					}
					FieldInfo fieldInfo2 = FindField(actionType, paramName[j]);
					if ((object)fieldInfo2 != null)
					{
						nextParamIndex = j;
						if (TryConvertParameter(context, action, fieldInfo2, j))
						{
							UsedIndices.Add(j);
							LoadedFields.Add(fieldInfo2);
						}
					}
				}
			}
			FieldInfo[] fields = GetFields(actionType);
			int num3 = fields.Length;
			if (num3 > LoadedFields.Count)
			{
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo3 in array)
				{
					if (!LoadedFields.Contains(fieldInfo3))
					{
						LogInfo(context, "New parameter: " + fieldInfo3.Name + " (set to default value).");
					}
				}
			}
			else if (num2 - num > num3)
			{
				for (int l = num; l < num2; l++)
				{
					bool flag = false;
					string text = paramName[l];
					FieldInfo[] array = fields;
					foreach (FieldInfo fieldInfo4 in array)
					{
						if (!(text != fieldInfo4.Name))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						LogInfo(context, "Parameter removed: " + text);
					}
				}
			}
			return action;
		}

		private FieldInfo FindField(Type actionType, int paramIndex)
		{
			string text = paramName[paramIndex];
			ParamDataType paramDataType = this.paramDataType[paramIndex];
			FieldInfo[] fields = GetFields(actionType);
			foreach (FieldInfo fieldInfo in fields)
			{
				ParamDataType paramDataType2 = GetParamDataType(fieldInfo.FieldType);
				if (!(fieldInfo.Name == text) || paramDataType2 != paramDataType || LoadedFields.Contains(fieldInfo))
				{
					continue;
				}
				if (paramDataType2 == ParamDataType.Array)
				{
					Type elementType = fieldInfo.GetType().GetElementType();
					if ((object)elementType == null)
					{
						return null;
					}
					_ = arrayParamTypes[paramIndex] == elementType.FullName;
					return fieldInfo;
				}
				return fieldInfo;
			}
			return null;
		}

		private static FieldInfo FindField(Type actionType, string name)
		{
			FieldInfo[] fields = GetFields(actionType);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == name && !LoadedFields.Contains(fieldInfo))
				{
					return fieldInfo;
				}
			}
			return null;
		}

		private bool TryConvertParameter(Context context, FsmStateAction action, FieldInfo field, int paramIndex)
		{
			Type fieldType = field.FieldType;
			ParamDataType paramDataType = this.paramDataType[paramIndex];
			ParamDataType paramDataType2 = GetParamDataType(fieldType);
			if (paramDataType2 != ParamDataType.Array && paramDataType == paramDataType2)
			{
				LoadActionField(context.currentFsm, action, field, paramIndex);
			}
			else if (paramDataType == ParamDataType.Enum && paramDataType2 == ParamDataType.FsmEnum)
			{
				LogInfo(context, field.Name + ": Upgraded from Enum to FsmEnum");
				object[] customAttributes = field.GetCustomAttributes(inherit: true);
				Type enumType = typeof(Enum);
				object[] array = customAttributes;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] is ObjectTypeAttribute objectTypeAttribute)
					{
						enumType = objectTypeAttribute.ObjectType;
						break;
					}
				}
				field.SetValue(action, new FsmEnum("", enumType, FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex])));
			}
			if (paramDataType == ParamDataType.String && paramDataType2 == ParamDataType.FsmString)
			{
				LogInfo(context, field.Name + ": Upgraded from string to FsmString");
				if (context.currentFsm.DataVersion > 1 && stringParams != null && stringParams.Count > 0)
				{
					field.SetValue(action, new FsmString
					{
						Value = GetStringParam(paramIndex)
					});
				}
				else
				{
					field.SetValue(action, new FsmString
					{
						Value = FsmUtility.ByteArrayToString(byteDataAsArray, paramDataPos[paramIndex], paramByteDataSize[paramIndex])
					});
				}
			}
			else if (paramDataType == ParamDataType.Integer && paramDataType2 == ParamDataType.FsmInt)
			{
				LogInfo(context, field.Name + ": Upgraded from int to FsmInt");
				field.SetValue(action, new FsmInt
				{
					Value = FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.Float && paramDataType2 == ParamDataType.FsmFloat)
			{
				LogInfo(context, field.Name + ": Upgraded from float to FsmFloat");
				field.SetValue(action, new FsmFloat
				{
					Value = FsmUtility.BitConverter.ToSingle(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.Boolean && paramDataType2 == ParamDataType.FsmBool)
			{
				LogInfo(context, field.Name + ": Upgraded from bool to FsmBool");
				field.SetValue(action, new FsmBool
				{
					Value = FsmUtility.BitConverter.ToBoolean(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.GameObject && paramDataType2 == ParamDataType.FsmGameObject)
			{
				LogInfo(context, field.Name + ": Upgraded from from GameObject to FsmGameObject");
				field.SetValue(action, new FsmGameObject
				{
					Value = (GameObject)unityObjectParams[paramDataPos[paramIndex]]
				});
			}
			else if (paramDataType == ParamDataType.GameObject && paramDataType2 == ParamDataType.FsmOwnerDefault)
			{
				LogInfo(context, field.Name + ": Upgraded from GameObject to FsmOwnerDefault");
				FsmOwnerDefault value = new FsmOwnerDefault
				{
					GameObject = new FsmGameObject
					{
						Value = (GameObject)unityObjectParams[paramDataPos[paramIndex]]
					}
				};
				field.SetValue(action, value);
			}
			else if (paramDataType == ParamDataType.FsmGameObject && paramDataType2 == ParamDataType.FsmOwnerDefault)
			{
				LogInfo(context, field.Name + ": Converted from FsmGameObject to FsmOwnerDefault");
				FsmOwnerDefault value2 = new FsmOwnerDefault
				{
					GameObject = fsmGameObjectParams[paramDataPos[paramIndex]],
					OwnerOption = OwnerDefaultOption.SpecifyGameObject
				};
				field.SetValue(action, value2);
			}
			else if (paramDataType == ParamDataType.Vector3 && paramDataType2 == ParamDataType.FsmVector3)
			{
				LogInfo(context, field.Name + ": Upgraded from Vector3 to FsmVector3");
				field.SetValue(action, new FsmVector3
				{
					Value = FsmUtility.ByteArrayToVector3(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.Vector2 && paramDataType2 == ParamDataType.FsmVector2)
			{
				LogInfo(context, field.Name + ": Upgraded from Vector2 to FsmVector2");
				field.SetValue(action, new FsmVector2
				{
					Value = FsmUtility.ByteArrayToVector2(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.Rect && paramDataType2 == ParamDataType.FsmRect)
			{
				LogInfo(context, field.Name + ": Upgraded from Rect to FsmRect");
				field.SetValue(action, new FsmRect
				{
					Value = FsmUtility.ByteArrayToRect(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.Quaternion && paramDataType2 == ParamDataType.Quaternion)
			{
				LogInfo(context, field.Name + ": Upgraded from Quaternion to FsmQuaternion");
				field.SetValue(action, new FsmQuaternion
				{
					Value = FsmUtility.ByteArrayToQuaternion(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType == ParamDataType.Color && paramDataType2 == ParamDataType.FsmColor)
			{
				LogInfo(context, field.Name + ": Upgraded from Color to FsmColor");
				field.SetValue(action, new FsmColor
				{
					Value = FsmUtility.ByteArrayToColor(byteDataAsArray, paramDataPos[paramIndex])
				});
			}
			else if (paramDataType2 == ParamDataType.FsmMaterial && paramDataType == ParamDataType.ObjectReference)
			{
				LogInfo(context, field.Name + ": Upgraded from Material to FsmMaterial");
				field.SetValue(action, new FsmMaterial
				{
					Value = (unityObjectParams[paramDataPos[paramIndex]] as Material)
				});
			}
			else if (paramDataType2 == ParamDataType.FsmTexture && paramDataType == ParamDataType.ObjectReference)
			{
				LogInfo(context, field.Name + ": Upgraded from Texture to FsmTexture");
				field.SetValue(action, new FsmTexture
				{
					Value = (unityObjectParams[paramDataPos[paramIndex]] as Texture)
				});
			}
			else if (paramDataType2 == ParamDataType.FsmObject && paramDataType == ParamDataType.ObjectReference)
			{
				LogInfo(context, field.Name + ": Upgraded from Object to FsmObject");
				field.SetValue(action, new FsmObject
				{
					Value = unityObjectParams[paramDataPos[paramIndex]]
				});
			}
			else
			{
				if (paramDataType2 != ParamDataType.Array)
				{
					return false;
				}
				Type globalType = ReflectionUtils.GetGlobalType(arrayParamTypes[paramDataPos[paramIndex]]);
				Type elementType = fieldType.GetElementType();
				if ((object)elementType == null)
				{
					LogError(context, "Could not make array: " + field.Name);
					return false;
				}
				int num = arrayParamSizes[paramDataPos[paramIndex]];
				Array array2 = Array.CreateInstance(elementType, num);
				if ((object)globalType != elementType)
				{
					ParamDataType originalParamType = GetParamDataType(globalType);
					ParamDataType currentParamType = GetParamDataType(elementType);
					for (int j = 0; j < num; j++)
					{
						nextParamIndex++;
						if (!TryConvertArrayElement(context.currentFsm, array2, originalParamType, currentParamType, j, nextParamIndex))
						{
							LogError(context, "Failed to convert Array: " + field.Name + " From: " + originalParamType.ToString() + " To: " + currentParamType);
							return false;
						}
						LogInfo(context, field.Name + ": Upgraded Array from " + globalType.FullName + " to " + currentParamType);
					}
				}
				else
				{
					for (int k = 0; k < num; k++)
					{
						nextParamIndex++;
						LoadArrayElement(context.currentFsm, array2, globalType, k, nextParamIndex);
					}
				}
				field.SetValue(action, array2);
			}
			return true;
		}

		private bool TryConvertArrayElement(Fsm fsm, Array field, ParamDataType originalParamType, ParamDataType currentParamType, int elementIndex, int paramIndex)
		{
			if (elementIndex >= field.GetLength(0))
			{
				UnityEngine.Debug.LogError("Bad array index: " + elementIndex);
				return false;
			}
			if (paramIndex >= paramDataPos.Count)
			{
				UnityEngine.Debug.LogError("Bad param index: " + paramIndex);
				return false;
			}
			object obj = ConvertType(fsm, originalParamType, currentParamType, paramIndex);
			if (obj == null)
			{
				return false;
			}
			field.SetValue(obj, elementIndex);
			return true;
		}

		private object ConvertType(Fsm fsm, ParamDataType originalParamType, ParamDataType currentParamType, int paramIndex)
		{
			if (originalParamType == ParamDataType.String && currentParamType == ParamDataType.FsmString)
			{
				string value = ((fsm.DataVersion <= 1 || stringParams == null || stringParams.Count <= 0) ? FsmUtility.ByteArrayToString(byteDataAsArray, paramDataPos[paramIndex], paramByteDataSize[paramIndex]) : stringParams[paramDataPos[paramIndex]]);
				return new FsmString
				{
					Value = value
				};
			}
			if (originalParamType == ParamDataType.Integer && currentParamType == ParamDataType.FsmInt)
			{
				return new FsmInt
				{
					Value = FsmUtility.BitConverter.ToInt32(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.Float && currentParamType == ParamDataType.FsmFloat)
			{
				return new FsmFloat
				{
					Value = FsmUtility.BitConverter.ToSingle(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.Boolean && currentParamType == ParamDataType.FsmBool)
			{
				return new FsmBool
				{
					Value = FsmUtility.BitConverter.ToBoolean(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.GameObject && currentParamType == ParamDataType.FsmGameObject)
			{
				return new FsmGameObject
				{
					Value = (GameObject)unityObjectParams[paramDataPos[paramIndex]]
				};
			}
			if (originalParamType == ParamDataType.GameObject && currentParamType == ParamDataType.FsmOwnerDefault)
			{
				return new FsmOwnerDefault
				{
					GameObject = new FsmGameObject
					{
						Value = (GameObject)unityObjectParams[paramDataPos[paramIndex]]
					},
					OwnerOption = OwnerDefaultOption.SpecifyGameObject
				};
			}
			if (originalParamType == ParamDataType.FsmGameObject && currentParamType == ParamDataType.FsmOwnerDefault)
			{
				return new FsmOwnerDefault
				{
					GameObject = fsmGameObjectParams[paramDataPos[paramIndex]],
					OwnerOption = OwnerDefaultOption.SpecifyGameObject
				};
			}
			if (originalParamType == ParamDataType.Vector2 && currentParamType == ParamDataType.FsmVector2)
			{
				return new FsmVector2
				{
					Value = FsmUtility.ByteArrayToVector2(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.Vector3 && currentParamType == ParamDataType.FsmVector3)
			{
				return new FsmVector3
				{
					Value = FsmUtility.ByteArrayToVector3(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.Rect && currentParamType == ParamDataType.FsmRect)
			{
				return new FsmRect
				{
					Value = FsmUtility.ByteArrayToRect(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.Quaternion && currentParamType == ParamDataType.Quaternion)
			{
				return new FsmQuaternion
				{
					Value = FsmUtility.ByteArrayToQuaternion(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (originalParamType == ParamDataType.Color && currentParamType == ParamDataType.FsmColor)
			{
				return new FsmColor
				{
					Value = FsmUtility.ByteArrayToColor(byteDataAsArray, paramDataPos[paramIndex])
				};
			}
			if (currentParamType == ParamDataType.FsmMaterial && originalParamType == ParamDataType.ObjectReference)
			{
				return new FsmMaterial
				{
					Value = (unityObjectParams[paramDataPos[paramIndex]] as Material)
				};
			}
			if (currentParamType == ParamDataType.FsmTexture && originalParamType == ParamDataType.ObjectReference)
			{
				return new FsmTexture
				{
					Value = (unityObjectParams[paramDataPos[paramIndex]] as Texture)
				};
			}
			if (currentParamType == ParamDataType.FsmObject && originalParamType == ParamDataType.ObjectReference)
			{
				return new FsmObject
				{
					Value = unityObjectParams[paramDataPos[paramIndex]]
				};
			}
			return null;
		}

		public void SaveActions(FsmState state, FsmStateAction[] actions)
		{
			ClearActionData();
			foreach (FsmStateAction action in actions)
			{
				SaveAction(state.Fsm, action);
			}
		}

		private void SaveAction(Fsm fsm, FsmStateAction action)
		{
			if (action != null)
			{
				Type type = action.GetType();
				ActionHashCodeLookup.Remove(type);
				actionNames.Add(type.ToString());
				customNames.Add(action.IsAutoNamed ? "~AutoName" : action.Name);
				actionEnabled.Add(action.Enabled);
				actionIsOpen.Add(action.IsOpen);
				actionStartIndex.Add(nextParamIndex);
				actionHashCodes.Add(GetActionTypeHashCode(type));
				FieldInfo[] fields = GetFields(type);
				foreach (FieldInfo fieldInfo in fields)
				{
					Type fieldType = fieldInfo.FieldType;
					object value = fieldInfo.GetValue(action);
					paramName.Add(fieldInfo.Name);
					SaveActionField(fsm, fieldType, value);
					nextParamIndex++;
				}
			}
		}

		private void SaveActionField(Fsm fsm, Type fieldType, object obj)
		{
			if ((object)fieldType == typeof(FsmAnimationCurve))
			{
				if (animationCurveParams == null)
				{
					animationCurveParams = new List<FsmAnimationCurve>();
				}
				paramDataType.Add(ParamDataType.FsmAnimationCurve);
				paramDataPos.Add(animationCurveParams.Count);
				paramByteDataSize.Add(0);
				animationCurveParams.Add(obj as FsmAnimationCurve);
			}
			else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
			{
				if (unityObjectParams == null)
				{
					unityObjectParams = new List<UnityEngine.Object>();
				}
				paramDataType.Add(((object)fieldType == typeof(GameObject)) ? ParamDataType.GameObject : ParamDataType.ObjectReference);
				paramDataPos.Add(unityObjectParams.Count);
				paramByteDataSize.Add(0);
				unityObjectParams.Add(obj as UnityEngine.Object);
			}
			else if ((object)fieldType == typeof(FunctionCall))
			{
				if (functionCallParams == null)
				{
					functionCallParams = new List<FunctionCall>();
				}
				paramDataType.Add(ParamDataType.FunctionCall);
				paramDataPos.Add(functionCallParams.Count);
				paramByteDataSize.Add(0);
				functionCallParams.Add(obj as FunctionCall);
			}
			else if ((object)fieldType == typeof(FsmTemplateControl))
			{
				if (fsmTemplateControlParams == null)
				{
					fsmTemplateControlParams = new List<FsmTemplateControl>();
				}
				paramDataType.Add(ParamDataType.FsmTemplateControl);
				paramDataPos.Add(fsmTemplateControlParams.Count);
				paramByteDataSize.Add(0);
				fsmTemplateControlParams.Add(obj as FsmTemplateControl);
			}
			else if ((object)fieldType == typeof(FsmVar))
			{
				if (fsmVarParams == null)
				{
					fsmVarParams = new List<FsmVar>();
				}
				paramDataType.Add(ParamDataType.FsmVar);
				paramDataPos.Add(fsmVarParams.Count);
				paramByteDataSize.Add(0);
				fsmVarParams.Add(obj as FsmVar);
			}
			else if ((object)fieldType == typeof(FsmProperty))
			{
				if (fsmPropertyParams == null)
				{
					fsmPropertyParams = new List<FsmProperty>();
				}
				paramDataType.Add(ParamDataType.FsmProperty);
				paramDataPos.Add(fsmPropertyParams.Count);
				paramByteDataSize.Add(0);
				fsmPropertyParams.Add(obj as FsmProperty);
			}
			else if ((object)fieldType == typeof(FsmEventTarget))
			{
				if (fsmEventTargetParams == null)
				{
					fsmEventTargetParams = new List<FsmEventTarget>();
				}
				paramDataType.Add(ParamDataType.FsmEventTarget);
				paramDataPos.Add(fsmEventTargetParams.Count);
				paramByteDataSize.Add(0);
				fsmEventTargetParams.Add(obj as FsmEventTarget);
			}
			else if ((object)fieldType == typeof(LayoutOption))
			{
				if (layoutOptionParams == null)
				{
					layoutOptionParams = new List<LayoutOption>();
				}
				paramDataType.Add(ParamDataType.LayoutOption);
				paramDataPos.Add(layoutOptionParams.Count);
				paramByteDataSize.Add(0);
				layoutOptionParams.Add(obj as LayoutOption);
			}
			else if ((object)fieldType == typeof(FsmGameObject))
			{
				if (fsmGameObjectParams == null)
				{
					fsmGameObjectParams = new List<FsmGameObject>();
				}
				paramDataType.Add(ParamDataType.FsmGameObject);
				paramDataPos.Add(fsmGameObjectParams.Count);
				paramByteDataSize.Add(0);
				fsmGameObjectParams.Add(obj as FsmGameObject);
			}
			else if ((object)fieldType == typeof(FsmOwnerDefault))
			{
				if (fsmOwnerDefaultParams == null)
				{
					fsmOwnerDefaultParams = new List<FsmOwnerDefault>();
				}
				paramDataType.Add(ParamDataType.FsmOwnerDefault);
				paramDataPos.Add(fsmOwnerDefaultParams.Count);
				paramByteDataSize.Add(0);
				fsmOwnerDefaultParams.Add(obj as FsmOwnerDefault);
			}
			else if ((object)fieldType == typeof(FsmString))
			{
				if (fsmStringParams == null)
				{
					fsmStringParams = new List<FsmString>();
				}
				paramDataType.Add(ParamDataType.FsmString);
				paramDataPos.Add(fsmStringParams.Count);
				paramByteDataSize.Add(0);
				fsmStringParams.Add(obj as FsmString);
			}
			else
			{
				if (fieldType.IsArray)
				{
					Type elementType = fieldType.GetElementType();
					if ((object)elementType == null)
					{
						return;
					}
					Array array = ((obj == null) ? Array.CreateInstance(elementType, 0) : ((Array)obj));
					if (arrayParamSizes == null)
					{
						arrayParamSizes = new List<int>();
						arrayParamTypes = new List<string>();
					}
					paramDataType.Add(ParamDataType.Array);
					paramDataPos.Add(arrayParamSizes.Count);
					paramByteDataSize.Add(0);
					arrayParamSizes.Add(array.Length);
					arrayParamTypes.Add(elementType.FullName);
					{
						foreach (object item in array)
						{
							nextParamIndex++;
							paramName.Add("");
							SaveActionField(fsm, elementType, item);
						}
						return;
					}
				}
				if ((object)fieldType == typeof(float))
				{
					paramDataType.Add(ParamDataType.Float);
					AddByteData(FsmUtility.BitConverter.GetBytes((float)obj));
				}
				else if ((object)fieldType == typeof(int))
				{
					paramDataType.Add(ParamDataType.Integer);
					AddByteData(FsmUtility.BitConverter.GetBytes((int)obj));
				}
				else if ((object)fieldType == typeof(bool))
				{
					paramDataType.Add(ParamDataType.Boolean);
					AddByteData(FsmUtility.BitConverter.GetBytes((bool)obj));
				}
				else if ((object)fieldType == typeof(Color))
				{
					paramDataType.Add(ParamDataType.Color);
					AddByteData(FsmUtility.ColorToByteArray((Color)obj));
				}
				else if ((object)fieldType == typeof(Vector2))
				{
					paramDataType.Add(ParamDataType.Vector2);
					AddByteData(FsmUtility.Vector2ToByteArray((Vector2)obj));
				}
				else if ((object)fieldType == typeof(Vector3))
				{
					paramDataType.Add(ParamDataType.Vector3);
					AddByteData(FsmUtility.Vector3ToByteArray((Vector3)obj));
				}
				else if ((object)fieldType == typeof(Vector4))
				{
					paramDataType.Add(ParamDataType.Vector4);
					AddByteData(FsmUtility.Vector4ToByteArray((Vector4)obj));
				}
				else if ((object)fieldType == typeof(Rect))
				{
					paramDataType.Add(ParamDataType.Rect);
					AddByteData(FsmUtility.RectToByteArray((Rect)obj));
				}
				else if ((object)fieldType == typeof(FsmFloat))
				{
					paramDataType.Add(ParamDataType.FsmFloat);
					if (fsm.DataVersion > 1)
					{
						if (fsmFloatParams == null)
						{
							fsmFloatParams = new List<FsmFloat>();
						}
						paramDataPos.Add(fsmFloatParams.Count);
						paramByteDataSize.Add(0);
						fsmFloatParams.Add(obj as FsmFloat);
					}
					else
					{
						AddByteData(FsmUtility.FsmFloatToByteArray(obj as FsmFloat));
					}
				}
				else if ((object)fieldType == typeof(FsmInt))
				{
					paramDataType.Add(ParamDataType.FsmInt);
					if (fsm.DataVersion > 1)
					{
						if (fsmIntParams == null)
						{
							fsmIntParams = new List<FsmInt>();
						}
						paramDataPos.Add(fsmIntParams.Count);
						paramByteDataSize.Add(0);
						fsmIntParams.Add(obj as FsmInt);
					}
					else
					{
						AddByteData(FsmUtility.FsmIntToByteArray(obj as FsmInt));
					}
				}
				else if ((object)fieldType == typeof(FsmBool))
				{
					paramDataType.Add(ParamDataType.FsmBool);
					if (fsm.DataVersion > 1)
					{
						if (fsmBoolParams == null)
						{
							fsmBoolParams = new List<FsmBool>();
						}
						paramDataPos.Add(fsmBoolParams.Count);
						paramByteDataSize.Add(0);
						fsmBoolParams.Add(obj as FsmBool);
					}
					else
					{
						AddByteData(FsmUtility.FsmBoolToByteArray(obj as FsmBool));
					}
				}
				else if ((object)fieldType == typeof(FsmVector2))
				{
					paramDataType.Add(ParamDataType.FsmVector2);
					if (fsm.DataVersion > 1)
					{
						if (fsmVector2Params == null)
						{
							fsmVector2Params = new List<FsmVector2>();
						}
						paramDataPos.Add(fsmVector2Params.Count);
						paramByteDataSize.Add(0);
						fsmVector2Params.Add(obj as FsmVector2);
					}
					else
					{
						AddByteData(FsmUtility.FsmVector2ToByteArray(obj as FsmVector2));
					}
				}
				else if ((object)fieldType == typeof(FsmVector3))
				{
					paramDataType.Add(ParamDataType.FsmVector3);
					if (fsm.DataVersion > 1)
					{
						if (fsmVector3Params == null)
						{
							fsmVector3Params = new List<FsmVector3>();
						}
						paramDataPos.Add(fsmVector3Params.Count);
						paramByteDataSize.Add(0);
						fsmVector3Params.Add(obj as FsmVector3);
					}
					else
					{
						AddByteData(FsmUtility.FsmVector3ToByteArray(obj as FsmVector3));
					}
				}
				else if ((object)fieldType == typeof(FsmRect))
				{
					paramDataType.Add(ParamDataType.FsmRect);
					if (fsm.DataVersion > 1)
					{
						if (fsmRectParams == null)
						{
							fsmRectParams = new List<FsmRect>();
						}
						paramDataPos.Add(fsmRectParams.Count);
						paramByteDataSize.Add(0);
						fsmRectParams.Add(obj as FsmRect);
					}
					else
					{
						AddByteData(FsmUtility.FsmRectToByteArray(obj as FsmRect));
					}
				}
				else if ((object)fieldType == typeof(FsmQuaternion))
				{
					paramDataType.Add(ParamDataType.FsmQuaternion);
					if (fsm.DataVersion > 1)
					{
						if (fsmQuaternionParams == null)
						{
							fsmQuaternionParams = new List<FsmQuaternion>();
						}
						paramDataPos.Add(fsmQuaternionParams.Count);
						paramByteDataSize.Add(0);
						fsmQuaternionParams.Add(obj as FsmQuaternion);
					}
					else
					{
						AddByteData(FsmUtility.FsmQuaternionToByteArray(obj as FsmQuaternion));
					}
				}
				else if ((object)fieldType == typeof(FsmColor))
				{
					paramDataType.Add(ParamDataType.FsmColor);
					if (fsm.DataVersion > 1)
					{
						if (fsmColorParams == null)
						{
							fsmColorParams = new List<FsmColor>();
						}
						paramDataPos.Add(fsmColorParams.Count);
						paramByteDataSize.Add(0);
						fsmColorParams.Add(obj as FsmColor);
					}
					else
					{
						AddByteData(FsmUtility.FsmColorToByteArray(obj as FsmColor));
					}
				}
				else if ((object)fieldType == typeof(FsmEvent))
				{
					paramDataType.Add(ParamDataType.FsmEvent);
					if (fsm.DataVersion > 1)
					{
						SaveString((obj != null) ? ((FsmEvent)obj).Name : string.Empty);
					}
					else
					{
						AddByteData(FsmUtility.FsmEventToByteArray(obj as FsmEvent));
					}
				}
				else if ((object)fieldType == typeof(string))
				{
					paramDataType.Add(ParamDataType.String);
					if (fsm.DataVersion > 1)
					{
						SaveString(obj as string);
					}
					else
					{
						AddByteData(FsmUtility.StringToByteArray(obj as string));
					}
				}
				else if ((object)fieldType == typeof(FsmObject))
				{
					if (fsmObjectParams == null)
					{
						fsmObjectParams = new List<FsmObject>();
					}
					paramDataType.Add(ParamDataType.FsmObject);
					paramDataPos.Add(fsmObjectParams.Count);
					paramByteDataSize.Add(0);
					fsmObjectParams.Add(obj as FsmObject);
				}
				else if ((object)fieldType == typeof(FsmArray))
				{
					if (fsmArrayParams == null)
					{
						fsmArrayParams = new List<FsmArray>();
					}
					paramDataType.Add(ParamDataType.FsmArray);
					paramDataPos.Add(fsmArrayParams.Count);
					paramByteDataSize.Add(0);
					fsmArrayParams.Add(obj as FsmArray);
				}
				else if ((object)fieldType == typeof(FsmEnum))
				{
					if (fsmEnumParams == null)
					{
						fsmEnumParams = new List<FsmEnum>();
					}
					paramDataType.Add(ParamDataType.FsmEnum);
					paramDataPos.Add(fsmEnumParams.Count);
					paramByteDataSize.Add(0);
					fsmEnumParams.Add(obj as FsmEnum);
				}
				else if ((object)fieldType == typeof(FsmMaterial))
				{
					if (fsmObjectParams == null)
					{
						fsmObjectParams = new List<FsmObject>();
					}
					paramDataType.Add(ParamDataType.FsmMaterial);
					paramDataPos.Add(fsmObjectParams.Count);
					paramByteDataSize.Add(0);
					fsmObjectParams.Add(obj as FsmObject);
				}
				else if ((object)fieldType == typeof(FsmTexture))
				{
					if (fsmObjectParams == null)
					{
						fsmObjectParams = new List<FsmObject>();
					}
					paramDataType.Add(ParamDataType.FsmTexture);
					paramDataPos.Add(fsmObjectParams.Count);
					paramByteDataSize.Add(0);
					fsmObjectParams.Add(obj as FsmObject);
				}
				else if (fieldType.IsEnum)
				{
					paramDataType.Add(ParamDataType.Enum);
					AddByteData(FsmUtility.BitConverter.GetBytes(Convert.ToInt32(obj)));
				}
				else if (fieldType.IsClass)
				{
					if (customTypeSizes == null)
					{
						customTypeSizes = new List<int>();
						customTypeNames = new List<string>();
					}
					if (obj == null)
					{
						obj = Activator.CreateInstance(fieldType);
					}
					paramDataType.Add(ParamDataType.CustomClass);
					paramDataPos.Add(customTypeSizes.Count);
					customTypeNames.Add(fieldType.FullName);
					paramByteDataSize.Add(0);
					FieldInfo[] fields = GetFields(fieldType);
					customTypeSizes.Add(fields.Length);
					FieldInfo[] array2 = fields;
					foreach (FieldInfo fieldInfo in array2)
					{
						nextParamIndex++;
						paramName.Add(fieldInfo.Name);
						SaveActionField(fsm, fieldInfo.FieldType, fieldInfo.GetValue(obj));
					}
				}
				else if (obj != null)
				{
					UnityEngine.Debug.LogError("Save Action: Unsupported parameter type: " + fieldType);
					paramDataType.Add(ParamDataType.Unsupported);
					paramDataPos.Add(byteData.Count);
					paramByteDataSize.Add(0);
				}
				else
				{
					paramDataType.Add(ParamDataType.Unsupported);
					paramDataPos.Add(byteData.Count);
					paramByteDataSize.Add(0);
				}
			}
		}

		private void AddByteData(ICollection<byte> bytes)
		{
			paramDataPos.Add(byteData.Count);
			if (bytes != null)
			{
				paramByteDataSize.Add(bytes.Count);
				byteData.AddRange(bytes);
			}
			else
			{
				paramByteDataSize.Add(0);
			}
		}

		private void SaveString(string str)
		{
			if (stringParams == null)
			{
				stringParams = new List<string>();
			}
			paramDataPos.Add(stringParams.Count);
			paramByteDataSize.Add(0);
			stringParams.Add(str ?? string.Empty);
		}

		private static ParamDataType GetParamDataType(Type type)
		{
			if ((object)type == typeof(FsmOwnerDefault))
			{
				return ParamDataType.FsmOwnerDefault;
			}
			if ((object)type == typeof(FsmEventTarget))
			{
				return ParamDataType.FsmEventTarget;
			}
			if ((object)type == typeof(FsmEvent))
			{
				return ParamDataType.FsmEvent;
			}
			if ((object)type == typeof(FsmFloat))
			{
				return ParamDataType.FsmFloat;
			}
			if ((object)type == typeof(FsmInt))
			{
				return ParamDataType.FsmInt;
			}
			if ((object)type == typeof(FsmBool))
			{
				return ParamDataType.FsmBool;
			}
			if ((object)type == typeof(FsmString))
			{
				return ParamDataType.FsmString;
			}
			if ((object)type == typeof(FsmGameObject))
			{
				return ParamDataType.FsmGameObject;
			}
			if ((object)type == typeof(FunctionCall))
			{
				return ParamDataType.FunctionCall;
			}
			if ((object)type == typeof(FsmProperty))
			{
				return ParamDataType.FsmProperty;
			}
			if ((object)type == typeof(FsmVector2))
			{
				return ParamDataType.FsmVector2;
			}
			if ((object)type == typeof(FsmVector3))
			{
				return ParamDataType.FsmVector3;
			}
			if ((object)type == typeof(FsmRect))
			{
				return ParamDataType.FsmRect;
			}
			if ((object)type == typeof(FsmQuaternion))
			{
				return ParamDataType.FsmQuaternion;
			}
			if ((object)type == typeof(FsmObject))
			{
				return ParamDataType.FsmObject;
			}
			if ((object)type == typeof(FsmMaterial))
			{
				return ParamDataType.FsmMaterial;
			}
			if ((object)type == typeof(FsmTexture))
			{
				return ParamDataType.FsmTexture;
			}
			if ((object)type == typeof(FsmColor))
			{
				return ParamDataType.FsmColor;
			}
			if ((object)type == typeof(int))
			{
				return ParamDataType.Integer;
			}
			if ((object)type == typeof(bool))
			{
				return ParamDataType.Boolean;
			}
			if ((object)type == typeof(float))
			{
				return ParamDataType.Float;
			}
			if ((object)type == typeof(string))
			{
				return ParamDataType.String;
			}
			if ((object)type == typeof(Color))
			{
				return ParamDataType.Color;
			}
			if ((object)type == typeof(LayerMask))
			{
				return ParamDataType.LayerMask;
			}
			if ((object)type == typeof(Vector2))
			{
				return ParamDataType.Vector2;
			}
			if ((object)type == typeof(Vector3))
			{
				return ParamDataType.Vector3;
			}
			if ((object)type == typeof(Vector4))
			{
				return ParamDataType.Vector4;
			}
			if ((object)type == typeof(Quaternion))
			{
				return ParamDataType.Quaternion;
			}
			if ((object)type == typeof(Rect))
			{
				return ParamDataType.Rect;
			}
			if ((object)type == typeof(AnimationCurve))
			{
				return ParamDataType.AnimationCurve;
			}
			if ((object)type == typeof(GameObject))
			{
				return ParamDataType.GameObject;
			}
			if ((object)type == typeof(LayoutOption))
			{
				return ParamDataType.LayoutOption;
			}
			if ((object)type == typeof(FsmVar))
			{
				return ParamDataType.FsmVar;
			}
			if ((object)type == typeof(FsmEnum))
			{
				return ParamDataType.FsmEnum;
			}
			if ((object)type == typeof(FsmArray))
			{
				return ParamDataType.FsmArray;
			}
			if ((object)type == typeof(FsmTemplateControl))
			{
				return ParamDataType.FsmTemplateControl;
			}
			if ((object)type == typeof(FsmAnimationCurve))
			{
				return ParamDataType.FsmAnimationCurve;
			}
			if (type.IsArray)
			{
				return ParamDataType.Array;
			}
			if (type.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				return ParamDataType.ObjectReference;
			}
			if (type.IsEnum)
			{
				return ParamDataType.Enum;
			}
			if (type.IsClass)
			{
				return ParamDataType.CustomClass;
			}
			return ParamDataType.Unsupported;
		}

		[Conditional("DEBUG_LOG")]
		private static void DebugLog(object message, LogColor logColor = LogColor.None)
		{
		}

		[Conditional("DEBUG_LOG")]
		private static void DebugLogWarning(object message)
		{
		}
	}
}
