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
	public class RectangleType
	{
		private float minXField;

		private float minYField;

		private float maxXField;

		private float maxYField;

		[XmlAttribute]
		public float MinX
		{
			get
			{
				return minXField;
			}
			set
			{
				minXField = value;
			}
		}

		[XmlAttribute]
		public float MinY
		{
			get
			{
				return minYField;
			}
			set
			{
				minYField = value;
			}
		}

		[XmlAttribute]
		public float MaxX
		{
			get
			{
				return maxXField;
			}
			set
			{
				maxXField = value;
			}
		}

		[XmlAttribute]
		public float MaxY
		{
			get
			{
				return maxYField;
			}
			set
			{
				maxYField = value;
			}
		}
	}
}
