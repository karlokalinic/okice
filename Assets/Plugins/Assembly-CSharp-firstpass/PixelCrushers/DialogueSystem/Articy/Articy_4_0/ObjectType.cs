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
	public class ObjectType
	{
		private object[] itemsField;

		private int countField;

		private string typeField;

		private int allowUnsetTemplateField;

		private int allowAllTemplatesField;

		[XmlElement("AllowedCategory", typeof(AllowedCategory))]
		[XmlElement("AllowedTemplate", typeof(AllowedTemplate))]
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

		[XmlAttribute]
		public string Type
		{
			get
			{
				return typeField;
			}
			set
			{
				typeField = value;
			}
		}

		[XmlAttribute]
		public int AllowUnsetTemplate
		{
			get
			{
				return allowUnsetTemplateField;
			}
			set
			{
				allowUnsetTemplateField = value;
			}
		}

		[XmlAttribute]
		public int AllowAllTemplates
		{
			get
			{
				return allowAllTemplatesField;
			}
			set
			{
				allowAllTemplatesField = value;
			}
		}
	}
}
