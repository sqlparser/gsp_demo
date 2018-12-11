using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser.demos.tableColumnRename;

namespace gudusoft.gsqlparser.test.gettablecolumns
{
    [TestClass()]
    public class testTableColumnRename
    {
        [TestMethod()]
        public void testRenameTableColumn()
        {
            string text = "CREATE PROCEDURE [dbo].[Testprocedure_2]\n" + "        @BusinessID NVARCHAR(100)\n" + " AS\n" + " BEGIN\n" + "   SET NOCOUNT  ON;\n" + "   SELECT dbo.tb_Rentals.*,\n" + "          MinimalRentalID,\n" + "          SEA.Name,\n" + "          SEA.BeginDay,\n" + "          SEA.EndDay,\n" + "          dbo.tb_RentalTypes.Name AS TypeName\n" + "   FROM   dbo.tb_Rentals,\n" + "          dbo.tb_Seasons SEA,\n" + "          dbo.tb_RentalTypes,\n" + "          dbo.tb_RentalToSeason\n" + "   WHERE  dbo.tb_Rentals.BusinessID_XXX = SEA.BusinessID \n" + "          AND dbo.tb_Rentals.RentalTypeID = dbo.tb_RentalTypes.RentalTypeID\n" + "          AND dbo.tb_RentalToSeason.RentalID = dbo.tb_Rentals.RentalID\n" + "          AND dbo.tb_RentalToSeason.SeasonID = SEA.SeasonID\n" + "          AND dbo.tb_Rentals.BusinessID = @BusinessID \n" + "          AND @BusinessID IN (SELECT DISTINCT dbo.tb_Rentals.BusinessID_XXX\n" + "                              FROM   dbo.tb_Rentals\n" + "                              WHERE  dbo.tb_Rentals.BusinessID = @BusinessID)\n" + " END";
            IDictionary<string, IList<string>> metaInfo = new Dictionary<string, IList<string>>();
            metaInfo["dbo.tb_Seasons".ToLower()] = new List<string>(new string[] { "MinimalRentalID".ToLower() });
            tableColumnRename ro = new  tableColumnRename(EDbVendor.dbvmssql, text, metaInfo);
            ro.renameColumn("dbo.tb_Seasons.MinimalRentalID", "MinimalRentalID_xx");
            string result = ro.ModifiedText;
            string renamedSql = "CREATE PROCEDURE [dbo].[Testprocedure_2]\n" +
                                "        @BusinessID NVARCHAR(100)\n" +
                                " AS\n" +
                                " BEGIN\n" +
                                "   SET NOCOUNT  ON;\n" +
                                "   SELECT dbo.tb_Rentals.*,\n" +
                                "          MinimalRentalID_xx,\n" +
                                "          SEA.Name,\n" +
                                "          SEA.BeginDay,\n" +
                                "          SEA.EndDay,\n" +
                                "          dbo.tb_RentalTypes.Name AS TypeName\n" +
                                "   FROM   dbo.tb_Rentals,\n" +
                                "          dbo.tb_Seasons SEA,\n" +
                                "          dbo.tb_RentalTypes,\n" +
                                "          dbo.tb_RentalToSeason\n" +
                                "   WHERE  dbo.tb_Rentals.BusinessID_XXX = SEA.BusinessID \n" +
                                "          AND dbo.tb_Rentals.RentalTypeID = dbo.tb_RentalTypes.RentalTypeID\n" +
                                "          AND dbo.tb_RentalToSeason.RentalID = dbo.tb_Rentals.RentalID\n" +
                                "          AND dbo.tb_RentalToSeason.SeasonID = SEA.SeasonID\n" +
                                "          AND dbo.tb_Rentals.BusinessID = @BusinessID \n" +
                                "          AND @BusinessID IN (SELECT DISTINCT dbo.tb_Rentals.BusinessID_XXX\n" +
                                "                              FROM   dbo.tb_Rentals\n" +
                                "                              WHERE  dbo.tb_Rentals.BusinessID = @BusinessID)\n" +
                                " END";
            Assert.IsTrue(result.Equals(renamedSql));
        }
    }
}
