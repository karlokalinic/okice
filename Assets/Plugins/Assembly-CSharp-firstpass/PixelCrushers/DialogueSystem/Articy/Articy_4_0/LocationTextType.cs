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
	public class LocationTextType
	{
		private LocalizableTextType displayNameField;

		private LocalizableTextType textField;

		private string colorField;

		private string technicalNameField;

		private string externalIdField;

		private string shortIdField;

		private string urlField;

		private FeaturesType featuresField;

		private VisibilityType visibilityField;

		private SelectabilityType selectabilityField;

		private PreviewImageType previewImageField;

		private PointType positionField;

		private float zIndexField;

		private FillType fillField;

		private OutlineType outlineField;

		private bool dropShadowField;

		private PointType displayNamePositionField;

		private TransformationType transformationField;

		private VerticesType[] coordinatesField;

		private LocationAnchorsType anchorsField;

		private string idField;

		private string objectTemplateReferenceField;

		private string objectTemplateReferenceNameField;

		private CShapeType cShapeField;

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

		public VisibilityType Visibility
		{
			get
			{
				return visibilityField;
			}
			set
			{
				visibilityField = value;
			}
		}

		public SelectabilityType Selectability
		{
			get
			{
				return selectabilityField;
			}
			set
			{
				selectabilityField = value;
			}
		}

		public PreviewImageType PreviewImage
		{
			get
			{
				return previewImageField;
			}
			set
			{
				previewImageField = value;
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

		public FillType Fill
		{
			get
			{
				return fillField;
			}
			set
			{
				fillField = value;
			}
		}

		public OutlineType Outline
		{
			get
			{
				return outlineField;
			}
			set
			{
				outlineField = value;
			}
		}

		public bool DropShadow
		{
			get
			{
				return dropShadowField;
			}
			set
			{
				dropShadowField = value;
			}
		}

		public PointType DisplayNamePosition
		{
			get
			{
				return displayNamePositionField;
			}
			set
			{
				displayNamePositionField = value;
			}
		}

		public TransformationType Transformation
		{
			get
			{
				return transformationField;
			}
			set
			{
				transformationField = value;
			}
		}

		[XmlArrayItem("Vertices", IsNullable = false)]
		public VerticesType[] Coordinates
		{
			get
			{
				return coordinatesField;
			}
			set
			{
				coordinatesField = value;
			}
		}

		public LocationAnchorsType Anchors
		{
			get
			{
				return anchorsField;
			}
			set
			{
				anchorsField = value;
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

		[XmlAttribute]
		public CShapeType CShape
		{
			get
			{
				return cShapeField;
			}
			set
			{
				cShapeField = value;
			}
		}

		public LocationTextType()
		{
			cShapeField = CShapeType.Rectangle;
		}
	}
}
