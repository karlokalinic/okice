using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_4_0
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[XmlType(Namespace = "http://www.articy.com/schemas/articydraft/4.0/XmlContentExport_FullProject.xsd")]
	public enum BackgroundImagePositioningModeType
	{
		Fitting = 0,
		Filling = 1,
		Stretched = 2,
		Repeating = 3,
		Centered = 4
	}
}
