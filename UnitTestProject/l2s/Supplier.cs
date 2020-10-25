using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Suppliers")]
	public partial class Supplier
	{
		[Column(Name = "SupplierID", IsPrimaryKey = true, IsDbGenerated = true)]
		public int SupplierID { get; set; }
		
		[Column(Name = "CompanyName", CanBeNull = false)]
		public string CompanyName { get; set; }
		
		[Column(Name = "ContactName")]
		public string ContactName { get; set; }
		
		[Column(Name = "ContactTitle")]
		public string ContactTitle { get; set; }
		
		[Column(Name = "Address")]
		public string Address { get; set; }
		
		[Column(Name = "City")]
		public string City { get; set; }
		
		[Column(Name = "Region")]
		public string Region { get; set; }
		
		[Column(Name = "PostalCode")]
		public string PostalCode { get; set; }
		
		[Column(Name = "Country")]
		public string Country { get; set; }
		
		[Column(Name = "Phone")]
		public string Phone { get; set; }
		
		[Column(Name = "Fax")]
		public string Fax { get; set; }
		
		[Column(Name = "HomePage", UpdateCheck = UpdateCheck.Never)]
		public string HomePage { get; set; }
		
		private EntitySet<Product> _Products;
		
		public Supplier()
		{
			this._Products = new EntitySet<Product>();
		}
		
		[Association(Name = "Supplier_Product", Storage = "_Products", ThisKey = "SupplierID", OtherKey = "SupplierID", IsForeignKey = false)]
		public EntitySet<Product> Products
		{
			get
			{
				return this._Products;
			}
			set
			{
				this._Products.Assign(value);
			}
		}
	}
}