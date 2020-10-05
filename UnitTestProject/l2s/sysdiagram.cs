using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "sysdiagrams")]
	public partial class sysdiagram
	{
		[Column(Name = "principal_id", CanBeNull = false)]
		public int principal_id { get; set; }
		
		[Column(Name = "diagram_id", IsPrimaryKey = true, IsDbGenerated = true)]
		public int diagram_id { get; set; }
		
		[Column(Name = "version")]
		public int? version { get; set; }
		
		[Column(Name = "definition")]
		public byte[] definition { get; set; }
	}
}