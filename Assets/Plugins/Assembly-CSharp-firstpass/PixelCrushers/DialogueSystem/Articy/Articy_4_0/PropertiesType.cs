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
	public class PropertiesType
	{
		private object[] itemsField;

		private int countField;

		[XmlElement("Boolean", typeof(BooleanPropertyType))]
		[XmlElement("Enum", typeof(EnumPropertyType))]
		[XmlElement("LocalizableText", typeof(LocalizableTextPropertyType))]
		[XmlElement("NamedReference", typeof(ReferenceSlotPropertyType))]
		[XmlElement("Number", typeof(NumberPropertyType))]
		[XmlElement("References", typeof(ReferenceStripPropertyType))]
		[XmlElement("String", typeof(StringPropertyType))]
		public object[] Items
		{
			get
			{
				return itemsField;
			}
			set
			{
				itemsField = value;
			}
		}

		[XmlAttribute]
		public int Count
		{
			get
			{
				return countField;
			}
			set
			{
				countField = value;
			}
		}
	}
}
