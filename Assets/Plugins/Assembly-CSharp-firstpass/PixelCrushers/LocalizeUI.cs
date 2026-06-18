using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class LocalizeUI : MonoBehaviour
	{
		[Tooltip("Overrides the global text table.")]
		[SerializeField]
		private TextTable m_textTable;

		[Tooltip("Overrides the UILocalizationManager's Localized Fonts.")]
		[SerializeField]
		private LocalizedFonts m_localizedFonts;

		[Tooltip("(Optional) If assigned, use this instead of the UI element's text's value as the field lookup value.")]
		[SerializeField]
		private string m_fieldName = string.Empty;

		private bool m_started;

		private List<string> m_fieldNames = new List<string>();

		private List<string> m_tmpFieldNames = new List<string>();

		private Text m_text;

		private Dropdown m_dropdown;

		private TextMeshPro m_textMeshPro;

		private TextMeshProUGUI m_textMeshProUGUI;

		private TMP_Dropdown m_textMeshProDropdown;

		private bool m_lookedForTMP;

		public TextTable textTable
		{
			get
			{
				return m_textTable;
			}
			set
			{
				m_textTable = value;
			}
		}

		public LocalizedFonts localizedFonts
		{
			get
			{
				return m_localizedFonts;
			}
			set
			{
				m_localizedFonts = value;
			}
		}

		public string fieldName
		{
			get
			{
				if (!string.IsNullOrEmpty(m_fieldName))
				{
					return m_fieldName;
				}
				return null;
			}
			set
			{
				m_fieldName = value;
			}
		}

		protected bool started
		{
			get
			{
				return m_started;
			}
			private set
			{
				m_started = value;
			}
		}

		public List<string> fieldNames
		{
			get
			{
				return m_fieldNames;
			}
			set
			{
				m_fieldNames = value;
			}
		}

		public List<string> tmpFieldNames
		{
			get
			{
				return m_tmpFieldNames;
			}
			set
			{
				m_tmpFieldNames = value;
			}
		}

		public Text text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text = value;
			}
		}

		public Dropdown dropdown
		{
			get
			{
				return m_dropdown;
			}
			set
			{
				m_dropdown = value;
			}
		}

		public TextMeshPro textMeshPro
		{
			get
			{
				return m_textMeshPro;
			}
			set
			{
				m_textMeshPro = value;
			}
		}

		public TextMeshProUGUI textMeshProUGUI
		{
			get
			{
				return m_textMeshProUGUI;
			}
			set
			{
				m_textMeshProUGUI = value;
			}
		}

		public TMP_Dropdown textMeshProDropdown
		{
			get
			{
				return m_textMeshProDropdown;
			}
			set
			{
				m_textMeshProDropdown = value;
			}
		}

		protected virtual void Start()
		{
			started = true;
			UpdateText();
		}

		protected virtual void OnEnable()
		{
			if (started)
			{
				UpdateText();
			}
		}

		public virtual void ValidateFieldNames()
		{
			if (!string.IsNullOrEmpty(fieldName) || fieldNames.Count > 0)
			{
				return;
			}
			if (text == null && dropdown == null)
			{
				text = GetComponent<Text>();
				dropdown = GetComponent<Dropdown>();
			}
			bool num = text != null || dropdown != null;
			if (!m_lookedForTMP)
			{
				m_lookedForTMP = true;
				textMeshPro = GetComponent<TextMeshPro>();
				textMeshProUGUI = GetComponent<TextMeshProUGUI>();
				textMeshProDropdown = GetComponent<TMP_Dropdown>();
			}
			if (!num && !(textMeshPro != null) && !(textMeshProUGUI != null) && !(textMeshProDropdown != null))
			{
				return;
			}
			if (string.IsNullOrEmpty(fieldName))
			{
				fieldName = ((text != null) ? text.text : string.Empty);
			}
			if (dropdown != null && fieldNames.Count != dropdown.options.Count)
			{
				fieldNames.Clear();
				dropdown.options.ForEach(delegate(Dropdown.OptionData opt)
				{
					fieldNames.Add(opt.text);
				});
			}
			if (textMeshPro != null && string.IsNullOrEmpty(fieldName))
			{
				fieldName = ((textMeshPro != null) ? textMeshPro.text : string.Empty);
			}
			if (textMeshProUGUI != null && string.IsNullOrEmpty(fieldName))
			{
				fieldName = ((textMeshProUGUI != null) ? textMeshProUGUI.text : string.Empty);
			}
			if (textMeshProDropdown != null && tmpFieldNames.Count != textMeshProDropdown.options.Count)
			{
				tmpFieldNames.Clear();
				textMeshProDropdown.options.ForEach(delegate(TMP_Dropdown.OptionData opt)
				{
					tmpFieldNames.Add(opt.text);
				});
			}
		}

		public virtual void UpdateText()
		{
			string text = ((UILocalizationManager.instance != null) ? UILocalizationManager.instance.currentLanguage : string.Empty);
			if (textTable == null && (UILocalizationManager.instance == null || UILocalizationManager.instance.textTable == null))
			{
				Debug.LogWarning("No localized text table is assigned to " + base.name + " or a UI Localized Manager component.", this);
				return;
			}
			if (!HasLanguage(text))
			{
				Debug.LogWarning("Text table does not have a language '" + text + "'.", textTable);
			}
			LocalizedFonts localizedFonts = ((m_localizedFonts != null) ? m_localizedFonts : UILocalizationManager.instance.localizedFonts);
			Font font = ((localizedFonts != null) ? localizedFonts.GetFont(text) : null);
			if (this.text == null && dropdown == null)
			{
				this.text = GetComponent<Text>();
				dropdown = GetComponent<Dropdown>();
			}
			bool num = this.text != null || dropdown != null;
			TMP_FontAsset tMP_FontAsset = ((localizedFonts != null) ? localizedFonts.GetTextMeshProFont(text) : null);
			if (!m_lookedForTMP)
			{
				m_lookedForTMP = true;
				textMeshPro = GetComponent<TextMeshPro>();
				textMeshProUGUI = GetComponent<TextMeshProUGUI>();
				textMeshProDropdown = GetComponent<TMP_Dropdown>();
			}
			if (!num && !(textMeshPro != null) && !(textMeshProUGUI != null) && !(textMeshProDropdown != null))
			{
				Debug.LogWarning("Localize UI didn't find a localizable UI component on " + base.name + ".", this);
				return;
			}
			if (string.IsNullOrEmpty(fieldName))
			{
				fieldName = ((this.text != null) ? this.text.text : string.Empty);
			}
			if (dropdown != null && fieldNames.Count != dropdown.options.Count)
			{
				fieldNames.Clear();
				dropdown.options.ForEach(delegate(Dropdown.OptionData opt)
				{
					fieldNames.Add(opt.text);
				});
			}
			if (this.text != null)
			{
				if (!HasField(fieldName))
				{
					Debug.LogWarning("Text table does not have a field '" + fieldName + "'.", textTable);
				}
				else
				{
					this.text.text = GetLocalizedText(fieldName);
					if (font != null)
					{
						this.text.font = font;
					}
				}
			}
			if (dropdown != null)
			{
				for (int num2 = 0; num2 < dropdown.options.Count; num2++)
				{
					if (num2 < fieldNames.Count)
					{
						dropdown.options[num2].text = GetLocalizedText(fieldNames[num2]);
					}
				}
				dropdown.captionText.text = GetLocalizedText(fieldNames[dropdown.value]);
				if (font != null)
				{
					dropdown.captionText.font = font;
					dropdown.itemText.font = font;
				}
			}
			if (!m_lookedForTMP)
			{
				m_lookedForTMP = true;
				textMeshPro = GetComponent<TextMeshPro>();
				textMeshProUGUI = GetComponent<TextMeshProUGUI>();
			}
			if (textMeshPro != null)
			{
				if (string.IsNullOrEmpty(fieldName))
				{
					fieldName = ((textMeshPro != null) ? textMeshPro.text : string.Empty);
				}
				if (!HasField(fieldName))
				{
					Debug.LogWarning("Text table does not have a field '" + fieldName + "'.", textTable);
				}
				else
				{
					textMeshPro.text = GetLocalizedText(fieldName);
					if (tMP_FontAsset != null)
					{
						textMeshPro.font = tMP_FontAsset;
						textMeshPro.enabled = false;
						textMeshPro.enabled = true;
					}
				}
			}
			if (textMeshProUGUI != null)
			{
				if (string.IsNullOrEmpty(fieldName))
				{
					fieldName = ((textMeshProUGUI != null) ? textMeshProUGUI.text : string.Empty);
				}
				if (!HasField(fieldName))
				{
					Debug.LogWarning("Text table does not have a field '" + fieldName + "'.", textTable);
				}
				else
				{
					textMeshProUGUI.text = GetLocalizedText(fieldName);
					if (tMP_FontAsset != null)
					{
						textMeshProUGUI.font = tMP_FontAsset;
					}
					textMeshProUGUI.enabled = false;
					textMeshProUGUI.enabled = true;
				}
			}
			if (!(textMeshProDropdown != null))
			{
				return;
			}
			if (tmpFieldNames.Count != textMeshProDropdown.options.Count)
			{
				tmpFieldNames.Clear();
				textMeshProDropdown.options.ForEach(delegate(TMP_Dropdown.OptionData opt)
				{
					tmpFieldNames.Add(opt.text);
				});
			}
			for (int num3 = 0; num3 < textMeshProDropdown.options.Count; num3++)
			{
				if (num3 < tmpFieldNames.Count)
				{
					textMeshProDropdown.options[num3].text = GetLocalizedText(tmpFieldNames[num3]);
				}
			}
			textMeshProDropdown.captionText.text = GetLocalizedText(tmpFieldNames[textMeshProDropdown.value]);
			if (tMP_FontAsset != null)
			{
				textMeshProDropdown.captionText.font = tMP_FontAsset;
				textMeshProDropdown.itemText.font = tMP_FontAsset;
			}
		}

		protected virtual bool HasLanguage(string language)
		{
			if (!(textTable != null) || !textTable.HasLanguage(language))
			{
				return UILocalizationManager.instance.HasLanguage(language);
			}
			return true;
		}

		protected virtual bool HasField(string fieldName)
		{
			if (!(textTable != null) || !textTable.HasField(fieldName))
			{
				return UILocalizationManager.instance.HasField(fieldName);
			}
			return true;
		}

		protected virtual string GetLocalizedText(string fieldName)
		{
			if (!(textTable != null) || !textTable.HasField(fieldName))
			{
				return UILocalizationManager.instance.GetLocalizedText(fieldName);
			}
			return textTable.GetFieldTextForLanguage(fieldName, GlobalTextTable.currentLanguage);
		}

		public virtual void SetFieldName(string newFieldName = "")
		{
			if (text == null)
			{
				text = GetComponent<Text>();
			}
			fieldName = ((string.IsNullOrEmpty(newFieldName) && text != null) ? text.text : newFieldName);
		}

		public virtual void UpdateDropdownOptions()
		{
			fieldNames.Clear();
			tmpFieldNames.Clear();
			UpdateText();
		}
	}
}
