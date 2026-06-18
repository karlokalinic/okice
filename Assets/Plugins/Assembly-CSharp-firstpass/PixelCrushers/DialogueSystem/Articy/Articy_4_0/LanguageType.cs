using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_4_0
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.articy.com/schemas/articydraft/4.0/XmlContentExport_FullProject.xsd")]
	public class LanguageType
	{
		private string isoNameField;

		private string displayNameField;

		private bool isPrimaryField;

		private bool isCustomField;

		private int languageIdField;

		private bool isVoiceOverField;

		public string IsoName
		{
			get
			{
				return isoNameField;
			}
			set
			{
				isoNameField = value;
			}
		}

		public string DisplayName
		{
			get
			{
				return displayNameField;
			}
			set
			{
				displayNameField = value;
			}
		}

		public bool IsPrimary
		{
			get
			{
				return isPrimaryField;
			}
			set
			{
				isPrimaryField = value;
			}
		}

		public bool IsCustom
		{
			get
			{
				return isCustomField;
			}
			set
			{
				isCustomField = value;
			}
		}

		public int LanguageId
		{
			get
			{
				return languageIdField;
			}
			set
			{
				languageIdField = value;
			}
		}

		public bool IsVoiceOver
		{
			get
			{
				return isVoiceOverField;
			}
			set
			{
				isVoiceOverField = value;
			}
		}
	}
}
