using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using HutongGames;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using UnityEngine;

[AddComponentMenu("PlayMaker/PlayMakerFSM")]
[HelpURL("https://hutonggames.fogbugz.com/f/page?W1224")]
public class PlayMakerFSM : MonoBehaviour, ISerializationCallbackReceiver
{
	private delegate void AddEventHandlerDelegate(PlayMakerFSM fsm);

	public static Action<string> OnSettingChanged;

	private static readonly List<PlayMakerFSM> fsmList = new List<PlayMakerFSM>();

	public static bool ApplicationIsQuitting;

	[SerializeField]
	private Fsm fsm;

	[SerializeField]
	private FsmTemplate fsmTemplate;

	[SerializeField]
	private bool eventHandlerComponentsAdded;

	public Action OnReset;

	public Action OnValidated;

	private static Thread mainThread = Thread.CurrentThread;

	private AddEventHandlerDelegate addEventHandlers;

	public static string VersionNotes => "";

	public static string VersionLabel => "";

	public static List<PlayMakerFSM> FsmList => fsmList;

	public static bool IsMainThread => Thread.CurrentThread == mainThread;

	public static bool NotMainThread => !IsMainThread;

	public FsmTemplate FsmTemplate => fsmTemplate;

	public static bool DrawGizmos { get; set; }

	private AddEventHandlerDelegate AddEventHandlers
	{
		get
		{
			if (addEventHandlers == null)
			{
				MethodInfo method = ReflectionUtils.GetGlobalType("HutongGames.PlayMaker.FsmProcessor").GetMethod("OnPreprocess");
				addEventHandlers = (AddEventHandlerDelegate)Delegate.CreateDelegate(typeof(AddEventHandlerDelegate), null, method);
			}
			return addEventHandlers;
		}
	}

	public Fsm Fsm
	{
		get
		{
			if (fsm != null)
			{
				fsm.Owner = this;
			}
			return fsm;
		}
		set
		{
			fsm = value;
			fsm.Init(this);
		}
	}

	public string FsmName
	{
		get
		{
			return fsm.Name;
		}
		set
		{
			fsm.Name = value;
		}
	}

	public string FsmDescription
	{
		get
		{
			return fsm.Description;
		}
		set
		{
			fsm.Description = value;
		}
	}

	public bool Active => fsm.Active;

	public string ActiveStateName
	{
		get
		{
			if (fsm.ActiveState != null)
			{
				return fsm.ActiveState.Name;
			}
			return "";
		}
	}

	public FsmState[] FsmStates => fsm.States;

	public FsmEvent[] FsmEvents => fsm.Events;

	public FsmTransition[] FsmGlobalTransitions => fsm.GlobalTransitions;

	public FsmVariables FsmVariables => fsm.Variables;

	public bool UsesTemplate => fsmTemplate != null;

	[UsedImplicitly]
	[ContextMenu("Show Full FSM Inspector")]
	public void ShowFullFsmInspector()
	{
		if (OnSettingChanged != null)
		{
			OnSettingChanged("ShowFullFsmInspector");
		}
	}

	public static PlayMakerFSM FindFsmOnGameObject(GameObject go, string fsmName)
	{
		foreach (PlayMakerFSM fsm in fsmList)
		{
			if (fsm.gameObject == go && fsm.FsmName == fsmName)
			{
				return fsm;
			}
		}
		return null;
	}

	public static void InitInEditor()
	{
		fsmList?.Clear();
		ApplicationIsQuitting = false;
	}

	public void Reset()
	{
		if (fsm == null)
		{
			fsm = new Fsm();
		}
		fsmTemplate = null;
		fsm.Reset(this);
		if (OnReset != null)
		{
			OnReset();
		}
	}

	private void OnValidate()
	{
		if (OnValidated != null)
		{
			OnValidated();
		}
	}

	private void Awake()
	{
		if (Application.isEditor && Fsm != null)
		{
			Fsm.InitInEditor();
		}
		mainThread = Thread.CurrentThread;
		PlayMakerGlobals.Initialize();
		if (!PlayMakerGlobals.IsEditor)
		{
			FsmLog.LoggingEnabled = false;
		}
		Init();
	}

	public void Preprocess()
	{
		if (fsmTemplate != null)
		{
			InitTemplate();
		}
		else
		{
			InitFsm();
		}
		fsm.Preprocess(this);
		AddEventHandlerComponents();
	}

