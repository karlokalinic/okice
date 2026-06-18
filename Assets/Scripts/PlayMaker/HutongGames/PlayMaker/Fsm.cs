using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class Fsm : INameable, IComparable
	{
		[Serializable]
		[Flags]
		private enum EditorFlags
		{
			none = 0,
			nameIsExpanded = 1,
			controlsIsExpanded = 2,
			debugIsExpanded = 4,
			experimentalIsExpanded = 8,
			infoIsExpanded = 0x10,
			inputsIsExpanded = 0x20,
			outputsIsExpanded = 0x40,
			eventsIsExpanded = 0x80,
			settingsIsExpanded = 0x100
		}

		public const int CurrentDataVersion = 2;

		public const int DefaultMaxLoops = 1000;

		private MethodInfo updateHelperSetDirty;

		private const string StartStateName = "State 1";

		public static FsmEventData EventData = new FsmEventData();

		public Action<FsmEvent> OnOutputEvent;

		[SerializeField]
		private int dataVersion;

		[NonSerialized]
		private MonoBehaviour owner;

		[SerializeField]
		private FsmTemplate usedInTemplate;

		[SerializeField]
		private string name = "FSM";

		[SerializeField]
		private string startState;

		[SerializeField]
		private FsmState[] states = new FsmState[1];

		[SerializeField]
		private FsmEvent[] events = new FsmEvent[0];

		[SerializeField]
		private FsmTransition[] globalTransitions = new FsmTransition[0];

		[SerializeField]
		private FsmVariables variables = new FsmVariables();

		[SerializeField]
		[TextArea(0, 10)]
		private string description = "";

		[SerializeField]
		private string docUrl;

		[SerializeField]
		private bool showStateLabel;

		[SerializeField]
		private int maxLoopCount;

		[SerializeField]
		private string watermark = "";

		[SerializeField]
		private string password;

		[SerializeField]
		private bool locked;

		[SerializeField]
		private bool manualUpdate;

		[SerializeField]
		private int[] outVariableIndices = new int[0];

		[NonSerialized]
		private NamedVariable[] outputVariables;

		[SerializeField]
		private bool keepDelayedEventsOnStateExit;

		[SerializeField]
		private bool preprocessed;

		[NonSerialized]
		private Fsm host;

		[NonSerialized]
		private Fsm rootFsm;

		[NonSerialized]
		private List<Fsm> subFsmList;

		[NonSerialized]
		public bool setDirty;

		private bool activeStateEntered;

		public List<FsmEvent> ExposedEvents = new List<FsmEvent>();

		public List<FsmEvent> OutputEvents = new List<FsmEvent>();

		private FsmLog myLog;

		public bool RestartOnEnable = true;

		public bool ResetVariablesOnEnable;

		[NonSerialized]
		private FsmVariables _defaultVariableValues;

		[NonSerialized]
		private bool hasErrors;

		public bool EnableDebugFlow;

		public bool EnableBreakpoints = true;

		[NonSerialized]
		public bool StepFrame;

		[NonSerialized]
		private readonly List<DelayedEvent> delayedEvents = new List<DelayedEvent>();

		[NonSerialized]
		private readonly List<DelayedEvent> updateEvents = new List<DelayedEvent>();

		[NonSerialized]
		private readonly List<DelayedEvent> removeEvents = new List<DelayedEvent>();

		[SerializeField]
		private EditorFlags editorFlags = EditorFlags.nameIsExpanded | EditorFlags.inputsIsExpanded | EditorFlags.outputsIsExpanded | EditorFlags.eventsIsExpanded;

		[NonSerialized]
		private bool initialized;

		[SerializeField]
		private string activeStateName;

		[NonSerialized]
		private FsmState activeState;

		[NonSerialized]
		private FsmState switchToState;

		[NonSerialized]
		private FsmState previousActiveState;

		public Action<FsmState> StateChanged;

		[NonSerialized]
		private FsmState editState;

		[SerializeField]
		private bool mouseEvents;

		[SerializeField]
		private bool handleLevelLoaded;

		[SerializeField]
		private bool handleTriggerEnter2D;

		[SerializeField]
		private bool handleTriggerExit2D;

		[SerializeField]
		private bool handleTriggerStay2D;

		[SerializeField]
		private bool handleCollisionEnter2D;

		[SerializeField]
		private bool handleCollisionExit2D;

		[SerializeField]
		private bool handleCollisionStay2D;

		[SerializeField]
		private bool handleTriggerEnter;

		[SerializeField]
		private bool handleTriggerExit;

		[SerializeField]
		private bool handleTriggerStay;

		[SerializeField]
		private bool handleCollisionEnter;

		[SerializeField]
		private bool handleCollisionExit;

		[SerializeField]
		private bool handleCollisionStay;

		[SerializeField]
		private bool handleParticleCollision;

		[SerializeField]
		private bool handleControllerColliderHit;

		[SerializeField]
		private bool handleJointBreak;

		[SerializeField]
		private bool handleJointBreak2D;

		[SerializeField]
		private bool handleOnGUI;

		[SerializeField]
		private bool handleFixedUpdate;

		[SerializeField]
		private bool handleLateUpdate;

		[SerializeField]
		private bool handleApplicationEvents;

		[SerializeField]
		private UiEvents handleUiEvents;

		[SerializeField]
		private bool handleLegacyNetworking;

		private static Dictionary<Fsm, RaycastHit2D> lastRaycastHit2DInfoLUT;

		[SerializeField]
		private bool handleAnimatorMove;

		[SerializeField]
		private bool handleAnimatorIK;

		private bool disableSent;

		private static readonly FsmEventTarget targetSelf = new FsmEventTarget();

		[Obsolete("Use PlayMakerPrefs.Colors instead.")]
		public static readonly Color[] StateColors = new Color[8]
		{
			Color.grey,
			new Color(0.54509807f, 57f / 85f, 0.9411765f),
			new Color(0.24313726f, 0.7607843f, 0.6901961f),
			new Color(22f / 51f, 0.7607843f, 0.24313726f),
			new Color(1f, 0.8745098f, 16f / 85f),
			new Color(1f, 47f / 85f, 16f / 85f),
			new Color(0.7607843f, 0.24313726f, 0.2509804f),
			new Color(0.54509807f, 0.24313726f, 0.7607843f)
		};

		public static List<Fsm> FsmList
		{
			get
			{
				List<Fsm> list = new List<Fsm>();
				foreach (PlayMakerFSM fsm in PlayMakerFSM.FsmList)
				{
					if (fsm != null && fsm.Fsm != null)
					{
						list.Add(fsm.Fsm);
					}
				}
				return list;
			}
		}

		public static List<Fsm> SortedFsmList
		{
			get
			{
				List<Fsm> fsmList = FsmList;
				fsmList.Sort();
				return fsmList;
			}
		}

		private MethodInfo UpdateHelperSetDirty => updateHelperSetDirty ?? (updateHelperSetDirty = ReflectionUtils.GetGlobalType("HutongGames.PlayMaker.UpdateHelper").GetMethod("SetDirty"));

		public bool ManualUpdate
		{
			get
			{
				return manualUpdate;
			}
			set
			{
				manualUpdate = value;
			}
		}

		public bool KeepDelayedEventsOnStateExit
		{
			get
			{
				return keepDelayedEventsOnStateExit;
			}
			set
			{
				keepDelayedEventsOnStateExit = value;
			}
		}

		public bool Preprocessed
		{
			get
			{
				return preprocessed;
			}
			set
			{
				preprocessed = value;
			}
		}

		public Fsm Host
		{
			get
			{
				return host;
			}
			private set
			{
				host = value;
			}
		}

		public string Password => password;

		public bool Locked => locked;

		public FsmTemplate Template
		{
			get
			{
				if (!(Owner != null))
				{
					return null;
				}
				return ((PlayMakerFSM)Owner).FsmTemplate;
			}
		}

		public bool IsSubFsm => host != null;

		public Fsm RootFsm => rootFsm ?? (rootFsm = GetRootFsm());

		public List<Fsm> SubFsmList => subFsmList ?? (subFsmList = new List<Fsm>());

		public FsmVariables DefaultVariableValues
		{
			get
			{
				return _defaultVariableValues;
			}
			private set
			{
				_defaultVariableValues = value;
			}
		}

		public bool HasErrors
		{
			get
			{
				return hasErrors;
			}
			set
			{
				if (!value)
				{
					FsmState[] array = States;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].HasErrors = false;
					}
				}
				hasErrors = value;
			}
		}

		public bool Started { get; private set; }

		public List<DelayedEvent> DelayedEvents => delayedEvents;

		public int DataVersion
		{
			get
			{
				return dataVersion;
			}
			set
			{
				dataVersion = value;
			}
		}

		public MonoBehaviour Owner
		{
			get
			{
				return owner;
			}
			set
			{
				owner = value;
			}
		}

		public bool NameIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.nameIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.nameIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.nameIsExpanded;
				}
			}
		}

		public bool ControlsIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.controlsIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.controlsIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.controlsIsExpanded;
				}
			}
		}

		public bool DebugIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.debugIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.debugIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.debugIsExpanded;
				}
			}
		}

		public bool ExperimentalIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.experimentalIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.experimentalIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.experimentalIsExpanded;
				}
			}
		}

		public bool InfoIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.infoIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.infoIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.infoIsExpanded;
				}
			}
		}

		public bool InputsIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.inputsIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.inputsIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.inputsIsExpanded;
				}
			}
		}

		public bool OutputsIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.outputsIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.outputsIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.outputsIsExpanded;
				}
			}
		}

		public bool EventsIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.eventsIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.eventsIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.eventsIsExpanded;
				}
			}
		}

		public bool SettingsIsExpanded
		{
			get
			{
				return (editorFlags & EditorFlags.settingsIsExpanded) != 0;
			}
			set
			{
				if (value)
				{
					editorFlags |= EditorFlags.settingsIsExpanded;
				}
				else
				{
					editorFlags &= ~EditorFlags.settingsIsExpanded;
				}
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
				name = value;
			}
		}

		public FsmTemplate UsedInTemplate
		{
			get
			{
				return usedInTemplate;
			}
			set
			{
				usedInTemplate = value;
			}
		}

		public string StartState
		{
			get
			{
				return startState;
			}
			set
			{
				startState = value;
			}
		}

		public FsmState[] States
		{
			get
			{
				return states;
			}
			set
			{
				states = value;
			}
		}

		public FsmEvent[] Events
		{
			get
			{
				return events;
			}
			set
			{
				events = value;
			}
		}

		public FsmTransition[] GlobalTransitions
		{
			get
			{
				return globalTransitions;
			}
			set
			{
				globalTransitions = value;
			}
		}

		public FsmVariables Variables
		{
			get
			{
				return variables;
			}
			set
			{
				variables = value;
			}
		}

		public FsmEventTarget EventTarget { get; set; }

		public bool Initialized => initialized;

		public bool Active
		{
			get
			{
				if (owner != null && owner.gameObject != null && !Finished)
				{
					return ActiveState != null;
				}
				return false;
			}
		}

		public bool Finished { get; private set; }

		public bool IsSwitchingState => switchToState != null;

		public FsmState ActiveState
		{
			get
			{
				if (activeState == null && activeStateName != "")
				{
					activeState = GetState(activeStateName);
				}
				return activeState;
			}
			private set
			{
				activeState = value;
				activeStateName = ((activeState == null) ? "" : activeState.Name);
			}
		}

		public string ActiveStateName => activeStateName;

		public FsmState PreviousActiveState
		{
			get
			{
				return previousActiveState;
			}
			private set
			{
				previousActiveState = value;
			}
		}

		public FsmTransition LastTransition { get; private set; }

		public int MaxLoopCount
		{
			get
			{
				if (maxLoopCount <= 0)
				{
					return 1000;
				}
				return maxLoopCount;
			}
		}

		public int MaxLoopCountOverride
		{
			get
			{
				return maxLoopCount;
			}
			set
			{
				maxLoopCount = Mathf.Max(0, value);
			}
		}

		public string OwnerName
		{
			get
			{
				if (!(owner != null))
				{
					return "";
				}
				return owner.name;
			}
		}

		public string OwnerDebugName
		{
			get
			{
				if (PlayMakerFSM.NotMainThread)
				{
					return "";
				}
				if (!(owner != null))
				{
					return "[missing Owner]";
				}
				return owner.name;
			}
		}

		public GameObject GameObject
		{
			get
			{
				if (!(Owner != null))
				{
					return null;
				}
				return Owner.gameObject;
			}
		}

		public string GameObjectName
		{
			get
			{
				if (!(Owner != null))
				{
					return "[missing GameObject]";
				}
				return Owner.gameObject.name;
			}
		}

		public UnityEngine.Object OwnerObject
		{
			get
			{
				if ((bool)UsedInTemplate)
				{
					return UsedInTemplate;
				}
				return Owner;
			}
		}

		public PlayMakerFSM FsmComponent => Owner as PlayMakerFSM;

		public FsmLog MyLog => myLog ?? (myLog = FsmLog.GetLog(this));

		public bool IsModifiedPrefabInstance { get; set; }

		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
			}
		}

		public string Watermark
		{
			get
			{
				return watermark;
			}
			set
			{
				watermark = value;
			}
		}

		public bool ShowStateLabel
		{
			get
			{
				return showStateLabel;
			}
			set
			{
				showStateLabel = value;
			}
		}

		private string GuiLabel => OwnerName + " : " + Name;

		public string DocUrl
		{
			get
			{
				return docUrl;
			}
			set
			{
				docUrl = value;
			}
		}

		public FsmState EditState
		{
			get
			{
				return editState;
			}
			set
			{
				editState = value;
			}
		}

		public static GameObject LastClickedObject { get; set; }

		public static bool BreakpointsEnabled { get; set; }

		public static bool HitBreakpoint { get; set; }

		public static Fsm BreakAtFsm { get; private set; }

		public static FsmState BreakAtState { get; private set; }

		public static bool IsBreak { get; private set; }

		public static bool IsErrorBreak { get; private set; }

		public static string LastError { get; private set; }

		public static bool StepToStateChange { get; set; }

		public static Fsm StepFsm { get; set; }

		public bool SwitchedState { get; set; }

		public bool MouseEvents
		{
			get
			{
				return mouseEvents;
			}
			set
			{
				preprocessed = false;
				mouseEvents = value;
				if (host != null)
				{
					host.MouseEvents |= value;
				}
			}
		}

		public bool HandleLevelLoaded
		{
			get
			{
				return handleLevelLoaded;
			}
			set
			{
				handleLevelLoaded = value;
				if (host != null)
				{
					host.HandleLevelLoaded |= value;
				}
			}
		}

		public bool HandleTriggerEnter2D
		{
			get
			{
				return handleTriggerEnter2D;
			}
			set
			{
				preprocessed = false;
				handleTriggerEnter2D = value;
				if (host != null)
				{
					host.HandleTriggerEnter2D |= value;
				}
			}
		}

		public bool HandleTriggerExit2D
		{
			get
			{
				return handleTriggerExit2D;
			}
			set
			{
				preprocessed = false;
				handleTriggerExit2D = value;
				if (host != null)
				{
					host.HandleTriggerExit2D |= value;
				}
			}
		}

		public bool HandleTriggerStay2D
		{
			get
			{
				return handleTriggerStay2D;
			}
			set
			{
				preprocessed = false;
				handleTriggerStay2D = value;
				if (host != null)
				{
					host.HandleTriggerStay2D |= value;
				}
			}
		}

		public bool HandleCollisionEnter2D
		{
			get
			{
				return handleCollisionEnter2D;
			}
			set
			{
				preprocessed = false;
				handleCollisionEnter2D = value;
				if (host != null)
				{
					host.HandleCollisionEnter2D |= value;
				}
			}
		}

		public bool HandleCollisionExit2D
		{
			get
			{
				return handleCollisionExit2D;
			}
			set
			{
				preprocessed = false;
				handleCollisionExit2D = value;
				if (host != null)
				{
					host.HandleCollisionExit2D |= value;
				}
			}
		}

		public bool HandleCollisionStay2D
		{
			get
			{
				return handleCollisionStay2D;
			}
			set
			{
				preprocessed = false;
				handleCollisionStay2D = value;
				if (host != null)
				{
					host.HandleCollisionStay2D |= value;
				}
			}
		}

		public bool HandleTriggerEnter
		{
			get
			{
				return handleTriggerEnter;
			}
			set
			{
				preprocessed = false;
				handleTriggerEnter = value;
				if (host != null)
				{
					host.HandleTriggerEnter |= value;
				}
			}
		}

		public bool HandleTriggerExit
		{
			get
			{
				return handleTriggerExit;
			}
			set
			{
				preprocessed = false;
				handleTriggerExit = value;
				if (host != null)
				{
					host.HandleTriggerExit |= value;
				}
			}
		}

		public bool HandleTriggerStay
		{
			get
			{
				return handleTriggerStay;
			}
			set
			{
				preprocessed = false;
				handleTriggerStay = value;
				if (host != null)
				{
					host.HandleTriggerStay |= value;
				}
			}
		}

		public bool HandleCollisionEnter
		{
			get
			{
				return handleCollisionEnter;
			}
			set
			{
				preprocessed = false;
				handleCollisionEnter = value;
				if (host != null)
				{
					host.HandleCollisionEnter |= value;
				}
			}
		}

		public bool HandleCollisionExit
		{
			get
			{
				return handleCollisionExit;
			}
			set
			{
				preprocessed = false;
				handleCollisionExit = value;
				if (host != null)
				{
					host.HandleCollisionExit |= value;
				}
			}
		}

		public bool HandleCollisionStay
		{
			get
			{
				return handleCollisionStay;
			}
			set
			{
				preprocessed = false;
				handleCollisionStay = value;
				if (host != null)
				{
					host.HandleCollisionStay |= value;
				}
			}
		}

		public bool HandleParticleCollision
		{
			get
			{
				return handleParticleCollision;
			}
			set
			{
				preprocessed = false;
				handleParticleCollision = value;
				if (host != null)
				{
					host.HandleParticleCollision |= value;
				}
			}
		}

		public bool HandleControllerColliderHit
		{
			get
			{
				return handleControllerColliderHit;
			}
			set
			{
				preprocessed = false;
				handleControllerColliderHit = value;
				if (host != null)
				{
					host.HandleControllerColliderHit |= value;
				}
			}
		}

		public bool HandleJointBreak
		{
			get
			{
				return handleJointBreak;
			}
			set
			{
				preprocessed = false;
				handleJointBreak = value;
				if (host != null)
				{
					host.HandleJointBreak |= value;
				}
			}
		}

		public bool HandleJointBreak2D
		{
			get
			{
				return handleJointBreak2D;
			}
			set
			{
				preprocessed = false;
				handleJointBreak2D = value;
				if (host != null)
				{
					host.HandleJointBreak2D |= value;
				}
			}
		}

		public bool HandleOnGUI
		{
			get
			{
				return handleOnGUI;
			}
			set
			{
				preprocessed = false;
				handleOnGUI = value;
				if (host != null)
				{
					host.HandleOnGUI |= value;
				}
			}
		}

		public bool HandleFixedUpdate
		{
			get
			{
				return handleFixedUpdate;
			}
			set
			{
				preprocessed = false;
				handleFixedUpdate = value;
				if (host != null)
				{
					host.HandleFixedUpdate |= value;
				}
			}
		}

		public bool HandleLateUpdate
		{
			get
			{
				return handleLateUpdate;
			}
			set
			{
				preprocessed = false;
				handleLateUpdate = value;
				if (host != null)
				{
					host.HandleLateUpdate |= value;
				}
			}
		}

		public bool HandleApplicationEvents
		{
			get
			{
				return handleApplicationEvents;
			}
			set
			{
				preprocessed = false;
				handleApplicationEvents = value;
				if (host != null)
				{
					host.HandleApplicationEvents |= value;
				}
			}
		}

		public UiEvents HandleUiEvents
		{
			get
			{
				return handleUiEvents;
			}
			set
			{
				preprocessed = false;
				handleUiEvents = value;
				if (host != null)
				{
					host.HandleUiEvents |= value;
				}
			}
		}

		public bool HandleLegacyNetworking
		{
			get
			{
				return handleLegacyNetworking;
			}
			set
			{
				preprocessed = false;
				handleLegacyNetworking = value;
				if (host != null)
				{
					host.HandleLegacyNetworking |= value;
				}
			}
		}

		public Collision CollisionInfo { get; set; }

		public Collider TriggerCollider { get; set; }

		public Collision2D Collision2DInfo { get; set; }

		public Collider2D TriggerCollider2D { get; set; }

		public float JointBreakForce { get; private set; }

		public Joint2D BrokenJoint2D { get; private set; }

		public GameObject ParticleCollisionGO { get; set; }

		public GameObject CollisionGO
		{
			get
			{
				if (CollisionInfo == null)
				{
					return null;
				}
				return CollisionInfo.gameObject;
			}
		}

		public GameObject Collision2dGO
		{
			get
			{
				if (Collision2DInfo == null)
				{
					return null;
				}
				return Collision2DInfo.gameObject;
			}
		}

		public GameObject TriggerGO
		{
			get
			{
				if (!(TriggerCollider != null))
				{
					return null;
				}
				return TriggerCollider.gameObject;
			}
		}

		public GameObject Trigger2dGO
		{
			get
			{
				if (!(TriggerCollider2D != null))
				{
					return null;
				}
				return TriggerCollider2D.gameObject;
			}
		}

		public string TriggerName { get; private set; }

		public string CollisionName { get; private set; }

		public string Trigger2dName { get; private set; }

		public string Collision2dName { get; private set; }

		public ControllerColliderHit ControllerCollider { get; set; }

		public RaycastHit RaycastHitInfo { get; set; }

		public bool HandleAnimatorMove
		{
			get
			{
				return handleAnimatorMove;
			}
			set
			{
				preprocessed = false;
				handleAnimatorMove = value;
				if (host != null)
				{
					host.HandleAnimatorMove |= value;
				}
			}
		}

		public bool HandleAnimatorIK
		{
			get
			{
				return handleAnimatorIK;
			}
			set
			{
				preprocessed = false;
				handleAnimatorIK = value;
				if (host != null)
				{
					host.HandleAnimatorIK |= value;
				}
			}
		}

		[Obsolete("Use PlayMakerPrefs.ArrowColor")]
		public static Color DebugLookAtColor { get; set; }

		[Obsolete("Use PlayMakerPrefs.ArrowColor")]
		public static Color DebugRaycastColor { get; set; }

		public void ClearOutputFlags()
		{
			outVariableIndices = new int[0];
		}

		public NamedVariable[] GetOutputVariables(bool refresh = true)
		{
			if (!refresh && outputVariables != null)
			{
				return outputVariables;
			}
			variables.Reinitialize();
			NamedVariable[] allNamedVariables = variables.GetAllNamedVariables();
			outputVariables = new NamedVariable[outVariableIndices.Length];
			for (int i = 0; i < outVariableIndices.Length; i++)
			{
				int num = outVariableIndices[i];
				if (num != -1 && num < allNamedVariables.Length)
				{
					outputVariables[i] = allNamedVariables[num];
				}
			}
			return outputVariables;
		}

		public string[] GetOutputVariableNames()
		{
			NamedVariable[] array = GetOutputVariables();
			string[] array2 = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					array2[i] = array[i].Name;
				}
			}
			return array2;
		}

		public void SetOutputVariables(string[] outputVarNames)
		{
			List<int> list = new List<int>();
			foreach (string variableName in outputVarNames)
			{
				list.Add(variables.GetVariableIndex(variableName));
			}
			outVariableIndices = list.ToArray();
		}

		public string SanityCheckOutputIndices()
		{
			if (!OutVariableIndicesNeedFixing())
			{
				return "";
			}
			List<int> list = new List<int>();
			int[] array = outVariableIndices;
			foreach (int num in array)
			{
				if (num != -1)
				{
					list.Add(num);
				}
			}
			outVariableIndices = list.ToArray();
			return "\nFixed missing output variables.";
		}

		private bool OutVariableIndicesNeedFixing()
		{
			int[] array = outVariableIndices;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == -1)
				{
					return true;
				}
			}
			return false;
		}

		public void Lock(string pass)
		{
			if (!Locked)
			{
				password = pass;
				locked = true;
			}
		}

		public void Unlock(string pass)
		{
			if (string.IsNullOrEmpty(password) || pass == password)
			{
				locked = false;
			}
		}

		public void KillDelayedEvents()
		{
			delayedEvents.Clear();
		}

		public void ForcePreprocess()
		{
			ResetEventHandlerFlags();
		}

		private void ResetEventHandlerFlags()
		{
			handleApplicationEvents = false;
			handleCollisionEnter = false;
			HandleCollisionExit = false;
			handleCollisionStay = false;
			handleCollisionEnter2D = false;
			HandleCollisionExit2D = false;
			handleCollisionStay2D = false;
			handleTriggerEnter = false;
			handleTriggerExit = false;
			handleTriggerStay = false;
			handleTriggerEnter2D = false;
			handleTriggerExit2D = false;
			handleTriggerStay2D = false;
			handleControllerColliderHit = false;
			handleFixedUpdate = false;
			handleLateUpdate = false;
			handleOnGUI = false;
			handleAnimatorIK = false;
			handleAnimatorMove = false;
			handleJointBreak = false;
			handleJointBreak2D = false;
			handleParticleCollision = false;
			handleLegacyNetworking = false;
			handleUiEvents = UiEvents.None;
			preprocessed = false;
		}

		public static void RecordLastRaycastHit2DInfo(Fsm fsm, RaycastHit2D info)
		{
			if (lastRaycastHit2DInfoLUT == null)
			{
				lastRaycastHit2DInfoLUT = new Dictionary<Fsm, RaycastHit2D>();
			}
			lastRaycastHit2DInfoLUT[fsm] = info;
		}

		public static RaycastHit2D GetLastRaycastHit2DInfo(Fsm fsm)
		{
			if (lastRaycastHit2DInfoLUT == null)
			{
				return default(RaycastHit2D);
			}
			return lastRaycastHit2DInfoLUT[fsm];
		}

		public static Fsm NewTempFsm()
		{
			return new Fsm
			{
				dataVersion = 2
			};
		}

		public Fsm()
		{
		}

		public Fsm(Fsm source, FsmVariables overrideVariables = null)
		{
			dataVersion = source.DataVersion;
			owner = source.Owner;
			name = source.Name;
			description = source.Description;
			startState = source.StartState;
			docUrl = source.docUrl;
			showStateLabel = source.showStateLabel;
			maxLoopCount = source.maxLoopCount;
			watermark = source.Watermark;
			RestartOnEnable = source.RestartOnEnable;
			EnableDebugFlow = source.EnableDebugFlow;
			EnableBreakpoints = source.EnableBreakpoints;
			ResetVariablesOnEnable = source.ResetVariablesOnEnable;
			states = new FsmState[source.States.Length];
			for (int i = 0; i < source.States.Length; i++)
			{
				source.States[i].Fsm = source;
				states[i] = new FsmState(source.States[i]);
			}
			events = new FsmEvent[source.Events.Length];
			for (int j = 0; j < source.Events.Length; j++)
			{
				events[j] = new FsmEvent(source.Events[j]);
			}
			ExposedEvents = new List<FsmEvent>();
			foreach (FsmEvent exposedEvent in source.ExposedEvents)
			{
				ExposedEvents.Add(new FsmEvent(exposedEvent));
			}
			OutputEvents = new List<FsmEvent>();
			foreach (FsmEvent outputEvent in source.OutputEvents)
			{
				OutputEvents.Add(new FsmEvent(outputEvent));
			}
			outVariableIndices = new int[source.outVariableIndices.Length];
			Array.Copy(source.outVariableIndices, outVariableIndices, outVariableIndices.Length);
			globalTransitions = new FsmTransition[source.globalTransitions.Length];
			for (int k = 0; k < globalTransitions.Length; k++)
			{
				globalTransitions[k] = new FsmTransition(source.globalTransitions[k]);
			}
			variables = new FsmVariables(source.Variables);
			if (overrideVariables != null)
			{
				variables.OverrideVariableValues(overrideVariables);
			}
		}

		public Fsm CreateSubFsm(FsmTemplateControl templateControl)
		{
			Fsm fsm = templateControl.InstantiateFsm();
			fsm.Host = this;
			fsm.Init(Owner);
			templateControl.ID = SubFsmList.Count;
			SubFsmList.Add(fsm);
			return fsm;
		}

		private Fsm GetRootFsm()
		{
			Fsm fsm = this;
			while (fsm.Host != null)
			{
				fsm = fsm.Host;
			}
			return fsm;
		}

		public void CheckIfDirty()
		{
			if (setDirty && ((object)Owner != null || (object)UsedInTemplate != null))
			{
				UpdateHelperSetDirty.Invoke(null, new object[1] { this });
				setDirty = false;
			}
		}

		public void Reset(MonoBehaviour component)
		{
			dataVersion = 2;
			owner = component;
			name = "FSM";
			description = "";
			docUrl = "";
			globalTransitions = new FsmTransition[0];
			events = new FsmEvent[0];
			variables = new FsmVariables();
			states = new FsmState[1];
			States[0] = new FsmState(this)
			{
				Fsm = this,
				Name = "State 1",
				Position = new Rect(50f, 100f, 100f, 16f)
			};
			startState = "State 1";
			outVariableIndices = new int[0];
			ExposedEvents = new List<FsmEvent>();
			OutputEvents = new List<FsmEvent>();
			EnableDebugFlow = false;
			EnableBreakpoints = true;
			ResetVariablesOnEnable = false;
		}

		public void UpdateDataVersion()
		{
			dataVersion = 2;
			SaveActions();
		}

		public void SaveActions()
		{
			FsmState[] array = States;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SaveActions();
			}
		}

		public void Clear(MonoBehaviour component)
		{
			dataVersion = 2;
			owner = component;
			description = "";
			docUrl = "";
			globalTransitions = new FsmTransition[0];
			events = new FsmEvent[0];
			variables = new FsmVariables();
			states = new FsmState[1];
			States[0] = new FsmState(this)
			{
				Fsm = this,
				Name = "State 1",
				Position = new Rect(50f, 100f, 100f, 16f)
			};
			startState = "State 1";
			outVariableIndices = new int[0];
		}

		private void FixDataVersion()
		{
			dataVersion = DeduceDataVersion();
			if (!PlayMakerGlobals.IsBuilding)
			{
				setDirty = true;
			}
		}

		private int DeduceDataVersion()
		{
			FsmState[] array = States;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ActionData.UsesDataVersion2())
				{
					return 2;
				}
			}
			array = States;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ActionData.ActionCount > 0)
				{
					return 1;
				}
			}
			return 2;
		}

		public void Preprocess(MonoBehaviour component)
		{
			ResetEventHandlerFlags();
			owner = component;
			if (PlayMakerGlobals.IsBuilding)
			{
				Reinitialize();
			}
			else
			{
				InitData();
			}
			Preprocess();
		}

		private void Preprocess()
		{
			FsmState[] array = states;
			foreach (FsmState fsmState in array)
			{
				FsmStateAction[] actions = fsmState.Actions;
				foreach (FsmStateAction obj in actions)
				{
					obj.Init(fsmState);
					obj.OnPreprocess();
				}
			}
			CheckFsmEventsForEventHandlers();
			preprocessed = true;
		}

		private void Awake()
		{
			FsmState[] array = states;
			foreach (FsmState fsmState in array)
			{
				FsmStateAction[] actions = fsmState.Actions;
				foreach (FsmStateAction obj in actions)
				{
					obj.Init(fsmState);
					obj.Awake();
				}
			}
			if (!preprocessed)
			{
				CheckFsmEventsForEventHandlers();
			}
		}

		public void Init(MonoBehaviour component)
		{
			owner = component;
			InitData();
			if (!preprocessed)
			{
				Preprocess();
			}
			Awake();
		}

		public void Reinitialize()
		{
			if (Application.isPlaying)
			{
				UnityEngine.Debug.LogWarning("PlayMaker: Reinitializing FSM while playing.\nThis could lead to unexpected behaviour!");
			}
			initialized = false;
			variables.Reinitialize();
			InitData();
		}

		public void InitStates()
		{
			FsmState[] array = states;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Fsm = this;
			}
		}

		public void InitData()
		{
			if (dataVersion == 0)
			{
				FixDataVersion();
			}
			if (Initialized)
			{
				return;
			}
			initialized = true;
			InitEvents();
			FsmState[] array = states;
			FsmTransition[] transitions;
			foreach (FsmState obj in array)
			{
				obj.Fsm = this;
				obj.LoadActions();
				transitions = obj.Transitions;
				foreach (FsmTransition fsmTransition in transitions)
				{
					if (!string.IsNullOrEmpty(fsmTransition.ToState))
					{
						fsmTransition.ToFsmState = GetState(fsmTransition.ToState);
					}
					if (!string.IsNullOrEmpty(fsmTransition.EventName))
					{
						FsmEvent fsmEvent = FsmEvent.GetFsmEvent(fsmTransition.EventName);
						fsmTransition.FsmEvent = fsmEvent;
					}
				}
			}
			transitions = globalTransitions;
			foreach (FsmTransition fsmTransition2 in transitions)
			{
				if (!string.IsNullOrEmpty(fsmTransition2.ToState))
				{
					fsmTransition2.ToFsmState = GetState(fsmTransition2.ToState);
				}
				if (!string.IsNullOrEmpty(fsmTransition2.EventName))
				{
					FsmEvent fsmEvent2 = FsmEvent.GetFsmEvent(fsmTransition2.EventName);
					fsmTransition2.FsmEvent = fsmEvent2;
				}
			}
			CheckIfDirty();
		}

		public void InitEvents()
		{
			for (int i = 0; i < events.Length; i++)
			{
				events[i] = FsmEvent.GetFsmEvent(events[i]);
			}
			for (int j = 0; j < OutputEvents.Count; j++)
			{
				if (OutputEvents[j] != null)
				{
					OutputEvents[j] = FsmEvent.GetFsmEvent(OutputEvents[j]);
				}
			}
			for (int k = 0; k < ExposedEvents.Count; k++)
			{
				if (ExposedEvents[k] != null)
				{
					ExposedEvents[k] = FsmEvent.GetFsmEvent(ExposedEvents[k]);
				}
			}
		}

		private void CheckFsmEventsForEventHandlers()
		{
			FsmEvent[] array = Events;
			foreach (FsmEvent fsmEvent in array)
			{
				if (fsmEvent != null && fsmEvent.IsSystemEvent)
				{
					if (fsmEvent == FsmEvent.TriggerEnter)
					{
						RootFsm.HandleTriggerEnter = true;
					}
					else if (fsmEvent == FsmEvent.TriggerExit)
					{
						RootFsm.HandleTriggerExit = true;
					}
					else if (fsmEvent == FsmEvent.TriggerStay)
					{
						RootFsm.HandleTriggerStay = true;
					}
					else if (fsmEvent == FsmEvent.CollisionEnter)
					{
						RootFsm.HandleCollisionEnter = true;
					}
					else if (fsmEvent == FsmEvent.CollisionExit)
					{
						RootFsm.HandleCollisionExit = true;
					}
					else if (fsmEvent == FsmEvent.CollisionStay)
					{
						RootFsm.HandleCollisionStay = true;
					}
					else if (fsmEvent == FsmEvent.TriggerEnter2D)
					{
						RootFsm.HandleTriggerEnter2D = true;
					}
					else if (fsmEvent == FsmEvent.TriggerExit2D)
					{
						RootFsm.HandleTriggerExit2D = true;
					}
					else if (fsmEvent == FsmEvent.TriggerStay2D)
					{
						RootFsm.HandleTriggerStay2D = true;
					}
					else if (fsmEvent == FsmEvent.CollisionEnter2D)
					{
						RootFsm.HandleCollisionEnter2D = true;
					}
					else if (fsmEvent == FsmEvent.CollisionExit2D)
					{
						RootFsm.HandleCollisionExit2D = true;
					}
					else if (fsmEvent == FsmEvent.CollisionStay2D)
					{
						RootFsm.HandleCollisionStay2D = true;
					}
					else if (fsmEvent == FsmEvent.ParticleCollision)
					{
						RootFsm.HandleParticleCollision = true;
					}
					else if (fsmEvent == FsmEvent.ControllerColliderHit)
					{
						RootFsm.HandleControllerColliderHit = true;
					}
					else if (fsmEvent == FsmEvent.JointBreak)
					{
						RootFsm.HandleJointBreak = true;
					}
					else if (fsmEvent == FsmEvent.JointBreak2D)
					{
						RootFsm.HandleJointBreak2D = true;
					}
					else if (fsmEvent.IsMouseEvent)
					{
						RootFsm.MouseEvents = true;
					}
					else if (fsmEvent.IsApplicationEvent)
					{
						RootFsm.HandleApplicationEvents = true;
					}
					else if (fsmEvent.IsLegacyNetworkEvent)
					{
						RootFsm.HandleLegacyNetworking = true;
					}
					else if (fsmEvent == FsmEvent.LevelLoaded)
					{
						RootFsm.HandleLevelLoaded = true;
					}
					else if (fsmEvent == FsmEvent.UiClick)
					{
						RootFsm.HandleUiEvents |= UiEvents.Click;
					}
					else if (fsmEvent == FsmEvent.UiBeginDrag)
					{
						RootFsm.HandleUiEvents |= UiEvents.BeginDrag;
					}
					else if (fsmEvent == FsmEvent.UiDrag)
					{
						RootFsm.HandleUiEvents |= UiEvents.Drag;
					}
					else if (fsmEvent == FsmEvent.UiEndDrag)
					{
						RootFsm.HandleUiEvents |= UiEvents.EndDrag;
					}
					else if (fsmEvent == FsmEvent.UiDrop)
					{
						RootFsm.HandleUiEvents |= UiEvents.Drop;
					}
					else if (fsmEvent == FsmEvent.UiPointerClick)
					{
						RootFsm.HandleUiEvents |= UiEvents.PointerClick;
					}
					else if (fsmEvent == FsmEvent.UiPointerDown)
					{
						RootFsm.HandleUiEvents |= UiEvents.PointerDown;
					}
					else if (fsmEvent == FsmEvent.UiPointerEnter)
					{
						RootFsm.HandleUiEvents |= UiEvents.PointerEnter;
					}
					else if (fsmEvent == FsmEvent.UiPointerExit)
					{
						RootFsm.HandleUiEvents |= UiEvents.PointerExit;
					}
					else if (fsmEvent == FsmEvent.UiPointerUp)
					{
						RootFsm.HandleUiEvents |= UiEvents.PointerUp;
					}
					else if (fsmEvent == FsmEvent.UiBoolValueChanged)
					{
						RootFsm.HandleUiEvents |= UiEvents.BoolValueChanged;
					}
					else if (fsmEvent == FsmEvent.UiFloatValueChanged)
					{
						RootFsm.HandleUiEvents |= UiEvents.FloatValueChanged;
					}
					else if (fsmEvent == FsmEvent.UiIntValueChanged)
					{
						RootFsm.HandleUiEvents |= UiEvents.IntValueChanged;
					}
					else if (fsmEvent == FsmEvent.UiVector2ValueChanged)
					{
						RootFsm.HandleUiEvents |= UiEvents.Vector2ValueChanged;
					}
					else if (fsmEvent == FsmEvent.UiEndEdit)
					{
						RootFsm.HandleUiEvents |= UiEvents.EndEdit;
					}
				}
			}
			foreach (Fsm subFsm in SubFsmList)
			{
				subFsm.CheckFsmEventsForEventHandlers();
			}
		}

		public void InitInEditor()
		{
			initialized = false;
			Finished = false;
			disableSent = false;
			ActiveState = null;
			switchToState = null;
			if (HandleLevelLoaded)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
			}
		}

		public void OnEnable()
		{
			Finished = false;
			disableSent = false;
			if (ResetVariablesOnEnable)
			{
				if (DefaultVariableValues == null)
				{
					DefaultVariableValues = new FsmVariables(variables);
				}
				else
				{
					variables.ApplyVariableValues(DefaultVariableValues);
				}
			}
			if (HandleLevelLoaded)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				SceneManager.sceneLoaded += OnSceneLoaded;
			}
			if (ActiveState == null || RestartOnEnable)
			{
				if (ActiveState != null)
				{
					ActiveState.OnExit();
				}
				ActiveState = GetState(startState);
				activeStateEntered = false;
				if (Started)
				{
					Start();
				}
			}
		}

		public void SaveDefaultVariableValues()
		{
			DefaultVariableValues = new FsmVariables(variables);
		}

		public void ResetVariableValues()
		{
			if (DefaultVariableValues == null)
			{
				UnityEngine.Debug.LogWarning("Default variable values not saved. Call SaveDefaultVariableValues before ResetVariableValues.");
			}
			else
			{
				variables.ApplyVariableValues(DefaultVariableValues);
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			Event(FsmEvent.LevelLoaded);
		}

		public void Start()
		{
			if (FsmLog.LoggingEnabled)
			{
				MyLog.LogStart(ActiveState);
			}
			Started = true;
			Finished = false;
			int stackCount = FsmExecutionStack.StackCount;
			FsmExecutionStack.PushFsm(this);
			if (ActiveState == null)
			{
				ActiveState = GetState(startState);
				activeStateEntered = false;
			}
			if (BreakpointsEnabled && EnableBreakpoints && ActiveState.IsBreakpoint)
			{
				DoBreakpoint();
			}
			else
			{
				switchToState = ActiveState;
				UpdateStateChanges();
			}
			FsmExecutionStack.PopFsm();
			int stackCount2 = FsmExecutionStack.StackCount;
			if (stackCount2 != stackCount)
			{
				UnityEngine.Debug.LogError("Stack error: " + (stackCount2 - stackCount));
			}
		}

		public void Update()
		{
			if (owner == null)
			{
				return;
			}
			int stackCount = FsmExecutionStack.StackCount;
			if (!HitBreakpoint)
			{
				FsmExecutionStack.PushFsm(this);
				if (!activeStateEntered)
				{
					Continue();
				}
				UpdateDelayedEvents();
				if (ActiveState != null)
				{
					UpdateState(ActiveState);
				}
				FsmExecutionStack.PopFsm();
				int stackCount2 = FsmExecutionStack.StackCount;
				if (stackCount2 != stackCount)
				{
					UnityEngine.Debug.LogError("Stack error: " + (stackCount2 - stackCount));
				}
			}
		}

		public void UpdateDelayedEvents()
		{
			removeEvents.Clear();
			updateEvents.Clear();
			updateEvents.AddRange(delayedEvents);
			for (int i = 0; i < updateEvents.Count; i++)
			{
				DelayedEvent delayedEvent = updateEvents[i];
				delayedEvent.Update();
				if (delayedEvent.Finished)
				{
					removeEvents.Add(delayedEvent);
				}
			}
			for (int j = 0; j < removeEvents.Count; j++)
			{
				DelayedEvent item = removeEvents[j];
				delayedEvents.Remove(item);
			}
		}

		public void ClearDelayedEvents()
		{
			delayedEvents.Clear();
		}

		public void FixedUpdate()
		{
			FsmExecutionStack.PushFsm(this);
			if (ActiveState != null && activeStateEntered)
			{
				FixedUpdateState(ActiveState);
			}
			FsmExecutionStack.PopFsm();
		}

		public void LateUpdate()
		{
			FsmExecutionStack.PushFsm(this);
			if (ActiveState != null && activeStateEntered)
			{
				LateUpdateState(ActiveState);
			}
			FsmExecutionStack.PopFsm();
		}

		public void OnDisable()
		{
			OnOutputEvent = null;
			Stop();
			if (HandleLevelLoaded)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
			}
		}

		public void SendDisableEvent()
		{
			if (!disableSent)
			{
				disableSent = true;
				ProcessEvent(FsmEvent.Disable);
			}
		}

		public void Stop()
		{
			SendDisableEvent();
			if (RestartOnEnable)
			{
				StopAndReset();
			}
			Finished = true;
			if (FsmLog.LoggingEnabled)
			{
				MyLog.LogStop();
			}
		}

		private void StopAndReset()
		{
			FsmExecutionStack.PushFsm(this);
			if (ActiveState != null && activeStateEntered)
			{
				ExitState(ActiveState);
			}
			ActiveState = null;
			LastTransition = null;
			SwitchedState = false;
			HitBreakpoint = false;
			FsmExecutionStack.PopFsm();
		}

		public bool HasEvent(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				return false;
			}
			FsmEvent[] array = events;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Name == eventName)
				{
					return true;
				}
			}
			return false;
		}

		public void ProcessEvent(FsmEvent fsmEvent, FsmEventData eventData = null)
		{
			if (!Active || FsmEvent.IsNullOrEmpty(fsmEvent))
			{
				return;
			}
			if (!Started)
			{
				Start();
			}
			if (!Active)
			{
				return;
			}
			if (eventData != null)
			{
				SetEventDataSentByInfo(eventData);
			}
			FsmExecutionStack.PushFsm(this);
			if (IsSwitchingState || (ActiveState.HandlesOnEvent && ActiveState.OnEvent(fsmEvent)))
			{
				FsmExecutionStack.PopFsm();
				return;
			}
			FsmTransition[] array = globalTransitions;
			foreach (FsmTransition fsmTransition in array)
			{
				if (fsmTransition.FsmEvent == fsmEvent)
				{
					if (FsmLog.LoggingEnabled)
					{
						MyLog.LogEvent(fsmEvent, activeState);
					}
					if (DoTransition(fsmTransition, isGlobal: true))
					{
						FsmExecutionStack.PopFsm();
						return;
					}
				}
			}
			array = ActiveState.Transitions;
			foreach (FsmTransition fsmTransition2 in array)
			{
				if (fsmTransition2.FsmEvent == fsmEvent)
				{
					if (FsmLog.LoggingEnabled)
					{
						MyLog.LogEvent(fsmEvent, activeState);
					}
					if (DoTransition(fsmTransition2, isGlobal: false))
					{
						FsmExecutionStack.PopFsm();
						return;
					}
				}
			}
			FsmExecutionStack.PopFsm();
			if (OnOutputEvent != null && OutputEvents.Contains(fsmEvent))
			{
				OnOutputEvent(fsmEvent);
			}
		}

		public static void SetEventDataSentByInfo()
		{
			EventData.SentByFsm = FsmExecutionStack.ExecutingFsm;
			EventData.SentByState = FsmExecutionStack.ExecutingState;
			EventData.SentByAction = FsmExecutionStack.ExecutingAction;
		}

		private static void SetEventDataSentByInfo(FsmEventData eventData)
		{
			EventData.SentByFsm = eventData.SentByFsm;
			EventData.SentByState = eventData.SentByState;
			EventData.SentByAction = eventData.SentByAction;
		}

		private static FsmEventData GetEventDataSentByInfo()
		{
			return new FsmEventData
			{
				SentByFsm = FsmExecutionStack.ExecutingFsm,
				SentByState = FsmExecutionStack.ExecutingState,
				SentByAction = FsmExecutionStack.ExecutingAction
			};
		}

		public void Event(FsmEventTarget eventTarget, string fsmEventName)
		{
			if (!string.IsNullOrEmpty(fsmEventName))
			{
				Event(eventTarget, FsmEvent.GetFsmEvent(fsmEventName));
			}
		}

		public void Event(GameObject fromGameObject, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			EventData.SentByGameObject = fromGameObject;
			Event(eventTarget, fsmEvent);
		}

		public void Event(GameObject fromGameObject, FsmEvent fsmEvent)
		{
			EventData.SentByGameObject = fromGameObject;
			Event(fsmEvent);
		}

		public void Event(FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			SetEventDataSentByInfo();
			if (eventTarget == null)
			{
				eventTarget = targetSelf;
			}
			if (FsmLog.LoggingEnabled && eventTarget.target != FsmEventTarget.EventTarget.Self)
			{
				MyLog.LogSendEvent(activeState, fsmEvent, eventTarget);
			}
			switch (eventTarget.target)
			{
			case FsmEventTarget.EventTarget.Self:
				ProcessEvent(fsmEvent);
				break;
			case FsmEventTarget.EventTarget.GameObject:
			{
				GameObject ownerDefaultTarget = GetOwnerDefaultTarget(eventTarget.gameObject);
				BroadcastEventToGameObject(ownerDefaultTarget, fsmEvent, EventData, eventTarget.sendToChildren.Value, eventTarget.excludeSelf.Value);
				break;
			}
			case FsmEventTarget.EventTarget.GameObjectFSM:
			{
				GameObject ownerDefaultTarget = GetOwnerDefaultTarget(eventTarget.gameObject);
				SendEventToFsmOnGameObject(ownerDefaultTarget, eventTarget.fsmName.Value, fsmEvent);
				break;
			}
			case FsmEventTarget.EventTarget.FSMComponent:
				if (eventTarget.fsmComponent != null)
				{
					eventTarget.fsmComponent.Fsm.ProcessEvent(fsmEvent);
				}
				break;
			case FsmEventTarget.EventTarget.BroadcastAll:
				BroadcastEvent(fsmEvent, eventTarget.excludeSelf.Value);
				break;
			case FsmEventTarget.EventTarget.HostFSM:
				if (Host != null)
				{
					Host.ProcessEvent(fsmEvent);
				}
				break;
			case FsmEventTarget.EventTarget.SubFSMs:
				foreach (Fsm item in new List<Fsm>(SubFsmList))
				{
					item.ProcessEvent(fsmEvent);
				}
				break;
			}
			if (FsmExecutionStack.ExecutingFsm != this)
			{
				FsmExecutionStack.PushFsm(this);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void Event(string fsmEventName)
		{
			if (!string.IsNullOrEmpty(fsmEventName))
			{
				Event(FsmEvent.GetFsmEvent(fsmEventName));
			}
		}

		public void Event(FsmEvent fsmEvent)
		{
			if (fsmEvent != null)
			{
				Event(EventTarget, fsmEvent);
			}
		}

		public DelayedEvent DelayedEvent(FsmEvent fsmEvent, float delay)
		{
			DelayedEvent delayedEvent = new DelayedEvent(this, fsmEvent, delay);
			delayedEvents.Add(delayedEvent);
			return delayedEvent;
		}

		public DelayedEvent DelayedEvent(FsmEventTarget eventTarget, FsmEvent fsmEvent, float delay)
		{
			DelayedEvent delayedEvent = new DelayedEvent(this, eventTarget, fsmEvent, delay);
			delayedEvents.Add(delayedEvent);
			return delayedEvent;
		}

		public void BroadcastEvent(string fsmEventName, bool excludeSelf = false)
		{
			if (!string.IsNullOrEmpty(fsmEventName))
			{
				BroadcastEvent(FsmEvent.GetFsmEvent(fsmEventName), excludeSelf);
			}
		}

		public void BroadcastEvent(FsmEvent fsmEvent, bool excludeSelf = false)
		{
			FsmEventData eventData = new FsmEventData(EventData);
			SetEventDataSentByInfo();
			FsmEventData source = new FsmEventData(EventData);
			foreach (PlayMakerFSM item in new List<PlayMakerFSM>(PlayMakerFSM.FsmList))
			{
				if (!(item == null) && item.Fsm != null && (!excludeSelf || item.Fsm != this))
				{
					EventData = new FsmEventData(source);
					item.Fsm.ProcessEvent(fsmEvent);
				}
			}
			EventData = eventData;
		}

		public void BroadcastEventToGameObject(GameObject go, string fsmEventName, bool sendToChildren, bool excludeSelf = false)
		{
			if (!string.IsNullOrEmpty(fsmEventName))
			{
				BroadcastEventToGameObject(go, FsmEvent.GetFsmEvent(fsmEventName), EventData, sendToChildren, excludeSelf);
			}
		}

		public void BroadcastEventToGameObject(GameObject go, FsmEvent fsmEvent, FsmEventData eventData, bool sendToChildren, bool excludeSelf = false)
		{
			if (go == null)
			{
				return;
			}
			FsmEventData eventData2 = new FsmEventData(EventData);
			SetEventDataSentByInfo();
			List<Fsm> list = new List<Fsm>();
			foreach (PlayMakerFSM fsm in PlayMakerFSM.FsmList)
			{
				if (!(fsm == null))
				{
					if (fsm.gameObject == go)
					{
						list.Add(fsm.Fsm);
					}
					else if (sendToChildren && IsFsmChildOfGameObject(fsm, go.transform))
					{
						list.Add(fsm.Fsm);
					}
				}
			}
			if (excludeSelf)
			{
				list.Remove(this);
			}
			foreach (Fsm item in list)
			{
				EventData = new FsmEventData(eventData);
				item.ProcessEvent(fsmEvent);
			}
			EventData = eventData2;
		}

		private bool IsFsmChildOfGameObject(PlayMakerFSM fsm, Transform root)
		{
			if (fsm == null)
			{
				return false;
			}
			GameObject gameObject = fsm.gameObject;
			if (gameObject == null)
			{
				return false;
			}
			Transform transform = gameObject.transform;
			while (transform != null)
			{
				if (transform == root)
				{
					return true;
				}
				transform = transform.parent;
			}
			return false;
		}

		public void SendEventToFsmOnGameObject(GameObject gameObject, string fsmName, string fsmEventName)
		{
			if (!string.IsNullOrEmpty(fsmEventName))
			{
				SendEventToFsmOnGameObject(gameObject, fsmName, FsmEvent.GetFsmEvent(fsmEventName));
			}
		}

		public void SendEventToFsmOnGameObject(GameObject gameObject, string fsmName, FsmEvent fsmEvent)
		{
			if (gameObject == null)
			{
				return;
			}
			SetEventDataSentByInfo();
			List<PlayMakerFSM> list = new List<PlayMakerFSM>(PlayMakerFSM.FsmList);
			if (string.IsNullOrEmpty(fsmName))
			{
				foreach (PlayMakerFSM item in list)
				{
					if (item != null && item.gameObject == gameObject)
					{
						item.Fsm.ProcessEvent(fsmEvent);
					}
				}
				return;
			}
			foreach (PlayMakerFSM item2 in list)
			{
				if (item2 != null && item2.gameObject == gameObject && fsmName == item2.Fsm.Name)
				{
					item2.Fsm.ProcessEvent(fsmEvent);
					break;
				}
			}
		}

		public bool HasState(string stateName)
		{
			FsmState[] array = States;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Name == stateName)
				{
					return true;
				}
			}
			return false;
		}

		public void SetState(string stateName)
		{
			SwitchState(GetState(stateName));
		}

		public void UpdateStateChanges()
		{
			while (IsSwitchingState && !HitBreakpoint)
			{
				SwitchState(switchToState);
			}
			for (int i = 0; i < States.Length; i++)
			{
				States[i].ResetLoopCount();
			}
			EventTarget = null;
		}

		private bool DoTransition(FsmTransition transition, bool isGlobal)
		{
			FsmState toFsmState = transition.ToFsmState;
			if (toFsmState == null)
			{
				return false;
			}
			LastTransition = transition;
			if (PlayMakerGlobals.IsEditor)
			{
				MyLog.LogTransition(isGlobal ? null : ActiveState, transition);
			}
			switchToState = toFsmState;
			if (EventData.SentByFsm != this)
			{
				UpdateStateChanges();
			}
			return true;
		}

		public void SwitchState(FsmState toState)
		{
			if (toState != null)
			{
				if (ActiveState != null && activeStateEntered)
				{
					ExitState(ActiveState);
				}
				ActiveState = toState;
				if ((BreakpointsEnabled && EnableBreakpoints && toState.IsBreakpoint) || (StepToStateChange && (StepFsm == null || StepFsm == this)))
				{
					DoBreakpoint();
				}
				else
				{
					EnterState(toState);
				}
			}
		}

		public void GotoPreviousState()
		{
			if (PreviousActiveState != null)
			{
				SwitchState(PreviousActiveState);
			}
		}

		public void ReEnterState()
		{
			SwitchState(ActiveState);
		}

		private void EnterState(FsmState state)
		{
			if (state == null)
			{
				return;
			}
			EventTarget = null;
			SwitchedState = true;
			activeStateEntered = true;
			switchToState = null;
			if (FsmLog.LoggingEnabled)
			{
				MyLog.LogEnterState(state);
			}
			if (state.loopCount >= MaxLoopCount)
			{
				Owner.enabled = false;
				MyLog.LogError("Loop count exceeded limit: " + MaxLoopCount + ". Override the default limit (1000) in FSM Inspector > Settings > Max Loop Override.");
				return;
			}
			state.Fsm = this;
			if (StateChanged != null)
			{
				StateChanged(state);
			}
			state.OnEnter();
		}

		private void FixedUpdateState(FsmState state)
		{
			state.Fsm = this;
			state.OnFixedUpdate();
			UpdateStateChanges();
		}

		private void UpdateState(FsmState state)
		{
			state.Fsm = this;
			state.OnUpdate();
			UpdateStateChanges();
		}

		private void LateUpdateState(FsmState state)
		{
			state.Fsm = this;
			state.OnLateUpdate();
			UpdateStateChanges();
		}

		private void ExitState(FsmState state)
		{
			PreviousActiveState = state;
			state.Fsm = this;
			if (FsmLog.LoggingEnabled)
			{
				MyLog.LogExitState(state);
			}
			ActiveState = null;
			state.OnExit();
			if (!keepDelayedEventsOnStateExit)
			{
				KillDelayedEvents();
			}
		}

		public Fsm GetSubFsm(string subFsmName)
		{
			for (int i = 0; i < SubFsmList.Count; i++)
			{
				Fsm fsm = SubFsmList[i];
				if (fsm != null && fsm.name == subFsmName)
				{
					return fsm;
				}
			}
			return null;
		}

		public static string GetFullFsmLabel(Fsm fsm)
		{
			if (fsm == null)
			{
				return "None (FSM)";
			}
			return fsm.OwnerName + " : " + fsm.Name;
		}

		public GameObject GetOwnerDefaultTarget(FsmOwnerDefault ownerDefault)
		{
			if (ownerDefault == null)
			{
				return null;
			}
			if (ownerDefault.OwnerOption != OwnerDefaultOption.UseOwner)
			{
				return ownerDefault.GameObject.Value;
			}
			return GameObject;
		}

		public FsmState GetState(string stateName)
		{
			FsmState[] array = states;
			foreach (FsmState fsmState in array)
			{
				if (fsmState.Name == stateName)
				{
					return fsmState;
				}
			}
			return null;
		}

		public static int GetStateIndex(Fsm fsm, FsmState state)
		{
			if (fsm == null || state == null)
			{
				return -1;
			}
			string text = state.Name;
			for (int i = 0; i < fsm.States.Length; i++)
			{
				if (fsm.States[i].Name == text)
				{
					return i;
				}
			}
			return -1;
		}

		[Obsolete("Use FsmEvent.GetFsmEvent(eventName) instead.")]
		public FsmEvent GetEvent(string eventName)
		{
			return FsmEvent.GetFsmEvent(eventName);
		}

		public FsmEvent FindEvent(string eventName)
		{
			FsmEvent[] array = Events;
			foreach (FsmEvent fsmEvent in array)
			{
				if (fsmEvent.Name == eventName)
				{
					return fsmEvent;
				}
			}
			return null;
		}

		public int CompareTo(object obj)
		{
			if (obj is Fsm fsm)
			{
				return GuiLabel.CompareTo(fsm.GuiLabel);
			}
			return 0;
		}

		public List<FsmTransition> GetGlobalTransitionsToState(FsmState state)
		{
			List<FsmTransition> list = new List<FsmTransition>();
			if (state == null)
			{
				return list;
			}
			string text = state.Name;
			FsmTransition[] array = GlobalTransitions;
			foreach (FsmTransition fsmTransition in array)
			{
				if (fsmTransition.ToState == text)
				{
					list.Add(fsmTransition);
				}
			}
			return list;
		}

		public FsmObject GetFsmObject(string varName)
		{
			return variables.GetFsmObject(varName);
		}

		public FsmMaterial GetFsmMaterial(string varName)
		{
			return variables.GetFsmMaterial(varName);
		}

		public FsmTexture GetFsmTexture(string varName)
		{
			return variables.GetFsmTexture(varName);
		}

		public FsmFloat GetFsmFloat(string varName)
		{
			return variables.GetFsmFloat(varName);
		}

		public FsmInt GetFsmInt(string varName)
		{
			return variables.GetFsmInt(varName);
		}

		public FsmBool GetFsmBool(string varName)
		{
			return variables.GetFsmBool(varName);
		}

		public FsmString GetFsmString(string varName)
		{
			return variables.GetFsmString(varName);
		}

		public FsmVector2 GetFsmVector2(string varName)
		{
			return variables.GetFsmVector2(varName);
		}

		public FsmVector3 GetFsmVector3(string varName)
		{
			return variables.GetFsmVector3(varName);
		}

		public FsmRect GetFsmRect(string varName)
		{
			return variables.GetFsmRect(varName);
		}

		public FsmQuaternion GetFsmQuaternion(string varName)
		{
			return variables.GetFsmQuaternion(varName);
		}

		public FsmColor GetFsmColor(string varName)
		{
			return variables.GetFsmColor(varName);
		}

		public FsmGameObject GetFsmGameObject(string varName)
		{
			return variables.GetFsmGameObject(varName);
		}

		public FsmArray GetFsmArray(string varName)
		{
			return variables.GetFsmArray(varName);
		}

		public FsmEnum GetFsmEnum(string varName)
		{
			return variables.GetFsmEnum(varName);
		}

		public void OnDrawGizmos()
		{
			if (owner == null)
			{
				return;
			}
			if (PlayMakerFSM.DrawGizmos)
			{
				Gizmos.DrawIcon(owner.transform.position, "PlaymakerIcon.png");
			}
			if (EditState == null)
			{
				return;
			}
			EditState.Fsm = this;
			if (EditState.ActionData != null)
			{
				FsmStateAction[] actions = EditState.Actions;
				for (int i = 0; i < actions.Length; i++)
				{
					actions[i].OnDrawActionGizmos();
				}
			}
		}

		public void OnDrawGizmosSelected()
		{
			if (EditState == null)
			{
				return;
			}
			EditState.Fsm = this;
			if (EditState.ActionData != null)
			{
				FsmStateAction[] actions = EditState.Actions;
				for (int i = 0; i < actions.Length; i++)
				{
					actions[i].OnDrawActionGizmosSelected();
				}
			}
		}

		public void OnCollisionEnter(Collision collisionInfo)
		{
			FsmExecutionStack.PushFsm(this);
			CollisionInfo = collisionInfo;
			CollisionName = collisionInfo.gameObject.name;
			if (ActiveState.OnCollisionEnter(collisionInfo))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.CollisionEnter);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnCollisionStay(Collision collisionInfo)
		{
			FsmExecutionStack.PushFsm(this);
			CollisionInfo = collisionInfo;
			CollisionName = collisionInfo.gameObject.name;
			if (ActiveState.OnCollisionStay(collisionInfo))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.CollisionStay);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnCollisionExit(Collision collisionInfo)
		{
			FsmExecutionStack.PushFsm(this);
			CollisionInfo = collisionInfo;
			CollisionName = collisionInfo.gameObject.name;
			if (ActiveState.OnCollisionExit(collisionInfo))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.CollisionExit);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			FsmExecutionStack.PushFsm(this);
			TriggerCollider = other;
			TriggerName = other.gameObject.name;
			if (ActiveState.OnTriggerEnter(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.TriggerEnter);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnTriggerStay(Collider other)
		{
			FsmExecutionStack.PushFsm(this);
			TriggerCollider = other;
			TriggerName = other.gameObject.name;
			if (ActiveState.OnTriggerStay(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.TriggerStay);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnTriggerExit(Collider other)
		{
			FsmExecutionStack.PushFsm(this);
			TriggerCollider = other;
			TriggerName = other.gameObject.name;
			if (ActiveState.OnTriggerExit(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.TriggerExit);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnParticleCollision(GameObject other)
		{
			FsmExecutionStack.PushFsm(this);
			ParticleCollisionGO = other;
			if (ActiveState.OnParticleCollision(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.ParticleCollision);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnCollisionEnter2D(Collision2D collisionInfo)
		{
			FsmExecutionStack.PushFsm(this);
			Collision2DInfo = collisionInfo;
			Collision2dName = collisionInfo.gameObject.name;
			if (ActiveState.OnCollisionEnter2D(collisionInfo))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.CollisionEnter2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnCollisionStay2D(Collision2D collisionInfo)
		{
			FsmExecutionStack.PushFsm(this);
			Collision2DInfo = collisionInfo;
			Collision2dName = collisionInfo.gameObject.name;
			if (ActiveState.OnCollisionStay2D(collisionInfo))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.CollisionStay2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnCollisionExit2D(Collision2D collisionInfo)
		{
			FsmExecutionStack.PushFsm(this);
			Collision2DInfo = collisionInfo;
			Collision2dName = collisionInfo.gameObject.name;
			if (ActiveState.OnCollisionExit2D(collisionInfo))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.CollisionExit2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnTriggerEnter2D(Collider2D other)
		{
			FsmExecutionStack.PushFsm(this);
			TriggerCollider2D = other;
			Trigger2dName = other.name;
			if (ActiveState.OnTriggerEnter2D(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.TriggerEnter2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnTriggerStay2D(Collider2D other)
		{
			FsmExecutionStack.PushFsm(this);
			TriggerCollider2D = other;
			Trigger2dName = other.name;
			if (ActiveState.OnTriggerStay2D(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.TriggerStay2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnTriggerExit2D(Collider2D other)
		{
			FsmExecutionStack.PushFsm(this);
			TriggerCollider2D = other;
			Trigger2dName = other.name;
			if (ActiveState.OnTriggerExit2D(other))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.TriggerExit2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnControllerColliderHit(ControllerColliderHit collider)
		{
			FsmExecutionStack.PushFsm(this);
			ControllerCollider = collider;
			if (ActiveState.OnControllerColliderHit(collider))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.ControllerColliderHit);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnJointBreak(float breakForce)
		{
			FsmExecutionStack.PushFsm(this);
			JointBreakForce = breakForce;
			if (ActiveState.OnJointBreak(breakForce))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.JointBreak);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnJointBreak2D(Joint2D brokenJoint)
		{
			FsmExecutionStack.PushFsm(this);
			BrokenJoint2D = brokenJoint;
			if (ActiveState.OnJointBreak2D(brokenJoint))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				Event(FsmEvent.JointBreak2D);
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnAnimatorMove()
		{
			FsmExecutionStack.PushFsm(this);
			if (ActiveState.OnAnimatorMove())
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnAnimatorIK(int layerIndex)
		{
			FsmExecutionStack.PushFsm(this);
			if (ActiveState.OnAnimatorIK(layerIndex))
			{
				UpdateStateChanges();
				FsmExecutionStack.PopFsm();
			}
			else
			{
				FsmExecutionStack.PopFsm();
			}
		}

		public void OnGUI()
		{
			if (ActiveState != null)
			{
				ActiveState.OnGUI();
			}
		}

		private void DoBreakpoint()
		{
			activeStateEntered = false;
			DoBreak();
		}

		public void DoBreakError(string error)
		{
			IsErrorBreak = true;
			LastError = error;
			DoBreak();
		}

		private void DoBreak()
		{
			BreakAtFsm = FsmExecutionStack.ExecutingFsm;
			BreakAtState = FsmExecutionStack.ExecutingState;
			HitBreakpoint = true;
			IsBreak = true;
			if (FsmLog.LoggingEnabled && !PlayMakerFSM.ApplicationIsQuitting)
			{
				MyLog.LogBreak();
			}
			StepToStateChange = false;
		}

		private void Continue()
		{
			activeStateEntered = true;
			HitBreakpoint = false;
			IsErrorBreak = false;
			IsBreak = false;
			EnterState(ActiveState);
		}

		public void OnDestroy()
		{
			if (subFsmList != null)
			{
				foreach (Fsm subFsm in subFsmList)
				{
					subFsm.OnDestroy();
				}
				subFsmList.Clear();
			}
			if (EventData.SentByFsm == this)
			{
				EventData = new FsmEventData();
			}
			if (PlayMakerGUI.SelectedFSM == this)
			{
				PlayMakerGUI.SelectedFSM = null;
			}
			if (myLog != null)
			{
				myLog.OnDestroy();
			}
			if (lastRaycastHit2DInfoLUT != null)
			{
				lastRaycastHit2DInfoLUT.Remove(this);
			}
			owner = null;
		}

		[Conditional("DEBUG_LOG")]
		private void DebugLog(object message, LogColor logColor = LogColor.None)
		{
		}

		[Conditional("DEBUG_EVENTS")]
		private void DebugEvent(object message, LogColor logColor = LogColor.None)
		{
		}

		[Conditional("DEBUG_LIFETIME")]
		private void DebugLifetime(object message, LogColor logColor = LogColor.None)
		{
		}

		[Conditional("DEBUG_DEBUGGER")]
		private void DebugDebugger(object message, LogColor logColor = LogColor.None)
		{
		}
	}
}
