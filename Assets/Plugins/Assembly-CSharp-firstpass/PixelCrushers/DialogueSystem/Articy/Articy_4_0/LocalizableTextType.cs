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
	public class LocalizableTextType
	{
		private LocalizedStringType[] localizedStringField;

		private int countField;

		private bool hasMarkupField;

		[XmlElement("LocalizedString")]
		public LocalizedStringType[] LocalizedString
		{
			get
			{
				return localizedStringField;
			}
			set
			{
				localizedStringField = value;
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
		public bool HasMarkup
		{
			get
			{
				return hasMarkupField;
			}
			set
			{
				hasMarkupField = value;
			}
		}

		public LocalizableTextType()
		{
			hasMarkupField = false;
		}
	}
}
