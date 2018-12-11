using System;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;

    using Document = System.Xml.Linq.XDocument;
    using Element = System.Xml.Linq.XElement;

    using gudusoft.gsqlparser.demos.dlineage.columnImpact;
    using gudusoft.gsqlparser.demos.dlineage.metadata;
    using gudusoft.gsqlparser.demos.dlineage.util;
    using gudusoft.gsqlparser.demos.dlineage.model.metadata;
    using gudusoft.gsqlparser.demos.dlineage.model.xml;
    using gudusoft.gsqlparser.demos.dlineage.model.ddl.schema;

    using System.Xml.Linq;
    using System.IO;
    using System.Text;

    public class DlineageCommon
	{

		private IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns = new Dictionary<TableMetaData, IList<ColumnMetaData>>();
		private Tuple<procedureImpactResult, IList<ProcedureMetaData>> procedures = new Tuple<procedureImpactResult, IList<ProcedureMetaData>>(new procedureImpactResult(), new List<ProcedureMetaData>());

		private bool strict = false;
		private bool showUIInfo = false;
		private FileInfo sqlDir;
		private FileInfo[] sqlFiles;
		private string sqlContent;
		private EDbVendor vendor;

		public DlineageCommon(string sqlContent, EDbVendor vendor, bool strict, bool showUIInfo)
		{
			this.strict = strict;
			this.showUIInfo = showUIInfo;
			this.vendor = vendor;
			this.sqlFiles = null;
			this.sqlContent = sqlContent;
			tableColumns.Clear();
			procedures = new Tuple<procedureImpactResult, IList<ProcedureMetaData>>(new procedureImpactResult(), new List<ProcedureMetaData>());

			string content = sqlContent;
			string database = null;

			database = (new DDLParser(tableColumns, procedures, vendor, content.ToUpper(), strict, database)).Database;

			database = (new ViewParser(tableColumns, vendor, content.ToUpper(), strict, database)).Database;

			database = (new ProcedureRelationScanner(procedures, vendor, content.ToUpper(), strict, database)).Database;
		}

		public DlineageCommon(string[] sqlContents, EDbVendor vendor, bool strict, bool showUIInfo)
		{
			this.strict = strict;
			this.showUIInfo = showUIInfo;
			this.vendor = vendor;
			this.sqlFiles = null;
			tableColumns.Clear();
			procedures = new Tuple<procedureImpactResult, IList<ProcedureMetaData>>(new procedureImpactResult(), new List<ProcedureMetaData>());

			for (int i = 0; i < sqlContents.Length; i++)
			{
				string content = sqlContents[i];
				string databaseTemp = null;
                databaseTemp = (new DDLParser(tableColumns, procedures, vendor, content.ToUpper(), strict, databaseTemp)).Database;
			}

			string database = null;
			for (int i = 0; i < sqlContents.Length; i++)
			{
				string content = sqlContents[i];
				database = (new ViewParser(tableColumns, vendor, content.ToUpper(), strict, database)).Database;
			}

			database = null;
			for (int i = 0; i < sqlContents.Length; i++)
			{
				string content = sqlContents[i];
				database = (new ProcedureRelationScanner(procedures, vendor, content.ToUpper(), strict, database)).Database;
			}
		}

		public DlineageCommon(FileInfo[] sqlFiles, EDbVendor vendor, bool strict, bool showUIInfo)
		{
			this.strict = strict;
			this.showUIInfo = showUIInfo;
			this.vendor = vendor;
			this.sqlFiles = sqlFiles;
			tableColumns.Clear();
			procedures = new Tuple<procedureImpactResult, IList<ProcedureMetaData>>(new procedureImpactResult(), new List<ProcedureMetaData>());
			FileInfo[] children = sqlFiles;

			for (int i = 0; i < children.Length; i++)
			{
				FileInfo child = children[i];
				if (child.Attributes.HasFlag(FileAttributes.Directory))
				{
					continue;
				}
				string content = SQLUtil.getFileContent(child);
				string databaseTemp = null;
                databaseTemp = (new DDLParser(tableColumns, procedures, vendor, content.ToUpper(), strict, databaseTemp)).Database;
			}

			string database = null;
			for (int i = 0; i < children.Length; i++)
			{
				FileInfo child = children[i];
				if (child.Attributes.HasFlag(FileAttributes.Directory))
				{
					continue;
				}
				string content = SQLUtil.getFileContent(child);
				database = (new ViewParser(tableColumns, vendor, content.ToUpper(), strict, database)).Database;
			}

			database = null;
			for (int i = 0; i < children.Length; i++)
			{
				FileInfo child = children[i];
				if (child.Attributes.HasFlag(FileAttributes.Directory))
				{
					continue;
				}
				string content = SQLUtil.getFileContent(child);
				database = (new ProcedureRelationScanner(procedures, vendor, content.ToUpper(), strict, database)).Database;
			}
		}

		public DlineageCommon(FileInfo sqlDir, EDbVendor vendor, bool strict, bool showUIInfo)
		{
			this.strict = strict;
			this.showUIInfo = showUIInfo;
			this.sqlDir = sqlDir;
			this.vendor = vendor;
			tableColumns.Clear();
			procedures = new Tuple<procedureImpactResult, IList<ProcedureMetaData>>(new procedureImpactResult(), new List<ProcedureMetaData>());
			FileInfo[] children = listFiles(sqlDir);

			for (int i = 0; i < children.Length; i++)
			{
				FileInfo child = children[i];
				if (child.Attributes.HasFlag(FileAttributes.Directory))
				{
					continue;
				}
				string content = SQLUtil.getFileContent(child);

				string databaseTemp = null;

                databaseTemp = (new DDLParser(tableColumns, procedures, vendor, content.ToUpper(), strict, databaseTemp)).Database;

			}

			string database = null;
			for (int i = 0; i < children.Length; i++)
			{
				FileInfo child = children[i];
				if (child.Attributes.HasFlag(FileAttributes.Directory))
				{
					continue;
				}
				string content = SQLUtil.getFileContent(child);

				database = (new ViewParser(tableColumns, vendor, content.ToUpper(), strict, database)).Database;

			}

			database = null;
			for (int i = 0; i < children.Length; i++)
			{
				FileInfo child = children[i];
				if (child.Attributes.HasFlag(FileAttributes.Directory))
				{
					continue;
				}
				string content = SQLUtil.getFileContent(child);

				database = (new ProcedureRelationScanner(procedures, vendor, content.ToUpper(), strict, database)).Database;
			}
		}

		public virtual void columnImpact()
		{
			string result = getColumnImpactResult(false);
			Console.WriteLine(result);
		}

		public virtual string ColumnImpactResult
		{
			get
			{
				return getColumnImpactResult(true);
			}
		}

		public virtual string getColumnImpactResult(bool analyzeDlineage)
		{
			if (string.ReferenceEquals(sqlContent, null))
			{
				Document doc = null;
				Element columnImpactResult = null;
				
                doc = new Document();
                XDeclaration declaration = new XDeclaration("1.0", "utf-8", "no");
                doc.Declaration = declaration;
                columnImpactResult = new XElement("columnImpactResult");
                doc.Add(columnImpactResult);

				if (sqlDir != null && sqlDir.Attributes.HasFlag(FileAttributes.Directory))
				{
					Element dirNode = new Element("dir");
					dirNode.Add(new XAttribute("name", sqlDir.FullName));
					columnImpactResult.Add(dirNode);
				}
				

				FileInfo[] children = sqlFiles == null ? listFiles(sqlDir) : sqlFiles;
				for (int i = 0; i < children.Length; i++)
				{
					FileInfo child = children[i];
					if (child.Attributes.HasFlag(FileAttributes.Directory))
					{
						continue;
					}
					if (child != null)
					{
						Element fileNode = new Element("file");
						fileNode.Add(new XAttribute("name", child.FullName));
						ColumnImpact impact = new ColumnImpact(fileNode, vendor, tableColumns, strict);
						impact.Debug = false;
						impact.ShowUIInfo = showUIInfo;
						impact.TraceErrorMessage = false;
						impact.AnalyzeDlineage = analyzeDlineage;
						impact.ignoreTopSelect(false);
						impact.impactSQL();
						if (!string.ReferenceEquals(impact.ErrorMessage, null) && impact.ErrorMessage.Trim().Length > 0)
						{
							Console.Error.WriteLine(impact.ErrorMessage.Trim());
						}
						if (fileNode.HasElements)
						{
							columnImpactResult.Add(fileNode);
						}
					}
				}
				if (doc != null)
				{
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
			}
			else
			{
				ColumnImpact impact = new ColumnImpact(sqlContent, vendor, tableColumns, strict);
				impact.Debug = false;
				impact.ShowUIInfo = showUIInfo;
				impact.TraceErrorMessage = false;
				impact.AnalyzeDlineage = true;
				impact.impactSQL();
				if (!string.ReferenceEquals(impact.ErrorMessage, null) && impact.ErrorMessage.Trim().Length > 0)
				{
					Console.Error.WriteLine(impact.ErrorMessage.Trim());
				}
				return impact.ImpactResult;
			}
			return null;
		}
        public virtual columnImpactResult generateColumnImpact(StringBuilder errorMessage)
        {
            StringBuilder sw = new StringBuilder();
            TextWriter systemSteam = Console.Error;
            TextWriter pw = new StringWriter(sw);
            Console.SetError(pw);

            string impactResult = this.ColumnImpactResult;
            if (pw != null)
            {
                pw.Close();
            }

            Console.SetError(systemSteam);

            if (sw != null)
            {
                if (errorMessage != null)
                {
                    errorMessage.Append(sw.ToString().Trim());
                }
            }

            return getColumnImpactResult(impactResult);
        }

        private columnImpactResult getColumnImpactResult(string result)
        {
            try
            {
                string[] results = result.Split(new char[] { '\n' });
                StringBuilder buffer = new StringBuilder();
                for (int i = 0; i < results.Length; i++)
                {
                    string line = results[i];
                    if (line.IndexOf("columnImpactResult", StringComparison.Ordinal) != -1 || line.IndexOf("targetColumn", StringComparison.Ordinal) != -1 || line.IndexOf("sourceColumn", StringComparison.Ordinal) != -1 || line.IndexOf("linkTable", StringComparison.Ordinal) != -1)
                    {
                        buffer.Append(line).Append("\n");
                    }
                }
                return XML2Model.loadXML(buffer.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            return null;
        }
        public virtual IList<ColumnMetaData[]> collectDlineageRelations(columnImpactResult impactResult)
        {
            IList<ColumnMetaData[]> relations = new List<ColumnMetaData[]>();
            if (impactResult == null)
            {
                return relations;
            }
            database[] dataMetaInfos = this.DataMetaInfos;
            if (dataMetaInfos == null)
            {
                return relations;
            }

            MetaScanner scanner = new MetaScanner(this);

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

                                string sourceTableName = null;
                                if (!string.ReferenceEquals(source.tableOwner, null) && !"unknown".Equals(source.tableOwner, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    sourceTableName = source.tableOwner + "." + source.tableName;
                                }
                                else
                                {
                                    sourceTableName = source.tableName;
                                }

                                ColumnMetaData sourceColumn = scanner.getColumnMetaData(sourceTableName + "." + source.name);

                                string targetTableName = null;
                                if (!string.ReferenceEquals(link.tableOwner, null) && !"unknown".Equals(link.tableOwner, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    targetTableName = link.tableOwner + "." + link.tableName;
                                }
                                else
                                {
                                    targetTableName = link.tableName;
                                }

                                ColumnMetaData targetColumn = scanner.getColumnMetaData(targetTableName, link.name);

                                if (sourceColumn != null && targetColumn != null)
                                {
                                    relations.Add(new ColumnMetaData[] { targetColumn, sourceColumn });
                                }
                                else
                                {
                                    if (sourceColumn == null)
                                    {
                                        Console.Error.WriteLine(sourceTableName + "." + source.name + " should not be null.");
                                    }

                                    if (targetColumn == null)
                                    {
                                        Console.Error.WriteLine(targetTableName + "." + link.name + " should not be null.");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return relations;
        }

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public virtual void forwardAnalyze(string tableColumn, IList<ColumnMetaData[]> relations)
		{
			ColumnMetaData columnMetaData = (new MetaScanner(this)).getColumnMetaData(tableColumn);
			IList<ColumnMetaData> columns = new List<ColumnMetaData>();
			IEnumerator<TableMetaData> iter = tableColumns.Keys.GetEnumerator();
			while (iter.MoveNext())
			{
				((List<ColumnMetaData>)columns).AddRange(tableColumns[iter.Current]);
			}
			if (columnMetaData != null)
			{
				outputForwardAnalyze(columnMetaData, columns, 0, relations);
			}
		}

		public virtual void backwardAnalyze(string viewColumn, IList<ColumnMetaData[]> relations)
		{
			ColumnMetaData columnMetaData = (new MetaScanner(this)).getColumnMetaData(viewColumn);
			if (columnMetaData != null)
			{
				outputBackwardAnalyze(columnMetaData, 0, relations);
			}
		}

		private void outputBackwardAnalyze(ColumnMetaData columnMetaData, int level, IList<ColumnMetaData[]> relations)
		{
			if (level > 0)
			{
				for (int i = 0; i < level; i++)
				{
					Console.Write("---");
				}
				Console.Write(">");
			}
			Console.WriteLine(columnMetaData.DisplayFullName);
			if (columnMetaData.ReferColumns != null && columnMetaData.ReferColumns.Length > 0)
			{
				for (int i = 0; i < columnMetaData.ReferColumns.Length; i++)
				{
					ColumnMetaData sourceColumn = columnMetaData.ReferColumns[i];
					if (containsRelation(columnMetaData, sourceColumn, relations))
					{
						outputBackwardAnalyze(columnMetaData.ReferColumns[i], level + 1, relations);
					}
				}
			}
		}

		private bool containsRelation(ColumnMetaData targetColumn, ColumnMetaData sourceColumn, IList<ColumnMetaData[]> relations)
		{
			if (relations == null)
			{
				return false;
			}
			for (int i = 0; i < relations.Count; i++)
			{
				ColumnMetaData[] relation = relations[i];
				if (relation[0].Equals(targetColumn) && relation[1].Equals(sourceColumn))
				{
					return true;
				}
			}
			return false;
		}

		private void outputForwardAnalyze(ColumnMetaData columnMetaData, IList<ColumnMetaData> columns, int level, IList<ColumnMetaData[]> relations)
		{
			if (level > 0)
			{
				for (int i = 0; i < level; i++)
				{
					Console.Write("---");
				}
				Console.Write(">");
			}
			Console.WriteLine(columnMetaData.DisplayFullName);
			for (int i = 0; i < columns.Count; i++)
			{
				ColumnMetaData targetColumn = columns[i];
				if (new List<ColumnMetaData>(targetColumn.ReferColumns).Contains(columnMetaData))
				{
					if (containsRelation(targetColumn, columnMetaData, relations))
					{
						outputForwardAnalyze(targetColumn, columns, level + 1, relations);
					}
				}
			}
		}

		public virtual void outputDDLSchema()
		{
			Console.WriteLine((new DDLSchema(tableColumns)).SchemaXML);
		}

		public virtual database[] DataMetaInfos
		{
			get
			{
				return (new DDLSchema(tableColumns)).DataMetaInfos;
			}
		}

		private FileInfo[] listFiles(FileInfo sqlFiles)
		{
			List<FileInfo> children = new List<FileInfo>();
			if (sqlFiles != null)
			{
				listFiles(sqlFiles.FullName, children);
			}
			return children.ToArray();
		}

		private void listFiles(string rootFilePath, List<FileInfo> children)
		{
            FileInfo rootFile = new FileInfo(rootFilePath);

            if (!rootFile.Attributes.HasFlag(FileAttributes.Directory))
			{
				children.Add(rootFile);
			}
			else
			{
                FileInfo[] files = new DirectoryInfo(rootFile.FullName).GetFiles();
				for (int i = 0; i < files.Length; i++)
				{
					listFiles(files[i].FullName, children);
				}

                DirectoryInfo[] dirs = new DirectoryInfo(rootFile.FullName).GetDirectories();
                for (int i = 0; i < dirs.Length; i++)
                {
                    listFiles(dirs[i].FullName, children);
                }
            }
		}

		public virtual IDictionary<TableMetaData, IList<ColumnMetaData>> MetaData
		{
			get
			{
				return tableColumns;
			}
		}

		public virtual Tuple<procedureImpactResult, IList<ProcedureMetaData>> Procedures
		{
			get
			{
				return procedures;
			}
		}

		public virtual bool Strict
		{
			get
			{
				return strict;
			}
		}

		public virtual EDbVendor Vendor
		{
			get
			{
				return vendor;
			}
		}

	}

}