	private void Init()
	{
		if (fsmTemplate != null)
		{
			if (Application.isPlaying)
			{
				InitTemplate();
			}
		}
		else
		{
			InitFsm();
		}
		if (PlayMakerGlobals.IsEditor)
		{
			fsm.Preprocessed = false;
			eventHandlerComponentsAdded = false;
		}
		fsm.Init(this);
		if (!eventHandlerComponentsAdded || !fsm.Preprocessed)
		{
			AddEventHandlerComponents();
		}
	}

	private void InitTemplate()
	{
		Fsm fsm = new Fsm(fsmTemplate.fsm, this.fsm.Variables)
		{
			UsedInTemplate = null,
			Name = this.fsm.Name,
			EnableDebugFlow = this.fsm.EnableDebugFlow,
			EnableBreakpoints = this.fsm.EnableBreakpoints,
			ShowStateLabel = this.fsm.ShowStateLabel,
			ControlsIsExpanded = this.fsm.ControlsIsExpanded,
			InputsIsExpanded = this.fsm.InputsIsExpanded,
			OutputsIsExpanded = this.fsm.OutputsIsExpanded,
			EventsIsExpanded = this.fsm.EventsIsExpanded,
			DebugIsExpanded = this.fsm.DebugIsExpanded,
			InfoIsExpanded = this.fsm.InfoIsExpanded
		};
		this.fsm = fsm;
	}

	private void InitFsm()
	{
		if (fsm == null)
		{
			Reset();
		}
		if (fsm == null)
		{
			UnityEngine.Debug.LogError("Could not initialize FSM!");
			base.enabled = false;
		}
	}

	public void AddEventHandlerComponents()
	{
		if (PlayMakerPrefs.LogPerformanceWarnings)
		{
			_ = PlayMakerGlobals.IsEditor;
		}
		if (fsm.MouseEvents)
		{
			AddEventHandlerComponent<PlayMakerMouseEvents>();
		}
		if (fsm.HandleCollisionEnter)
		{
			AddEventHandlerComponent<PlayMakerCollisionEnter>();
		}
		if (fsm.HandleCollisionExit)
		{
			AddEventHandlerComponent<PlayMakerCollisionExit>();
		}
		if (fsm.HandleCollisionStay)
		{
			AddEventHandlerComponent<PlayMakerCollisionStay>();
		}
		if (fsm.HandleTriggerEnter)
		{
			AddEventHandlerComponent<PlayMakerTriggerEnter>();
		}
		if (fsm.HandleTriggerExit)
		{
			AddEventHandlerComponent<PlayMakerTriggerExit>();
		}
		if (fsm.HandleTriggerStay)
		{
			AddEventHandlerComponent<PlayMakerTriggerStay>();
		}
		if (fsm.HandleCollisionEnter2D)
		{
			AddEventHandlerComponent<PlayMakerCollisionEnter2D>();
		}
		if (fsm.HandleCollisionExit2D)
		{
			AddEventHandlerComponent<PlayMakerCollisionExit2D>();
		}
		if (fsm.HandleCollisionStay2D)
		{
			AddEventHandlerComponent<PlayMakerCollisionStay2D>();
		}
		if (fsm.HandleTriggerEnter2D)
		{
			AddEventHandlerComponent<PlayMakerTriggerEnter2D>();
		}
		if (fsm.HandleTriggerExit2D)
		{
			AddEventHandlerComponent<PlayMakerTriggerExit2D>();
		}
		if (fsm.HandleTriggerStay2D)
		{
			AddEventHandlerComponent<PlayMakerTriggerStay2D>();
		}
		if (fsm.HandleParticleCollision)
		{
			AddEventHandlerComponent<PlayMakerParticleCollision>();
		}
		if (fsm.HandleControllerColliderHit)
		{
			AddEventHandlerComponent<PlayMakerControllerColliderHit>();
		}
		if (fsm.HandleJointBreak)
		{
			AddEventHandlerComponent<PlayMakerJointBreak>();
		}
		if (fsm.HandleJointBreak2D)
		{
			AddEventHandlerComponent<PlayMakerJointBreak>();
		}
		if (fsm.HandleFixedUpdate)
		{
			AddEventHandlerComponent<PlayMakerFixedUpdate>();
		}
		if (fsm.HandleLateUpdate)
		{
			AddEventHandlerComponent<PlayMakerLateUpdate>();
		}
		if (fsm.HandleOnGUI && GetComponent<PlayMakerOnGUI>() == null)
		{
			base.gameObject.AddComponent<PlayMakerOnGUI>().playMakerFSM = this;
		}
		if (fsm.HandleApplicationEvents)
		{
			AddEventHandlerComponent<PlayMakerApplicationEvents>();
		}
		if (fsm.HandleAnimatorMove)
		{
			AddEventHandlerComponent<PlayMakerAnimatorMove>();
		}
		if (fsm.HandleAnimatorIK)
		{
			AddEventHandlerComponent<PlayMakerAnimatorIK>();
		}
		if (fsm.HandleLegacyNetworking || fsm.HandleUiEvents != UiEvents.None)
		{
			AddEventHandlers(this);
		}
		eventHandlerComponentsAdded = true;
	}

