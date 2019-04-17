using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.stmt.netezza;
using gudusoft.gsqlparser.nodes.netezza;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestNetezza
    {
        TGSqlParser parser;

        public UnitTestNetezza()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvnetezza);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestQuery()
        {
            parser.sqltext = "select f from t where f>1";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage);
        }

        [TestMethod]
        public void TestNetezzaFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"netezza\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestNetezzaFiles2()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\netezza\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void testNetezzaSelectFromExternalTable()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "insert into \"$hist_column_access_1\"(npsid, npsinstanceid, opid, logentryid, seqid, sessionid, dbid, dbname, schemaid, schemaname, tableid, tablename, columnid, columnname, usage) select 1, * from external '/nz/data.1.0/hist/loading/alc_20140215_151309.473439/alc_co_20140215_151309.473439' (npsinstanceid int, opid bigint, logentryid bigint, seqid integer, sessionid bigint, dbid int, dbname nvarchar (128), schemaid int , schemaname nvarchar (128), tableid int, tablename nvarchar (128), columnid int, columnname nvarchar (128), usage int) using( logdir '/nz/data.1.0/hist/loading/alc_20140215_151309.473439' encoding 'internal' escapechar '\\' fillrecord true ctrlchars true crinstring true)";
            Assert.IsTrue(sqlparser.parse() == 0);
            TInsertSqlStatement insertSqlStatement = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            TTable table = insertSqlStatement.tables.getTable(0);
            Assert.IsTrue(table.TableName.ToString().Equals("\"$hist_column_access_1\"", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement select = (TSelectSqlStatement)insertSqlStatement.SubQuery;
            table = select.tables.getTable(0);
            Assert.IsTrue(table.TableName.ToString().Equals("'/nz/data.1.0/hist/loading/alc_20140215_151309.473439/alc_co_20140215_151309.473439'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testMinusSetToString()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "CREATE TABLE X AS \n" + "SELECT A FROM B\n" + "minus\n" + "SELECT A FROM C";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCreateTableSqlStatement ct = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TSelectSqlStatement select = ct.SubQuery;
            Assert.IsTrue(!string.ReferenceEquals(select.ToString(), null));

        }

        [TestMethod]
        public void testGroomTable1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "GROOM TABLE table_name VERSIONS";
            Assert.IsTrue(sqlparser.parse() == 0);
            TNetezzaGroomTable groomTable = (TNetezzaGroomTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(groomTable.TableName.ToString().Equals("table_name", StringComparison.CurrentCultureIgnoreCase));
            TModeChoice modeChoice = groomTable.ModeChoice;
            Assert.IsTrue(modeChoice.ModeChoice == EModeChoice.versions);

        }

        [TestMethod]
        public void testGroomTable2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "GROOM TABLE X;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TNetezzaGroomTable groomTable = (TNetezzaGroomTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(groomTable.TableName.ToString().Equals("x", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testGroomTable3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "GROOM TABLE table_name PAGES ALL";
            Assert.IsTrue(sqlparser.parse() == 0);
            TNetezzaGroomTable groomTable = (TNetezzaGroomTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(groomTable.TableName.ToString().Equals("table_name", StringComparison.CurrentCultureIgnoreCase));
            TModeChoice modeChoice = groomTable.ModeChoice;
            Assert.IsTrue(modeChoice.ModeChoice == EModeChoice.pagesAll);
        }

        [TestMethod]
        public void testGenerateStatistics1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "GENERATE STATISTICS ON X;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TNetezzaGenerateStatistics getStatistics = (TNetezzaGenerateStatistics)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(getStatistics.TableName.ToString().Equals("X", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testGenerateStatistics2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "GENERATE STATISTICS ON cows (cnumber)";
            Assert.IsTrue(sqlparser.parse() == 0);
            TNetezzaGenerateStatistics getStatistics = (TNetezzaGenerateStatistics)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(getStatistics.TableName.ToString().Equals("cows", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(getStatistics.Columns.getObjectName(0).ToString().Equals("cnumber", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testGenerateStatistics3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "GENERATE EXPRESS STATISTICS ON cows";
            Assert.IsTrue(sqlparser.parse() == 0);
            TNetezzaGenerateStatistics getStatistics = (TNetezzaGenerateStatistics)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(getStatistics.TableName.ToString().Equals("cows", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(getStatistics.Express);
        }

        [TestMethod]
        public void testDropSchema()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "drop schema temp_schema cascade;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TDropSchemaSqlStatement dropSchemaSqlStatement = (TDropSchemaSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(dropSchemaSqlStatement.SchemaName.ToString().Equals("temp_schema", StringComparison.CurrentCultureIgnoreCase));
            if (dropSchemaSqlStatement.DropBehavior != null)
            {
                Assert.IsTrue(dropSchemaSqlStatement.DropBehavior.ToString().Equals("cascade", StringComparison.CurrentCultureIgnoreCase));
            }

        }

        [TestMethod]
        public void testCTEInCreateView()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "CREATE OR REPLACE VIEW FCT_PI_CLAIM_EVENTS_IMPACT_SUMMARY_INSIGHT AS \n" + "\n" + "WITH CLAIM_LODGEMENT AS (\n" + "\n" + "     SELECT  CLAIM_ID\n" + "\t       , EVENT_TS                AS LODGEMENT_TIMESTAMP\n" + "     FROM (\t\n" + "\t \n" + "     SELECT       \n" + "       CCR_CC_HISTORY.CLAIMID        AS CLAIM_ID\n" + "     , CCR_CC_HISTORY.EVENTTIMESTAMP AS EVENT_TS \n" + "\t , ROW_NUMBER() OVER (PARTITION BY CLAIMID \n" + "\t                          ORDER BY CCR_CC_HISTORY.EVENTTIMESTAMP DESC\n" + "\t\t\t\t\t\t\t         , CCR_CC_HISTORY.ID DESC)\n" + "\t\t\t\t\t\t\t         AS OPEN_EVENT_DESC_SEQ\t         \n" + "       FROM CCR_CC_HISTORY CCR_CC_HISTORY\n" + "  \n" + "  LEFT JOIN (\n" + "    \tSELECT CCR_CC_TRANSACTION.CLAIMID                                     AS CLAIM_ID\n" + "             , MIN(CCR_CC_TRANSACTION.CREATETIME)                             AS ACTIVATION_DATETIME\n" + "          FROM CCR_CC_TRANSACTION CCR_CC_TRANSACTION   \n" + "      GROUP BY CCR_CC_TRANSACTION.CLAIMID \t\t\t\t\t  \n" + "\t      ) CLAIM_ACTIVATION\n" + "\t\t  \n" + "         ON CLAIM_ACTIVATION.CLAIM_ID\t  = CCR_CC_HISTORY.CLAIMID\n" + "  \n" + "       JOIN CCR_CCTL_HISTORYTYPE CCR_CCTL_HISTORYTYPE\n" + "         ON CCR_CCTL_HISTORYTYPE.ID    =  CCR_CC_HISTORY.\"xxxx\"\n" + "        AND CCR_CCTL_HISTORYTYPE.NAME IN ('xxxx', 'xxxx') \n" + "        AND CCR_CC_HISTORY.EXPOSUREID IS NULL   \n" + "\n" + "      WHERE CCR_CC_HISTORY.EVENTTIMESTAMP <= COALESCE(ACTIVATION_DATETIME, CCR_CC_HISTORY.EVENTTIMESTAMP)   \n" + "\t  \n" + ") AS OPEN_REOPEN_DATES\n" + "\n" + "      WHERE OPEN_EVENT_DESC_SEQ = 1\n" + "\n" + ")\n" + ", TRANSACTION_SUMMARY AS (\n" + "\n" + "SELECT *\n" + "FROM   INT_PI_CLAIM_FINANCIALS_LIVE\n" + "\n" + ")\n" + "   SELECT \n" + "          CCR_CC_CLAIM.ID                                  AS CLAIM_ID\n" + "\t\t, CCR_CC_CLAIM.CLAIMNUMBER                         AS CLAIM_NUMBER\n" + "\t\t, CCR_CC_POLICY.POLICYNUMBER                       AS POLICY_NUMBER\n" + "\t\t\n" + "\t\t \n" + "\t\t, CCR_CCTL_SC_BRAND.NAME                           AS BRAND\n" + "\t\t, CCR_CC_CATASTROPHE.NAME                          AS CATASTROPHE_NAME   \n" + "        , CCR_CC_CATASTROPHE.CATASTROPHENUMBER             AS CATASTROPHE_CODE\n" + "\t    , CCR_CCTL_SC_CLAIMTYPE.NAME                       AS CLAIM_TYPE\t\n" + "\t\t, DATE(CCR_CC_CLAIM.CLOSEDATE)                     AS CLOSED_DATE\t  \n" + "\t\t, INT_PI_CLAIM_EVENT.EVENT_NAME                    AS EVENT_NAME\n" + "\t    , CCR_CCTL_SC_CLAIMPRIORITY.NAME                   AS HOME_CLAIM_CATEGORY\n" + "        , DATE(CLAIM_LODGEMENT.LODGEMENT_TIMESTAMP)        AS LODGEMENT_DATE\n" + "\t\t, DATE(CLAIM_LODGEMENT.LODGEMENT_TIMESTAMP)        AS LODGEMENT_TIME\t\t\n" + "\t\t, COALESCE(CCR_CCTL_SC_LOSSCAUSE.NAME, CCR_CCTL_LOSSCAUSE.NAME)\n" + "\t\t                                                   AS LOSS_CAUSE\t\t\n" + "\t\t, DATE(CCR_CC_CLAIM.LOSSDATE)                      AS LOSS_DATE\n" + "\n" + "\t    , CCR_CC_ADDRESS_LOSS.POSTALCODE                   AS LOSS_POSTCODE\n" + "\t    , CCR_CCX_SC_PAFPOSTCODES.LOCALITY                 AS LOSS_POSTCODE_LABEL\t  \n" + "\t    , UPPER(CCR_CC_ADDRESS_LOSS.CITY)                  AS LOSS_SUBURB\n" + "\t    , CCR_CCTL_STATE_LOSS.NAME                         AS LOSS_STATE\n" + "\t    , CASE WHEN CCR_CCTL_LOSSTYPE.NAME = 'xxxx' \n" + "\t\t       THEN 'Home' \n" + "\t\t\t   ELSE  CCR_CCTL_LOSSTYPE.NAME \n" + "\t\t   END                                             AS LOSS_TYPE\n" + "\t    , CAST(CASE WHEN CCR_CCTL_LOSSTYPE.TYPECODE = 'xxxx' \n" + "\t                 AND CCR_CCTL_SC_BRAND.TYPECODE = 'xxxx' \n" + "\t\t  \t        THEN 'xxxx'\n" + "\t                WHEN CCR_CCTL_LOSSTYPE.TYPECODE = 'xxxx' \n" + "\t\t\t         AND CCR_CCTL_VEHICLESTYLE.TYPECODE IN ('xxxx','xxxx','xxxx','xxxx') \n" + "\t\t\t        THEN 'xxxx'\n" + "\t\t\t        ELSE 'xxxx'\n" + "\t           END AS CHAR(1))                             AS SPECIALIST_CLAIM_FLAG\t\n" + "\t   , CAST(CASE WHEN CCR_CC_INCIDENT.VEHICLEOPERABLE = 0 \n" + "\t              THEN 'xxxx' \n" + "\t\t\t      ELSE 'xxxx' \n" + "\t\t      END AS CHAR(1))                              AS VEHICLE_DRIVABLE_FLAG\n" + "\t   \t\t  \n" + "\n" + "       \n" + "\t  , CAST(1 AS INTEGER)                                 AS CLAIM_VOLUME\n" + "\t  , TRANSACTION_SUMMARY.NET_INCURRED_EX_GST          \n" + "\t  , TRANSACTION_SUMMARY.NET_INCURRED                   AS SYSTEM_COST\t\t\n" + "\t  \n" + "\t   \n" + "\t  , CCR_CC_CLAIM_MAXTS.MAX_COMMIT_TIMESTAMP\n" + "                                \n" + "     FROM  CCR_CC_CLAIM CCR_CC_CLAIM\n" + "\t \n" + "\t JOIN  TRANSACTION_SUMMARY TRANSACTION_SUMMARY\n" + "\t   ON  TRANSACTION_SUMMARY.CLAIM_ID                    = CCR_CC_CLAIM.ID\n" + "\t\t \n" + "     JOIN  CCR_CC_POLICY CCR_CC_POLICY\n" + "       ON  CCR_CC_CLAIM.POLICYID                           = CCR_CC_POLICY.ID\n" + "       \n" + "     JOIN  CCR_CCTL_SC_BRAND CCR_CCTL_SC_BRAND\n" + "       ON  CCR_CCTL_SC_BRAND.ID                            = CCR_CC_POLICY.SC_BRAND  \t   \n" + "\t   \n" + "     JOIN  CCR_CCTL_LOSSTYPE CCR_CCTL_LOSSTYPE\n" + "       ON  CCR_CC_CLAIM.LOSSTYPE                           = CCR_CCTL_LOSSTYPE.ID\t \n" + "\t   \n" + "     JOIN  CCR_CC_ADDRESS CCR_CC_ADDRESS_LOSS\n" + "       ON  CCR_CC_ADDRESS_LOSS.ID                          = CCR_CC_CLAIM.LOSSLOCATIONID \n" + "\t   \n" + "     JOIN  CCR_CCTL_STATE CCR_CCTL_STATE_LOSS\n" + "       ON  CCR_CC_ADDRESS_LOSS.STATE                       = CCR_CCTL_STATE_LOSS.ID\t  \n" + "\t   \n" + "\t     \n" + "     JOIN  (\n" + "\t      SELECT POSTCODE\n" + "\t\t       , LOCALITY\n" + "\t\t\t   , ROW_NUMBER() OVER (PARTITION BY POSTCODE \n" + "\t\t\t                            ORDER BY LOCALITY) \n" + "\t\t\t\t\t\t\t\t\t\t          AS ALPHA_ORDER \n" + "\t        FROM CCR_CCX_SC_PAFPOSTCODES\n" + "\t\t   ) CCR_CCX_SC_PAFPOSTCODES\n" + "\t\t   \n" + "       ON CCR_CCX_SC_PAFPOSTCODES.POSTCODE                 = CCR_CC_ADDRESS_LOSS.POSTALCODE\n" + "      AND CCR_CCX_SC_PAFPOSTCODES.ALPHA_ORDER              = 1 \t   \n" + "\t  \n" + "     JOIN  (SELECT MAX(COMMIT_TIMESTAMP) AS MAX_COMMIT_TIMESTAMP FROM CCR_CC_CLAIM) CCR_CC_CLAIM_MAXTS\n" + "\t   ON  1=1\n" + "\t   \n" + "LEFT JOIN  CCR_CCTL_LOSSCAUSE CCR_CCTL_SC_LOSSCAUSE\n" + "       ON  CCR_CCTL_SC_LOSSCAUSE.ID                        = CCR_CC_CLAIM.SC_LOSSCAUSE  \n" + "\t   \n" + "LEFT JOIN  CCR_CCTL_LOSSCAUSE CCR_CCTL_LOSSCAUSE\n" + "       ON  CCR_CCTL_LOSSCAUSE.ID                           = CCR_CC_CLAIM.LOSSCAUSE    \n" + "  \n" + "LEFT JOIN  CCR_CCTL_SC_YESNOONLY CCR_CCTL_SC_YESNOONLY\n" + "       ON CCR_CCTL_SC_YESNOONLY.ID                         = CCR_CC_CLAIM.SC_INCIDENTREPORT\n" + "\t \n" + "LEFT JOIN CLAIM_LODGEMENT CLAIM_LODGEMENT\t\t  \n" + "       ON CLAIM_LODGEMENT.CLAIM_ID                         = CCR_CC_CLAIM.ID\n" + "\t\t\n" + "LEFT JOIN  CCR_CC_CATASTROPHE CCR_CC_CATASTROPHE\n" + "       ON  CCR_CC_CATASTROPHE.ID                           = CCR_CC_CLAIM.CATASTROPHEID \t\n" + "\t   \n" + "LEFT JOIN  INT_PI_CLAIM_EVENT INT_PI_CLAIM_EVENT\n" + "       ON  INT_PI_CLAIM_EVENT.CLAIM_ID                     = CCR_CC_CLAIM.ID\n" + "\t   \n" + "LEFT JOIN  CCR_CC_INCIDENT CCR_CC_INCIDENT\n" + "       ON  CCR_CC_INCIDENT.CLAIMID                         = CCR_CC_CLAIM.ID\n" + "\n" + "LEFT JOIN  CCR_CCTL_LOSSPARTYTYPE CCR_CCTL_LOSSPARTYTYPE_PR\n" + "       ON  CCR_CCTL_LOSSPARTYTYPE_PR.ID                    = CCR_CC_INCIDENT.SC_LOSSPARTY\n" + "\t   \n" + "LEFT JOIN  CCR_CCTL_LOSSPARTYTYPE CCR_CCTL_LOSSPARTYTYPE_AUTO\n" + "       ON  CCR_CCTL_LOSSPARTYTYPE_AUTO.ID                  = CCR_CC_INCIDENT.VEHICLELOSSPARTY\t   \n" + "\t   \n" + "LEFT JOIN  CCR_CC_VEHICLE CCR_CC_VEHICLE\n" + "       ON  CCR_CC_VEHICLE.ID                               = CCR_CC_INCIDENT.VEHICLEID\n" + "\t   \n" + "LEFT JOIN  CCR_CCTL_VEHICLESTYLE CCR_CCTL_VEHICLESTYLE\n" + "       ON  CCR_CCTL_VEHICLESTYLE.ID                        = CCR_CC_VEHICLE.STYLE  \t  \n" + "\t   \n" + "LEFT JOIN  CCR_CCTL_SC_CLAIMPRIORITY CCR_CCTL_SC_CLAIMPRIORITY\n" + "       ON  CCR_CCTL_SC_CLAIMPRIORITY.ID                    = CCR_CC_CLAIM.SC_USERCLAIMPRIORITY\n" + "\n" + "LEFT JOIN  CCR_CCTL_SC_CLAIMTYPE CCR_CCTL_SC_CLAIMTYPE\n" + "       ON  CCR_CCTL_SC_CLAIMTYPE.ID\t                       = CCR_CC_CLAIM.SC_CLAIMTYPE  \t  \t   \n" + "\n" + "    WHERE CCR_CC_CLAIM.RETIRED                             = 0\n" + "      AND CCR_CC_POLICY.RETIRED                            = 0\n" + "\t     \n" + "\t  AND  COALESCE(CCR_CCTL_SC_YESNOONLY.NAME,'xxxx')      <> 'xxxx'\n" + "\t     \n" + "\t  AND (   CCR_CCTL_LOSSTYPE.TYPECODE                   = 'xxxx' \n" + "\t     OR ( CCR_CCTL_LOSSPARTYTYPE_AUTO.TYPECODE         = 'xxxx' AND CCR_CCTL_LOSSTYPE.TYPECODE = 'xxxx')\n" + "\t\t  )\n" + "\t\t \t                                     \n" + ";";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCreateViewSqlStatement viewSqlStatement = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(viewSqlStatement.ViewName.ToString().Equals("FCT_PI_CLAIM_EVENTS_IMPACT_SUMMARY_INSIGHT", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement select = viewSqlStatement.Subquery;
            Console.WriteLine(select.CteList.size());
        }

        [TestMethod]
        public void testRenameTable()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvnetezza);
            sqlparser.sqltext = "ALTER TABLE X RENAME TO Y;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTable = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTable.TableName.ToString().Equals("X", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOption ato = alterTable.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ato.OptionType == EAlterTableOptionType.RenameTable);
            Assert.IsTrue(ato.NewTableName.ToString().Equals("Y", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "ALTER TABLE EVRST_HIST_NK..CLC_D_ACCOUNT_STATUS_HIER RENAME TO EVRST_HIST_NK..CLC_D_ACCOUNT_STATUS_HIER_ARCH";
            Assert.IsTrue(sqlparser.parse() == 0);

            alterTable = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTable.TableName.ToString().Equals("EVRST_HIST_NK..CLC_D_ACCOUNT_STATUS_HIER", StringComparison.CurrentCultureIgnoreCase));

            ato = alterTable.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ato.OptionType == EAlterTableOptionType.RenameTable);
            Assert.IsTrue(ato.NewTableName.ToString().Equals("EVRST_HIST_NK..CLC_D_ACCOUNT_STATUS_HIER_ARCH", StringComparison.CurrentCultureIgnoreCase));


        }
    }
}
