using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Sys.Data;

namespace UnitTestProject
{
    [TestClass]
    public class SqlBuilderUnitTest
    {
        SqlExpr ProductId = "ProductId".ColumnName();
        string Products = "Products";
        string Categories = "Categories";

        public SqlBuilderUnitTest()
        {

        }

        [TestMethod]
        public void TOP_TestMethod()
        {
            string sql = "SELECT TOP 20 * FROM Products WHERE [ProductId] < 10";
            string query = new SqlBuilder().SELECT().TOP(20).COLUMNS().FROM(Products).WHERE(ProductId < 10).ToString();

            Debug.Assert(sql == query.Substring(0, sql.Length));
        }

        [TestMethod]
        public void IS_NULL_TestMethod()
        {
            string sql = "SELECT COUNT(*) FROM Products WHERE [ProductId] IS NULL";
            string query = new SqlBuilder().SELECT().COLUMNS(SqlExpr.COUNT).FROM(Products).WHERE(ProductId.IS_NULL()).ToString();

            Debug.Assert(sql == query.Substring(0, sql.Length));
        }

        [TestMethod]
        public void IS_NOT_NULL_TestMethod()
        {
            string sql = "SELECT COUNT(*) FROM Products WHERE [ProductId] IS NOT NULL";
            string query = new SqlBuilder().SELECT().COLUMNS(SqlExpr.COUNT).FROM(Products).WHERE(ProductId != null).ToString();

            Debug.Assert(sql == query.Substring(0, sql.Length));
        }

        [TestMethod]
        public void BETWEEN_TestMethod()
        {
            string sql = "SELECT COUNT(*) FROM Products WHERE [ProductId] BETWEEN 10 AND 30";
            string query = new SqlBuilder().SELECT().COLUMNS(SqlExpr.COUNT).FROM(Products).WHERE(ProductId.BETWEEN(10, 30)).ToString();

            Debug.Assert(sql == query.Substring(0, sql.Length));
        }

        [TestMethod]
        public void JOIN_TestMethod()
        {
            string sql = @"SELECT Categories.[CategoryName], Products.[ProductName], Products.[QuantityPerUnit], Products.[UnitsInStock], Products.[Discontinued] 
FROM Categories INNER JOIN Products ON Categories.[CategoryID] = Products.[CategoryID] 
WHERE Products.[Discontinued] <> 1 ";
            
            string query = new SqlBuilder()
                .SELECT()
                .COLUMNS(
                    "CategoryName".ColumnName(Categories), 
                    "ProductName".ColumnName(Products),
                    "QuantityPerUnit".ColumnName(Products),
                    "UnitsInStock".ColumnName(Products),
                    "Discontinued".ColumnName(Products)
                    )
                .AppendLine()
                .FROM(Categories)
                .INNER().JOIN(Products).ON("CategoryID".ColumnName(Categories) == "CategoryID".ColumnName(Products))
                .AppendLine()
                .WHERE("Discontinued".ColumnName(Products) != 1)
                .ToString();

            Debug.Assert(sql == query.Substring(0, sql.Length));
        }
    }
}
