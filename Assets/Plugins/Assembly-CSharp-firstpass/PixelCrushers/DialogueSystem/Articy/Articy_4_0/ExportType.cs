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
	[XmlRoot("Export", Namespace = "http://www.articy.com/schemas/articydraft/4.0/XmlContentExport_FullProject.xsd", IsNullable = false)]
	public class ExportType
	{
		private SettingsType settingsField;

		private ContentType contentField;

		private HierarchyType hierarchyField;

		private ExportErrorsType exportErrorsField;

		private string versionField;

		private string creatorToolField;

		private string creatorVersionField;

		private DateTime createdOnField;

		public SettingsType Settings
		{
			get
			{
				return settingsField;
			}
			set
			{
				settingsField = value;
			}
		}

		public ContentType Content
		{
			get
			{
				return contentField;
			}
			set
			{
				contentField = value;
			}
		}

		public HierarchyType Hierarchy
		{
			get
			{
				return hierarchyField;
			}
			set
			{
				hierarchyField = value;
			}
		}

		public ExportErrorsType ExportErrors
		{
			get
			{
				return exportErrorsField;
			}
			set
			{
				exportErrorsField = value;
			}
		}

		[XmlAttribute]
		public string Version
		{
			get
			{
				return versionField;
			}
			set
			{
				versionField = value;
			}
		}

		[XmlAttribute]
		public string CreatorTool
		{
			get
			{
				return creatorToolField;
			}
			set
			{
				creatorToolField = value;
			}
		}

		[XmlAttribute]
		public string CreatorVersion
		{
			get
			{
				return creatorVersionField;
			}
			set
			{
				creatorVersionField = value;
			}
		}

		[XmlAttribute]
		public DateTime CreatedOn
		{
			get
			{
				return createdOnField;
			}
			set
			{
				createdOnField = value;
			}
		}

		public ExportType()
		{
			versionField = "3.0";
		}
	}
}
