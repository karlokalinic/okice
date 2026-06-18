using System;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmEvent : IComparable, INameable
	{
		private static Dictionary<string, FsmEvent> _eventLookup;

		private static readonly object syncObj = new object();

		[SerializeField]
		private string name;

		[SerializeField]
		private bool isSystemEvent;

		[SerializeField]
		private bool isGlobal;

		[NonSerialized]
		private string _path;

		public static PlayMakerGlobals GlobalsComponent => PlayMakerGlobals.Instance;

		public static List<string> globalEvents => PlayMakerGlobals.Instance.Events;

		private static Dictionary<string, FsmEvent> eventLookup
		{
			get
			{
				if (_eventLookup == null)
				{
					_eventLookup = new Dictionary<string, FsmEvent>(100);
					Initialize();
				}
				return _eventLookup;
			}
		}

		public static List<FsmEvent> EventList => new List<FsmEvent>(eventLookup.Values);

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

		public bool IsUnityEvent
		{
			get
			{
				if (isSystemEvent)
				{
					return Name != "FINISHED";
				}
				return false;
			}
		}

		public bool IsSystemEvent
		{
			get
			{
				return isSystemEvent;
			}
			set
			{
				isSystemEvent = value;
			}
		}

		public bool IsMouseEvent
		{
			get
			{
				if (this != MouseDown && this != MouseDrag && this != MouseEnter && this != MouseExit && this != MouseOver && this != MouseUp)
				{
					return this == MouseUpAsButton;
				}
				return true;
			}
		}

		public bool IsApplicationEvent
		{
			get
			{
				if (this != ApplicationFocus)
				{
					return this == ApplicationPause;
				}
				return true;
			}
		}

		public bool IsLegacyNetworkEvent
		{
			get
			{
				if (this != NetworkInstantiate && this != PlayerConnected && this != PlayerDisconnected && this != ConnectedToServer && this != DisconnectedFromServer && this != FailedToConnect && this != FailedToConnectToMasterServer && this != MasterServerEvent)
				{
					return this == ServerInitialized;
				}
				return true;
			}
		}

		public bool IsCollisionEvent
		{
			get
			{
				if (this != CollisionEnter && this != CollisionStay)
				{
					return this == CollisionExit;
				}
				return true;
			}
		}

		public bool IsTriggerEvent
		{
			get
			{
				if (this != TriggerEnter && this != TriggerStay)
				{
					return this == TriggerExit;
				}
				return true;
			}
		}

		public bool IsCollision2DEvent
		{
			get
			{
				if (this != CollisionEnter2D && this != CollisionStay2D)
				{
					return this == CollisionExit2D;
				}
				return true;
			}
		}

		public bool IsTrigger2DEvent
		{
			get
			{
				if (this != TriggerEnter2D && this != TriggerStay2D)
				{
					return this == TriggerExit2D;
				}
				return true;
			}
		}

		public bool IsUIEvent
		{
			get
			{
				if (this != UiClick && this != UiBeginDrag && this != UiDrag && this != UiEndDrag && this != UiDrop && this != UiPointerUp && this != UiPointerClick && this != UiPointerDown && this != UiPointerEnter && this != UiPointerExit && this != UiBoolValueChanged && this != UiFloatValueChanged && this != UiIntValueChanged && this != UiVector2ValueChanged)
				{
					return this == UiEndEdit;
				}
				return true;
			}
		}

		public bool IsGlobal
		{
			get
			{
				return isGlobal;
			}
			set
			{
				if (value)
				{
					PlayMakerGlobals.AddGlobalEvent(name);
				}
				else
				{
					PlayMakerGlobals.RemoveGlobalEvent(name);
				}
				isGlobal = value;
				SanityCheckEventList();
			}
		}

		public string Path
		{
			get
			{
				return _path;
			}
			set
			{
				_path = value;
			}
		}

		public static FsmEvent BecameInvisible { get; private set; }

		public static FsmEvent BecameVisible { get; private set; }

		public static FsmEvent CollisionEnter { get; private set; }

		public static FsmEvent CollisionExit { get; private set; }

		public static FsmEvent CollisionStay { get; private set; }

		public static FsmEvent CollisionEnter2D { get; private set; }

		public static FsmEvent CollisionExit2D { get; private set; }

		public static FsmEvent CollisionStay2D { get; private set; }

		public static FsmEvent ControllerColliderHit { get; private set; }

		public static FsmEvent Finished { get; private set; }

		public static FsmEvent LevelLoaded { get; private set; }

		public static FsmEvent MouseDown { get; private set; }

		public static FsmEvent MouseDrag { get; private set; }

		public static FsmEvent MouseEnter { get; private set; }

		public static FsmEvent MouseExit { get; private set; }

		public static FsmEvent MouseOver { get; private set; }

		public static FsmEvent MouseUp { get; private set; }

		public static FsmEvent MouseUpAsButton { get; private set; }

		public static FsmEvent TriggerEnter { get; private set; }

		public static FsmEvent TriggerExit { get; private set; }

		public static FsmEvent TriggerStay { get; private set; }

		public static FsmEvent TriggerEnter2D { get; private set; }

		public static FsmEvent TriggerExit2D { get; private set; }

		public static FsmEvent TriggerStay2D { get; private set; }

		public static FsmEvent ApplicationFocus { get; private set; }

		public static FsmEvent ApplicationPause { get; private set; }

		public static FsmEvent ApplicationQuit { get; private set; }

		public static FsmEvent ParticleCollision { get; private set; }

		public static FsmEvent JointBreak { get; private set; }

		public static FsmEvent JointBreak2D { get; private set; }

		public static FsmEvent Disable { get; private set; }

		public static FsmEvent PlayerConnected { get; private set; }

		public static FsmEvent ServerInitialized { get; private set; }

		public static FsmEvent ConnectedToServer { get; private set; }

		public static FsmEvent PlayerDisconnected { get; private set; }

		public static FsmEvent DisconnectedFromServer { get; private set; }

		public static FsmEvent FailedToConnect { get; private set; }

		public static FsmEvent FailedToConnectToMasterServer { get; private set; }

		public static FsmEvent MasterServerEvent { get; private set; }

		public static FsmEvent NetworkInstantiate { get; private set; }

		public static FsmEvent UiBeginDrag { get; private set; }

		public static FsmEvent UiDrag { get; private set; }

		public static FsmEvent UiEndDrag { get; private set; }

		public static FsmEvent UiClick { get; private set; }

		public static FsmEvent UiDrop { get; private set; }

		public static FsmEvent UiPointerClick { get; private set; }

		public static FsmEvent UiPointerDown { get; private set; }

		public static FsmEvent UiPointerEnter { get; private set; }

		public static FsmEvent UiPointerExit { get; private set; }

		public static FsmEvent UiPointerUp { get; private set; }

		public static FsmEvent UiBoolValueChanged { get; private set; }

		public static FsmEvent UiFloatValueChanged { get; private set; }

		public static FsmEvent UiIntValueChanged { get; private set; }

		public static FsmEvent UiVector2ValueChanged { get; private set; }

		public static FsmEvent UiEndEdit { get; private set; }

		public static void InitInEditor()
		{
			_eventLookup = null;
		}

		private static void Initialize()
		{
			PlayMakerGlobals.Initialize();
			AddSystemEvents();
			AddGlobalEvents();
		}

		public static bool IsNullOrEmpty(FsmEvent fsmEvent)
		{
			if (fsmEvent != null)
			{
				return string.IsNullOrEmpty(fsmEvent.name);
			}
			return true;
		}

		public FsmEvent(string name)
		{
			lock (syncObj)
			{
				this.name = name;
				if (!eventLookup.ContainsKey(name))
				{
					eventLookup[name] = this;
				}
			}
		}

		public FsmEvent(FsmEvent source)
		{
			lock (syncObj)
			{
				if (source != null)
				{
					name = source.name;
					isSystemEvent = source.isSystemEvent;
					isGlobal = source.isGlobal;
					if (eventLookup.TryGetValue(source.name, out var value))
					{
						value.isGlobal |= isGlobal;
					}
					else
					{
						eventLookup.Add(source.name, source);
					}
				}
			}
		}

		int IComparable.CompareTo(object obj)
		{
			FsmEvent fsmEvent = (FsmEvent)obj;
			if (isSystemEvent && !fsmEvent.isSystemEvent)
			{
				return -1;
			}
			if (!isSystemEvent && fsmEvent.isSystemEvent)
			{
				return 1;
			}
			return string.Compare(name, fsmEvent.name, StringComparison.OrdinalIgnoreCase);
		}

		public static bool EventListContainsEvent(List<FsmEvent> fsmEventList, string fsmEventName)
		{
			lock (syncObj)
			{
				if (fsmEventList == null || string.IsNullOrEmpty(fsmEventName))
				{
					return false;
				}
				for (int i = 0; i < fsmEventList.Count; i++)
				{
					if (fsmEventList[i].Name == fsmEventName)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static void RemoveEventFromEventList(FsmEvent fsmEvent)
		{
			if (fsmEvent.isSystemEvent)
			{
				Debug.LogError("RemoveEventFromEventList: Trying to delete System Event: " + fsmEvent.Name);
			}
			eventLookup.Remove(fsmEvent.name);
		}

		public static FsmEvent FindEvent(string eventName)
		{
			lock (syncObj)
			{
				eventLookup.TryGetValue(eventName, out var value);
				return value;
			}
		}

		public static bool IsEventGlobal(string eventName)
		{
			return globalEvents.Contains(eventName);
		}

		public static void SetEventIsGlobal(string eventName)
		{
			if (eventLookup.TryGetValue(eventName, out var value))
			{
				value.isGlobal = true;
			}
			PlayMakerGlobals.AddGlobalEvent(eventName);
		}

		public static bool EventListContains(string eventName)
		{
			return FindEvent(eventName) != null;
		}

		public static FsmEvent GetFsmEvent(string eventName)
		{
			lock (syncObj)
			{
				if (string.IsNullOrEmpty(eventName))
				{
					return new FsmEvent("");
				}
				if (eventLookup.TryGetValue(eventName, out var value))
				{
					return PlayMakerGlobals.IsPlaying ? value : new FsmEvent(value);
				}
				FsmEvent fsmEvent = new FsmEvent(eventName);
				return PlayMakerGlobals.IsPlaying ? fsmEvent : new FsmEvent(fsmEvent);
			}
		}

		public static FsmEvent GetFsmEvent(FsmEvent fsmEvent)
		{
			if (fsmEvent == null || fsmEvent.name == null)
			{
				return null;
			}
			lock (syncObj)
			{
				if (eventLookup.TryGetValue(fsmEvent.Name, out var value))
				{
					value.isGlobal |= fsmEvent.isGlobal;
					fsmEvent.isGlobal |= value.isGlobal;
					return PlayMakerGlobals.IsPlaying ? value : fsmEvent;
				}
				eventLookup[fsmEvent.name] = fsmEvent;
				return PlayMakerGlobals.IsPlaying ? fsmEvent : new FsmEvent(fsmEvent);
			}
		}

		public static FsmEvent AddFsmEvent(FsmEvent fsmEvent)
		{
			eventLookup.Add(fsmEvent.name, fsmEvent);
			return fsmEvent;
		}

		private static void AddSystemEvents()
		{
			Finished = AddSystemEvent("FINISHED", "System Events");
			Disable = AddSystemEvent("DISABLE", "System Events");
			BecameInvisible = AddSystemEvent("BECAME INVISIBLE", "System Events");
			BecameVisible = AddSystemEvent("BECAME VISIBLE", "System Events");
			LevelLoaded = AddSystemEvent("LEVEL LOADED", "System Events");
			MouseDown = AddSystemEvent("MOUSE DOWN", "System Events");
			MouseDrag = AddSystemEvent("MOUSE DRAG", "System Events");
			MouseEnter = AddSystemEvent("MOUSE ENTER", "System Events");
			MouseExit = AddSystemEvent("MOUSE EXIT", "System Events");
			MouseOver = AddSystemEvent("MOUSE OVER", "System Events");
			MouseUp = AddSystemEvent("MOUSE UP", "System Events");
			MouseUpAsButton = AddSystemEvent("MOUSE UP AS BUTTON", "System Events");
			CollisionEnter = AddSystemEvent("COLLISION ENTER", "System Events");
			CollisionExit = AddSystemEvent("COLLISION EXIT", "System Events");
			CollisionStay = AddSystemEvent("COLLISION STAY", "System Events");
			ControllerColliderHit = AddSystemEvent("CONTROLLER COLLIDER HIT", "System Events");
			TriggerEnter = AddSystemEvent("TRIGGER ENTER", "System Events");
			TriggerExit = AddSystemEvent("TRIGGER EXIT", "System Events");
			TriggerStay = AddSystemEvent("TRIGGER STAY", "System Events");
			CollisionEnter2D = AddSystemEvent("COLLISION ENTER 2D", "System Events");
			CollisionExit2D = AddSystemEvent("COLLISION EXIT 2D", "System Events");
			CollisionStay2D = AddSystemEvent("COLLISION STAY 2D", "System Events");
			TriggerEnter2D = AddSystemEvent("TRIGGER ENTER 2D", "System Events");
			TriggerExit2D = AddSystemEvent("TRIGGER EXIT 2D", "System Events");
			TriggerStay2D = AddSystemEvent("TRIGGER STAY 2D", "System Events");
			PlayerConnected = AddSystemEvent("PLAYER CONNECTED", "Network Events");
			ServerInitialized = AddSystemEvent("SERVER INITIALIZED", "Network Events");
			ConnectedToServer = AddSystemEvent("CONNECTED TO SERVER", "Network Events");
			PlayerDisconnected = AddSystemEvent("PLAYER DISCONNECTED", "Network Events");
			DisconnectedFromServer = AddSystemEvent("DISCONNECTED FROM SERVER", "Network Events");
			FailedToConnect = AddSystemEvent("FAILED TO CONNECT", "Network Events");
			FailedToConnectToMasterServer = AddSystemEvent("FAILED TO CONNECT TO MASTER SERVER", "Network Events");
			MasterServerEvent = AddSystemEvent("MASTER SERVER EVENT", "Network Events");
			NetworkInstantiate = AddSystemEvent("NETWORK INSTANTIATE", "Network Events");
			ApplicationFocus = AddSystemEvent("APPLICATION FOCUS", "System Events");
			ApplicationPause = AddSystemEvent("APPLICATION PAUSE", "System Events");
			ApplicationQuit = AddSystemEvent("APPLICATION QUIT", "System Events");
			ParticleCollision = AddSystemEvent("PARTICLE COLLISION", "System Events");
			JointBreak = AddSystemEvent("JOINT BREAK", "System Events");
			JointBreak2D = AddSystemEvent("JOINT BREAK 2D", "System Events");
			UiBeginDrag = AddSystemEvent("UI BEGIN DRAG", "UI Events");
			UiDrag = AddSystemEvent("UI DRAG", "UI Events");
			UiEndDrag = AddSystemEvent("UI END DRAG", "UI Events");
			UiClick = AddSystemEvent("UI CLICK", "UI Events");
			UiDrop = AddSystemEvent("UI DROP", "UI Events");
			UiPointerClick = AddSystemEvent("UI POINTER CLICK", "UI Events");
			UiPointerDown = AddSystemEvent("UI POINTER DOWN", "UI Events");
			UiPointerEnter = AddSystemEvent("UI POINTER ENTER", "UI Events");
			UiPointerExit = AddSystemEvent("UI POINTER EXIT", "UI Events");
			UiPointerUp = AddSystemEvent("UI POINTER UP", "UI Events");
			UiBoolValueChanged = AddSystemEvent("UI BOOL VALUE CHANGED", "UI Events");
			UiFloatValueChanged = AddSystemEvent("UI FLOAT VALUE CHANGED", "UI Events");
			UiIntValueChanged = AddSystemEvent("UI INT VALUE CHANGED", "UI Events");
			UiVector2ValueChanged = AddSystemEvent("UI VECTOR2 VALUE CHANGED", "UI Events");
			UiEndEdit = AddSystemEvent("UI END EDIT", "UI Events");
		}

		private static FsmEvent AddSystemEvent(string eventName, string path = "")
		{
			return new FsmEvent(eventName)
			{
				IsSystemEvent = true,
				Path = ((path == "") ? "" : (path + "/"))
			};
		}

		private static void AddGlobalEvents()
		{
			for (int i = 0; i < globalEvents.Count; i++)
			{
				new FsmEvent(globalEvents[i]).isGlobal = true;
			}
		}

		public static void SanityCheckEventList()
		{
			foreach (FsmEvent @event in EventList)
			{
				if (IsEventGlobal(@event.name))
				{
					@event.isGlobal = true;
				}
				if (@event.isGlobal && !globalEvents.Contains(@event.name))
				{
					globalEvents.Add(@event.name);
				}
			}
		}
	}
}
