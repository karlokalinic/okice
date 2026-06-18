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
	public class PinType
	{
		private string expressionField;

		private string idField;

		private int indexField;

		private SemanticType semanticField;

		public string Expression
		{
			get
			{
				return expressionField;
			}
			set
			{
				expressionField = value;
			}
		}

		[XmlAttribute]
		public string Id
		{
			get
			{
				return idField;
			}
			set
			{
				idField = value;
			}
		}

		[XmlAttribute]
		public int Index
		{
			get
			{
				return indexField;
			}
			set
			{
				indexField = value;
			}
		}

		[XmlAttribute]
		public SemanticType Semantic
		{
			get
			{
				return semanticField;
			}
			set
			{
				semanticField = value;
			}
		}
	}
}
