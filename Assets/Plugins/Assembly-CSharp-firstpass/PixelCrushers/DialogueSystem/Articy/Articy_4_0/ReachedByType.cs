using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_4_0
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[XmlType(Namespace = "http://www.articy.com/schemas/articydraft/4.0/XmlContentExport_FullProject.xsd")]
	public enum ReachedByType
	{
		Invalid = 0,
		JourneyStart = 1,
		Skip = 2,
		Next = 3,
		Submerge = 4,
		Emerge = 5,
		Branch = 6,
		EndPoint = 7
	}
}
