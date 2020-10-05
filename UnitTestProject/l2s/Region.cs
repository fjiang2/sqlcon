using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Region")]
	public partial class Region
	{
		[Column(Name = "RegionID", IsPrimaryKey = true)]
		public int RegionID { get; set; }
		
		[Column(Name = "RegionDescription", CanBeNull = false)]
		public string RegionDescription { get; set; }
		
		private EntitySet<Territory> _Territories;
		
		public Region()
		{
			this._Territories = new EntitySet<Territory>();
		}
		
		[Association(Name = "Region_Territory", Storage = "_Territories", ThisKey = "RegionID", OtherKey = "RegionID", IsForeignKey = false)]
		public EntitySet<Territory> Territories
		{
			get
			{
				return this._Territories;
			}
			set
			{
				this._Territories.Assign(value);
			}
		}
	}
}