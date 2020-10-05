using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class Shippers
	{
		public int ShipperID { get; set; }
		public string CompanyName { get; set; }
		public string Phone { get; set; }
	}
	
	public static class ShippersExtension
	{
		public const string TableName = "Shippers";
		public static readonly string[] Keys = new string[] { _SHIPPERID };
		public static readonly string[] Identity = new string[] { _SHIPPERID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<Orders>
			{
				ThisKey = _SHIPPERID,
				OtherKey = OrdersExtension._SHIPVIA,
				OneToMany = true
			}
		};
		
		public static List<Shippers> ToShippersCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Shippers NewObject(DataRow row)
		{
			return new Shippers
			{
				ShipperID = row.GetField<int>(_SHIPPERID),
				CompanyName = row.GetField<string>(_COMPANYNAME),
				Phone = row.GetField<string>(_PHONE)
			};
		}
		
		public static void FillObject(this Shippers item, DataRow row)
		{
			item.ShipperID = row.GetField<int>(_SHIPPERID);
			item.CompanyName = row.GetField<string>(_COMPANYNAME);
			item.Phone = row.GetField<string>(_PHONE);
		}
		
		public static void UpdateRow(this Shippers item, DataRow row)
		{
			row.SetField(_SHIPPERID, item.ShipperID);
			row.SetField(_COMPANYNAME, item.CompanyName);
			row.SetField(_PHONE, item.Phone);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_SHIPPERID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_COMPANYNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_PHONE, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Shippers> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Shippers> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Shippers item)
		{
			return new Dictionary<string,object>() 
			{
				[_SHIPPERID] = item.ShipperID,
				[_COMPANYNAME] = item.CompanyName,
				[_PHONE] = item.Phone
			};
		}
		
		public static Shippers FromDictionary(this IDictionary<string, object> dict)
		{
			return new Shippers
			{
				ShipperID = (int)dict[_SHIPPERID],
				CompanyName = (string)dict[_COMPANYNAME],
				Phone = (string)dict[_PHONE]
			};
		}
		
		public static bool CompareTo(this Shippers a, Shippers b)
		{
			return a.ShipperID == b.ShipperID
			&& a.CompanyName == b.CompanyName
			&& a.Phone == b.Phone;
		}
		
		public static void CopyTo(this Shippers from, Shippers to)
		{
			to.ShipperID = from.ShipperID;
			to.CompanyName = from.CompanyName;
			to.Phone = from.Phone;
		}
		
		public static string ToSimpleString(this Shippers obj)
		{
			return string.Format("{{ShipperID:{0}, CompanyName:{1}, Phone:{2}}}", 
			obj.ShipperID, 
			obj.CompanyName, 
			obj.Phone);
		}
		
		public const string _SHIPPERID = "ShipperID";
		public const string _COMPANYNAME = "CompanyName";
		public const string _PHONE = "Phone";
	}
}