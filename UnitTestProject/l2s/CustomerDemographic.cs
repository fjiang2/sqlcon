using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "CustomerDemographics")]
	public partial class CustomerDemographic
	{
		[Column(Name = "CustomerTypeID", IsPrimaryKey = true)]
		public string CustomerTypeID { get; set; }
		
		[Column(Name = "CustomerDesc", UpdateCheck = UpdateCheck.Never)]
		public string CustomerDesc { get; set; }
		
		private EntitySet<CustomerCustomerDemo> _CustomerCustomerDemoes;
		
		public CustomerDemographic()
		{
			this._CustomerCustomerDemoes = new EntitySet<CustomerCustomerDemo>();
		}
		
		[Association(Name = "CustomerDemographic_CustomerCustomerDemo", Storage = "_CustomerCustomerDemoes", ThisKey = "CustomerTypeID", OtherKey = "CustomerTypeID", IsForeignKey = false)]
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
	}
}