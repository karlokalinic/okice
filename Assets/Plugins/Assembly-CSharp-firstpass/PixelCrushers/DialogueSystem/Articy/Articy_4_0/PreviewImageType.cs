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
	public class PreviewImageType
	{
		private RectangleType viewBoxField;

		private string idRefField;

		private ViewBoxModeType modeField;

		public RectangleType ViewBox
		{
			get
			{
				return viewBoxField;
			}
			set
			{
				viewBoxField = value;
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

		[XmlAttribute]
		public ViewBoxModeType Mode
		{
			get
			{
				return modeField;
			}
			set
			{
				modeField = value;
			}
		}
	}
}
