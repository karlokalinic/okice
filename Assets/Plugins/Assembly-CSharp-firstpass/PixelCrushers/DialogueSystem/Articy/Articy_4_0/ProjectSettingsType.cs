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
	public class ProjectSettingsType
	{
		private FlowSettingsType flowSettingsField;

		private LocationSettingsType locationSettingsField;

		private ExternalApplicationsType externalApplicationsField;

		private LanguageSetupType languageSetupField;

		private string idField;

		public FlowSettingsType FlowSettings
		{
			get
			{
				return flowSettingsField;
			}
			set
			{
				flowSettingsField = value;
			}
		}

		public LocationSettingsType LocationSettings
		{
			get
			{
				return locationSettingsField;
			}
			set
			{
				locationSettingsField = value;
			}
		}

		public ExternalApplicationsType ExternalApplications
		{
			get
			{
				return externalApplicationsField;
			}
			set
			{
				externalApplicationsField = value;
			}
		}

		public LanguageSetupType LanguageSetup
		{
			get
			{
				return languageSetupField;
			}
			set
			{
				languageSetupField = value;
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
	}
}
