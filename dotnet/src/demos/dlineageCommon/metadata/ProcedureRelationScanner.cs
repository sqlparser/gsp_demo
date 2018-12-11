using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage.metadata
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TStatementList = gudusoft.gsqlparser.TStatementList;
    using TFunctionCall = gudusoft.gsqlparser.nodes.TFunctionCall;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TParseTreeVisitor = gudusoft.gsqlparser.nodes.TParseTreeVisitor;
    using TCallStatement = gudusoft.gsqlparser.stmt.TCallStatement;
    using TStoredProcedureSqlStatement = gudusoft.gsqlparser.stmt.TStoredProcedureSqlStatement;
    using TUseDatabase = gudusoft.gsqlparser.stmt.TUseDatabase;
    using TMssqlExecute = gudusoft.gsqlparser.stmt.mssql.TMssqlExecute;


    using ProcedureMetaData = gudusoft.gsqlparser.demos.dlineage.model.metadata.ProcedureMetaData;
    using procedure = gudusoft.gsqlparser.demos.dlineage.model.xml.procedure;
    using procedureImpactResult = gudusoft.gsqlparser.demos.dlineage.model.xml.procedureImpactResult;
    using sourceProcedure = gudusoft.gsqlparser.demos.dlineage.model.xml.sourceProcedure;
    using targetProcedure = gudusoft.gsqlparser.demos.dlineage.model.xml.targetProcedure;
    using SQLUtil = gudusoft.gsqlparser.demos.dlineage.util.SQLUtil;
    using System;

    public class ProcedureRelationScanner
	{

		private Tuple<procedureImpactResult, IList<ProcedureMetaData>> procedures;
		private bool strict = false;
		private string database = null;
		private EDbVendor vendor = EDbVendor.dbvmssql;

		public ProcedureRelationScanner(Tuple<procedureImpactResult, IList<ProcedureMetaData>> procedures, EDbVendor vendor, string sqlText, bool strict, string database)
		{
			this.strict = strict;
			this.vendor = vendor;
			this.database = database;
			this.procedures = procedures;
			TGSqlParser parser = new TGSqlParser(vendor);
			parser.sqltext = sqlText;
			checkDDL(parser);
		}

		private void checkDDL(TGSqlParser sqlparser)
		{
			int ret = sqlparser.parse();
			if (ret == 0)
			{
				TStatementList stmts = sqlparser.sqlstatements;
				parseStatementList(stmts);
			}
		}

		private void parseStatementList(TStatementList stmts)
		{
			for (int i = 0; i < stmts.size(); i++)
			{
				TCustomSqlStatement stmt = stmts.get(i);
				parseStatement(stmt);
			}
		}

		private void parseStatement(TCustomSqlStatement stmt)
		{

			if (stmt is TUseDatabase)
			{
				TUseDatabase use = (TUseDatabase) stmt;
				database = use.DatabaseName.ToString();
			}
			else if (stmt is TStoredProcedureSqlStatement)
			{
				TStoredProcedureSqlStatement procedureStmt = (TStoredProcedureSqlStatement) stmt;
				parseProcedureStmt(procedureStmt);
			}
		}

		private void parseProcedureStmt(TStoredProcedureSqlStatement procedureStmt)
		{
            if(procedureStmt.StoredProcedureName == null)
            {
                return;
            }
			ProcedureMetaData procedureMetaData = getProcedureMetaData(procedureStmt.StoredProcedureName);
			procedureMetaData = getProcedureMetaData(procedureMetaData, true);

			TObjectName procedureName = procedureStmt.StoredProcedureName;
			procedure procedure = new procedure();
			procedure.name = procedureMetaData.DisplayName;
			procedure.owner = getOwnerString(procedureMetaData);
			procedure.coordinate = procedureName.startToken.lineNo + "," + procedureName.startToken.columnNo;
			procedure.highlightInfo = procedureName.startToken.offset + "," + (procedureName.endToken.offset - procedureName.startToken.offset + procedureName.endToken.astext.Length);
            List<procedure> procedureList = getProcedureList(procedures.Item1);
            procedureList.Add(procedure);
            procedures.Item1.procedures = procedureList.ToArray();


            parseProcedureLineage(procedureStmt, procedureMetaData, procedure);

		}

		private List<procedure> getProcedureList(procedureImpactResult impactResult)
		{
			if (impactResult.procedures == null)
			{
				impactResult.procedures = new procedure[0];
			}
			return new List<procedure>(impactResult.procedures);
		}

		public virtual ProcedureMetaData getProcedureMetaData(ProcedureMetaData parentProcedure, TObjectName procedureName)
		{
			ProcedureMetaData procedureMetaData = new ProcedureMetaData(vendor, strict);
			procedureMetaData.Name = procedureName.PartString == null ? procedureName.ObjectString : procedureName.PartString;
			if (procedureName.SchemaString != null)
			{
				procedureMetaData.SchemaName = procedureName.SchemaString;
			}
			else
			{
				procedureMetaData.SchemaName = parentProcedure.SchemaName;
				procedureMetaData.SchemaDisplayName = parentProcedure.SchemaDisplayName;
			}

			if (isNotEmpty(procedureName.DatabaseString))
			{
				procedureMetaData.CatalogName = procedureName.DatabaseString;
			}
			else
			{
				procedureMetaData.CatalogName = parentProcedure.CatalogName;
				procedureMetaData.CatalogDisplayName = parentProcedure.CatalogDisplayName;
			}
			return procedureMetaData;
		}

		private ProcedureMetaData getProcedureMetaData(TObjectName procedureName)
		{
			ProcedureMetaData procedureMetaData = new ProcedureMetaData(vendor, strict);
			procedureMetaData.Name = procedureName.PartString == null ? procedureName.ObjectString : procedureName.PartString;
			procedureMetaData.SchemaName = procedureName.SchemaString;
			if (isNotEmpty(procedureName.DatabaseString))
			{
				procedureMetaData.CatalogName = procedureName.DatabaseString;
			}
			else
			{
				procedureMetaData.CatalogName = database;
			}
			return procedureMetaData;
		}

		internal class functionVisitor : TParseTreeVisitor
		{
			private readonly ProcedureRelationScanner outerInstance;


			internal ProcedureMetaData parentProcedure;
			internal targetProcedure targetProcedure;

			public functionVisitor(ProcedureRelationScanner outerInstance, ProcedureMetaData parentProcedure, procedure procedure)
			{
				this.outerInstance = outerInstance;
				this.parentProcedure = parentProcedure;
				this.targetProcedure = new targetProcedure();
				targetProcedure.coordinate = procedure.coordinate;
				targetProcedure.highlightInfo = procedure.highlightInfo;
				targetProcedure.name = procedure.name;
				targetProcedure.owner = procedure.owner;
                List<targetProcedure> targetProcedureList = getTargetProcedureList(outerInstance.procedures.Item1);
                targetProcedureList.Add(targetProcedure);
                outerInstance.procedures.Item1.targetProcedures = targetProcedureList.ToArray();
            }

			internal virtual List<targetProcedure> getTargetProcedureList(procedureImpactResult impactResult)
			{
				if (impactResult.targetProcedures == null)
				{
					impactResult.targetProcedures = new targetProcedure[0];
				}
				return new List<targetProcedure>(impactResult.targetProcedures);
			}

			public override void preVisit(TFunctionCall node)
			{
				if (node.FunctionName != null)
				{
					TObjectName procedureName = node.FunctionName;
					ProcedureMetaData procedureMetaData = getProcedureMetaData(procedureName);
					setProcedureDlinage(procedureMetaData, procedureName);
				}
			}

			internal virtual ProcedureMetaData getProcedureMetaData(TObjectName procedureName)
			{
				ProcedureMetaData procedureMetaData = outerInstance.getProcedureMetaData(parentProcedure, procedureName);
				procedureMetaData = outerInstance.getProcedureMetaData(procedureMetaData, false);
				if (procedureMetaData == null)
				{
					return null;
				}
				if (string.ReferenceEquals(procedureMetaData.CatalogName, null))
				{
					procedureMetaData.CatalogName = parentProcedure.CatalogName;
					procedureMetaData.CatalogDisplayName = parentProcedure.CatalogDisplayName;
				}
				if (string.ReferenceEquals(procedureMetaData.SchemaName, null))
				{
					procedureMetaData.SchemaName = parentProcedure.SchemaName;
					procedureMetaData.SchemaDisplayName = parentProcedure.SchemaDisplayName;
				}
				return procedureMetaData;
			}

			internal virtual void setProcedureDlinage(ProcedureMetaData procedureMetaData, TObjectName procedureName)
			{
				if (procedureMetaData == null)
				{
					return;
				}
				sourceProcedure sourceProcedure = new sourceProcedure();
				sourceProcedure.name = procedureMetaData.DisplayName;
				sourceProcedure.owner = outerInstance.getOwnerString(procedureMetaData);
				sourceProcedure.coordinate = procedureName.startToken.lineNo + "," + procedureName.startToken.columnNo;
				sourceProcedure.highlightInfo = procedureName.startToken.offset + "," + (procedureName.endToken.offset - procedureName.startToken.offset + procedureName.endToken.astext.Length);

                List<sourceProcedure> sourceProcedureList = getSourceProcedureList(targetProcedure);
                sourceProcedureList.Add(sourceProcedure);
                targetProcedure.sourceProcedures = sourceProcedureList.ToArray();
            }

			internal virtual List<sourceProcedure> getSourceProcedureList(targetProcedure targetProcedure)
			{
				if (targetProcedure.sourceProcedures == null)
				{
					targetProcedure.sourceProcedures = new sourceProcedure[0];
				}
                return new List<sourceProcedure>(targetProcedure.sourceProcedures);
			}

			public override void preVisit(TCallStatement statement)
			{
				if (statement.RoutineName != null)
				{
					TObjectName procedureName = statement.RoutineName;
					ProcedureMetaData procedureMetaData = getProcedureMetaData(procedureName);
					setProcedureDlinage(procedureMetaData, procedureName);
				}
			}

			public override void preVisit(TMssqlExecute statement)
			{
				if (statement.ModuleName != null)
				{
					TObjectName procedureName = statement.ModuleName;
					ProcedureMetaData procedureMetaData = getProcedureMetaData(procedureName);
					setProcedureDlinage(procedureMetaData, procedureName);
				}
			}
		}

		private void parseProcedureLineage(TStoredProcedureSqlStatement procedureStmt, ProcedureMetaData procedureMetaData, procedure sourceProcedure)
		{
			functionVisitor fv = new functionVisitor(this, procedureMetaData, sourceProcedure);
			procedureStmt.acceptChildren(fv);
		}

		private static bool isNotEmpty(string str)
		{
			return !string.ReferenceEquals(str, null) && str.Trim().Length > 0;
		}

		private ProcedureMetaData getProcedureMetaData(ProcedureMetaData procedureMetaData, bool replace)
		{
			int index = procedures.Item2.IndexOf(procedureMetaData);
			if (index != -1)
			{
				if (replace)
				{
					procedures.Item2.RemoveAt(index);
					procedures.Item2.Add(procedureMetaData);
					return procedureMetaData;
				}
				else
				{
					return procedures.Item2[index];
				}
			}
			else
			{
				if (replace)
				{
					procedures.Item2.Add(procedureMetaData);
					return procedureMetaData;
				}
				else
				{
					return null;
				}
			}
		}

		public virtual string Database
		{
			get
			{
				return database;
			}
		}

		private string getOwnerString(ProcedureMetaData procedureMetaData)
		{
			StringBuilder buffer = new StringBuilder();
			if (!SQLUtil.isEmpty(procedureMetaData.CatalogDisplayName))
			{
				buffer.Append(procedureMetaData.CatalogDisplayName).Append(".");
			}
			if (!SQLUtil.isEmpty(procedureMetaData.SchemaDisplayName))
			{
				buffer.Append(procedureMetaData.SchemaDisplayName);
			}
			return buffer.ToString().ToUpper();
		}

		private string getOwnerString(TObjectName objectName)
		{
			StringBuilder buffer = new StringBuilder();
			if (!SQLUtil.isEmpty(objectName.ServerString))
			{
				buffer.Append(objectName.ServerString).Append(".");
			}
			if (!SQLUtil.isEmpty(objectName.DatabaseString))
			{
				buffer.Append(objectName.DatabaseString).Append(".");
			}
			if (!SQLUtil.isEmpty(objectName.SchemaString))
			{
				buffer.Append(objectName.SchemaString);
			}
			return buffer.ToString().ToUpper();
		}
	}

}