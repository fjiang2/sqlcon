using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;

namespace UnitTestProject.Northwind
{
	public partial class Products
	{
		public int ProductID { get; set; }
		public string ProductName { get; set; }
		public int SupplierID { get; set; }
		public int CategoryID { get; set; }
		public string QuantityPerUnit { get; set; }
		public decimal UnitPrice { get; set; }
		public short UnitsInStock { get; set; }
		public short UnitsOnOrder { get; set; }
		public short ReorderLevel { get; set; }
		public bool Discontinued { get; set; }
	}
	
	public static class ProductsExtension
	{
		public const string TableName = "Products";
		public static readonly string[] Keys = new string[] { _PRODUCTID };
		public static readonly string[] Identity = new string[] { _PRODUCTID };
		
		public static List<Products> ToProductsCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Products NewObject(DataRow row)
		{
			return new Products
			{
				ProductID = row.GetField<int>(_PRODUCTID),
				ProductName = row.GetField<string>(_PRODUCTNAME),
				SupplierID = row.GetField<int>(_SUPPLIERID),
				CategoryID = row.GetField<int>(_CATEGORYID),
				QuantityPerUnit = row.GetField<string>(_QUANTITYPERUNIT),
				UnitPrice = row.GetField<decimal>(_UNITPRICE),
				UnitsInStock = row.GetField<short>(_UNITSINSTOCK),
				UnitsOnOrder = row.GetField<short>(_UNITSONORDER),
				ReorderLevel = row.GetField<short>(_REORDERLEVEL),
				Discontinued = row.GetField<bool>(_DISCONTINUED)
			};
		}
		
		public static void FillObject(this Products item, DataRow row)
		{
			item.ProductID = row.GetField<int>(_PRODUCTID);
			item.ProductName = row.GetField<string>(_PRODUCTNAME);
			item.SupplierID = row.GetField<int>(_SUPPLIERID);
			item.CategoryID = row.GetField<int>(_CATEGORYID);
			item.QuantityPerUnit = row.GetField<string>(_QUANTITYPERUNIT);
			item.UnitPrice = row.GetField<decimal>(_UNITPRICE);
			item.UnitsInStock = row.GetField<short>(_UNITSINSTOCK);
			item.UnitsOnOrder = row.GetField<short>(_UNITSONORDER);
			item.ReorderLevel = row.GetField<short>(_REORDERLEVEL);
			item.Discontinued = row.GetField<bool>(_DISCONTINUED);
		}
		
