using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Categories")]
	public partial class Category
	{
		[Column(Name = "CategoryID", IsPrimaryKey = true, IsDbGenerated = true)]
		public int CategoryID { get; set; }
		
		[Column(Name = "CategoryName", CanBeNull = false)]
		public string CategoryName { get; set; }
		
		[Column(Name = "Description", UpdateCheck = UpdateCheck.Never)]
		public string Description { get; set; }
		
		[Column(Name = "Picture")]
		public byte[] Picture { get; set; }
		
		private EntitySet<Product> _Products;
		
		public Category()
		{
			this._Products = new EntitySet<Product>();
		}
		
		[Association(Name = "Category_Product", Storage = "_Products", ThisKey = "CategoryID", OtherKey = "CategoryID", IsForeignKey = false)]
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