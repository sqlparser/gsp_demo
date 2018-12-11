namespace gudusoft.gsqlparser.demos.formatsql.output.html
{

    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using GFmtOpt = gudusoft.gsqlparser.pp.para.GFmtOpt;
    using TCompactMode = gudusoft.gsqlparser.pp.para.styleenums.TCompactMode;
    using System.Collections.Generic;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.pp.output;

    public class HtmlRenderUtil
	{

		private const string RETURN_CODE = "\r\n";

		private HtmlOutputConfig config;
		private GFmtOpt option;
		private EDbVendor dbVendor;

		public HtmlRenderUtil(HtmlOutputConfig config, GFmtOpt option, EDbVendor dbVendor)
		{
			this.config = config;
			this.option = option;
			this.dbVendor = dbVendor;
		}

		public virtual string renderToken(TSourceToken token)
		{
			bool isLineBreak = false;
			string tokenString = token.astext;
			ETokenType type = token.tokentype;

			tokenString = tokenString.Replace(" ", "&nbsp;");
			tokenString = tokenString.Replace("\t", option.tabHtmlString);

			if (ETokenType.ttwhitespace.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSpan).render(tokenString);
			}
			else if (ETokenType.ttreturn.Equals(type))
			{
				if (tokenString.Contains(RETURN_CODE))
				{
					tokenString = tokenString.Replace(RETURN_CODE, RETURN_CODE + "<br>");
				}
				else
				{
					tokenString = tokenString.Replace("\n", RETURN_CODE + "<br>");
				}
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSpan).render(tokenString);
				isLineBreak = true;
			}
			else if (ETokenType.ttsimplecomment.Equals(type))
			{
				tokenString = tokenString.Replace("--", "&#45;&#45;");
				if (TCompactMode.Cpmugly.Equals(option.compactMode))
				{
					tokenString = ("/* " + tokenString + "*/");
				}
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkComment_dh).render(tokenString);
			}
			else if (ETokenType.ttbracketedcomment.Equals(type))
			{
				if (tokenString.Contains(RETURN_CODE))
				{
					tokenString = tokenString.Replace(RETURN_CODE, RETURN_CODE + "<br>");
				}
				else
				{
					tokenString = tokenString.Replace("\n", RETURN_CODE + "<br>");
				}
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkComment_dh).render(tokenString);
			}
			else if (ETokenType.ttidentifier.Equals(type) || ETokenType.ttdqstring.Equals(type) || ETokenType.ttdbstring.Equals(type) || ETokenType.ttbrstring.Equals(type))
			{
				if (ETokenType.ttdqstring.Equals(type) || ETokenType.ttdbstring.Equals(type))
				{
					tokenString = tokenString.Replace("\"", "&quot;");
				}

				int dbObjType = (int)token.DbObjType;
				switch (dbObjType)
				{
					case TObjectName.ttobjFunctionName :
						if (isVendorBuiltInFunction(tokenString, dbVendor))
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkBuiltInFunction).render(tokenString);
						}
						else
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkFunction).render(tokenString);
						}
						break;
					case TObjectName.ttobjDatatype :
						tokenString = config.getHighlightingElementRender(HighlightingElement.sfkDatatype).render(tokenString);
						break;
					default :
						if (EDbVendor.dbvmssql.Equals(dbVendor) && isMSSQLSystemVar(tokenString))
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkMssqlsystemvar).render(tokenString);
						}
						else
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkIdentifer).render(tokenString);
						}
						break;
				}
			}
			else if (ETokenType.ttnumber.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkNumber).render(tokenString);
			}
			else if (ETokenType.ttsqstring.Equals(type))
			{
				tokenString = tokenString.Replace("&nbsp;", " ");
				tokenString = tokenString.Replace("&", "&#38;");
				tokenString = tokenString.Replace(" ", "&nbsp;");
				tokenString = tokenString.Replace("\"", "&quot;");
				tokenString = tokenString.Replace("<", "&#60;");
				if (tokenString.Contains(RETURN_CODE))
				{
					tokenString = tokenString.Replace(RETURN_CODE, RETURN_CODE + "<br>");
				}
				else
				{
					tokenString = tokenString.Replace("\n", RETURN_CODE + "<br>");
				}
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSQString).render(tokenString);
			}
			else if (ETokenType.ttkeyword.Equals(type))
			{
				int dbObjType = (int)token.DbObjType;
				switch (dbObjType)
				{
					case TObjectName.ttobjFunctionName :
						if (isVendorBuiltInFunction(tokenString, dbVendor))
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkBuiltInFunction).render(tokenString);
						}
						else
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkFunction).render(tokenString);
						}
						break;
					case TObjectName.ttobjDatatype :
						tokenString = config.getHighlightingElementRender(HighlightingElement.sfkDatatype).render(tokenString);
						break;
					default :
						if (EDbVendor.dbvmssql.Equals(dbVendor) && isMSSQLSystemVar(tokenString))
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkMssqlsystemvar).render(tokenString);
						}
						else
						{
							tokenString = config.getHighlightingElementRender(HighlightingElement.sfkStandardkeyword).render(tokenString);
						}
						break;
				}
			}
			else if (ETokenType.ttsqlvar.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfksqlvar).render(tokenString);
			}
			else if (ETokenType.ttbindvar.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkbindvar).render(tokenString);
			}
			else if (ETokenType.ttmulticharoperator.Equals(type))
			{
				tokenString = tokenString.Replace("<", "&lt;");
				tokenString = tokenString.Replace(">", "&gt;");
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSymbol).render(tokenString);
			}
			else if (ETokenType.ttsinglecharoperator.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSymbol).render(tokenString);
			}
			else if (ETokenType.ttcomma.Equals(type) || ETokenType.ttperiod.Equals(type) || ETokenType.ttsemicolon.Equals(type) || ETokenType.ttdolorsign.Equals(type) || ETokenType.ttcolon.Equals(type) || ETokenType.ttplussign.Equals(type) || ETokenType.ttminussign.Equals(type) || ETokenType.ttasterisk.Equals(type) || ETokenType.ttslash.Equals(type) || ETokenType.ttstmt_delimiter.Equals(type) || ETokenType.ttequals.Equals(type) || ETokenType.ttatsign.Equals(type) || ETokenType.ttsemicolon2.Equals(type) || ETokenType.ttsemicolon3.Equals(type) || ETokenType.ttquestionmark.Equals(type))
			// || ETokenType.ttOpenSquareBracket.equals( type )
			// || ETokenType.ttCloseSquareBracket.equals( type )
			// || ETokenType.ttdot.equals( type )
					// || ETokenType.ttmulti.equals( type )
					// || ETokenType.ttHat.equals( type )
					// || ETokenType.ttDiv.equals( type )
					// || ETokenType.ttBitWise.equals( type )
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSymbol).render(tokenString);
			}
			else if (ETokenType.ttlessthan.Equals(type))
			{
				tokenString = "&lt;";
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSymbol).render(tokenString);
			}
			else if (ETokenType.ttgreaterthan.Equals(type))
			{
				tokenString = "&gt;";
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkSymbol).render(tokenString);
			}
			else if (ETokenType.ttleftparenthesis.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkopenbracket).render(tokenString);
			}
			else if (ETokenType.ttrightparenthesis.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkclosebracket).render(tokenString);
			}
			else if (ETokenType.ttsqlpluscmd.Equals(type))
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkOraclesqlplus).render(tokenString);
			}
			// else if ( ETokenType.ttUserCustomized.equals( type )){
			// tokenString = config.getHighlightingElementRender(
			// HighlightingElement.sfkUserCustomized )
			// .render( tokenString );
			// }
			else
			{
				tokenString = config.getHighlightingElementRender(HighlightingElement.sfkDefault).render(tokenString);
			}

			if (TCompactMode.Cpmugly.Equals(option.compactMode))
			{
				if (isLineBreak)
				{
					tokenString += "\r\n";
				}
			}
			return tokenString;
		}

		private bool isMSSQLSystemVar(string tokenString)
		{
			return false;
		}

		private bool isVendorBuiltInFunction(string funcName, EDbVendor dbVendor)
		{
			if (EDbVendor.dbvoracle.Equals(dbVendor))
			{
				return isOracleBuiltInFunction(funcName);
			}
			else if (EDbVendor.dbvmssql.Equals(dbVendor))
			{
				return isMssqlBuiltInFunction(funcName);
			}
			else if (EDbVendor.dbvmysql.Equals(dbVendor))
			{
				return isMysqlBuiltInFunction(funcName);
			}
			return false;
		}

		private bool isMysqlBuiltInFunction(string funcName)
		{
			string[] sysFuncNames = new string[]{"@@CONNECTIONS", "@@CPU_BUSY", "@@CURSOR_ROWS", "@@DATEFIRST", "@@DBTS", "@@ERROR", "@@FETCH_STATUS", "@@IDENTITY", "@@IDLE", "@@IO_BUSY", "@@LANGID", "@@LANGUAGE", "@@LOCK_TIMEOUT", "@@MAX_CONNECTIONS", "@@MAX_PRECISION", "@@NESTLEVEL", "@@OPTIONS", "@@PACKET_ERRORS", "@@PACK_RECEIVED", "@@PACK_SENT", "@@PROCID", "@@REMSERVER", "@@ROWCOUNT", "@@SERVERNAME", "@@SERVICENAME", "@@SPID", "@@TEXTSIZE", "@@TIMETICKS", "@@TOTAL_ERRORS", "@@TOTAL_READ", "@@TOTAL_WRITE", "@@TRANCOUNT", "@@VERSION", "ABS", "ACOS", "APP_NAME", "ASCII", "ASIN", "ATAN", "ATN2", "AVG", "BINARY_CHECKSUM", "CASE", "CAST", "CEILING", "CHAR", "CHARINDEX", "CHECKSUM", "CHECKSUM_AGG", "COALESCE", "COLLATIONPROPERTY", "COLUMNPROPERTY", "COL_LENGTH", "COL_NAME", "CONTAINSTABLE", "CONVERT", "COS", "COT", "COUNT", "COUNT_BIG", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR_STATUS", "DATABASEPROPERTY", "DATABASEPROPERTYEX", "DATALENGTH", "DATEADD", "DATEDIFF", "DATENAME", "DATEPART", "DAY", "DB_ID", "DB_NAME", "DEGREES", "DIFFERENCE", "EXP", "FILEGROUPPROPERTY", "FILEGROUP_ID", "FILEGROUP_NAME", "FILEPROPERTY", "FILE_ID", "FILE_NAME", "FLOOR", "FN_HELPCOLLATIONS", "FN_LISTEXTENDEDPROPERTY", "FN_SERVERSHAREDDRIVES", "FN_TRACE_GETEVENTINFO", "FN_TRACE_GETFILTERINFO", "FN_TRACE_GETINFO", "FN_TRACE_GETTABLE", "FN_VIRTUALFILESTATS", "FN_VIRTUALFILESTATS", "FORMATMESSAGE", "FREETEXTTABLE", "FULLTEXTCATALOGPROPERTY", "FULLTEXTSERVICEPROPERTY", "GETANSINULL", "GETDATE", "GETUTCDATE", "GROUPING", "HAS_DBACCESS", "HOST_ID", "HOST_NAME", "IDENTITY", "IDENT_CURRENT", "IDENT_INCR", "IDENT_SEED", "INDEXKEY_PROPERTY", "INDEXPROPERTY", "INDEX_COL", "ISDATE", "ISNULL", "ISNUMERIC", "IS_MEMBER", "IS_SRVROLEMEMBER", "LEFT", "LEN", "LOG", "LOG10", "LOWER", "LTRIM", "MAX", "MIN", "MONTH", "NCHAR", "NEWID", "NULLIF", "OBJECTPROPERTY", "OBJECT_ID", "OBJECT_NAME", "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML", "PARSENAME", "PATINDEX", "PATINDEX", "PERMISSIONS", "PI", "POWER", "QUOTENAME", "RADIANS", "RAND", "REPLACE", "REPLICATE", "REVERSE", "RIGHT", "ROUND", "ROWCOUNT_BIG", "RTRIM", "SCOPE_IDENTITY", "SERVERPROPERTY", "SESSIONPROPERTY", "SESSION_USER", "SIGN", "SIN", "SOUNDEX", "SPACE", "SQL_VARIANT_PROPERTY", "SQRT", "SQUARE", "STATS_DATE", "STDEV", "STDEVP", "STR", "STUFF", "SUBSTRING", "SUM", "SUSER_SID", "SUSER_SNAME", "SYSTEM_USER", "TAN", "TEXTPTR", "TEXTVALID", "TYPEPROPERTY", "UNICODE", "UPPER", "USER", "USER_ID", "USER_NAME", "VAR", "VARP", "YEAR"};
			return new List<string>(sysFuncNames).Contains(funcName.ToUpper());
		}

		private bool isMssqlBuiltInFunction(string funcName)
		{
			return false;
		}

		private bool isOracleBuiltInFunction(string funcName)
		{
			return false;
		}
	}

}