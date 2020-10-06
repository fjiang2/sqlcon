using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class Customers
	{
		public string CustomerID { get; set; }
		public string CompanyName { get; set; }
		public string ContactName { get; set; }
		public string ContactTitle { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public string Phone { get; set; }
		public string Fax { get; set; }
	}
	
	public static class CustomersExtension
	{
		public const string TableName = "Customers";
		public static readonly string[] Keys = new string[] { _CUSTOMERID };
		
		public static readonly IAssociation[] Associations = new IAssociation[]
		{
			new Association<CustomerCustomerDemo>
			{
				ThisKey = _CUSTOMERID,
				OtherKey = CustomerCustomerDemoExtension._CUSTOMERID,
				OneToMany = true
			},
			new Association<Orders>
			{
				ThisKey = _CUSTOMERID,
				OtherKey = OrdersExtension._CUSTOMERID,
				OneToMany = true
			}
		};
		
		public static List<Customers> ToCustomersCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Customers NewObject(DataRow row)
		{
			return new Customers
			{
				CustomerID = row.GetField<string>(_CUSTOMERID),
				CompanyName = row.GetField<string>(_COMPANYNAME),
				ContactName = row.GetField<string>(_CONTACTNAME),
				ContactTitle = row.GetField<string>(_CONTACTTITLE),
				Address = row.GetField<string>(_ADDRESS),
				City = row.GetField<string>(_CITY),
				Region = row.GetField<string>(_REGION),
				PostalCode = row.GetField<string>(_POSTALCODE),
				Country = row.GetField<string>(_COUNTRY),
				Phone = row.GetField<string>(_PHONE),
				Fax = row.GetField<string>(_FAX)
			};
		}
		
		public static void FillObject(this Customers item, DataRow row)
		{
			item.CustomerID = row.GetField<string>(_CUSTOMERID);
			item.CompanyName = row.GetField<string>(_COMPANYNAME);
			item.ContactName = row.GetField<string>(_CONTACTNAME);
			item.ContactTitle = row.GetField<string>(_CONTACTTITLE);
			item.Address = row.GetField<string>(_ADDRESS);
			item.City = row.GetField<string>(_CITY);
			item.Region = row.GetField<string>(_REGION);
			item.PostalCode = row.GetField<string>(_POSTALCODE);
			item.Country = row.GetField<string>(_COUNTRY);
			item.Phone = row.GetField<string>(_PHONE);
			item.Fax = row.GetField<string>(_FAX);
		}
		
		public static void UpdateRow(this Customers item, DataRow row)
		{
			row.SetField(_CUSTOMERID, item.CustomerID);
			row.SetField(_COMPANYNAME, item.CompanyName);
			row.SetField(_CONTACTNAME, item.ContactName);
			row.SetField(_CONTACTTITLE, item.ContactTitle);
			row.SetField(_ADDRESS, item.Address);
			row.SetField(_CITY, item.City);
			row.SetField(_REGION, item.Region);
			row.SetField(_POSTALCODE, item.PostalCode);
			row.SetField(_COUNTRY, item.Country);
			row.SetField(_PHONE, item.Phone);
			row.SetField(_FAX, item.Fax);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_CUSTOMERID, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_COMPANYNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_CONTACTNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_CONTACTTITLE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_ADDRESS, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_CITY, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_REGION, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_POSTALCODE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_COUNTRY, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_PHONE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_FAX, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Customers> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Customers> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Customers item)
		{
			return new Dictionary<string,object>() 
			{
				[_CUSTOMERID] = item.CustomerID,
				[_COMPANYNAME] = item.CompanyName,
				[_CONTACTNAME] = item.ContactName,
				[_CONTACTTITLE] = item.ContactTitle,
				[_ADDRESS] = item.Address,
				[_CITY] = item.City,
				[_REGION] = item.Region,
				[_POSTALCODE] = item.PostalCode,
				[_COUNTRY] = item.Country,
				[_PHONE] = item.Phone,
				[_FAX] = item.Fax
			};
		}
		
		public static Customers FromDictionary(this IDictionary<string, object> dict)
		{
			return new Customers
			{
				CustomerID = (string)dict[_CUSTOMERID],
				CompanyName = (string)dict[_COMPANYNAME],
				ContactName = (string)dict[_CONTACTNAME],
				ContactTitle = (string)dict[_CONTACTTITLE],
				Address = (string)dict[_ADDRESS],
				City = (string)dict[_CITY],
				Region = (string)dict[_REGION],
				PostalCode = (string)dict[_POSTALCODE],
				Country = (string)dict[_COUNTRY],
				Phone = (string)dict[_PHONE],
				Fax = (string)dict[_FAX]
			};
		}
		
		public static bool CompareTo(this Customers a, Customers b)
		{
			return a.CustomerID == b.CustomerID
			&& a.CompanyName == b.CompanyName
			&& a.ContactName == b.ContactName
			&& a.ContactTitle == b.ContactTitle
			&& a.Address == b.Address
			&& a.City == b.City
			&& a.Region == b.Region
			&& a.PostalCode == b.PostalCode
			&& a.Country == b.Country
			&& a.Phone == b.Phone
			&& a.Fax == b.Fax;
		}
		
		public static void CopyTo(this Customers from, Customers to)
		{
			to.CustomerID = from.CustomerID;
			to.CompanyName = from.CompanyName;
			to.ContactName = from.ContactName;
			to.ContactTitle = from.ContactTitle;
			to.Address = from.Address;
			to.City = from.City;
			to.Region = from.Region;
			to.PostalCode = from.PostalCode;
			to.Country = from.Country;
			to.Phone = from.Phone;
			to.Fax = from.Fax;
		}
		
		public static string ToSimpleString(this Customers obj)
		{
			return string.Format("{{CustomerID:{0}, CompanyName:{1}, ContactName:{2}, ContactTitle:{3}, Address:{4}, City:{5}, Region:{6}, PostalCode:{7}, Country:{8}, Phone:{9}, Fax:{10}}}", 
			obj.CustomerID, 
			obj.CompanyName, 
			obj.ContactName, 
			obj.ContactTitle, 
			obj.Address, 
			obj.City, 
			obj.Region, 
			obj.PostalCode, 
			obj.Country, 
			obj.Phone, 
			obj.Fax);
		}
		
		public const string _CUSTOMERID = "CustomerID";
		public const string _COMPANYNAME = "CompanyName";
		public const string _CONTACTNAME = "ContactName";
		public const string _CONTACTTITLE = "ContactTitle";
		public const string _ADDRESS = "Address";
		public const string _CITY = "City";
		public const string _REGION = "Region";
		public const string _POSTALCODE = "PostalCode";
		public const string _COUNTRY = "Country";
		public const string _PHONE = "Phone";
		public const string _FAX = "Fax";
	}
}