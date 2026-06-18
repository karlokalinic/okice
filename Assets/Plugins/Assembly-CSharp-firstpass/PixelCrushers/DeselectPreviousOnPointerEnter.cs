using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(Selectable))]
	public class DeselectPreviousOnPointerEnter : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IDeselectHandler, IEventSystemUser
	{
		[Tooltip("Do not deselect previous if previous is in this Exceptions list.")]
		[SerializeField]
		private List<GameObject> exceptions = new List<GameObject>();

		private EventSystem m_eventSystem;

		public EventSystem eventSystem
		{
			get
			{
				if (m_eventSystem == null)
				{
					m_eventSystem = EventSystem.current;
				}
				return m_eventSystem;
			}
			set
			{
				m_eventSystem = value;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!(eventSystem == null) && !eventSystem.alreadySelecting && !exceptions.Contains(eventSystem.currentSelectedGameObject))
			{
				eventSystem.SetSelectedGameObject(base.gameObject);
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			GetComponent<Selectable>().OnPointerExit(null);
		}
	}
}
