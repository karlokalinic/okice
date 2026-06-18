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
	public class SpotStyleType
	{
		private SpotStyleKindType kindField;

		private SizeNamesType sizeField;

		[XmlAttribute]
		public SpotStyleKindType Kind
		{
			get
			{
				return kindField;
			}
			set
			{
				kindField = value;
			}
		}

		[XmlAttribute]
		public SizeNamesType Size
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
	}
}
