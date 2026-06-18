using System.Collections.Generic;
using UnityEngine;

public class PlayMakerPrefs : ScriptableObject
{
	private static PlayMakerPrefs instance;

	private static readonly Color[] defaultColors = new Color[8]
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

	private static readonly string[] defaultColorNames = new string[8] { "Default", "Blue", "Cyan", "Green", "Yellow", "Orange", "Red", "Purple" };

	private static Color[] minimapColors;

	[Tooltip("Output performance warnings to Unity log.\nNote, logging can cause hitches, so you should disabled this in final builds!")]
	[SerializeField]
	private bool logPerformanceWarnings = true;

	[Tooltip("Show Event Handler Components automatically added on GameObjects.\nNormally you want to hide these to keep the Inspector cleaner.")]
	[SerializeField]
	private bool showEventHandlerComponents;

	[Tooltip("How long debug lines are visible for (in seconds).")]
	[SerializeField]
	private float debugLinesDuration = 0.5f;

	[Tooltip("Colors used by States etc.")]
	[SerializeField]
	private Color[] colors = new Color[24]
	{
		Color.grey,
		new Color(0.54509807f, 57f / 85f, 0.9411765f),
		new Color(0.24313726f, 0.7607843f, 0.6901961f),
		new Color(22f / 51f, 0.7607843f, 0.24313726f),
		new Color(1f, 0.8745098f, 16f / 85f),
		new Color(1f, 47f / 85f, 16f / 85f),
		new Color(0.7607843f, 0.24313726f, 0.2509804f),
		new Color(0.54509807f, 0.24313726f, 0.7607843f),
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey,
		Color.grey
	};

	[Tooltip("Descriptive names for each color.")]
	[SerializeField]
	private string[] colorNames = new string[24]
	{
		"Default", "Blue", "Cyan", "Green", "Yellow", "Orange", "Red", "Purple", "", "",
		"", "", "", "", "", "", "", "", "", "",
		"", "", "", ""
	};

	[Tooltip("Color used for Tween From Handles.")]
	[SerializeField]
	private Color tweenFromColor = new Color(0.007843138f, 0.4117647f, 0.9843137f);

	[Tooltip("Color used for Tween To Handles.")]
	[SerializeField]
	private Color tweenToColor = new Color(0.99215686f, 0.5882353f, 0.015686275f);

	[Tooltip("Color used to draw arrows. E.g., velocity, force, direction...")]
	[SerializeField]
	private Color arrowColor = new Color(0.99215686f, 0.5882353f, 0.015686275f);

	[SerializeField]
	private List<string> oldActionNames = new List<string>();

	[SerializeField]
	private List<string> newActionNames = new List<string>();

	[Tooltip("Organize Pools in the hierarchy , and store stacked instances per pool\nNote, this has little impact on performances, but better for organization")]
	[SerializeField]
	private bool organizePoolsInHierarchy = true;

	[Tooltip("If true, will provide a default name to the instances based on their pool index and prefab name\nNote, this has an impact on performance and should be turned off for maximum efficiency")]
	[SerializeField]
	private bool autoNamePoolInstances = true;

	[Tooltip("If true, hides pools organization ( if OrganizePoolsInHierarchy is true) and all pool stacked instances\nNote, this has no impact on performance, but account for a clean hierarchy, you don't need generally to see")]
	[SerializeField]
	private bool hidePoolsInHierarchy = true;

	public static PlayMakerPrefs Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load("PlayMakerPrefs") as PlayMakerPrefs;
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<PlayMakerPrefs>();
				}
			}
			return instance;
		}
	}

	public static float DebugLinesDuration
	{
		get
		{
			return Instance.debugLinesDuration;
		}
		set
		{
			Instance.debugLinesDuration = value;
		}
	}

	public static bool LogPerformanceWarnings
	{
		get
		{
			return Instance.logPerformanceWarnings;
		}
		set
		{
			Instance.logPerformanceWarnings = value;
		}
	}

	public static bool ShowEventHandlerComponents
	{
		get
		{
			return Instance.showEventHandlerComponents;
		}
		set
		{
			Instance.showEventHandlerComponents = value;
		}
	}

	public static Color TweenFromColor
	{
		get
		{
			return Instance.tweenFromColor;
		}
		set
		{
			Instance.tweenFromColor = value;
		}
	}

	public static Color TweenToColor
	{
		get
		{
			return Instance.tweenToColor;
		}
		set
		{
			Instance.tweenToColor = value;
		}
	}

	public static Color ArrowColor
	{
		get
		{
			return Instance.arrowColor;
		}
		set
		{
			Instance.arrowColor = value;
		}
	}

	public static Color[] Colors
	{
		get
		{
			return Instance.colors;
		}
		set
		{
			Instance.colors = value;
		}
	}

	public static string[] ColorNames
	{
		get
		{
			return Instance.colorNames;
		}
		set
		{
			Instance.colorNames = value;
		}
	}

	public static Color[] MinimapColors
	{
		get
		{
			if (minimapColors == null)
			{
				UpdateMinimapColors();
			}
			return minimapColors;
		}
	}

	public static bool OrganizePoolsInHierarchy
	{
		get
		{
			return Instance.organizePoolsInHierarchy;
		}
		set
		{
			Instance.organizePoolsInHierarchy = value;
		}
	}

	public static bool AutoNamePoolInstances
	{
		get
		{
			return Instance.autoNamePoolInstances;
		}
		set
		{
			Instance.autoNamePoolInstances = value;
		}
	}

	public static bool HidePoolsInHierarchy
	{
		get
		{
			return Instance.hidePoolsInHierarchy;
		}
		set
		{
			Instance.hidePoolsInHierarchy = value;
		}
	}

	public static void SaveChanges()
	{
		UpdateMinimapColors();
	}

	public void ResetDefaultColors()
	{
		tweenFromColor = new Color(0.9372549f, 0.34509805f, 0.007843138f);
		tweenToColor = new Color(0.99215686f, 0.5882353f, 0.015686275f);
		arrowColor = new Color(0.99215686f, 0.5882353f, 0.015686275f);
		for (int i = 0; i < defaultColors.Length; i++)
		{
			colors[i] = defaultColors[i];
			colorNames[i] = defaultColorNames[i];
		}
	}

	public void AddActionRenameRule(string oldName, string newName)
	{
		oldActionNames.Add(oldName);
		newActionNames.Add(newName);
	}

	public void DeleteActionRenameRule(int index)
	{
		oldActionNames.RemoveAt(index);
		newActionNames.RemoveAt(index);
	}

	public string GetNewActionName(string oldName)
	{
		if (oldActionNames.Count == 0)
		{
			return oldName;
		}
		string text = oldName;
		string text2;
		do
		{
			text2 = text;
			text = TryGetNewActionName(text);
		}
		while (text != text2);
		return text;
	}

	private static void UpdateMinimapColors()
	{
		minimapColors = new Color[Colors.Length];
		for (int i = 0; i < Colors.Length; i++)
		{
			Color color = Colors[i];
			minimapColors[i] = new Color(color.r, color.g, color.b, 0.5f);
		}
	}

	private string TryGetNewActionName(string oldName)
	{
		for (int i = 0; i < oldActionNames.Count; i++)
		{
			if (oldActionNames[i] == oldName)
			{
				return newActionNames[i];
			}
		}
		return oldName;
	}
}
