using System;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class LayoutOption
	{
		public enum LayoutOptionType
		{
			Width = 0,
			Height = 1,
			MinWidth = 2,
			MaxWidth = 3,
			MinHeight = 4,
			MaxHeight = 5,
			ExpandWidth = 6,
			ExpandHeight = 7
		}

		public LayoutOptionType option;

		public FsmFloat floatParam;

		public FsmBool boolParam;

		public LayoutOption()
		{
			ResetParameters();
		}

		public LayoutOption(LayoutOption source)
		{
			option = source.option;
			floatParam = new FsmFloat(source.floatParam);
			boolParam = new FsmBool(source.boolParam);
		}

		public void ResetParameters()
		{
			floatParam = 0f;
			boolParam = false;
		}

		public GUILayoutOption GetGUILayoutOption()
		{
			return option switch
			{
				LayoutOptionType.Width => GUILayout.Width(floatParam.Value), 
				LayoutOptionType.Height => GUILayout.Height(floatParam.Value), 
				LayoutOptionType.MinWidth => GUILayout.MinWidth(floatParam.Value), 
				LayoutOptionType.MaxWidth => GUILayout.MaxWidth(floatParam.Value), 
				LayoutOptionType.MinHeight => GUILayout.MinHeight(floatParam.Value), 
				LayoutOptionType.MaxHeight => GUILayout.MaxHeight(floatParam.Value), 
				LayoutOptionType.ExpandWidth => GUILayout.ExpandWidth(boolParam.Value), 
				LayoutOptionType.ExpandHeight => GUILayout.ExpandHeight(boolParam.Value), 
				_ => null, 
			};
		}
	}
}
