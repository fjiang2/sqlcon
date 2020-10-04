using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind
{
	public partial class Region
	{
		public int RegionID { get; set; }
		public string RegionDescription { get; set; }
	}
	
	public static class RegionExtension
	{
		public const string TableName = "Region";
		public static readonly string[] Keys = new string[] { _REGIONID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<Territories>
			{
				ThisKey = _REGIONID,
				OtherKey = TerritoriesExtension._REGIONID
			}
		};
		
		public static List<Region> ToRegionCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Region NewObject(DataRow row)
		{
			return new Region
			{
				RegionID = row.GetField<int>(_REGIONID),
				RegionDescription = row.GetField<string>(_REGIONDESCRIPTION)
			};
		}
		
		public static void FillObject(this Region item, DataRow row)
		{
			item.RegionID = row.GetField<int>(_REGIONID);
			item.RegionDescription = row.GetField<string>(_REGIONDESCRIPTION);
		}
		
		public static void UpdateRow(this Region item, DataRow row)
		{
			row.SetField(_REGIONID, item.RegionID);
			row.SetField(_REGIONDESCRIPTION, item.RegionDescription);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_REGIONID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_REGIONDESCRIPTION, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Region> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Region> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Region item)
		{
			return new Dictionary<string,object>() 
			{
				[_REGIONID] = item.RegionID,
				[_REGIONDESCRIPTION] = item.RegionDescription
			};
		}
		
		public static Region FromDictionary(this IDictionary<string, object> dict)
		{
			return new Region
			{
				RegionID = (int)dict[_REGIONID],
				RegionDescription = (string)dict[_REGIONDESCRIPTION]
			};
		}
		
		public static bool CompareTo(this Region a, Region b)
		{
			return a.RegionID == b.RegionID
			&& a.RegionDescription == b.RegionDescription;
		}
		
		public static void CopyTo(this Region from, Region to)
		{
			to.RegionID = from.RegionID;
			to.RegionDescription = from.RegionDescription;
		}
		
		public static string ToSimpleString(this Region obj)
		{
			return string.Format("{{RegionID:{0}, RegionDescription:{1}}}", 
			obj.RegionID, 
			obj.RegionDescription);
		}
		
		public const string _REGIONID = "RegionID";
		public const string _REGIONDESCRIPTION = "RegionDescription";
	}
}