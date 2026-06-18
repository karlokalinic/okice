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
	public class TextPropertyDefinitionType
	{
		private LocalizableTextType displayNameField;

		private string colorField;

		private string technicalNameField;

		private string tooltipTextField;

		private int isMandatoryField;

		private bool isMandatoryFieldSpecified;

		private int isLocalizedField;

		private bool isLocalizedFieldSpecified;

		private string placeholderValueField;

		private TextPropertyDefinitionValueType defaultValueField;

		private string disallowedCharsField;

		private int maxLengthField;

		private bool maxLengthFieldSpecified;

		private int allowsLinebreaksField;

		private bool allowsLinebreaksFieldSpecified;

		private string idField;

		private string basedOnField;

		public LocalizableTextType DisplayName
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

		public string Color
		{
			get
			{
				return colorField;
			}
			set
			{
				colorField = value;
			}
		}

		[XmlElement(DataType = "token")]
		public string TechnicalName
		{
			get
			{
				return technicalNameField;
			}
			set
			{
				technicalNameField = value;
			}
		}

		public string TooltipText
		{
			get
			{
				return tooltipTextField;
			}
			set
			{
				tooltipTextField = value;
			}
		}

		public int IsMandatory
		{
			get
			{
				return isMandatoryField;
			}
			set
			{
				isMandatoryField = value;
			}
		}

		[XmlIgnore]
		public bool IsMandatorySpecified
		{
			get
			{
				return isMandatoryFieldSpecified;
			}
			set
			{
				isMandatoryFieldSpecified = value;
			}
		}

		public int IsLocalized
		{
			get
			{
				return isLocalizedField;
			}
			set
			{
				isLocalizedField = value;
			}
		}

		[XmlIgnore]
		public bool IsLocalizedSpecified
		{
			get
			{
				return isLocalizedFieldSpecified;
			}
			set
			{
				isLocalizedFieldSpecified = value;
			}
		}

		public string PlaceholderValue
		{
			get
			{
				return placeholderValueField;
			}
			set
			{
				placeholderValueField = value;
			}
		}

		public TextPropertyDefinitionValueType DefaultValue
		{
			get
			{
				return defaultValueField;
			}
			set
			{
				defaultValueField = value;
			}
		}

		public string DisallowedChars
		{
			get
			{
				return disallowedCharsField;
			}
			set
			{
				disallowedCharsField = value;
			}
		}

		public int MaxLength
		{
			get
			{
				return maxLengthField;
			}
			set
			{
				maxLengthField = value;
			}
		}

		[XmlIgnore]
		public bool MaxLengthSpecified
		{
			get
			{
				return maxLengthFieldSpecified;
			}
			set
			{
				maxLengthFieldSpecified = value;
			}
		}

		public int AllowsLinebreaks
		{
			get
			{
				return allowsLinebreaksField;
			}
			set
			{
				allowsLinebreaksField = value;
			}
		}

		[XmlIgnore]
		public bool AllowsLinebreaksSpecified
		{
			get
			{
				return allowsLinebreaksFieldSpecified;
			}
			set
			{
				allowsLinebreaksFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public string Id
		{
			get
			{
				return idField;
			}
			set
			{
				idField = value;
			}
		}

		[XmlAttribute]
		public string BasedOn
		{
			get
			{
				return basedOnField;
			}
			set
			{
				basedOnField = value;
			}
		}
	}
}
