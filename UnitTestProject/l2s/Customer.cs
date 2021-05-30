using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Customers")]
	public partial class Customer
	{
		[Column(Name = "CustomerID", IsPrimaryKey = true)]
		public string CustomerID { get; set; }
		
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
		
		private EntitySet<CustomerCustomerDemo> _CustomerCustomerDemoes;
		private EntitySet<Order> _Orders;
		
		public Customer()
		{
			this._CustomerCustomerDemoes = new EntitySet<CustomerCustomerDemo>();
			this._Orders = new EntitySet<Order>();
		}
		
		[Association(Name = "Customer_CustomerCustomerDemo", Storage = "_CustomerCustomerDemoes", ThisKey = "CustomerID", OtherKey = "CustomerID", IsForeignKey = false)]
		public EntitySet<CustomerCustomerDemo> CustomerCustomerDemoes
		{
			get
			{
				return this._CustomerCustomerDemoes;
			}
			set
			{
				this._CustomerCustomerDemoes.Assign(value);
			}
		}
		
		[Association(Name = "Customer_Order", Storage = "_Orders", ThisKey = "CustomerID", OtherKey = "CustomerID", IsForeignKey = false)]
		public EntitySet<Order> Orders
		{
			get
			{
				return this._Orders;
			}
			set
			{
				this._Orders.Assign(value);
			}
		}
	}
}