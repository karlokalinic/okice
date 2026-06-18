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
	public class ApplicationDefinitionType
	{
		private string nameField;

		private string commandField;

		private string workingDirectoryField;

		public string Name
		{
			get
			{
				return nameField;
			}
			set
			{
				nameField = value;
			}
		}

		public string Command
		{
			get
			{
				return commandField;
			}
			set
			{
				commandField = value;
			}
		}

		public string WorkingDirectory
		{
			get
			{
				return workingDirectoryField;
			}
			set
			{
				workingDirectoryField = value;
			}
		}
	}
}
