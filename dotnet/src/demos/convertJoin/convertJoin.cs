using System;
using System.Collections.Generic;
using gudusoft.gsqlparser.demos.util;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.joinConvert;
using gudusoft;
//using gspv2 = gudusoft.gsqlparserV2.TGSqlParser;
//using dbendorv2 = gudusoft.gsqlparserV2.TDbVendor;
//using unitsv2 = gudusoft.gsqlparserV2.Units;

namespace gudusoft.gsqlparser.demos.convertJoin
{
    


    using System.IO;

    public class convertJoin
	{

		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: convertJoin [/f <scriptfile>] [/t <database type>]");
                Console.WriteLine("/f: Option, specify the sql script file path.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mssql, the default type is oracle");
				return;
			}

			EDbVendor vendor = Common.GetEDbVendor(args);

			string vendorString = EDbVendor.dbvmssql == vendor ? "SQL Server" : "Oracle";
			Console.WriteLine("SQL with " + vendorString + " propriety joins");

            string sqltext;

            IList<string> argList = new List<string>(args);
            int index = argList.IndexOf("/f");
            if (index != -1 && args.Length > index + 1)
            {
                sqltext = getFileContent(args[index + 1]);
            }
            else
            {
                sqltext = "SELECT * \n" +
                     "FROM   summit.mstr m, \n" +
                     "       summit.alt_name altname, \n" +
                     "       smmtccon.ccn_user ccu \n" +
                     "       uhelp.deg_coll deg \n" +
                     "WHERE  m.id = ? \n" +
                     "       AND m.id = altname.id(+) \n" +
                     "       AND m.id = ccu.id(+) \n" +
                     "       AND 'N' = ccu.admin(+) \n" +
                     "       AND altname.grad_name_ind(+) = '*'";
            }

            //Console.WriteLine(pp(vendor, sqltext));
			joinConverter converter = new joinConverter(sqltext, vendor);
			if (converter.convert() != 0)
			{
				Console.WriteLine(converter.ErrorMessage);
			}
			else
			{
				Console.WriteLine("\nSQL in ANSI joins");
				Console.WriteLine(pp(vendor,converter.Query));
			}
		}



        public static string getFileContent(string file)
		{
            string lcsqltext = "";
            try
            {
                StreamReader re = File.OpenText(file);

                string input = null;
                while ((input = re.ReadLine()) != null)
                {
                    if (lcsqltext.Length > 0)
                    {
                        lcsqltext = lcsqltext + Environment.NewLine;
                    }
                    lcsqltext = lcsqltext + input;
                }
                return lcsqltext.Trim();
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("File could not be read:");
                Console.WriteLine(e.Message);
            }
            return lcsqltext;
		}

        static string pp(EDbVendor dbVendor, string inputsql)
        {
            TGSqlParser parser = new TGSqlParser(dbVendor);
            parser.sqltext = inputsql;
            int ret = parser.parse();
            if (ret == 0)
            {
                gsqlparser.pp.para.GFmtOpt option = gsqlparser.pp.para.GFmtOptFactory.newInstance();
                option.caseDatatype = gsqlparser.pp.para.styleenums.TCaseOption.CoNoChange;
                option.caseFuncname = gsqlparser.pp.para.styleenums.TCaseOption.CoNoChange;
                option.caseIdentifier = gsqlparser.pp.para.styleenums.TCaseOption.CoNoChange;
                
                string result = gsqlparser.pp.stmtformatter.FormatterFactory.pp(parser, option);
                return result;
            }
            else
            {
                return inputsql;
            }
        }

        //static string ppv2(EDbVendor dbVendor, string inputsql)
        //{

        //    gspv2 parser = new gspv2(dbendorv2.DbVMssql);
        //    parser.SqlText.Text = inputsql;
        //    unitsv2.lzbasetype.gFmtOpt.case_column_name = gsqlparserV2.TCaseOption.coNoChange;
        //    unitsv2.lzbasetype.gFmtOpt.case_funcname = gsqlparserV2.TCaseOption.coNoChange;
        //    unitsv2.lzbasetype.gFmtOpt.case_identifier = gsqlparserV2.TCaseOption.coNoChange;
        //    unitsv2.lzbasetype.gFmtOpt.case_keywords = gsqlparserV2.TCaseOption.coNoChange;
        //    unitsv2.lzbasetype.gFmtOpt.case_table_name = gsqlparserV2.TCaseOption.coNoChange;

        //    int ret = parser.PrettyPrint();
        //    if (ret == 0)
        //    {
        //        string result = parser.FormattedSqlText.Text;
        //        return result;
        //    }
        //    else
        //    {
        //        //Console.WriteLine(parser.ErrorMessages);
        //        return inputsql;
        //    }
        //}


    }
}