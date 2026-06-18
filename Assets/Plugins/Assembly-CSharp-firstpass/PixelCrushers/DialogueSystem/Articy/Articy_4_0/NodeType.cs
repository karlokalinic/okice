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
	public class NodeType
	{
		private NodeType[] nodeField;

		private string[] textField;

		private string idRefField;

		private string typeField;

		[XmlElement("Node")]
		public NodeType[] Node
		{
			get
			{
				return nodeField;
			}
			set
			{
				nodeField = value;
			}
		}

		[XmlText]
		public string[] Text
		{
			get
			{
				return textField;
			}
			set
			{
				textField = value;
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
	}
}
