using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class CustomerCustomerDemo
	{
		public string CustomerID { get; set; }
		public string CustomerTypeID { get; set; }
	}
	
	public class CustomerCustomerDemoAssociation
	{
		public EntityRef<Customers> Customer { get; set; }
		public EntityRef<CustomerDemographics> CustomerDemographic { get; set; }
	}
	
	public static class CustomerCustomerDemoExtension
	{
		public const string TableName = "CustomerCustomerDemo";
		public static readonly string[] Keys = new string[] { _CUSTOMERID, _CUSTOMERTYPEID };
		
		public static readonly IConstraint[] Constraints = new IConstraint[]
		{
			new Constraint<Customers>
			{
				Name = "FK_CustomerCustomerDemo_Customers",
				ThisKey = _CUSTOMERID,
				OtherKey = CustomersExtension._CUSTOMERID,
				IsForeignKey = true
			},
			new Constraint<CustomerDemographics>
			{
				Name = "FK_CustomerCustomerDemo",
				ThisKey = _CUSTOMERTYPEID,
				OtherKey = CustomerDemographicsExtension._CUSTOMERTYPEID,
				IsForeignKey = true
			}
		};
		
		public static List<CustomerCustomerDemo> ToCustomerCustomerDemoCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static CustomerCustomerDemo NewObject(DataRow row)
		{
			return new CustomerCustomerDemo
			{
				CustomerID = row.GetField<string>(_CUSTOMERID),
				CustomerTypeID = row.GetField<string>(_CUSTOMERTYPEID)
			};
		}
		
		public static void FillObject(this CustomerCustomerDemo item, DataRow row)
		{
			item.CustomerID = row.GetField<string>(_CUSTOMERID);
			item.CustomerTypeID = row.GetField<string>(_CUSTOMERTYPEID);
		}
		
		public static void UpdateRow(this CustomerCustomerDemo item, DataRow row)
		{
			row.SetField(_CUSTOMERID, item.CustomerID);
			row.SetField(_CUSTOMERTYPEID, item.CustomerTypeID);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_CUSTOMERID, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_CUSTOMERTYPEID, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<CustomerCustomerDemo> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<CustomerCustomerDemo> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this CustomerCustomerDemo item)
		{
			return new Dictionary<string,object>() 
			{
				[_CUSTOMERID] = item.CustomerID,
				[_CUSTOMERTYPEID] = item.CustomerTypeID
			};
		}
		
		public static CustomerCustomerDemo FromDictionary(this IDictionary<string, object> dict)
		{
			return new CustomerCustomerDemo
			{
				CustomerID = (string)dict[_CUSTOMERID],
				CustomerTypeID = (string)dict[_CUSTOMERTYPEID]
			};
		}
		
		public static bool CompareTo(this CustomerCustomerDemo a, CustomerCustomerDemo b)
		{
			return a.CustomerID == b.CustomerID
			&& a.CustomerTypeID == b.CustomerTypeID;
		}
		
		public static void CopyTo(this CustomerCustomerDemo from, CustomerCustomerDemo to)
		{
			to.CustomerID = from.CustomerID;
			to.CustomerTypeID = from.CustomerTypeID;
		}
		
		public static string ToSimpleString(this CustomerCustomerDemo obj)
		{
			return string.Format("{{CustomerID:{0}, CustomerTypeID:{1}}}", 
			obj.CustomerID, 
			obj.CustomerTypeID);
		}
		
		public const string _CUSTOMERID = "CustomerID";
		public const string _CUSTOMERTYPEID = "CustomerTypeID";
		
		public static CustomerCustomerDemoAssociation GetAssociation(this CustomerCustomerDemo entity)
		{
			return entity.AsEnumerable().GetAssociation().FirstOrDefault();
		}
		
		public static IEnumerable<CustomerCustomerDemoAssociation> GetAssociation(this IEnumerable<CustomerCustomerDemo> entities)
		{
			var reader = entities.Expand();
			
			var associations = new List<CustomerCustomerDemoAssociation>();
			
			var _Customer = reader.Read<Customers>();
			var _CustomerDemographic = reader.Read<CustomerDemographics>();
			
			foreach (var entity in entities)
			{
				var association = new CustomerCustomerDemoAssociation
				{
					Customer = new EntityRef<Customers>(_Customer.FirstOrDefault(row => row.CustomerID == entity.CustomerID)),
					CustomerDemographic = new EntityRef<CustomerDemographics>(_CustomerDemographic.FirstOrDefault(row => row.CustomerTypeID == entity.CustomerTypeID)),
				};
				associations.Add(association);
			}
			
			return associations;
		}
	}
}