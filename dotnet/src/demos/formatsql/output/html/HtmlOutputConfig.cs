using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.formatsql.output.html
{
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.pp.output;
    using System.Drawing;
    using GFmtOpt = gudusoft.gsqlparser.pp.para.GFmtOpt;


    public class HtmlOutputConfig : OutputConfig
	{

		private string globalFontName = "Courier New";
		private float globalFontSize = 10;

		private IDictionary<HighlightingElement, HighlightingElementRender> highlightingElementMap = new Dictionary<HighlightingElement, HighlightingElementRender>();
		private HtmlRenderUtil render;
		private bool isInit = false;

		private void init()
		{
			isInit = true;

            FontFamily globalFontFamily = new FontFamily(globalFontName);
            Font plainFont = new Font(globalFontFamily, globalFontSize, FontStyle.Regular);
			Font boldFont = new Font(globalFontFamily, globalFontSize, FontStyle.Bold);
			Font italicFont = new Font(globalFontFamily, globalFontSize, FontStyle.Italic);

			Color color_800000 = Color.FromArgb(0x80, 0, 0);
			Color color_0000FF = Color.FromArgb(0, 0, 0xFF);
			Color color_000000 = Color.FromArgb(0, 0, 0);
			Color color_C0C0C0 = Color.FromArgb(0xC0, 0xC0, 0xC0);
			Color color_FF0080 = Color.FromArgb(0xFF, 0, 0x80);
			Color color_16711935 = Color.FromArgb(16711935);
			Color color_16711808 = Color.FromArgb(16711808);
			Color color_FF0000 = Color.FromArgb(0xFF, 0, 0);
			Color color_008000 = Color.FromArgb(0, 0x80, 0);

			addHighlightingElementRender(HighlightingElement.sfkSpan, new HtmlHighlightingElementRender(HighlightingElement.sfkSpan, Color.White, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkDefault, new HtmlHighlightingElementRender(HighlightingElement.sfkDefault, Color.Black, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkIdentifer, new HtmlHighlightingElementRender(HighlightingElement.sfkIdentifer, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkStandardkeyword, new HtmlHighlightingElementRender(HighlightingElement.sfkStandardkeyword, color_0000FF, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkNumber, new HtmlHighlightingElementRender(HighlightingElement.sfkNumber, color_000000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkDelimitedIdentifier, new HtmlHighlightingElementRender(HighlightingElement.sfkDelimitedIdentifier, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkSymbol, new HtmlHighlightingElementRender(HighlightingElement.sfkSymbol, color_C0C0C0, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkFunction, new HtmlHighlightingElementRender(HighlightingElement.sfkFunction, color_FF0080, boldFont), false);
			addHighlightingElementRender(HighlightingElement.sfkBuiltInFunction, new HtmlHighlightingElementRender(HighlightingElement.sfkBuiltInFunction, color_16711935, italicFont), false);
			addHighlightingElementRender(HighlightingElement.sfkDatatype, new HtmlHighlightingElementRender(HighlightingElement.sfkDatatype, Color.Black, italicFont), false);
			addHighlightingElementRender(HighlightingElement.sfkParameter, new HtmlHighlightingElementRender(HighlightingElement.sfkParameter, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkbindvar, new HtmlHighlightingElementRender(HighlightingElement.sfkbindvar, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkVendordbkeyword, new HtmlHighlightingElementRender(HighlightingElement.sfkVendordbkeyword, color_0000FF, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkSQString, new HtmlHighlightingElementRender(HighlightingElement.sfkSQString, color_FF0000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkDQString, new HtmlHighlightingElementRender(HighlightingElement.sfkDQString, color_FF0000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkComment_dh, new HtmlHighlightingElementRender(HighlightingElement.sfkComment_dh, color_008000, italicFont), false);
			addHighlightingElementRender(HighlightingElement.sfkComment_ss, new HtmlHighlightingElementRender(HighlightingElement.sfkComment_ss, color_008000, italicFont), false);
			addHighlightingElementRender(HighlightingElement.sfkComment_sign, new HtmlHighlightingElementRender(HighlightingElement.sfkComment_sign, color_008000, italicFont), false);
			addHighlightingElementRender(HighlightingElement.sfkMssqlsystemvar, new HtmlHighlightingElementRender(HighlightingElement.sfkMssqlsystemvar, color_16711935, italicFont), false);
			addHighlightingElementRender(HighlightingElement.sfksqlvar, new HtmlHighlightingElementRender(HighlightingElement.sfksqlvar, color_16711808, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkMssqlst1, new HtmlHighlightingElementRender(HighlightingElement.sfkMssqlst1, color_16711808, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkMssqlst2, new HtmlHighlightingElementRender(HighlightingElement.sfkMssqlst2, color_16711808, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkMssqlst3, new HtmlHighlightingElementRender(HighlightingElement.sfkMssqlst3, color_16711808, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkOracleplsqlkeyword, new HtmlHighlightingElementRender(HighlightingElement.sfkOracleplsqlkeyword, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkOraclepackage, new HtmlHighlightingElementRender(HighlightingElement.sfkOraclepackage, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkOraclecommand, new HtmlHighlightingElementRender(HighlightingElement.sfkOraclecommand, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkOracleplsqlmethod, new HtmlHighlightingElementRender(HighlightingElement.sfkOracleplsqlmethod, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkOraclerem, new HtmlHighlightingElementRender(HighlightingElement.sfkOraclerem, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkOraclesqlplus, new HtmlHighlightingElementRender(HighlightingElement.sfkOraclesqlplus, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkSybasesystemobj, new HtmlHighlightingElementRender(HighlightingElement.sfkSybasesystemobj, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkSybasest, new HtmlHighlightingElementRender(HighlightingElement.sfkSybasest, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkSybaseglobalvar, new HtmlHighlightingElementRender(HighlightingElement.sfkSybaseglobalvar, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkopenbracket, new HtmlHighlightingElementRender(HighlightingElement.sfkopenbracket, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkclosebracket, new HtmlHighlightingElementRender(HighlightingElement.sfkclosebracket, color_800000, plainFont), false);
			addHighlightingElementRender(HighlightingElement.sfkUserCustomized, new HtmlHighlightingElementRender(HighlightingElement.sfkUserCustomized, color_800000, plainFont), false);
		}

		public HtmlOutputConfig(GFmtOpt option, EDbVendor dbVendor)
		{
			render = new HtmlRenderUtil(this, option, dbVendor);
		}

		public virtual void addHighlightingElementRender(HighlightingElement element, HighlightingElementRender setting)
		{
			addHighlightingElementRender(element, setting, true);
		}

		private void addHighlightingElementRender(HighlightingElement element, HighlightingElementRender setting, bool @override)
		{
			if (@override)
			{
				highlightingElementMap[element] = setting;
			}
			else if (!highlightingElementMap.ContainsKey(element))
			{
				highlightingElementMap[element] = setting;
			}
		}

		public virtual bool containsHighlightingElementRender(HighlightingElement element)
		{
			return highlightingElementMap.ContainsKey(element);
		}

		public virtual string GlobalFontName
		{
			get
			{
				return globalFontName;
			}
			set
			{
				this.globalFontName = value;
			}
		}

		public virtual float GlobalFontSize
		{
			get
			{
				return globalFontSize;
			}
			set
			{
				this.globalFontSize = value;
			}
		}

		public virtual HighlightingElementRender getHighlightingElementRender(HighlightingElement element)
		{
			return highlightingElementMap[element];
		}

		public virtual void removeHighlightingElementRender(HighlightingElement element)
		{
			highlightingElementMap.Remove(element);
		}



		public virtual string renderHighlightingElement(TSourceToken token)
		{
			if (!isInit)
			{
				init();
			}
			return render.renderToken(token);
		}
	}

}