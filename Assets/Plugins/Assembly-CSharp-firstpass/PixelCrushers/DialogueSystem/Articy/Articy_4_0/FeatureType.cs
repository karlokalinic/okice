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
	public class FeatureType
	{
		private PropertiesType[] propertiesField;

		private string nameField;

		private string idRefField;

		[XmlElement("Properties")]
		public PropertiesType[] Properties
		{
			get
			{
				return propertiesField;
			}
			set
			{
				propertiesField = value;
			}
		}

		[XmlAttribute]
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

		[XmlAttribute]
		public string IdRef
		{
			get
			{
				return idRefField;
			}
			set
			{
				idRefField = value;
			}
		}
	}
}
