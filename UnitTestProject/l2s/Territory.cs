using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Territories")]
	public partial class Territory
	{
		[Column(Name = "TerritoryID", IsPrimaryKey = true)]
		public string TerritoryID { get; set; }
		
		[Column(Name = "TerritoryDescription", CanBeNull = false)]
		public string TerritoryDescription { get; set; }
		
		[Column(Name = "RegionID", CanBeNull = false)]
		public int RegionID { get; set; }
		
		private EntitySet<EmployeeTerritory> _EmployeeTerritories;
		
		private EntityRef<Region> _Region;
		
		public Territory()
		{
			this._EmployeeTerritories = new EntitySet<EmployeeTerritory>();
		}
		
		[Association(Name = "Territory_EmployeeTerritory", Storage = "_EmployeeTerritories", ThisKey = "TerritoryID", OtherKey = "TerritoryID", IsForeignKey = false)]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return this._EmployeeTerritories;
			}
			set
			{
				this._EmployeeTerritories.Assign(value);
			}
		}
		
		[Association(Name = "Region_Territory", Storage = "_Region", ThisKey = "RegionID", OtherKey = "RegionID", IsForeignKey = true)]
		public Region Region
		{
			get
			{
				return this._Region.Entity;
			}
			set
			{
				this._Region.Entity = value;
			}
		}
	}
}