using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PixelCrushers
{
	public static class UIUtility
	{
		public static void RequireEventSystem(string message = null)
		{
			if (GameObjectUtility.FindFirstObjectByType<EventSystem>() == null)
			{
				if (message != null)
				{
					Debug.LogWarning(message);
				}
				new GameObject("EventSystem").AddComponent<EventSystem>().gameObject.AddComponent<InputSystemUIInputModule>();
			}
		}

		public static void SetEventSystemInChildren(Transform t, EventSystem eventSystem)
		{
			if (t == null)
			{
				return;
			}
			IEventSystemUser component = t.GetComponent<IEventSystemUser>();
			if (component != null)
			{
				component.eventSystem = eventSystem;
			}
			foreach (Transform item in t)
			{
				SetEventSystemInChildren(item, eventSystem);
			}
		}

		public static int GetAnimatorNameHash(AnimatorStateInfo animatorStateInfo)
		{
			return animatorStateInfo.fullPathHash;
		}

		public static void Select(Selectable selectable, bool allowStealFocus = true, EventSystem eventSystem = null)
		{
			EventSystem eventSystem2 = ((eventSystem != null) ? eventSystem : EventSystem.current);
			if (!(eventSystem2 == null) && !(selectable == null) && !eventSystem2.alreadySelecting && (eventSystem2.currentSelectedGameObject == null || allowStealFocus))
			{
				EventSystem.current = eventSystem2;
				eventSystem2.SetSelectedGameObject(selectable.gameObject);
				selectable.Select();
				selectable.OnSelect(null);
			}
		}

		public static Font GetDefaultFont()
		{
			return Resources.GetBuiltinResource<Font>((SafeConvert.ToInt(Application.unityVersion.Split('.')[0]) >= 2022) ? "LegacyRuntime.ttf" : "Arial.ttf");
		}
	}
}
