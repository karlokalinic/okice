using UnityEngine;
using UnityEngine.Serialization;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class OverrideDialogueUI : OverrideUIBase
	{
		[Tooltip("Use this dialogue UI when this GameObject is involved in conversation.")]
		public GameObject ui;

		[Tooltip("If instantiating a prefab, keep it ready in memory instead of destroying it when conversation ends.")]
		[FormerlySerializedAs("dontDestroyPrefabIntance")]
		public bool dontDestroyPrefabInstance = true;

		protected virtual void OnDestroy()
		{
			if (!dontDestroyPrefabInstance && !Tools.IsPrefab(ui))
			{
				Object.Destroy(ui);
			}
		}
	}
}
