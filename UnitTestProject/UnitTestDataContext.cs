﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sys.Data.Linq;
using UnitTestProject.Northwind;
using System.Diagnostics;
using System.Linq;

namespace UnitTestProject
{
    /// <summary>
    /// Summary description for UnitTestDataContext
    /// </summary>
    [TestClass]
    public class UnitTestDataContext
    {
        string connectionString;
        public UnitTestDataContext()
        {
            if (Environment.MachineName.StartsWith("XPS"))
            {
                connectionString = "data source=localhost\\sqlexpress;initial catalog=Northwind;integrated security=SSPI;packet size=4096";
            }
            else
            {
                connectionString = "Server = (LocalDB)\\MSSQLLocalDB;initial catalog=Northwind;Integrated Security = true;";
            }
        }

        private TestContext testContextInstance;



        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethodSelectIQueryable()
        {
            using (var db = new DataContext(connectionString))
            {
                int id = 1000;
                string name = "Grandma's Boysenberry Spread";
                var table = db.GetTable<Products>();
                var rows = table.Select(row => row.ProductID < id && row.ProductName == name);

                Debug.Assert(rows.First(row => row.ProductID == 6).ProductName == "Grandma's Boysenberry Spread");
            }
        }


        [TestMethod]
        public void TestMethodSelect()
        {
            using (var db = new DataContext(connectionString))
            {
                var table = db.GetTable<Products>();
                var rows = table.Select("ProductID < 1000");

                Debug.Assert(rows.First(row => row.ProductID == 6).ProductName == "Grandma's Boysenberry Spread");
            }
        }

        [TestMethod]
        public void TestMethodInsert()
        {
            using (var db = new DataContext(connectionString))
            {
                var table = db.GetTable<Products>();
                Products product = new Products
                {
                    ProductID = 100,    //identity
                    ProductName = "iPhone"
                };

                table.InsertOnSubmit(product);
                string SQL = db.GetScript();
                Debug.Assert(SQL.StartsWith("INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES('iPhone',0,0,0,0,0,0,0)"));
            }
        }

        [TestMethod]
        public void TestMethodUpdate()
        {
            using (var db = new DataContext(connectionString))
            {
                var table = db.GetTable<Products>();
                var product = new
                {
                    ProductID = 100,
                    ProductName = "iPhone"
                };

                table.UpdateOnSubmit(product);
                string SQL = db.GetScript();
                Debug.Assert(SQL.StartsWith("UPDATE [Products] SET [ProductName] = 'iPhone' WHERE [ProductID] = 100"));
            }
        }

        [TestMethod]
        public void TestMethodInsertOrUpdate()
        {
            using (var db = new DataContext(connectionString))
            {
                var table = db.GetTable<Products>();
                var product = new Products
                {
                    ProductID = 100,
                    ProductName = "iPhone"
                };

                table.InsertOrUpdateOnSubmit(product);
                string SQL = db.GetScript();
                Debug.Assert(SQL.StartsWith("IF EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 100) UPDATE [Products] SET [ProductName] = 'iPhone',[SupplierID] = 0,[CategoryID] = 0,[QuantityPerUnit] = NULL,[UnitPrice] = 0,[UnitsInStock] = 0,[UnitsOnOrder] = 0,[ReorderLevel] = 0,[Discontinued] = 0 WHERE [ProductID] = 100 ELSE INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES('iPhone',0,0,0,0,0,0,0)"));
            }
        }

        [TestMethod]
        public void TestMethodDelete()
        {
            using (var db = new DataContext(connectionString))
            {
                var table = db.GetTable<Products>();
                var product = new Products
                {
                    ProductID = 100,
                    ProductName = "iPhone"
                };

                table.DeleteOnSubmit(product);
                string SQL = db.GetScript();
                Debug.Assert(SQL.StartsWith("DELETE FROM [Products] WHERE [ProductID] = 100"));
            }
        }
    }
}
