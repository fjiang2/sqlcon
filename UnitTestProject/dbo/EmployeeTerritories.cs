using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class EmployeeTerritories
	{
		public int EmployeeID { get; set; }
		public string TerritoryID { get; set; }
	}
	
	public static class EmployeeTerritoriesExtension
	{
		public const string TableName = "EmployeeTerritories";
		public static readonly string[] Keys = new string[] { _EMPLOYEEID, _TERRITORYID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<Employees>
			{
				Name = "FK_EmployeeTerritories_Employees",
				ThisKey = _EMPLOYEEID,
				OtherKey = EmployeesExtension._EMPLOYEEID,
				IsForeignKey = true
			},
			new Association<Territories>
			{
				Name = "FK_EmployeeTerritories_Territories",
				ThisKey = _TERRITORYID,
				OtherKey = TerritoriesExtension._TERRITORYID,
				IsForeignKey = true
			}
		};
		
		public static List<EmployeeTerritories> ToEmployeeTerritoriesCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static EmployeeTerritories NewObject(DataRow row)
		{
			return new EmployeeTerritories
			{
				EmployeeID = row.GetField<int>(_EMPLOYEEID),
				TerritoryID = row.GetField<string>(_TERRITORYID)
			};
		}
		
		public static void FillObject(this EmployeeTerritories item, DataRow row)
		{
			item.EmployeeID = row.GetField<int>(_EMPLOYEEID);
			item.TerritoryID = row.GetField<string>(_TERRITORYID);
		}
		
		public static void UpdateRow(this EmployeeTerritories item, DataRow row)
		{
			row.SetField(_EMPLOYEEID, item.EmployeeID);
			row.SetField(_TERRITORYID, item.TerritoryID);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_EMPLOYEEID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_TERRITORYID, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<EmployeeTerritories> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<EmployeeTerritories> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this EmployeeTerritories item)
		{
			return new Dictionary<string,object>() 
			{
				[_EMPLOYEEID] = item.EmployeeID,
				[_TERRITORYID] = item.TerritoryID
			};
		}
		
		public static EmployeeTerritories FromDictionary(this IDictionary<string, object> dict)
		{
			return new EmployeeTerritories
			{
				EmployeeID = (int)dict[_EMPLOYEEID],
				TerritoryID = (string)dict[_TERRITORYID]
			};
		}
		
		public static bool CompareTo(this EmployeeTerritories a, EmployeeTerritories b)
		{
			return a.EmployeeID == b.EmployeeID
			&& a.TerritoryID == b.TerritoryID;
		}
		
		public static void CopyTo(this EmployeeTerritories from, EmployeeTerritories to)
		{
			to.EmployeeID = from.EmployeeID;
			to.TerritoryID = from.TerritoryID;
		}
		
		public static string ToSimpleString(this EmployeeTerritories obj)
		{
			return string.Format("{{EmployeeID:{0}, TerritoryID:{1}}}", 
			obj.EmployeeID, 
			obj.TerritoryID);
		}
		
		public const string _EMPLOYEEID = "EmployeeID";
		public const string _TERRITORYID = "TerritoryID";
	}
}