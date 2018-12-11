using gudusoft.gsqlparser.pp.output;
using System.Drawing;
using System.Text;

namespace gudusoft.gsqlparser.demos.formatsql.output.html
{



	public class HtmlHighlightingElementRender : HighlightingElementRender
	{

		private Color backgroundColor;

		private HighlightingElement element;

		private string fontName;

		private float fontSize;

		private int fontStyle;

		private Color foregroundColor;

		private bool strikeOut;

		private bool underLine;

		public HtmlHighlightingElementRender(HighlightingElement element)
		{
			this.element = element;
		}

		public HtmlHighlightingElementRender(HighlightingElement element, Color foregroundColor, Color backgroundColor, Font font)
		{
			this.element = element;
			this.fontSize = font.Size;
			this.fontStyle = (int)font.Style;
			this.fontName = font.Name;
			this.underLine = font.Underline;
			this.strikeOut = font.Strikeout;
			this.foregroundColor = foregroundColor;
			this.backgroundColor = backgroundColor;
		}

        public HtmlHighlightingElementRender(HighlightingElement element, Color foregroundColor, Font font)
        {
            this.element = element;
            this.fontSize = font.Size;
            this.fontStyle = (int)font.Style;
            this.fontName = font.Name;
            this.underLine = font.Underline;
            this.strikeOut = font.Strikeout;
            this.foregroundColor = foregroundColor;
        }

        public virtual Color BackgroundColor
		{
			get
			{
				return backgroundColor;
			}
			set
			{
				this.backgroundColor = value;
			}
		}

		public virtual HighlightingElement Element
		{
			get
			{
				return element;
			}
		}

		public virtual string FontName
		{
			get
			{
				return fontName;
			}
			set
			{
				this.fontName = value;
			}
		}

		public virtual float FontSize
		{
			get
			{
				return fontSize;
			}
			set
			{
				this.fontSize = value;
			}
		}

		public virtual int FontStyle
		{
			get
			{
				return fontStyle;
			}
			set
			{
				this.fontStyle = value;
			}
		}

		public virtual Color ForegroundColor
		{
			get
			{
				return foregroundColor;
			}
			set
			{
				this.foregroundColor = value;
			}
		}

		public virtual bool StrikeOut
		{
			get
			{
				return strikeOut;
			}
			set
			{
				this.strikeOut = value;
			}
		}

		public virtual bool UnderLine
		{
			get
			{
				return underLine;
			}
			set
			{
				this.underLine = value;
			}
		}

		public virtual string render(string tokenText)
		{
			StringBuilder buffer = new StringBuilder();
			if (!string.ReferenceEquals(fontName, null))
			{
				buffer.Append("font-family:").Append(fontName).Append(";");
			}
			if (fontSize != 0)
			{
				buffer.Append("font-size:").Append(fontSize).Append("pt;");
			}
			if (fontStyle == (int)System.Drawing.FontStyle.Italic)
			{
				buffer.Append("font-sytle:italic;");
			}
			if (fontStyle == (int)System.Drawing.FontStyle.Bold)
			{
				buffer.Append("font-weight:bold;");
			}
			if (UnderLine)
			{
				buffer.Append("text-decoration: underline;");
			}
			if (StrikeOut)
			{
				buffer.Append("text-decoration: line-through;");
			}
			if (!foregroundColor.IsEmpty)
			{
				buffer.Append("color: " + color2String(foregroundColor) + ";");
			}
			if (!backgroundColor.IsEmpty)
			{
				buffer.Append("background-color: " + color2String(backgroundColor) + ";");
			}
			return "<span style=\"" + buffer.ToString() + "\">" + tokenText + "</span>";
		}

		public static string color2String(Color color)
		{
			string R = color.R.ToString("x");
			R = R.Length < 2 ? ('0' + R) : R;
			string B = color.B.ToString("x");
			B = B.Length < 2 ? ('0' + B) : B;
			string G = color.G.ToString("x");
			G = G.Length < 2 ? ('0' + G) : G;
			return '#' + R + G + B;
		}
	}

}