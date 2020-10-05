using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class Categories
	{
		public int CategoryID { get; set; }
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public byte[] Picture { get; set; }
	}
	
	public static class CategoriesExtension
	{
		public const string TableName = "Categories";
		public static readonly string[] Keys = new string[] { _CATEGORYID };
		public static readonly string[] Identity = new string[] { _CATEGORYID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<Products>
			{
				ThisKey = _CATEGORYID,
				OtherKey = ProductsExtension._CATEGORYID
			}
		};
		
		public static List<Categories> ToCategoriesCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Categories NewObject(DataRow row)
		{
			return new Categories
			{
				CategoryID = row.GetField<int>(_CATEGORYID),
				CategoryName = row.GetField<string>(_CATEGORYNAME),
				Description = row.GetField<string>(_DESCRIPTION),
				Picture = row.GetField<byte[]>(_PICTURE)
			};
		}
		
		public static void FillObject(this Categories item, DataRow row)
		{
			item.CategoryID = row.GetField<int>(_CATEGORYID);
			item.CategoryName = row.GetField<string>(_CATEGORYNAME);
			item.Description = row.GetField<string>(_DESCRIPTION);
			item.Picture = row.GetField<byte[]>(_PICTURE);
		}
		
		public static void UpdateRow(this Categories item, DataRow row)
		{
			row.SetField(_CATEGORYID, item.CategoryID);
			row.SetField(_CATEGORYNAME, item.CategoryName);
			row.SetField(_DESCRIPTION, item.Description);
			row.SetField(_PICTURE, item.Picture);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_CATEGORYID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_CATEGORYNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_DESCRIPTION, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_PICTURE, typeof(System.Byte[])));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Categories> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Categories> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Categories item)
		{
			return new Dictionary<string,object>() 
			{
				[_CATEGORYID] = item.CategoryID,
				[_CATEGORYNAME] = item.CategoryName,
				[_DESCRIPTION] = item.Description,
				[_PICTURE] = item.Picture
			};
		}
		
		public static Categories FromDictionary(this IDictionary<string, object> dict)
		{
			return new Categories
			{
				CategoryID = (int)dict[_CATEGORYID],
				CategoryName = (string)dict[_CATEGORYNAME],
				Description = (string)dict[_DESCRIPTION],
				Picture = (byte[])dict[_PICTURE]
			};
		}
		
		public static bool CompareTo(this Categories a, Categories b)
		{
			return a.CategoryID == b.CategoryID
			&& a.CategoryName == b.CategoryName
			&& a.Description == b.Description
			&& a.Picture == b.Picture;
		}
		
		public static void CopyTo(this Categories from, Categories to)
		{
			to.CategoryID = from.CategoryID;
			to.CategoryName = from.CategoryName;
			to.Description = from.Description;
			to.Picture = from.Picture;
		}
		
		public static string ToSimpleString(this Categories obj)
		{
			return string.Format("{{CategoryID:{0}, CategoryName:{1}, Description:{2}, Picture:{3}}}", 
			obj.CategoryID, 
			obj.CategoryName, 
			obj.Description, 
			obj.Picture);
		}
		
		public const string _CATEGORYID = "CategoryID";
		public const string _CATEGORYNAME = "CategoryName";
		public const string _DESCRIPTION = "Description";
		public const string _PICTURE = "Picture";
	}
}