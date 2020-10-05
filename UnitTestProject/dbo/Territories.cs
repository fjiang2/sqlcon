using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class Territories
	{
		public string TerritoryID { get; set; }
		public string TerritoryDescription { get; set; }
		public int RegionID { get; set; }
	}
	
	public static class TerritoriesExtension
	{
		public const string TableName = "Territories";
		public static readonly string[] Keys = new string[] { _TERRITORYID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<EmployeeTerritories>
			{
				ThisKey = _TERRITORYID,
				OtherKey = EmployeeTerritoriesExtension._TERRITORYID
			},
			new Association<Region>
			{
				Name = "FK_Territories_Region",
				ThisKey = _REGIONID,
				OtherKey = RegionExtension._REGIONID,
				IsForeignKey = true
			}
		};
		
		public static List<Territories> ToTerritoriesCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Territories NewObject(DataRow row)
		{
			return new Territories
			{
				TerritoryID = row.GetField<string>(_TERRITORYID),
				TerritoryDescription = row.GetField<string>(_TERRITORYDESCRIPTION),
				RegionID = row.GetField<int>(_REGIONID)
			};
		}
		
		public static void FillObject(this Territories item, DataRow row)
		{
			item.TerritoryID = row.GetField<string>(_TERRITORYID);
			item.TerritoryDescription = row.GetField<string>(_TERRITORYDESCRIPTION);
			item.RegionID = row.GetField<int>(_REGIONID);
		}
		
		public static void UpdateRow(this Territories item, DataRow row)
		{
			row.SetField(_TERRITORYID, item.TerritoryID);
			row.SetField(_TERRITORYDESCRIPTION, item.TerritoryDescription);
			row.SetField(_REGIONID, item.RegionID);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_TERRITORYID, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_TERRITORYDESCRIPTION, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_REGIONID, typeof(System.Int32)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Territories> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Territories> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Territories item)
		{
			return new Dictionary<string,object>() 
			{
				[_TERRITORYID] = item.TerritoryID,
				[_TERRITORYDESCRIPTION] = item.TerritoryDescription,
				[_REGIONID] = item.RegionID
			};
		}
		
		public static Territories FromDictionary(this IDictionary<string, object> dict)
		{
			return new Territories
			{
				TerritoryID = (string)dict[_TERRITORYID],
				TerritoryDescription = (string)dict[_TERRITORYDESCRIPTION],
				RegionID = (int)dict[_REGIONID]
			};
		}
		
		public static bool CompareTo(this Territories a, Territories b)
		{
			return a.TerritoryID == b.TerritoryID
			&& a.TerritoryDescription == b.TerritoryDescription
			&& a.RegionID == b.RegionID;
		}
		
		public static void CopyTo(this Territories from, Territories to)
		{
			to.TerritoryID = from.TerritoryID;
			to.TerritoryDescription = from.TerritoryDescription;
			to.RegionID = from.RegionID;
		}
		
		public static string ToSimpleString(this Territories obj)
		{
			return string.Format("{{TerritoryID:{0}, TerritoryDescription:{1}, RegionID:{2}}}", 
			obj.TerritoryID, 
			obj.TerritoryDescription, 
			obj.RegionID);
		}
		
		public const string _TERRITORYID = "TerritoryID";
		public const string _TERRITORYDESCRIPTION = "TerritoryDescription";
		public const string _REGIONID = "RegionID";
	}
}