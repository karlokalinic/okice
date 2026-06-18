using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[Serializable]
	public class UIDropdownField
	{
		[SerializeField]
		private Dropdown m_uiDropdown;

		[SerializeField]
		private TMP_Dropdown m_tmpDropdown;

		public Dropdown uiDropdown
		{
			get
			{
				return m_uiDropdown;
			}
			set
			{
				m_uiDropdown = value;
			}
		}

		public TMP_Dropdown tmpDropdown
		{
			get
			{
				return m_tmpDropdown;
			}
			set
			{
				m_tmpDropdown = value;
			}
		}

		public int value
		{
			get
			{
				if (m_tmpDropdown != null)
				{
					return m_tmpDropdown.value;
				}
				if (m_uiDropdown != null)
				{
					return m_uiDropdown.value;
				}
				return 0;
			}
			set
			{
				if (m_tmpDropdown != null)
				{
					m_tmpDropdown.value = value;
				}
				if (m_uiDropdown != null)
				{
					m_uiDropdown.value = value;
				}
			}
		}

		public bool enabled
		{
			get
			{
				if (m_tmpDropdown != null)
				{
					return m_tmpDropdown.enabled;
				}
				if (m_uiDropdown != null)
				{
					return m_uiDropdown.enabled;
				}
				return false;
			}
			set
			{
				if (m_tmpDropdown != null)
				{
					m_tmpDropdown.enabled = value;
				}
				if (m_uiDropdown != null)
				{
					m_uiDropdown.enabled = value;
				}
			}
		}

		public GameObject gameObject
		{
			get
			{
				if (tmpDropdown != null)
				{
					return tmpDropdown.gameObject;
				}
				if (!(uiDropdown != null))
				{
					return null;
				}
				return uiDropdown.gameObject;
			}
		}

		public bool isActiveSelf
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeSelf;
			}
		}

		public bool activeInHierarchy
		{
			get
			{
				if (!(gameObject != null))
				{
					return false;
				}
				return gameObject.activeInHierarchy;
			}
		}

		public UIDropdownField()
		{
			uiDropdown = null;
			m_tmpDropdown = null;
		}

		public UIDropdownField(Dropdown uiDropdown)
		{
			this.uiDropdown = uiDropdown;
			m_tmpDropdown = null;
		}

		public UIDropdownField(TMP_Dropdown tmpDropdown)
		{
			uiDropdown = null;
			m_tmpDropdown = tmpDropdown;
		}

		public void SetActive(bool value)
		{
			if (uiDropdown != null)
			{
				uiDropdown.gameObject.SetActive(value);
			}
			if (tmpDropdown != null)
			{
				tmpDropdown.gameObject.SetActive(value);
			}
		}

		public void ClearOptions()
		{
			if (uiDropdown != null)
			{
				uiDropdown.ClearOptions();
			}
			if (tmpDropdown != null)
			{
				tmpDropdown.ClearOptions();
			}
		}

		public void AddOption(string text)
		{
			if (uiDropdown != null)
			{
				uiDropdown.options.Add(new Dropdown.OptionData(text));
			}
			if (tmpDropdown != null)
			{
				tmpDropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
		}

		public static bool IsNull(UIDropdownField uiDropdownField)
		{
			if (uiDropdownField == null)
			{
				return true;
			}
			if (uiDropdownField.uiDropdown != null)
			{
				return false;
			}
			if (uiDropdownField.tmpDropdown != null)
			{
				return false;
			}
			return true;
		}
	}
}
