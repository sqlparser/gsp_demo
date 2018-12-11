using gudusoft.gsqlparser.demos.util;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.pp.output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using gudusoft.gsqlparser.demos.formatsql.util;
using gudusoft.gsqlparser.pp.para;
using gudusoft.gsqlparser.pp.stmtformatter;
using System.Threading;
using System.Drawing;
using gudusoft.gsqlparser.demos.formatsql.output.html;

namespace gudusoft.gsqlparser.demos.formatsql
{
    class formatsql
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: formatsql [/f <script file>] [/t <database type>] [/o <output file path>] [/p] [/h] [/r] [/c]");
                Console.WriteLine("/f: Option, specify the sql script file path.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mysql, mssql and db2, the default type is oracle.");
                Console.WriteLine("/o: Option, write the output stream to the specified file.");
                Console.WriteLine("/p: Option, format sql as plain style.");
                Console.WriteLine("/h: Option, format sql as HTML style.");
                Console.WriteLine("/r: Option, format sql as RTF style.");
                Console.WriteLine("/c: option, use the custom format color and font setting.");
                Console.ReadLine();
                return;
            }

            string sqltext = @"SELECT e.last_name AS name,
                                e.commission_pct comm,
                                e.salary * 12 ""Annual Salary""
                                FROM scott.employees AS e
                                WHERE e.salary > 1000 or 1=1
                                ORDER BY
                                e.first_name,
                                e.last_name;";
            EDbVendor vendor = Common.GetEDbVendor(args);

            List<string> argList = new List<string>(args);
            int index = argList.IndexOf("/f");

            FileInfo file = null;
            if (index != -1 && args.Length > index + 1)
            {
                file = new FileInfo(args[index + 1]);
            }

            string outputFile = null;

            index = argList.IndexOf("/o");

            if (index != -1 && args.Length > index + 1)
            {
                outputFile = args[index + 1];
            }

            System.IO.StreamWriter writer = null;
            if (!string.ReferenceEquals(outputFile, null))
            {
                try
                {
                    writer = new StreamWriter(outputFile);
                    Console.SetOut(writer);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            bool html = argList.IndexOf("/h") != -1;
            bool rtf = argList.IndexOf("/r") != -1;
            bool custom = argList.IndexOf("/c") != -1;

            if (html || rtf)
            {
                if (file != null)
                {
                    ppInHtml(vendor, file, rtf, custom);
                }
                else
                {
                    ppInHtml(vendor, sqltext, rtf, custom);
                }
            }
            else
            {
                if (file != null)
                {
                    pp(vendor, file);
                }
                else
                {
                    pp(vendor, sqltext);
                }
            }

            try
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

        static void pp(EDbVendor dbVendor, string inputsql)
        {
            TGSqlParser parser = new TGSqlParser(dbVendor);
            parser.sqltext = inputsql;
            outputPlainFormat(parser);
        }

        static void pp(EDbVendor dbVendor, FileInfo sqlfile)
        {
            TGSqlParser parser = new TGSqlParser(dbVendor);
            parser.sqlfilename = sqlfile.FullName;
            outputPlainFormat(parser);
        }

        static void outputPlainFormat(TGSqlParser parser)
        {
            int ret = parser.parse();
            if (ret == 0)
            {
                GFmtOpt option = GFmtOptFactory.newInstance();
                string result = FormatterFactory.pp(parser, option);
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine(parser.Errormessage);
            }
        }

        static void ppInHtml(EDbVendor dbVendor, string inputsql, bool rtf, bool custom)
        {
            TGSqlParser parser = new TGSqlParser(dbVendor);
            parser.sqltext = inputsql;
            outputHtmlFormat(parser, rtf, custom);
        }

        static void ppInHtml(EDbVendor dbVendor, FileInfo sqlfile, bool rtf, bool custom)
        {
            TGSqlParser parser = new TGSqlParser(dbVendor);
            parser.sqlfilename = sqlfile.FullName;
            outputHtmlFormat(parser, rtf, custom);
        }

        static void outputHtmlFormat(TGSqlParser parser, bool rtf, bool custom)
        {
            outputHtmlFormat(parser, null, rtf, custom);
        }

        static void outputHtmlFormat(TGSqlParser parser, GFmtOpt formatOption, bool rtf, bool custom)
        {
            int ret = parser.parse();
            if (ret == 0)
            {
                if (formatOption != null)
                {
                    formatOption.outputFmt = GOutputFmt.ofhtml;
                }
                else
                {
                    formatOption = GFmtOptFactory.newInstance();
                    formatOption.outputFmt = GOutputFmt.ofhtml;
                }

                if (custom)
                {
                    OutputConfig outputConfig = OutputConfigFactory.getOutputConfig(formatOption,
                            parser.DbVendor);
                    if (outputConfig is HtmlOutputConfig)
                    {
                        customOutputConfig((HtmlOutputConfig)outputConfig);
                    }
                    FormatterFactory.OutputConfig = outputConfig;
                }
                else {
                    FormatterFactory.OutputConfig = new HtmlOutputConfig(formatOption, parser.DbVendor);
                }

                string result = FormatterFactory.pp(parser, formatOption);
                if (rtf)
                {
                    StringBuilder buffer = new StringBuilder(result.Replace("</br>", "<br>"));
                    ParameterizedThreadStart ps = new ParameterizedThreadStart(convertHtmlToRtf);
                    Thread t = new Thread(ps);
                    t.IsBackground = true;
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start(buffer);
                    t.Join();
                    result = buffer.ToString();
                }
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine(parser.Errormessage);
            }
        }

        static void customOutputConfig(HtmlOutputConfig outputConfig)
        {
            outputConfig.GlobalFontSize = 12;
            outputConfig.GlobalFontName = "Courier New";
            outputConfig.addHighlightingElementRender(HighlightingElement.sfkStandardkeyword,
                    new HtmlHighlightingElementRender(HighlightingElement.sfkStandardkeyword,
                            Color.FromArgb(127, 0, 85),
                            new Font("Courier New", 12, FontStyle.Bold)));
            outputConfig.addHighlightingElementRender(HighlightingElement.sfkIdentifer,
                    new HtmlHighlightingElementRender(HighlightingElement.sfkIdentifer,
                            Color.Black,
                            new Font("Courier New", 12, FontStyle.Regular)));
            outputConfig.addHighlightingElementRender(HighlightingElement.sfkSQString,
                    new HtmlHighlightingElementRender(HighlightingElement.sfkSQString,
                            Color.Blue,
                            new Font("Courier New", 12, FontStyle.Regular)));
            outputConfig.addHighlightingElementRender(HighlightingElement.sfkSymbol,
                    new HtmlHighlightingElementRender(HighlightingElement.sfkSymbol,
                            Color.Red,
                            new Font("Courier New", 12, FontStyle.Regular)));
        }

        static void convertHtmlToRtf(object arg)
        {
            StringBuilder buffer = (StringBuilder)arg;
            string rtf = HtmlToRtfConverter.ConvertHtmlToRtf(buffer.ToString());
            buffer.Clear();
            buffer.Append(rtf);
        }
    }
}
