using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage.util
{
    using System.IO;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;


    public class SQLUtil
    {
        public const string TABLE_CONSTANT = "CONSTANT";
        public static bool isEmpty(string value)
        {
            return string.ReferenceEquals(value, null) || value.Trim().Length == 0;
        }

        public static string getFileContent(FileInfo file)
        {
            Encoding charset = null;
            string sqlfilename = file.FullName;
            string fileContent = "";

            try
            {
                System.IO.FileStream fr = new System.IO.FileStream(sqlfilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] bom = new byte[4];
                fr.Read(bom, 0, bom.Length);
                if (((bom[0] == unchecked((byte)0xFF)) && (bom[1] == unchecked((byte)0xFE))) || ((bom[0] == unchecked((byte)0xFE)) && (bom[1] == unchecked((byte)0xFF))))
                {
                    charset = Encoding.Unicode;
                    if (((bom[2] == unchecked((byte)0xFF)) && (bom[3] == unchecked((byte)0xFE))) || ((bom[2] == unchecked((byte)0xFE)) && (bom[3] == unchecked((byte)0xFF))))
                    {
                        charset = Encoding.UTF32;
                    }
                }
                else if ((bom[0] == unchecked((byte)0xEF)) && (bom[1] == unchecked((byte)0xBB)) && (bom[2] == unchecked((byte)0xBF)))
                {
                    charset = Encoding.UTF8; // UTF-8,EF BB BF
                }
                fr.Close();
            }
            catch (FileNotFoundException)
            {
                // e.printStackTrace(); //To change body of catch statement use File
                // | Settings | File Templates.
            }
            catch (IOException)
            {
                // e.printStackTrace(); //To change body of catch statement use File
                // | Settings | File Templates.
            }

            try
            {
                System.IO.StreamReader br = null;
                if (charset != null)
                    br = new System.IO.StreamReader(new System.IO.FileStream(sqlfilename, System.IO.FileMode.Open, System.IO.FileAccess.Read), charset);
                else
                    br = new System.IO.StreamReader(new System.IO.FileStream(sqlfilename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                if (!string.ReferenceEquals(charset, null))
                {
                    br.Read();
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    string line = br.ReadLine();

                    while (!string.ReferenceEquals(line, null))
                    {
                        sb.Append(line);
                        sb.AppendLine();
                        line = br.ReadLine();
                    }
                    fileContent = sb.ToString();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
                finally
                {
                    try
                    {
                        br.Close();
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            return fileContent.Trim();
        }

        public static string getInputStreamContent(System.IO.Stream @is, bool close)
        {
            try
            {
                StreamReader sr = new StreamReader(@is, System.Text.Encoding.GetEncoding("utf-8"));
                string content = sr.ReadToEnd().ToString();
                sr.Close();
                return content;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            return null;
        }

        public static string getFileContent(string filePath)
        {
            if (string.ReferenceEquals(filePath, null))
            {
                return null;
            }
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists || file.Attributes == FileAttributes.Directory)
            {
                return null;
            }
            return getFileContent(file);
        }

        public static IList<string> parseNames(string nameString)
        {
            String name = nameString.Trim();
            List<String> names = new List<String>();
            string[] splits = nameString.ToUpper().Split(new char[] { '.' });
            if ((name.StartsWith("\"") && name.EndsWith("\""))
                    || (name.StartsWith("[") && name.EndsWith("]")))
            {
                for (int i = 0; i < splits.Length; i++)
                {
                    string split = splits[i].Trim();
                    if (split.StartsWith("[", StringComparison.Ordinal) && !split.EndsWith("]", StringComparison.Ordinal))
                    {
                        StringBuilder buffer = new StringBuilder();
                        buffer.Append(splits[i]);
                        while (!(split = splits[++i].Trim()).EndsWith("]", StringComparison.Ordinal))
                        {
                            buffer.Append(".");
                            buffer.Append(splits[i]);
                        }

                        buffer.Append(".");
                        buffer.Append(splits[i]);

                        names.Add(buffer.ToString());
                        continue;
                    }
                    if (split.StartsWith("\"", StringComparison.Ordinal) && !split.EndsWith("\"", StringComparison.Ordinal))
                    {
                        StringBuilder buffer = new StringBuilder();
                        buffer.Append(splits[i]);
                        while (!(split = splits[++i].Trim()).EndsWith("\"", StringComparison.Ordinal))
                        {
                            buffer.Append(".");
                            buffer.Append(splits[i]);
                        }

                        buffer.Append(".");
                        buffer.Append(splits[i]);

                        names.Add(buffer.ToString());
                        continue;
                    }
                    names.Add(splits[i]);
                }
            }
            else
            {
                names.AddRange(splits);
            }
            return names;
        }
        public static string trimObjectName(string @string)
        {
            if (string.ReferenceEquals(@string, null))
            {
                return @string;
            }

            if (@string.IndexOf('.') != -1
                && @string.Length < 128)
            {
                IList<string> splits = parseNames(@string);
                StringBuilder buffer = new StringBuilder();
                for (int i = 0; i < splits.Count; i++)
                {
                    buffer.Append(splits[i]);
                    if (i < splits.Count - 1)
                    {
                        buffer.Append(".");
                    }
                }
                @string = buffer.ToString();
            }
            else
            {
                if (@string.StartsWith("\"", StringComparison.Ordinal) && @string.EndsWith("\"", StringComparison.Ordinal))
                {
                    return @string.Substring(1, (@string.Length - 1) - 1);
                }

                if (@string.StartsWith("[", StringComparison.Ordinal) && @string.EndsWith("]", StringComparison.Ordinal))
                {
                    return @string.Substring(1, (@string.Length - 1) - 1);
                }
            }
            return @string;
        }


        private static int virtualTableIndex = -1;
        private static IDictionary<string, string> virtualTableNames = new Dictionary<string, string>();

        public static string generateVirtualTableName(TCustomSqlStatement stmt)
        {
            lock (typeof(SQLUtil))
            {
                if (virtualTableNames.ContainsKey(stmt.ToString()))
                {
                    return virtualTableNames[stmt.ToString()];
                }
                else
                {
                    string tableName = null;
                    virtualTableIndex++;
                    if (virtualTableIndex == 0)
                    {
                        tableName = "RESULT SET COLUMNS";
                    }
                    else
                    {
                        tableName = "RESULT SET COLUMNS " + virtualTableIndex;
                    }
                    virtualTableNames[stmt.ToString()] = tableName;
                    return tableName;
                }
            }
        }

        public static void resetVirtualTableNames()
        {
            lock (typeof(SQLUtil))
            {
                virtualTableIndex = -1;
                virtualTableNames.Clear();
            }
        }
    }

}