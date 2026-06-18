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
	public class LocationAnchorType
	{
		private float xField;

		private bool xFieldSpecified;

		private float yField;

		private bool yFieldSpecified;

		private string colorField;

		private AnchorSizeNamesType sizeField;

		private bool sizeFieldSpecified;

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

		[XmlIgnore]
		public bool XSpecified
		{
			get
			{
				return xFieldSpecified;
			}
			set
			{
				xFieldSpecified = value;
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

		[XmlIgnore]
		public bool YSpecified
		{
			get
			{
				return yFieldSpecified;
			}
			set
			{
				yFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public string Color
		{
			get
			{
				return colorField;
			}
			set
			{
				colorField = value;
			}
		}

		[XmlAttribute]
		public AnchorSizeNamesType Size
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

		[XmlIgnore]
		public bool SizeSpecified
		{
			get
			{
				return sizeFieldSpecified;
			}
			set
			{
				sizeFieldSpecified = value;
			}
		}
	}
}
