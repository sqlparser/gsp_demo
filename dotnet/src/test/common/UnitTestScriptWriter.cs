using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace gudusoft.gsqlparser.test
{
    [TestClass]
    public class UnitTestScriptWriter
    {
        TGSqlParser parser;
        public UnitTestScriptWriter()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvoracle);
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
        public virtual void testScriptWriterCrossApply()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT d.department_name, v.employee_id, v.last_name\n" + "  FROM departments d CROSS APPLY (SELECT * FROM employees e\n" + "                                  WHERE e.department_id = d.department_id) v";

            sqlparser.parse();
            
            //Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
        }

    }
}
