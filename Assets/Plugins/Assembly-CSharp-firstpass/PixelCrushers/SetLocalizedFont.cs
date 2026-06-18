using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class SetLocalizedFont : MonoBehaviour
	{
		[SerializeField]
		protected bool m_setOnEnable = true;

		[Tooltip("Overrides UILocalizationManager's Localized Fonts if set.")]
		[SerializeField]
		protected LocalizedFonts m_localizedFonts;

		protected bool m_started;

		protected float m_initialFontSize = -1f;

		protected Text text;

		protected TextMeshProUGUI textMeshPro;

		protected virtual void Awake()
		{
			text = GetComponent<Text>();
			textMeshPro = GetComponent<TextMeshProUGUI>();
		}

		protected virtual void Start()
		{
			m_started = true;
			if (m_setOnEnable)
			{
				SetCurrentLocalizedFont();
			}
			UILocalizationManager.languageChanged += OnLanguageChanged;
		}

		protected virtual void OnDestroy()
		{
			UILocalizationManager.languageChanged -= OnLanguageChanged;
		}

		protected virtual void OnEnable()
		{
			if (m_started)
			{
				SetCurrentLocalizedFont();
			}
		}

		protected virtual void OnLanguageChanged(string language)
		{
			SetCurrentLocalizedFont();
		}

		public virtual void SetCurrentLocalizedFont()
		{
			if (m_initialFontSize == -1f)
			{
				if (text != null)
				{
					m_initialFontSize = text.fontSize;
				}
				if (textMeshPro != null)
				{
					m_initialFontSize = textMeshPro.fontSize;
				}
			}
			LocalizedFonts localizedFonts = ((m_localizedFonts != null) ? m_localizedFonts : UILocalizationManager.instance.localizedFonts);
			if (localizedFonts == null)
			{
				return;
			}
			if (text != null)
			{
				Font font = localizedFonts.GetFont(UILocalizationManager.instance.currentLanguage);
				if (font != null)
				{
					text.font = font;
					text.fontSize = Mathf.RoundToInt(localizedFonts.GetFontScale(UILocalizationManager.instance.currentLanguage) * m_initialFontSize);
				}
			}
			if (textMeshPro != null)
			{
				TMP_FontAsset textMeshProFont = localizedFonts.GetTextMeshProFont(UILocalizationManager.instance.currentLanguage);
				if (textMeshProFont != null)
				{
					textMeshPro.font = textMeshProFont;
					textMeshPro.fontSize = localizedFonts.GetFontScale(UILocalizationManager.instance.currentLanguage) * m_initialFontSize;
				}
			}
		}
	}
}
