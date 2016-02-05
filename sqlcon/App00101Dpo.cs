using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using Sys.Data;
using Sys.Data.Manager;
																																								
namespace Sys.Dpo
{
	[Table("app00101", Level.Application, Pack = false)]    //Primary Keys = Person_ID;  Identity = Person_ID;
	public partial class App00101Dpo
		: DPObject
	{
		[Column(_Person_ID, CType.Int, Identity = true, Primary = true)]
		public int Person_ID { get; set; }
																																								
		[Column(_SSN, CType.NVarChar, Nullable = true, Length = 10)]
		public string SSN { get; set; }
																																								
		[Column(_First_Name, CType.NVarChar, Length = 50)]
		public string First_Name { get; set; }
																																								
		[Column(_Last_Name, CType.NVarChar, Length = 50)]
		public string Last_Name { get; set; }
																																								
		[Column(_Middle_Name, CType.NVarChar, Nullable = true, Length = 50)]
		public string Middle_Name { get; set; }
																																								
		[Column(_Nick_Name, CType.NVarChar, Nullable = true, Length = 50)]
		public string Nick_Name { get; set; }
																																								
		[Column(_Prefix_Name, CType.NVarChar, Nullable = true, Length = 50)]
		public string Prefix_Name { get; set; }
																																								
		[Column(_Suffix_Name, CType.NVarChar, Nullable = true, Length = 50)]
		public string Suffix_Name { get; set; }
																																								
		[Column(_Gender_Enum, CType.Int, Nullable = true)]
		public int? Gender_Enum { get; set; }
																																								
		[Column(_Birthday, CType.DateTime, Nullable = true)]
		public DateTime? Birthday { get; set; }
																																								
		[Column(_MaritalStatus_Enum, CType.Int, Nullable = true)]
		public int? MaritalStatus_Enum { get; set; }
																																								
		[Column(_Citizen, CType.Bit, Nullable = true)]
		public bool? Citizen { get; set; }
																																								
		[Column(_Inactive, CType.Bit)]
		public bool Inactive { get; set; }
																																								
		public App00101Dpo()
		{
		}
																																								
		public App00101Dpo(DataRow row)
			:base(row)
		{
		}
																																								
		public App00101Dpo(int person_id)
		{
			this.Person_ID = person_id;
			this.Load();
			if(!this.Exists)
			{
				this.Person_ID = person_id;
			}
		}
																																								
		protected override int DPObjectId
		{
			get
			{
				return this.Person_ID;
			}
		}
																																								
		public override IPrimaryKeys Primary
		{
			get
			{
				return new PrimaryKeys(new string[] { _Person_ID });
			}
		}
																																								
		public override IIdentityKeys Identity
		{
			get
			{
				 return new IdentityKeys(new string[] { _Person_ID });
			}
		}
																																								
		public override void Fill(DataRow row)
		{
			this.Person_ID = GetField<int>(row, _Person_ID);
			this.SSN = GetField<string>(row, _SSN);
			this.First_Name = GetField<string>(row, _First_Name);
			this.Last_Name = GetField<string>(row, _Last_Name);
			this.Middle_Name = GetField<string>(row, _Middle_Name);
			this.Nick_Name = GetField<string>(row, _Nick_Name);
			this.Prefix_Name = GetField<string>(row, _Prefix_Name);
			this.Suffix_Name = GetField<string>(row, _Suffix_Name);
			this.Gender_Enum = GetField<int?>(row, _Gender_Enum);
			this.Birthday = GetField<DateTime?>(row, _Birthday);
			this.MaritalStatus_Enum = GetField<int?>(row, _MaritalStatus_Enum);
			this.Citizen = GetField<bool?>(row, _Citizen);
			this.Inactive = GetField<bool>(row, _Inactive);
		}
																																								
		public override void Collect(DataRow row)
		{
			SetField(row, _Person_ID, this.Person_ID);
			SetField(row, _SSN, this.SSN);
			SetField(row, _First_Name, this.First_Name);
			SetField(row, _Last_Name, this.Last_Name);
			SetField(row, _Middle_Name, this.Middle_Name);
			SetField(row, _Nick_Name, this.Nick_Name);
			SetField(row, _Prefix_Name, this.Prefix_Name);
			SetField(row, _Suffix_Name, this.Suffix_Name);
			SetField(row, _Gender_Enum, this.Gender_Enum);
			SetField(row, _Birthday, this.Birthday);
			SetField(row, _MaritalStatus_Enum, this.MaritalStatus_Enum);
			SetField(row, _Citizen, this.Citizen);
			SetField(row, _Inactive, this.Inactive);
		}
		#region CONSTANT
		public const string _Person_ID = "Person_ID";
		public const string _SSN = "SSN";
		public const string _First_Name = "First_Name";
		public const string _Last_Name = "Last_Name";
		public const string _Middle_Name = "Middle_Name";
		public const string _Nick_Name = "Nick_Name";
		public const string _Prefix_Name = "Prefix_Name";
		public const string _Suffix_Name = "Suffix_Name";
		public const string _Gender_Enum = "Gender_Enum";
		public const string _Birthday = "Birthday";
		public const string _MaritalStatus_Enum = "MaritalStatus_Enum";
		public const string _Citizen = "Citizen";
		public const string _Inactive = "Inactive";
		#endregion
	}
}