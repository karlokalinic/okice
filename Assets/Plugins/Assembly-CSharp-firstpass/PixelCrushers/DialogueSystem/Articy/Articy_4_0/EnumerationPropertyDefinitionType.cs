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
	public class EnumerationPropertyDefinitionType
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

		private int defaultValueField;

		private bool defaultValueFieldSpecified;

		private EnumerationValuesDefinitionType valuesField;

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

		public int DefaultValue
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

		[XmlIgnore]
		public bool DefaultValueSpecified
		{
			get
			{
				return defaultValueFieldSpecified;
			}
			set
			{
				defaultValueFieldSpecified = value;
			}
		}

		public EnumerationValuesDefinitionType Values
		{
			get
			{
				return valuesField;
			}
			set
			{
				valuesField = value;
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
