using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind
{
	public partial class Orders
	{
		public int OrderID { get; set; }
		public string CustomerID { get; set; }
		public int EmployeeID { get; set; }
		public DateTime OrderDate { get; set; }
		public DateTime RequiredDate { get; set; }
		public DateTime ShippedDate { get; set; }
		public int ShipVia { get; set; }
		public decimal Freight { get; set; }
		public string ShipName { get; set; }
		public string ShipAddress { get; set; }
		public string ShipCity { get; set; }
		public string ShipRegion { get; set; }
		public string ShipPostalCode { get; set; }
		public string ShipCountry { get; set; }
	}
	
	public static class OrdersExtension
	{
		public const string TableName = "Orders";
		public static readonly string[] Keys = new string[] { _ORDERID };
		public static readonly string[] Identity = new string[] { _ORDERID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<Order_Details>
			{
				ThisKey = _ORDERID,
				OtherKey = Order_DetailsExtension._ORDERID
			},
			new Association<Customers>
			{
				Name = "FK_Orders_Customers",
				ThisKey = _CUSTOMERID,
				OtherKey = CustomersExtension._CUSTOMERID,
				IsForeignKey = true
			},
			new Association<Employees>
			{
				Name = "FK_Orders_Employees",
				ThisKey = _EMPLOYEEID,
				OtherKey = EmployeesExtension._EMPLOYEEID,
				IsForeignKey = true
			},
			new Association<Shippers>
			{
				Name = "FK_Orders_Shippers",
				ThisKey = _SHIPVIA,
				OtherKey = ShippersExtension._SHIPPERID,
				IsForeignKey = true
			}
		};
		
		public static List<Orders> ToOrdersCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Orders NewObject(DataRow row)
		{
			return new Orders
			{
				OrderID = row.GetField<int>(_ORDERID),
				CustomerID = row.GetField<string>(_CUSTOMERID),
				EmployeeID = row.GetField<int>(_EMPLOYEEID),
				OrderDate = row.GetField<DateTime>(_ORDERDATE),
				RequiredDate = row.GetField<DateTime>(_REQUIREDDATE),
				ShippedDate = row.GetField<DateTime>(_SHIPPEDDATE),
				ShipVia = row.GetField<int>(_SHIPVIA),
				Freight = row.GetField<decimal>(_FREIGHT),
				ShipName = row.GetField<string>(_SHIPNAME),
				ShipAddress = row.GetField<string>(_SHIPADDRESS),
				ShipCity = row.GetField<string>(_SHIPCITY),
				ShipRegion = row.GetField<string>(_SHIPREGION),
				ShipPostalCode = row.GetField<string>(_SHIPPOSTALCODE),
				ShipCountry = row.GetField<string>(_SHIPCOUNTRY)
			};
		}
		
		public static void FillObject(this Orders item, DataRow row)
		{
			item.OrderID = row.GetField<int>(_ORDERID);
			item.CustomerID = row.GetField<string>(_CUSTOMERID);
			item.EmployeeID = row.GetField<int>(_EMPLOYEEID);
			item.OrderDate = row.GetField<DateTime>(_ORDERDATE);
			item.RequiredDate = row.GetField<DateTime>(_REQUIREDDATE);
			item.ShippedDate = row.GetField<DateTime>(_SHIPPEDDATE);
			item.ShipVia = row.GetField<int>(_SHIPVIA);
			item.Freight = row.GetField<decimal>(_FREIGHT);
			item.ShipName = row.GetField<string>(_SHIPNAME);
			item.ShipAddress = row.GetField<string>(_SHIPADDRESS);
			item.ShipCity = row.GetField<string>(_SHIPCITY);
			item.ShipRegion = row.GetField<string>(_SHIPREGION);
			item.ShipPostalCode = row.GetField<string>(_SHIPPOSTALCODE);
			item.ShipCountry = row.GetField<string>(_SHIPCOUNTRY);
		}
		
		public static void UpdateRow(this Orders item, DataRow row)
		{
			row.SetField(_ORDERID, item.OrderID);
			row.SetField(_CUSTOMERID, item.CustomerID);
			row.SetField(_EMPLOYEEID, item.EmployeeID);
			row.SetField(_ORDERDATE, item.OrderDate);
			row.SetField(_REQUIREDDATE, item.RequiredDate);
			row.SetField(_SHIPPEDDATE, item.ShippedDate);
			row.SetField(_SHIPVIA, item.ShipVia);
			row.SetField(_FREIGHT, item.Freight);
			row.SetField(_SHIPNAME, item.ShipName);
			row.SetField(_SHIPADDRESS, item.ShipAddress);
			row.SetField(_SHIPCITY, item.ShipCity);
			row.SetField(_SHIPREGION, item.ShipRegion);
			row.SetField(_SHIPPOSTALCODE, item.ShipPostalCode);
			row.SetField(_SHIPCOUNTRY, item.ShipCountry);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_ORDERID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_CUSTOMERID, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_EMPLOYEEID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_ORDERDATE, typeof(System.DateTime)));
			dt.Columns.Add(new DataColumn(_REQUIREDDATE, typeof(System.DateTime)));
			dt.Columns.Add(new DataColumn(_SHIPPEDDATE, typeof(System.DateTime)));
			dt.Columns.Add(new DataColumn(_SHIPVIA, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_FREIGHT, typeof(System.Decimal)));
			dt.Columns.Add(new DataColumn(_SHIPNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_SHIPADDRESS, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_SHIPCITY, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_SHIPREGION, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_SHIPPOSTALCODE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_SHIPCOUNTRY, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Orders> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Orders> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Orders item)
		{
			return new Dictionary<string,object>() 
			{
				[_ORDERID] = item.OrderID,
				[_CUSTOMERID] = item.CustomerID,
				[_EMPLOYEEID] = item.EmployeeID,
				[_ORDERDATE] = item.OrderDate,
				[_REQUIREDDATE] = item.RequiredDate,
				[_SHIPPEDDATE] = item.ShippedDate,
				[_SHIPVIA] = item.ShipVia,
				[_FREIGHT] = item.Freight,
				[_SHIPNAME] = item.ShipName,
				[_SHIPADDRESS] = item.ShipAddress,
				[_SHIPCITY] = item.ShipCity,
				[_SHIPREGION] = item.ShipRegion,
				[_SHIPPOSTALCODE] = item.ShipPostalCode,
				[_SHIPCOUNTRY] = item.ShipCountry
			};
		}
		
		public static Orders FromDictionary(this IDictionary<string, object> dict)
		{
			return new Orders
			{
				OrderID = (int)dict[_ORDERID],
				CustomerID = (string)dict[_CUSTOMERID],
				EmployeeID = (int)dict[_EMPLOYEEID],
				OrderDate = (DateTime)dict[_ORDERDATE],
				RequiredDate = (DateTime)dict[_REQUIREDDATE],
				ShippedDate = (DateTime)dict[_SHIPPEDDATE],
				ShipVia = (int)dict[_SHIPVIA],
				Freight = (decimal)dict[_FREIGHT],
				ShipName = (string)dict[_SHIPNAME],
				ShipAddress = (string)dict[_SHIPADDRESS],
				ShipCity = (string)dict[_SHIPCITY],
				ShipRegion = (string)dict[_SHIPREGION],
				ShipPostalCode = (string)dict[_SHIPPOSTALCODE],
				ShipCountry = (string)dict[_SHIPCOUNTRY]
			};
		}
		
		public static bool CompareTo(this Orders a, Orders b)
		{
			return a.OrderID == b.OrderID
			&& a.CustomerID == b.CustomerID
			&& a.EmployeeID == b.EmployeeID
			&& a.OrderDate == b.OrderDate
			&& a.RequiredDate == b.RequiredDate
			&& a.ShippedDate == b.ShippedDate
			&& a.ShipVia == b.ShipVia
			&& a.Freight == b.Freight
			&& a.ShipName == b.ShipName
			&& a.ShipAddress == b.ShipAddress
			&& a.ShipCity == b.ShipCity
			&& a.ShipRegion == b.ShipRegion
			&& a.ShipPostalCode == b.ShipPostalCode
			&& a.ShipCountry == b.ShipCountry;
		}
		
		public static void CopyTo(this Orders from, Orders to)
		{
			to.OrderID = from.OrderID;
			to.CustomerID = from.CustomerID;
			to.EmployeeID = from.EmployeeID;
			to.OrderDate = from.OrderDate;
			to.RequiredDate = from.RequiredDate;
			to.ShippedDate = from.ShippedDate;
			to.ShipVia = from.ShipVia;
			to.Freight = from.Freight;
			to.ShipName = from.ShipName;
			to.ShipAddress = from.ShipAddress;
			to.ShipCity = from.ShipCity;
			to.ShipRegion = from.ShipRegion;
			to.ShipPostalCode = from.ShipPostalCode;
			to.ShipCountry = from.ShipCountry;
		}
		
		public static string ToSimpleString(this Orders obj)
		{
			return string.Format("{{OrderID:{0}, CustomerID:{1}, EmployeeID:{2}, OrderDate:{3}, RequiredDate:{4}, ShippedDate:{5}, ShipVia:{6}, Freight:{7}, ShipName:{8}, ShipAddress:{9}, ShipCity:{10}, ShipRegion:{11}, ShipPostalCode:{12}, ShipCountry:{13}}}", 
			obj.OrderID, 
			obj.CustomerID, 
			obj.EmployeeID, 
			obj.OrderDate, 
			obj.RequiredDate, 
			obj.ShippedDate, 
			obj.ShipVia, 
			obj.Freight, 
			obj.ShipName, 
			obj.ShipAddress, 
			obj.ShipCity, 
			obj.ShipRegion, 
			obj.ShipPostalCode, 
			obj.ShipCountry);
		}
		
		public const string _ORDERID = "OrderID";
		public const string _CUSTOMERID = "CustomerID";
		public const string _EMPLOYEEID = "EmployeeID";
		public const string _ORDERDATE = "OrderDate";
		public const string _REQUIREDDATE = "RequiredDate";
		public const string _SHIPPEDDATE = "ShippedDate";
		public const string _SHIPVIA = "ShipVia";
		public const string _FREIGHT = "Freight";
		public const string _SHIPNAME = "ShipName";
		public const string _SHIPADDRESS = "ShipAddress";
		public const string _SHIPCITY = "ShipCity";
		public const string _SHIPREGION = "ShipRegion";
		public const string _SHIPPOSTALCODE = "ShipPostalCode";
		public const string _SHIPCOUNTRY = "ShipCountry";
	}
}