		public static void UpdateRow(this Products item, DataRow row)
		{
			row.SetField(_PRODUCTID, item.ProductID);
			row.SetField(_PRODUCTNAME, item.ProductName);
			row.SetField(_SUPPLIERID, item.SupplierID);
			row.SetField(_CATEGORYID, item.CategoryID);
			row.SetField(_QUANTITYPERUNIT, item.QuantityPerUnit);
			row.SetField(_UNITPRICE, item.UnitPrice);
			row.SetField(_UNITSINSTOCK, item.UnitsInStock);
			row.SetField(_UNITSONORDER, item.UnitsOnOrder);
			row.SetField(_REORDERLEVEL, item.ReorderLevel);
			row.SetField(_DISCONTINUED, item.Discontinued);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_PRODUCTID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_PRODUCTNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_SUPPLIERID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_CATEGORYID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_QUANTITYPERUNIT, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_UNITPRICE, typeof(System.Decimal)));
			dt.Columns.Add(new DataColumn(_UNITSINSTOCK, typeof(System.Int16)));
			dt.Columns.Add(new DataColumn(_UNITSONORDER, typeof(System.Int16)));
			dt.Columns.Add(new DataColumn(_REORDERLEVEL, typeof(System.Int16)));
			dt.Columns.Add(new DataColumn(_DISCONTINUED, typeof(System.Boolean)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Products> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Products> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Products item)
		{
			return new Dictionary<string,object>() 
			{
				[_PRODUCTID] = item.ProductID,
				[_PRODUCTNAME] = item.ProductName,
				[_SUPPLIERID] = item.SupplierID,
				[_CATEGORYID] = item.CategoryID,
				[_QUANTITYPERUNIT] = item.QuantityPerUnit,
				[_UNITPRICE] = item.UnitPrice,
				[_UNITSINSTOCK] = item.UnitsInStock,
				[_UNITSONORDER] = item.UnitsOnOrder,
				[_REORDERLEVEL] = item.ReorderLevel,
				[_DISCONTINUED] = item.Discontinued
			};
		}
		
		public static Products FromDictionary(this IDictionary<string, object> dict)
		{
			return new Products
			{
				ProductID = (int)dict[_PRODUCTID],
				ProductName = (string)dict[_PRODUCTNAME],
				SupplierID = (int)dict[_SUPPLIERID],
				CategoryID = (int)dict[_CATEGORYID],
				QuantityPerUnit = (string)dict[_QUANTITYPERUNIT],
				UnitPrice = (decimal)dict[_UNITPRICE],
				UnitsInStock = (short)dict[_UNITSINSTOCK],
				UnitsOnOrder = (short)dict[_UNITSONORDER],
				ReorderLevel = (short)dict[_REORDERLEVEL],
				Discontinued = (bool)dict[_DISCONTINUED]
			};
		}
		
		public static bool CompareTo(this Products a, Products b)
		{
			return a.ProductID == b.ProductID
			&& a.ProductName == b.ProductName
			&& a.SupplierID == b.SupplierID
			&& a.CategoryID == b.CategoryID
			&& a.QuantityPerUnit == b.QuantityPerUnit
			&& a.UnitPrice == b.UnitPrice
			&& a.UnitsInStock == b.UnitsInStock
			&& a.UnitsOnOrder == b.UnitsOnOrder
			&& a.ReorderLevel == b.ReorderLevel
			&& a.Discontinued == b.Discontinued;
		}
		
		public static void CopyTo(this Products from, Products to)
		{
			to.ProductID = from.ProductID;
			to.ProductName = from.ProductName;
			to.SupplierID = from.SupplierID;
			to.CategoryID = from.CategoryID;
			to.QuantityPerUnit = from.QuantityPerUnit;
			to.UnitPrice = from.UnitPrice;
			to.UnitsInStock = from.UnitsInStock;
			to.UnitsOnOrder = from.UnitsOnOrder;
			to.ReorderLevel = from.ReorderLevel;
			to.Discontinued = from.Discontinued;
		}
		
		public static string ToSimpleString(this Products obj)
		{
			return string.Format("{{ProductID:{0}, ProductName:{1}, SupplierID:{2}, CategoryID:{3}, QuantityPerUnit:{4}, UnitPrice:{5}, UnitsInStock:{6}, UnitsOnOrder:{7}, ReorderLevel:{8}, Discontinued:{9}}}", 
			obj.ProductID, 
			obj.ProductName, 
			obj.SupplierID, 
			obj.CategoryID, 
			obj.QuantityPerUnit, 
			obj.UnitPrice, 
			obj.UnitsInStock, 
			obj.UnitsOnOrder, 
			obj.ReorderLevel, 
			obj.Discontinued);
		}
		
		public const string _PRODUCTID = "ProductID";
		public const string _PRODUCTNAME = "ProductName";
		public const string _SUPPLIERID = "SupplierID";
		public const string _CATEGORYID = "CategoryID";
		public const string _QUANTITYPERUNIT = "QuantityPerUnit";
		public const string _UNITPRICE = "UnitPrice";
		public const string _UNITSINSTOCK = "UnitsInStock";
		public const string _UNITSONORDER = "UnitsOnOrder";
		public const string _REORDERLEVEL = "ReorderLevel";
		public const string _DISCONTINUED = "Discontinued";
	}
}