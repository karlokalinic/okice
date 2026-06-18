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
	public class NumberPropertyDefinitionType
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

		private decimal defaultValueField;

		private bool defaultValueFieldSpecified;

		private decimal minValueField;

		private bool minValueFieldSpecified;

		private decimal maxValueField;

		private bool maxValueFieldSpecified;

		private int precisionField;

		private bool precisionFieldSpecified;

		private string unitField;

		private int displayThousandsSeparatorField;

		private bool displayThousandsSeparatorFieldSpecified;

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

		public decimal DefaultValue
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

		public decimal MinValue
		{
			get
			{
				return minValueField;
			}
			set
			{
				minValueField = value;
			}
		}

		[XmlIgnore]
		public bool MinValueSpecified
		{
			get
			{
				return minValueFieldSpecified;
			}
			set
			{
				minValueFieldSpecified = value;
			}
		}

		public decimal MaxValue
		{
			get
			{
				return maxValueField;
			}
			set
			{
				maxValueField = value;
			}
		}

		[XmlIgnore]
		public bool MaxValueSpecified
		{
			get
			{
				return maxValueFieldSpecified;
			}
			set
			{
				maxValueFieldSpecified = value;
			}
		}

		public int Precision
		{
			get
			{
				return precisionField;
			}
			set
			{
				precisionField = value;
			}
		}

		[XmlIgnore]
		public bool PrecisionSpecified
		{
			get
			{
				return precisionFieldSpecified;
			}
			set
			{
				precisionFieldSpecified = value;
			}
		}

		public string Unit
		{
			get
			{
				return unitField;
			}
			set
			{
				unitField = value;
			}
		}

		public int DisplayThousandsSeparator
		{
			get
			{
				return displayThousandsSeparatorField;
			}
			set
			{
				displayThousandsSeparatorField = value;
			}
		}

		[XmlIgnore]
		public bool DisplayThousandsSeparatorSpecified
		{
			get
			{
				return displayThousandsSeparatorFieldSpecified;
			}
			set
			{
				displayThousandsSeparatorFieldSpecified = value;
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
