using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "CustomerCustomerDemo")]
	public partial class CustomerCustomerDemo
	{
		[Column(Name = "CustomerID", IsPrimaryKey = true)]
		public string CustomerID { get; set; }
		
		[Column(Name = "CustomerTypeID", IsPrimaryKey = true)]
		public string CustomerTypeID { get; set; }
		
		private EntityRef<Customer> _Customer;
		private EntityRef<CustomerDemographic> _CustomerDemographic;
		
		[Association(Name = "Customer_CustomerCustomerDemo", Storage = "_Customer", ThisKey = "CustomerID", OtherKey = "CustomerID", IsForeignKey = true)]
		public Customer Customer
		{
			get
			{
				return this._Customer.Entity;
			}
			set
			{
				this._Customer.Entity = value;
			}
		}
		
		[Association(Name = "CustomerDemographic_CustomerCustomerDemo", Storage = "_CustomerDemographic", ThisKey = "CustomerTypeID", OtherKey = "CustomerTypeID", IsForeignKey = true)]
		public CustomerDemographic CustomerDemographic
		{
			get
			{
				return this._CustomerDemographic.Entity;
			}
			set
			{
				this._CustomerDemographic.Entity = value;
			}
		}
	}
}