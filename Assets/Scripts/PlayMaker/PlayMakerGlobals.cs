using System;
using System.Collections.Generic;
using System.Diagnostics;
using HutongGames;
using HutongGames.PlayMaker;
using UnityEngine;

public class PlayMakerGlobals : ScriptableObject
{
	private static PlayMakerGlobals instance;

	[NonSerialized]
	private bool initialized;

	[SerializeField]
	private FsmVariables variables = new FsmVariables();

	[SerializeField]
	private List<string> events = new List<string>();

	public static bool Initialized
	{
		get
		{
			if (instance != null)
			{
				return instance.initialized;
			}
			return false;
		}
	}

	public static bool IsPlayingInEditor { get; private set; }

	public static bool IsPlaying { get; private set; }

	public static bool IsEditor { get; private set; }

	public static bool IsBuilding { get; set; }

	public static PlayMakerGlobals Instance
	{
		get
		{
			Initialize();
			return instance;
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

	public List<string> Events
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

	public static void InitInEditor()
	{
		instance = null;
	}

	public static void InitApplicationFlags()
	{
		IsPlayingInEditor = Application.isEditor && Application.isPlaying;
		IsPlaying = Application.isPlaying || IsBuilding;
		IsEditor = Application.isEditor;
	}

	public static void Initialize()
	{
		if (Initialized)
		{
			return;
		}
		InitApplicationFlags();
		UnityEngine.Object obj = Resources.Load("PlayMakerGlobals", typeof(PlayMakerGlobals));
		if (obj != null)
		{
			if (IsPlayingInEditor)
			{
				if (instance != null)
				{
					Resources.UnloadAsset(instance);
				}
				PlayMakerGlobals playMakerGlobals = (PlayMakerGlobals)obj;
				instance = ScriptableObject.CreateInstance<PlayMakerGlobals>();
				instance.Variables = new FsmVariables(playMakerGlobals.variables);
				instance.Events = new List<string>(playMakerGlobals.Events);
			}
			else
			{
				instance = obj as PlayMakerGlobals;
			}
		}
		else
		{
			instance = ScriptableObject.CreateInstance<PlayMakerGlobals>();
		}
		if (instance != null)
		{
			instance.initialized = true;
		}
		else
		{
			UnityEngine.Debug.LogError("Couldn't make PlayMakerGlobals Instance!");
		}
	}

	public FsmEvent AddEvent(string eventName)
	{
		if (!events.Contains(eventName))
		{
			events.Add(eventName);
		}
		FsmEvent obj = FsmEvent.FindEvent(eventName) ?? FsmEvent.GetFsmEvent(eventName);
		obj.IsGlobal = true;
		return obj;
	}

	public static void AddGlobalEvent(string eventName)
	{
		if (!Instance.Events.Contains(eventName))
		{
			Instance.Events.Add(eventName);
		}
	}

	public static void RemoveGlobalEvent(string eventName)
	{
		Instance.events.RemoveAll((string m) => m == eventName);
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	public void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	[Conditional("DEBUG_LOG")]
	private void DebugLog(object message, LogColor logColor = LogColor.None)
	{
	}

	public static void ResetInstance()
	{
		instance = null;
	}
}
