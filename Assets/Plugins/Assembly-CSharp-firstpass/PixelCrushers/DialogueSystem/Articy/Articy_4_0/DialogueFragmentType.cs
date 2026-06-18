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
	public class DialogueFragmentType
	{
		private string displayNameField;

		private LocalizableTextType textField;

		private string colorField;

		private string technicalNameField;

		private string externalIdField;

		private string shortIdField;

		private string urlField;

		private FeaturesType featuresField;

		private PinsType pinsField;

		private PointType positionField;

		private SizeType sizeField;

		private float zIndexField;

		private ReferenceType speakerField;

		private LocalizableTextType stageDirectionsField;

		private LocalizableTextType menuTextField;

		private string idField;

		private string objectTemplateReferenceField;

		private string objectTemplateReferenceNameField;

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

		public LocalizableTextType Text
		{
			get
			{
				return textField;
			}
			set
			{
				textField = value;
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

		public string ExternalId
		{
			get
			{
				return externalIdField;
			}
			set
			{
				externalIdField = value;
			}
		}

		public string ShortId
		{
			get
			{
				return shortIdField;
			}
			set
			{
				shortIdField = value;
			}
		}

		public string Url
		{
			get
			{
				return urlField;
			}
			set
			{
				urlField = value;
			}
		}

		public FeaturesType Features
		{
			get
			{
				return featuresField;
			}
			set
			{
				featuresField = value;
			}
		}

		public PinsType Pins
		{
			get
			{
				return pinsField;
			}
			set
			{
				pinsField = value;
			}
		}

		public PointType Position
		{
			get
			{
				return positionField;
			}
			set
			{
				positionField = value;
			}
		}

		public SizeType Size
		{
			get
			{
				return sizeField;
			}
			set
			{
				sizeField = value;
			}
		}

		public float ZIndex
		{
			get
			{
				return zIndexField;
			}
			set
			{
				zIndexField = value;
			}
		}

		public ReferenceType Speaker
		{
			get
			{
				return speakerField;
			}
			set
			{
				speakerField = value;
			}
		}

		public LocalizableTextType StageDirections
		{
			get
			{
				return stageDirectionsField;
			}
			set
			{
				stageDirectionsField = value;
			}
		}

		public LocalizableTextType MenuText
		{
			get
			{
				return menuTextField;
			}
			set
			{
				menuTextField = value;
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
		public string ObjectTemplateReference
		{
			get
			{
				return objectTemplateReferenceField;
			}
			set
			{
				objectTemplateReferenceField = value;
			}
		}

		[XmlAttribute]
		public string ObjectTemplateReferenceName
		{
			get
			{
				return objectTemplateReferenceNameField;
			}
			set
			{
				objectTemplateReferenceNameField = value;
			}
		}
	}
}