	private void AddEventHandlerComponent<T>() where T : PlayMakerProxyBase
	{
		GetEventHandlerComponent<T>(base.gameObject).AddTarget(this);
	}

	public static T GetEventHandlerComponent<T>(GameObject go) where T : PlayMakerProxyBase
	{
		if (go == null)
		{
			return null;
		}
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
			if (!PlayMakerPrefs.ShowEventHandlerComponents)
			{
				val.hideFlags = HideFlags.HideInInspector;
			}
		}
		return val;
	}

	public void SetFsmTemplate(FsmTemplate template)
	{
		fsmTemplate = template;
		Fsm.Clear(this);
		if (template != null)
		{
			Fsm.Variables = new FsmVariables(fsmTemplate.fsm.Variables);
		}
		Init();
	}

	private void Start()
	{
		if (!fsm.Started)
		{
			fsm.Start();
		}
	}

	private void OnEnable()
	{
		fsmList.Add(this);
		fsm.OnEnable();
	}

	private void Update()
	{
		if (!fsm.Finished && !fsm.ManualUpdate)
		{
			fsm.Update();
		}
	}

	public IEnumerator DoCoroutine(IEnumerator routine)
	{
		while (true)
		{
			FsmExecutionStack.PushFsm(fsm);
			if (!routine.MoveNext())
			{
				break;
			}
			FsmExecutionStack.PopFsm();
			yield return routine.Current;
		}
		FsmExecutionStack.PopFsm();
	}

	private void OnDisable()
	{
		if (fsm.Started)
		{
			fsm.Event(FsmEvent.Disable);
		}
		fsmList.Remove(this);
		if (fsm != null && !fsm.Finished)
		{
			fsm.OnDisable();
		}
	}

	private void OnDestroy()
	{
		fsmList.Remove(this);
		if (fsm != null)
		{
			fsm.OnDestroy();
		}
		fsm = null;
	}

	private void OnApplicationQuit()
	{
		_ = ApplicationIsQuitting;
		fsm.Event(FsmEvent.ApplicationQuit);
		ApplicationIsQuitting = true;
	}

	private void OnDrawGizmos()
	{
		if (fsm != null)
		{
			fsm.OnDrawGizmos();
		}
	}

	public void SetState(string stateName)
	{
		fsm.SetState(stateName);
	}

	public void ChangeState(FsmEvent fsmEvent)
	{
		fsm.Event(fsmEvent);
	}

	[Obsolete("Use SendEvent(string) instead.")]
	public void ChangeState(string eventName)
	{
		fsm.Event(eventName);
	}

	public void SendEvent(string eventName)
	{
		fsm.Event(eventName);
	}

	[Obsolete("Use PlayMakerRPCProxy component with SendRemote actions.")]
	public void SendRemoteFsmEvent(string eventName)
	{
		fsm.Event(eventName);
	}

	[Obsolete("Use PlayMakerRPCProxy component with SendRemote actions.")]
	public void SendRemoteFsmEventWithData(string eventName, string eventData)
	{
		Fsm.EventData.StringData = eventData;
		fsm.Event(eventName);
	}

	public static void BroadcastEvent(string fsmEventName)
	{
		if (!string.IsNullOrEmpty(fsmEventName))
		{
			BroadcastEvent(FsmEvent.GetFsmEvent(fsmEventName));
		}
	}

	public static void BroadcastEvent(FsmEvent fsmEvent)
	{
		foreach (PlayMakerFSM item in new List<PlayMakerFSM>(FsmList))
		{
			if (!(item == null) && item.Fsm != null)
			{
				item.Fsm.ProcessEvent(fsmEvent);
			}
		}
	}

	private void OnBecameVisible()
	{
		fsm.Event(FsmEvent.BecameVisible);
	}

	private void OnBecameInvisible()
	{
		fsm.Event(FsmEvent.BecameInvisible);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (PlayMakerGlobals.Initialized)
		{
			fsm.InitData();
		}
	}

	[Conditional("DEBUG_LOG")]
	private static void DebugLog(object message, LogColor logColor = LogColor.None)
	{
	}
}
