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
	public class PointType
	{
		private float xField;

		private float yField;

		[XmlAttribute]
		public float X
		{
			get
			{
				return xField;
			}
			set
			{
				xField = value;
			}
		}

		[XmlAttribute]
		public float Y
		{
			get
			{
				return yField;
			}
			set
			{
				yField = value;
			}
		}
	}
}
