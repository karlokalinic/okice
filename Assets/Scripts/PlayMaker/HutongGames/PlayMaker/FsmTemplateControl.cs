using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmTemplateControl
	{
		public enum TargetType
		{
			FsmTemplate = 0,
			PlayMakerFSM = 1,
			FsmGameObject = 2
		}

		public TargetType targetType;

		[FormerlySerializedAs("fsmTemplate")]
		public UnityEngine.Object target;

		[FormerlySerializedAs("fsmVarOverrides")]
		public FsmVarOverride[] inputVariables;

		public FsmVarOverride[] outputVariables;

		public FsmEventMapping[] outputEvents;

		[NonSerialized]
		private int id;

		[NonSerialized]
		private bool initialized;

		[NonSerialized]
		private bool updateInputs = true;

		[NonSerialized]
		private bool updateOutputs = true;

		[NonSerialized]
		private bool updateEvents = true;

		[NonSerialized]
		private Fsm runFsm;

		public FsmTemplate fsmTemplate
		{
			get
			{
				return target as FsmTemplate;
			}
			set
			{
				target = value;
			}
		}

		public PlayMakerFSM fsmComponent
		{
			get
			{
				return target as PlayMakerFSM;
			}
			set
			{
				target = value;
			}
		}

		public GameObject fsmGameObject
		{
			get
			{
				return target as GameObject;
			}
			set
			{
				target = value;
			}
		}

		public Fsm targetFsm
		{
			get
			{
				if (fsmTemplate != null)
				{
					return fsmTemplate.fsm;
				}
				if (fsmComponent != null)
				{
					return fsmComponent.Fsm;
				}
				return null;
			}
		}

		public FsmVariables fsmVariables
		{
			get
			{
				if (fsmTemplate != null)
				{
					return fsmTemplate.fsm.Variables;
				}
				if (fsmComponent != null)
				{
					return fsmComponent.Fsm.Variables;
				}
				return null;
			}
		}

		public FsmVariables updateVariables
		{
			get
			{
				if (fsmTemplate != null)
				{
					return runFsm.Variables;
				}
				if (fsmComponent != null)
				{
					return fsmComponent.Fsm.Variables;
				}
				return null;
			}
		}

		public int ID
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		public bool ShowInputs => updateInputs;

		public bool ShowOutputs => updateOutputs;

		public bool ShowEvents => updateEvents;

		public Fsm RunFsm
		{
			get
			{
				return runFsm;
			}
			private set
			{
				runFsm = value;
			}
		}

		private FsmTemplateControl()
		{
			updateInputs = true;
			updateOutputs = true;
			updateEvents = true;
		}

		public FsmTemplateControl(TargetType targetType = TargetType.FsmTemplate)
		{
			this.targetType = targetType;
			inputVariables = new FsmVarOverride[0];
			outputVariables = new FsmVarOverride[0];
			outputEvents = new FsmEventMapping[0];
		}

		public FsmTemplateControl(FsmTemplateControl source)
		{
			targetType = source.targetType;
			target = source.target;
			inputVariables = CopyOverrides(source);
			outputVariables = CopyOutputVariables(source);
			outputEvents = CopyOutputEvents(source);
		}

		public void SetFsmTemplate(FsmTemplate template)
		{
			targetType = TargetType.FsmTemplate;
			fsmTemplate = template;
			ResetOverrides();
			InitOverrides();
		}

		public void SetFsmComponent(PlayMakerFSM fsm)
		{
			targetType = TargetType.PlayMakerFSM;
			fsmComponent = fsm;
			ResetOverrides();
			InitOverrides();
		}

		public void SetUpdates(bool inputs, bool outputs, bool events)
		{
			updateInputs = inputs;
			updateOutputs = outputs;
			updateEvents = events;
		}

		public void Reinitialize()
		{
			initialized = false;
		}

		public Fsm InstantiateFsm()
		{
			RunFsm = new Fsm(fsmTemplate.fsm);
			ApplyOverrides(RunFsm);
			return RunFsm;
		}

		public void InitFsm()
		{
			if (updateInputs)
			{
				UpdateInputs();
			}
		}

		public void UpdateInputs()
		{
			UpdateValues();
			ApplyOverrides(fsmComponent.Fsm);
		}

		private static FsmVarOverride[] CopyOverrides(FsmTemplateControl source)
		{
			FsmVarOverride[] array = new FsmVarOverride[source.inputVariables.Length];
			for (int i = 0; i < source.inputVariables.Length; i++)
			{
				array[i] = new FsmVarOverride(source.inputVariables[i]);
			}
			return array;
		}

		private static FsmVarOverride[] CopyOutputVariables(FsmTemplateControl source)
		{
			if (source == null || source.outputVariables == null)
			{
				return new FsmVarOverride[0];
			}
			FsmVarOverride[] array = new FsmVarOverride[source.outputVariables.Length];
			for (int i = 0; i < source.outputVariables.Length; i++)
			{
				array[i] = new FsmVarOverride(source.outputVariables[i]);
			}
			return array;
		}

		private static FsmEventMapping[] CopyOutputEvents(FsmTemplateControl source)
		{
			if (source == null || source.outputEvents == null)
			{
				return new FsmEventMapping[0];
			}
			FsmEventMapping[] array = new FsmEventMapping[source.outputEvents.Length];
			for (int i = 0; i < source.outputEvents.Length; i++)
			{
				array[i] = new FsmEventMapping(source.outputEvents[i]);
			}
			return array;
		}

		private void ResetOverrides()
		{
			if (inputVariables == null || inputVariables.Length != 0)
			{
				inputVariables = new FsmVarOverride[0];
			}
			if (outputVariables == null || outputVariables.Length != 0)
			{
				outputVariables = new FsmVarOverride[0];
			}
			if (outputEvents == null || outputEvents.Length != 0)
			{
				outputEvents = new FsmEventMapping[0];
			}
		}

		public void Init()
		{
			if (!initialized)
			{
				InitOverrides();
				initialized = true;
			}
		}

		private void InitOverrides()
		{
			if (target == null)
			{
				ResetOverrides();
				return;
			}
			if (updateInputs || updateOutputs)
			{
				NamedVariable[] allNamedVariables = fsmVariables.GetAllNamedVariables();
				if (updateInputs)
				{
					List<FsmVarOverride> list = new List<FsmVarOverride>(inputVariables);
					List<FsmVarOverride> list2 = new List<FsmVarOverride>();
					NamedVariable[] array = allNamedVariables;
					foreach (NamedVariable namedVariable in array)
					{
						if (namedVariable != null && namedVariable.ShowInInspector)
						{
							FsmVarOverride fsmVarOverride = list.Find((FsmVarOverride o) => o != null && o.variable != null && o.variable.Name == namedVariable.Name);
							list2.Add(fsmVarOverride ?? new FsmVarOverride(namedVariable, ""));
						}
					}
					inputVariables = list2.ToArray();
				}
				if (updateOutputs)
				{
					NamedVariable[] array2 = targetFsm.GetOutputVariables();
					List<FsmVarOverride> list3 = new List<FsmVarOverride>(outputVariables);
					List<FsmVarOverride> list4 = new List<FsmVarOverride>();
					NamedVariable[] array = array2;
					foreach (NamedVariable namedVariable2 in array)
					{
						FsmVarOverride fsmVarOverride2 = list3.Find((FsmVarOverride o) => o.variable.Name == namedVariable2.Name);
						list4.Add(fsmVarOverride2 ?? new FsmVarOverride(namedVariable2, ""));
					}
					outputVariables = list4.ToArray();
				}
			}
			if (!updateEvents)
			{
				return;
			}
			if (outputEvents == null)
			{
				outputEvents = new FsmEventMapping[0];
			}
			List<FsmEventMapping> list5 = new List<FsmEventMapping>(outputEvents);
			List<FsmEventMapping> list6 = new List<FsmEventMapping>();
			foreach (FsmEvent outputEvent in targetFsm.OutputEvents)
			{
				FsmEventMapping fsmEventMapping = list5.Find((FsmEventMapping o) => o.fromEvent.Name == outputEvent.Name);
				list6.Add((fsmEventMapping ?? new FsmEventMapping(outputEvent, null)).Init());
			}
			outputEvents = list6.ToArray();
		}

		public void UpdateValues()
		{
			FsmVarOverride[] array = inputVariables;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].fsmVar.UpdateValue();
			}
		}

		public void ApplyOverrides(Fsm overrideFsm)
		{
			FsmVarOverride[] array = inputVariables;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(overrideFsm.Variables);
			}
		}

		public void UpdateOutput(Fsm fsm)
		{
			FsmVarOverride[] array = outputVariables;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Update(updateVariables, fsm.Variables);
			}
		}

		public FsmEvent MapEvent(FsmEvent fsmEvent)
		{
			FsmEventMapping[] array = outputEvents;
			foreach (FsmEventMapping fsmEventMapping in array)
			{
				if (fsmEventMapping.fromEvent == fsmEvent)
				{
					return fsmEventMapping.toEvent;
				}
			}
			return null;
		}

		[Conditional("DEBUG_LOG")]
		private void DebugLog(object message, LogColor logColor = LogColor.None)
		{
		}
	}
}
