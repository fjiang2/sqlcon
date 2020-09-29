using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;

namespace UnitTestProject.Northwind
{
	public partial class Employees
	{
		public int EmployeeID { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Title { get; set; }
		public string TitleOfCourtesy { get; set; }
		public DateTime BirthDate { get; set; }
		public DateTime HireDate { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string PostalCode { get; set; }
		public string Country { get; set; }
		public string HomePhone { get; set; }
		public string Extension { get; set; }
		public byte[] Photo { get; set; }
		public string Notes { get; set; }
		public int ReportsTo { get; set; }
		public string PhotoPath { get; set; }
	}
	
	public static class EmployeesExtension
	{
		public const string TableName = "Employees";
		public static readonly string[] Keys = new string[] { _EMPLOYEEID };
		public static readonly string[] Identity = new string[] { _EMPLOYEEID };
		
		public static List<Employees> ToEmployeesCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Employees NewObject(DataRow row)
		{
			return new Employees
			{
				EmployeeID = row.GetField<int>(_EMPLOYEEID),
				LastName = row.GetField<string>(_LASTNAME),
				FirstName = row.GetField<string>(_FIRSTNAME),
				Title = row.GetField<string>(_TITLE),
				TitleOfCourtesy = row.GetField<string>(_TITLEOFCOURTESY),
				BirthDate = row.GetField<DateTime>(_BIRTHDATE),
				HireDate = row.GetField<DateTime>(_HIREDATE),
				Address = row.GetField<string>(_ADDRESS),
				City = row.GetField<string>(_CITY),
				Region = row.GetField<string>(_REGION),
				PostalCode = row.GetField<string>(_POSTALCODE),
				Country = row.GetField<string>(_COUNTRY),
				HomePhone = row.GetField<string>(_HOMEPHONE),
				Extension = row.GetField<string>(_EXTENSION),
				Photo = row.GetField<byte[]>(_PHOTO),
				Notes = row.GetField<string>(_NOTES),
				ReportsTo = row.GetField<int>(_REPORTSTO),
				PhotoPath = row.GetField<string>(_PHOTOPATH)
			};
		}
		
		public static void FillObject(this Employees item, DataRow row)
		{
			item.EmployeeID = row.GetField<int>(_EMPLOYEEID);
			item.LastName = row.GetField<string>(_LASTNAME);
			item.FirstName = row.GetField<string>(_FIRSTNAME);
			item.Title = row.GetField<string>(_TITLE);
			item.TitleOfCourtesy = row.GetField<string>(_TITLEOFCOURTESY);
			item.BirthDate = row.GetField<DateTime>(_BIRTHDATE);
			item.HireDate = row.GetField<DateTime>(_HIREDATE);
			item.Address = row.GetField<string>(_ADDRESS);
			item.City = row.GetField<string>(_CITY);
			item.Region = row.GetField<string>(_REGION);
			item.PostalCode = row.GetField<string>(_POSTALCODE);
			item.Country = row.GetField<string>(_COUNTRY);
			item.HomePhone = row.GetField<string>(_HOMEPHONE);
			item.Extension = row.GetField<string>(_EXTENSION);
			item.Photo = row.GetField<byte[]>(_PHOTO);
			item.Notes = row.GetField<string>(_NOTES);
			item.ReportsTo = row.GetField<int>(_REPORTSTO);
			item.PhotoPath = row.GetField<string>(_PHOTOPATH);
		}
		
		public static void UpdateRow(this Employees item, DataRow row)
		{
			row.SetField(_EMPLOYEEID, item.EmployeeID);
			row.SetField(_LASTNAME, item.LastName);
			row.SetField(_FIRSTNAME, item.FirstName);
			row.SetField(_TITLE, item.Title);
			row.SetField(_TITLEOFCOURTESY, item.TitleOfCourtesy);
			row.SetField(_BIRTHDATE, item.BirthDate);
			row.SetField(_HIREDATE, item.HireDate);
			row.SetField(_ADDRESS, item.Address);
			row.SetField(_CITY, item.City);
			row.SetField(_REGION, item.Region);
			row.SetField(_POSTALCODE, item.PostalCode);
			row.SetField(_COUNTRY, item.Country);
			row.SetField(_HOMEPHONE, item.HomePhone);
			row.SetField(_EXTENSION, item.Extension);
			row.SetField(_PHOTO, item.Photo);
			row.SetField(_NOTES, item.Notes);
			row.SetField(_REPORTSTO, item.ReportsTo);
			row.SetField(_PHOTOPATH, item.PhotoPath);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_EMPLOYEEID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_LASTNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_FIRSTNAME, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_TITLE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_TITLEOFCOURTESY, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_BIRTHDATE, typeof(System.DateTime)));
			dt.Columns.Add(new DataColumn(_HIREDATE, typeof(System.DateTime)));
			dt.Columns.Add(new DataColumn(_ADDRESS, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_CITY, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_REGION, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_POSTALCODE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_COUNTRY, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_HOMEPHONE, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_EXTENSION, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_PHOTO, typeof(System.Byte[])));
			dt.Columns.Add(new DataColumn(_NOTES, typeof(System.String)));
			dt.Columns.Add(new DataColumn(_REPORTSTO, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_PHOTOPATH, typeof(System.String)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Employees> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Employees> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Employees item)
		{
			return new Dictionary<string,object>() 
			{
				[_EMPLOYEEID] = item.EmployeeID,
				[_LASTNAME] = item.LastName,
				[_FIRSTNAME] = item.FirstName,
				[_TITLE] = item.Title,
				[_TITLEOFCOURTESY] = item.TitleOfCourtesy,
				[_BIRTHDATE] = item.BirthDate,
				[_HIREDATE] = item.HireDate,
				[_ADDRESS] = item.Address,
				[_CITY] = item.City,
				[_REGION] = item.Region,
				[_POSTALCODE] = item.PostalCode,
				[_COUNTRY] = item.Country,
				[_HOMEPHONE] = item.HomePhone,
				[_EXTENSION] = item.Extension,
				[_PHOTO] = item.Photo,
				[_NOTES] = item.Notes,
				[_REPORTSTO] = item.ReportsTo,
				[_PHOTOPATH] = item.PhotoPath
			};
		}
		
		public static Employees FromDictionary(this IDictionary<string, object> dict)
		{
			return new Employees
			{
				EmployeeID = (int)dict[_EMPLOYEEID],
				LastName = (string)dict[_LASTNAME],
				FirstName = (string)dict[_FIRSTNAME],
				Title = (string)dict[_TITLE],
				TitleOfCourtesy = (string)dict[_TITLEOFCOURTESY],
				BirthDate = (DateTime)dict[_BIRTHDATE],
				HireDate = (DateTime)dict[_HIREDATE],
				Address = (string)dict[_ADDRESS],
				City = (string)dict[_CITY],
				Region = (string)dict[_REGION],
				PostalCode = (string)dict[_POSTALCODE],
				Country = (string)dict[_COUNTRY],
				HomePhone = (string)dict[_HOMEPHONE],
				Extension = (string)dict[_EXTENSION],
				Photo = (byte[])dict[_PHOTO],
				Notes = (string)dict[_NOTES],
				ReportsTo = (int)dict[_REPORTSTO],
				PhotoPath = (string)dict[_PHOTOPATH]
			};
		}
		
		public static bool CompareTo(this Employees a, Employees b)
		{
			return a.EmployeeID == b.EmployeeID
			&& a.LastName == b.LastName
			&& a.FirstName == b.FirstName
			&& a.Title == b.Title
			&& a.TitleOfCourtesy == b.TitleOfCourtesy
			&& a.BirthDate == b.BirthDate
			&& a.HireDate == b.HireDate
			&& a.Address == b.Address
			&& a.City == b.City
			&& a.Region == b.Region
			&& a.PostalCode == b.PostalCode
			&& a.Country == b.Country
			&& a.HomePhone == b.HomePhone
			&& a.Extension == b.Extension
			&& a.Photo == b.Photo
			&& a.Notes == b.Notes
			&& a.ReportsTo == b.ReportsTo
			&& a.PhotoPath == b.PhotoPath;
		}
		
		public static void CopyTo(this Employees from, Employees to)
		{
			to.EmployeeID = from.EmployeeID;
			to.LastName = from.LastName;
			to.FirstName = from.FirstName;
			to.Title = from.Title;
			to.TitleOfCourtesy = from.TitleOfCourtesy;
			to.BirthDate = from.BirthDate;
			to.HireDate = from.HireDate;
			to.Address = from.Address;
			to.City = from.City;
			to.Region = from.Region;
			to.PostalCode = from.PostalCode;
			to.Country = from.Country;
			to.HomePhone = from.HomePhone;
			to.Extension = from.Extension;
			to.Photo = from.Photo;
			to.Notes = from.Notes;
			to.ReportsTo = from.ReportsTo;
			to.PhotoPath = from.PhotoPath;
		}
		
		public static string ToSimpleString(this Employees obj)
		{
			return string.Format("{{EmployeeID:{0}, LastName:{1}, FirstName:{2}, Title:{3}, TitleOfCourtesy:{4}, BirthDate:{5}, HireDate:{6}, Address:{7}, City:{8}, Region:{9}, PostalCode:{10}, Country:{11}, HomePhone:{12}, Extension:{13}, Photo:{14}, Notes:{15}, ReportsTo:{16}, PhotoPath:{17}}}", 
			obj.EmployeeID, 
			obj.LastName, 
			obj.FirstName, 
			obj.Title, 
			obj.TitleOfCourtesy, 
			obj.BirthDate, 
			obj.HireDate, 
			obj.Address, 
			obj.City, 
			obj.Region, 
			obj.PostalCode, 
			obj.Country, 
			obj.HomePhone, 
			obj.Extension, 
			obj.Photo, 
			obj.Notes, 
			obj.ReportsTo, 
			obj.PhotoPath);
		}
		
		public const string _EMPLOYEEID = "EmployeeID";
		public const string _LASTNAME = "LastName";
		public const string _FIRSTNAME = "FirstName";
		public const string _TITLE = "Title";
		public const string _TITLEOFCOURTESY = "TitleOfCourtesy";
		public const string _BIRTHDATE = "BirthDate";
		public const string _HIREDATE = "HireDate";
		public const string _ADDRESS = "Address";
		public const string _CITY = "City";
		public const string _REGION = "Region";
		public const string _POSTALCODE = "PostalCode";
		public const string _COUNTRY = "Country";
		public const string _HOMEPHONE = "HomePhone";
		public const string _EXTENSION = "Extension";
		public const string _PHOTO = "Photo";
		public const string _NOTES = "Notes";
		public const string _REPORTSTO = "ReportsTo";
		public const string _PHOTOPATH = "PhotoPath";
	}
}