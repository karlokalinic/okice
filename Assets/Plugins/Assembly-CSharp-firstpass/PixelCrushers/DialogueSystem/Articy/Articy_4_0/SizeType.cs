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
	public class SizeType
	{
		private float widthField;

		private float heightField;

		[XmlAttribute]
		public float Width
		{
			get
			{
				return widthField;
			}
			set
			{
				widthField = value;
			}
		}

		[XmlAttribute]
		public float Height
		{
			get
			{
				return heightField;
			}
			set
			{
				heightField = value;
			}
		}
	}
}
