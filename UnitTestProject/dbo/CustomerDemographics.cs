using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind
{
	public partial class CustomerDemographics
	{
		public string CustomerTypeID { get; set; }
		public string CustomerDesc { get; set; }
	}
	
	public static class CustomerDemographicsExtension
	{
		public const string TableName = "CustomerDemographics";
		public static readonly string[] Keys = new string[] { _CUSTOMERTYPEID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<CustomerCustomerDemo>
			{
				ThisKey = _CUSTOMERTYPEID,
				OtherKey = CustomerCustomerDemoExtension._CUSTOMERTYPEID
			}
		};
		
		public static List<CustomerDemographics> ToCustomerDemographicsCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static CustomerDemographics NewObject(DataRow row)
		{
			return new CustomerDemographics
			{
				CustomerTypeID = row.GetField<string>(_CUSTOMERTYPEID),
				CustomerDesc = row.GetField<string>(_CUSTOMERDESC)
			};
		}
		
		public static void FillObject(this CustomerDemographics item, DataRow row)
		{
			item.CustomerTypeID = row.GetField<string>(_CUSTOMERTYPEID);
			item.CustomerDesc = row.GetField<string>(_CUSTOMERDESC);
		}
		
		public static void UpdateRow(this CustomerDemographics item, DataRow row)
		{
			row.SetField(_CUSTOMERTYPEID, item.CustomerTypeID);
			row.SetField(_CUSTOMERDESC, item.CustomerDesc);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_CUSTOMERTYPEID, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_CUSTOMERDESC, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<CustomerDemographics> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<CustomerDemographics> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this CustomerDemographics item)
		{
			return new Dictionary<string,object>() 
			{
				[_CUSTOMERTYPEID] = item.CustomerTypeID,
				[_CUSTOMERDESC] = item.CustomerDesc
			};
		}
		
		public static CustomerDemographics FromDictionary(this IDictionary<string, object> dict)
		{
			return new CustomerDemographics
			{
				CustomerTypeID = (string)dict[_CUSTOMERTYPEID],
				CustomerDesc = (string)dict[_CUSTOMERDESC]
			};
		}
		
		public static bool CompareTo(this CustomerDemographics a, CustomerDemographics b)
		{
			return a.CustomerTypeID == b.CustomerTypeID
			&& a.CustomerDesc == b.CustomerDesc;
		}
		
		public static void CopyTo(this CustomerDemographics from, CustomerDemographics to)
		{
			to.CustomerTypeID = from.CustomerTypeID;
			to.CustomerDesc = from.CustomerDesc;
		}
		
		public static string ToSimpleString(this CustomerDemographics obj)
		{
			return string.Format("{{CustomerTypeID:{0}, CustomerDesc:{1}}}", 
			obj.CustomerTypeID, 
			obj.CustomerDesc);
		}
		
		public const string _CUSTOMERTYPEID = "CustomerTypeID";
		public const string _CUSTOMERDESC = "CustomerDesc";
	}
}