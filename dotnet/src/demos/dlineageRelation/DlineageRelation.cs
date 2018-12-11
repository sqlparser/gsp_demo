using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage
{
    using Document = System.Xml.Linq.XDocument;
    using Element = System.Xml.Linq.XElement;

    using System.Xml.Linq;
    using System.IO;
    using gudusoft.gsqlparser.demos.util;

    using gudusoft.gsqlparser.demos.dlineage.model.xml;
    using gudusoft.gsqlparser.demos.dlineage.model.ddl.schema;
    using gudusoft.gsqlparser.demos.dlineage.model.metadata;

    public class DlineageRelation
    {
        public virtual string generateDlineageRelation(DlineageCommon dlineage, columnImpactResult impactResult)
        {
            if (dlineage == null || impactResult == null)
            {
                return null;
            }

            Document doc = null;
            Element dlineageRelation = null;

            doc = new Document();
            XDeclaration declaration = new XDeclaration("1.0", "utf-8", "no");
            doc.Declaration = declaration;
            dlineageRelation = new XElement("dlineageRelation");
            doc.Add(dlineageRelation);

            if (doc != null)
            {
                appendTables(dlineage, dlineageRelation);
                appendProcedures(dlineage, dlineageRelation);
                appendColumnRelations(impactResult, dlineageRelation);
                appendProcedureRelations(dlineage, dlineageRelation);

                try
                {
                    StringBuilder xmlBuffer = new StringBuilder();

                    using (StringWriter writer = new Utf8StringWriter(xmlBuffer))
                    {
                        doc.Save(writer, SaveOptions.None);
                    }
                    string result = xmlBuffer.ToString().Trim();
                    return result;
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

            }
            return null;
        }

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        private void appendProcedureRelations(DlineageCommon dlineage, Element dlineageRelation)
        {
            targetProcedure[] targetProcedures = dlineage.Procedures.Item1.targetProcedures;
            if (targetProcedures != null && targetProcedures.Length > 0)
            {
                for (int z = 0; z < targetProcedures.Length; z++)
                {
                    targetProcedure target = targetProcedures[z];
                    if (target.sourceProcedures != null)
                    {
                        for (int j = 0; j < target.sourceProcedures.Length; j++)
                        {
                            sourceProcedure source = target.sourceProcedures[j];

                            Element relationNode = new Element("relation");

                            Element sourceNode = new Element("source");

                            sourceNode.Add(new XAttribute("coordinate", source.coordinate));

                            if (!string.ReferenceEquals(source.owner, null))
                            {
                                sourceNode.Add(new XAttribute("owner", source.owner));
                            }

                            sourceNode.Add(new XAttribute("procedure", source.name));

                            Element targetNode = new Element("target");

                            targetNode.Add(new XAttribute("coordinate", target.coordinate));

                            if (!string.ReferenceEquals(target.owner, null))
                            {
                                targetNode.Add(new XAttribute("owner", target.owner));
                            }

                            targetNode.Add(new XAttribute("procedure", target.name));

                            relationNode.Add(sourceNode);
                            relationNode.Add(targetNode);

                            bool append = true;
                            IEnumerator<Element> iter = dlineageRelation.Elements().GetEnumerator();
                            while (iter.MoveNext())
                            {
                                if (iter.Current.Equals(relationNode))
                                {
                                    append = false;
                                    break;
                                }
                            }
                            if (append)
                            {
                                dlineageRelation.Add(relationNode);
                            }
                        }
                    }
                }
            }

        }

        private void appendColumnRelations(columnImpactResult impactResult, Element dlineageRelation)
        {
            targetColumn[] targetColumns = impactResult.columns;
            if (impactResult != null && targetColumns != null)
            {
                for (int z = 0; z < targetColumns.Length; z++)
                {
                    targetColumn target = targetColumns[z];
                    if (target.linkTables != null && target.columns != null)
                    {
                        linkTable[] links = target.linkTables;
                        for (int i = 0; i < links.Length; i++)
                        {
                            linkTable link = links[i];
                            for (int j = 0; j < target.columns.Length; j++)
                            {
                                sourceColumn source = target.columns[j];

                                if ("true".Equals(source.orphan))
                                {
                                    continue;
                                }

                                if (!string.ReferenceEquals(source.clause, null))
                                {
                                    if ("select".Equals(link.type, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (!"select".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            continue;
                                        }
                                    }
                                    if ("view".Equals(link.type, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (!"select".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            continue;
                                        }
                                    }
                                    if ("insert".Equals(link.type, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (!"select".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            continue;
                                        }
                                    }
                                    if ("update".Equals(link.type, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (!"assign".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase) && !"select".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            continue;
                                        }
                                    }
                                    if ("merge".Equals(link.type, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (!"assign".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase) && !"select".Equals(source.clause, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            continue;
                                        }
                                    }
                                }

                                Element relationNode = new Element("relation");

                                Element sourceNode = new Element("source");

                                sourceNode.Add(new XAttribute("column", source.name));
                                sourceNode.Add(new XAttribute("coordinate", source.coordinate));
                                if (!string.ReferenceEquals(source.tableOwner, null) && !"unknown".Equals(source.tableOwner, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    sourceNode.Add(new XAttribute("table", source.tableOwner + "." + source.tableName));
                                }
                                else
                                {
                                    sourceNode.Add(new XAttribute("table", source.tableName));
                                }

                                Element targetNode = new Element("target");

                                targetNode.Add(new XAttribute("column", link.name));

                                if (!string.ReferenceEquals(target.aliasCoordinate, null))
                                {
                                    targetNode.Add(new XAttribute("coordinate", target.aliasCoordinate));
                                }
                                else
                                {
                                    targetNode.Add(new XAttribute("coordinate", link.coordinate));
                                }
                                if (!string.ReferenceEquals(link.tableOwner, null) && !"unknown".Equals(link.tableOwner, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    targetNode.Add(new XAttribute("table", link.tableOwner + "." + link.tableName));
                                }
                                else
                                {
                                    targetNode.Add(new XAttribute("table", link.tableName));
                                }

                                relationNode.Add(sourceNode);
                                relationNode.Add(targetNode);

                                bool append = true;
                                IEnumerator<Element> iter = dlineageRelation.Elements().GetEnumerator();
                                while (iter.MoveNext())
                                {
                                    if (iter.Current.Equals(relationNode))
                                    {
                                        append = false;
                                        break;
                                    }
                                }
                                if (append)
                                {
                                    dlineageRelation.Add(relationNode);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void appendTables(DlineageCommon dlineage, Element dlineageRelation)
        {
            if (dlineage.DataMetaInfos == null)
            {
                return;
            }
            for (int i = 0; i < dlineage.DataMetaInfos.Length; i++)
            {
                database db = dlineage.DataMetaInfos[i];
                for (int j = 0; j < db.tables.Length; j++)
                {
                    model.ddl.schema.table currentTable = db.tables[j];
                    if (currentTable.columns == null || currentTable.columns.Length == 0)
                    {
                        continue;
                    }

                    Element tableNode = new Element("table");

                    if (!string.ReferenceEquals(currentTable.isView, null) && true == Convert.ToBoolean(currentTable.isView))
                    {
                        tableNode.Add(new XAttribute("isView", currentTable.isView));
                    }

                    if (!string.ReferenceEquals(db.name, null) && !"unknown".Equals(db.name, StringComparison.CurrentCultureIgnoreCase))
                    {

                        tableNode.Add(new XAttribute("name", db.name + "." + currentTable.name));
                    }
                    else
                    {
                        tableNode.Add(new XAttribute("name", currentTable.name));
                    }

                    for (int k = 0; k < currentTable.columns.Length; k++)
                    {
                        column column = currentTable.columns[k];
                        Element columnNode = new Element("column");


                        if (column.autoIncrement != null)
                        {
                            columnNode.Add(new XAttribute("autoIncrement", column.autoIncrement.ToLower()));
                        }


                        if (column.defaultValue != null)
                        {
                            columnNode.Add(new XAttribute("default", column.defaultValue));
                        }
                        if (column.description != null)
                        {
                            columnNode.Add(new XAttribute("description", column.description));
                        }

                        columnNode.Add(new XAttribute("name", column.name));

                        if (column.primaryKey != null)
                        {
                            columnNode.Add(new XAttribute("primaryKey", column.primaryKey.ToLower()));
                        }
                        if (column.required != null)
                        {
                            columnNode.Add(new XAttribute("required", column.required.ToLower()));
                        }

                        if (column.size != null)
                        {
                            columnNode.Add(new XAttribute("size", column.size));
                        }

                        if (column.type != null)
                        {
                            columnNode.Add(new XAttribute("type", column.type));
                        }

                        tableNode.Add(columnNode);
                    }
                    dlineageRelation.Add(tableNode);
                }
            }
        }

        private void appendProcedures(DlineageCommon dlineage, Element dlineageRelation)
        {
            if (dlineage.Procedures != null && dlineage.Procedures.Item2.Count > 0)
            {
                for (int i = 0; i < dlineage.Procedures.Item2.Count; i++)
                {
                    ProcedureMetaData procedure = dlineage.Procedures.Item2[i];

                    Element procedureNode = new Element("procedure");
                    procedureNode.Add(new XAttribute("name", procedure.DisplayFullName));

                    if (procedure.Function)
                    {
                        procedureNode.Add(new XAttribute("isFunction", procedure.DisplayFullName));
                    }

                    if (procedure.Trigger)
                    {
                        procedureNode.Add(new XAttribute("isTrigger", procedure.DisplayFullName));
                    }
                    dlineageRelation.Add(procedureNode);
                }
            }

        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: DlineageRelation [/f <path_to_sql_file>] [/d <path_to_directory_includes_sql_files>] [/t <database type>] [/o <output file path>]");
                Console.WriteLine("/f: Option, specify the sql file path to analyze dlineage relation.");
                Console.WriteLine("/d: Option, specify the sql directory path to analyze dlineage relation.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mysql, mssql, db2, netezza, teradata, informix, sybase, postgresql, hive, greenplum and redshift, the default type is oracle");
                Console.WriteLine("/o: Option, write the output stream to the specified file.");
                Console.WriteLine("/log: Option, generate a dlineage.log file to log information.");
                return;
            }

            FileInfo sqlFiles = null;

            IList<string> argList = new List<string>(args);
            if (argList.IndexOf("/f") != -1 && argList.Count > argList.IndexOf("/f") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/f") + 1]);
                if (!sqlFiles.Exists || sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(sqlFiles + " is not a valid file.");
                    return;
                }
            }
            else if (argList.IndexOf("/d") != -1 && argList.Count > argList.IndexOf("/d") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/d") + 1]);
                if (!sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(sqlFiles + " is not a valid directory.");
                    return;
                }
            }

            string outputFile = null;

            int index = argList.IndexOf("/o");
            if (index != -1 && args.Length > index + 1)
            {
                outputFile = args[index + 1];
            }

            System.IO.FileStream writer = null;
            StreamWriter sw = null;

            if (!string.ReferenceEquals(outputFile, null))
            {
                try
                {
                    writer = new System.IO.FileStream(outputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    sw = new StreamWriter(writer);
                    Console.SetOut(sw);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            DlineageCommon dlineage;
            if (sqlFiles != null)
            {
                dlineage = new DlineageCommon(sqlFiles, Common.GetEDbVendor(args), false, false);
            }
            else
            {
                string sqltext = @"SELECT e.last_name AS name,
                                e.commission_pct comm,
                                e.salary * 12 ""Annual Salary""
                                FROM scott.employees AS e
                                WHERE e.salary > 1000 or 1=1
                                ORDER BY
                                e.first_name,
                                e.last_name;";

                dlineage = new DlineageCommon(sqltext, Common.GetEDbVendor(args), false, false);
            }

            StringBuilder errorBuffer = new StringBuilder();
            columnImpactResult impactResult = dlineage.generateColumnImpact(errorBuffer);

            DlineageRelation relation = new DlineageRelation();
            string result = relation.generateDlineageRelation(dlineage, impactResult);

            bool log = argList.IndexOf("/log") != -1;

            TextWriter pw = null;
            StringBuilder errsw = null;
            TextWriter systemSteam = Console.Error;

            try
            {
                errsw = new StringBuilder();
                pw = new StringWriter(errsw);
                Console.SetError(pw);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            if (!string.ReferenceEquals(result, null))
            {
                Console.WriteLine(result);

                if (writer != null)
                {
                    Console.Error.WriteLine(result);
                }
            }

            if (errorBuffer.Length > 0)
            {
                Console.Error.WriteLine("Error log:\n" + errorBuffer.ToString());
            }

            try
            {
                if (sw != null && writer != null)
                {
                    sw.Close();
                    writer.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            if (pw != null)
            {
                pw.Close();
            }

            if (errsw != null)
            {
                string errorMessage = errsw.ToString().Trim();
                if (errorMessage.Length > 0)
                {
                    if (log)
                    {
                        try
                        {
                            pw = new StreamWriter(new FileInfo("./dlineageRelation.log").FullName);
                            pw.WriteLine(errorMessage);
                            pw.Close();

                        }
                        catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                    }

                    Console.SetError(systemSteam);
                    Console.Error.WriteLine(errorMessage);
                }
            }
        }
    }
}