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
	public class CircleType
	{
		private float centerXField;

		private float centerYField;

		private float radiusField;

		[XmlAttribute]
		public float CenterX
		{
			get
			{
				return centerXField;
			}
			set
			{
				centerXField = value;
			}
		}

		[XmlAttribute]
		public float CenterY
		{
			get
			{
				return centerYField;
			}
			set
			{
				centerYField = value;
			}
		}

		[XmlAttribute]
		public float Radius
		{
			get
			{
				return radiusField;
			}
			set
			{
				radiusField = value;
			}
		}
	}
}
