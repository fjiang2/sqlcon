using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Employees
		: INotifyPropertyChanged
	{
		public Employees()
		{
		}
		private int _EmployeeID;
		
		partial void OnEmployeeIDChanging(int value);
		
		partial void OnEmployeeIDChanged();
		
		public int EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this.OnEmployeeIDChanging(value);
				this._EmployeeID = value;
				this.OnEmployeeIDChanged();
				this.OnPropertyChanged(nameof(EmployeeID));
			}
		}
		private string _LastName;
		
		partial void OnLastNameChanging(string value);
		
		partial void OnLastNameChanged();
		
		public string LastName
		{
			get
			{
				return this._LastName;
			}
			set
			{
				this.OnLastNameChanging(value);
				this._LastName = value;
				this.OnLastNameChanged();
				this.OnPropertyChanged(nameof(LastName));
			}
		}
		private string _FirstName;
		
		partial void OnFirstNameChanging(string value);
		
		partial void OnFirstNameChanged();
		
		public string FirstName
		{
			get
			{
				return this._FirstName;
			}
			set
			{
				this.OnFirstNameChanging(value);
				this._FirstName = value;
				this.OnFirstNameChanged();
				this.OnPropertyChanged(nameof(FirstName));
			}
		}
		private string _Title;
		
		partial void OnTitleChanging(string value);
		
		partial void OnTitleChanged();
		
		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this.OnTitleChanging(value);
				this._Title = value;
				this.OnTitleChanged();
				this.OnPropertyChanged(nameof(Title));
			}
		}
		private string _TitleOfCourtesy;
		
		partial void OnTitleOfCourtesyChanging(string value);
		
		partial void OnTitleOfCourtesyChanged();
		
		public string TitleOfCourtesy
		{
			get
			{
				return this._TitleOfCourtesy;
			}
			set
			{
				this.OnTitleOfCourtesyChanging(value);
				this._TitleOfCourtesy = value;
				this.OnTitleOfCourtesyChanged();
				this.OnPropertyChanged(nameof(TitleOfCourtesy));
			}
		}
		private DateTime _BirthDate;
		
		partial void OnBirthDateChanging(DateTime value);
		
		partial void OnBirthDateChanged();
		
		public DateTime BirthDate
		{
			get
			{
				return this._BirthDate;
			}
			set
			{
				this.OnBirthDateChanging(value);
				this._BirthDate = value;
				this.OnBirthDateChanged();
				this.OnPropertyChanged(nameof(BirthDate));
			}
		}
		private DateTime _HireDate;
		
		partial void OnHireDateChanging(DateTime value);
		
		partial void OnHireDateChanged();
		
		public DateTime HireDate
		{
			get
			{
				return this._HireDate;
			}
			set
			{
				this.OnHireDateChanging(value);
				this._HireDate = value;
				this.OnHireDateChanged();
				this.OnPropertyChanged(nameof(HireDate));
			}
		}
		private string _Address;
		
		partial void OnAddressChanging(string value);
		
		partial void OnAddressChanged();
		
		public string Address
		{
			get
			{
				return this._Address;
			}
			set
			{
				this.OnAddressChanging(value);
				this._Address = value;
				this.OnAddressChanged();
				this.OnPropertyChanged(nameof(Address));
			}
		}
		private string _City;
		
		partial void OnCityChanging(string value);
		
		partial void OnCityChanged();
		
		public string City
		{
			get
			{
				return this._City;
			}
			set
			{
				this.OnCityChanging(value);
				this._City = value;
				this.OnCityChanged();
				this.OnPropertyChanged(nameof(City));
			}
		}
		private string _Region;
		
		partial void OnRegionChanging(string value);
		
		partial void OnRegionChanged();
		
		public string Region
		{
			get
			{
				return this._Region;
			}
			set
			{
				this.OnRegionChanging(value);
				this._Region = value;
				this.OnRegionChanged();
				this.OnPropertyChanged(nameof(Region));
			}
		}
		private string _PostalCode;
		
		partial void OnPostalCodeChanging(string value);
		
		partial void OnPostalCodeChanged();
		
		public string PostalCode
		{
			get
			{
				return this._PostalCode;
			}
			set
			{
				this.OnPostalCodeChanging(value);
				this._PostalCode = value;
				this.OnPostalCodeChanged();
				this.OnPropertyChanged(nameof(PostalCode));
			}
		}
		private string _Country;
		
		partial void OnCountryChanging(string value);
		
		partial void OnCountryChanged();
		
		public string Country
		{
			get
			{
				return this._Country;
			}
			set
			{
				this.OnCountryChanging(value);
				this._Country = value;
				this.OnCountryChanged();
				this.OnPropertyChanged(nameof(Country));
			}
		}
		private string _HomePhone;
		
		partial void OnHomePhoneChanging(string value);
		
		partial void OnHomePhoneChanged();
		
		public string HomePhone
		{
			get
			{
				return this._HomePhone;
			}
			set
			{
				this.OnHomePhoneChanging(value);
				this._HomePhone = value;
				this.OnHomePhoneChanged();
				this.OnPropertyChanged(nameof(HomePhone));
			}
		}
		private string _Extension;
		
		partial void OnExtensionChanging(string value);
		
		partial void OnExtensionChanged();
		
		public string Extension
		{
			get
			{
				return this._Extension;
			}
			set
			{
				this.OnExtensionChanging(value);
				this._Extension = value;
				this.OnExtensionChanged();
				this.OnPropertyChanged(nameof(Extension));
			}
		}
		private byte[] _Photo;
		
		partial void OnPhotoChanging(byte[] value);
		
		partial void OnPhotoChanged();
		
		public byte[] Photo
		{
			get
			{
				return this._Photo;
			}
			set
			{
				this.OnPhotoChanging(value);
				this._Photo = value;
				this.OnPhotoChanged();
				this.OnPropertyChanged(nameof(Photo));
			}
		}
		private string _Notes;
		
		partial void OnNotesChanging(string value);
		
		partial void OnNotesChanged();
		
		public string Notes
		{
			get
			{
				return this._Notes;
			}
			set
			{
				this.OnNotesChanging(value);
				this._Notes = value;
				this.OnNotesChanged();
				this.OnPropertyChanged(nameof(Notes));
			}
		}
		private int _ReportsTo;
		
		partial void OnReportsToChanging(int value);
		
		partial void OnReportsToChanged();
		
		public int ReportsTo
		{
			get
			{
				return this._ReportsTo;
			}
			set
			{
				this.OnReportsToChanging(value);
				this._ReportsTo = value;
				this.OnReportsToChanged();
				this.OnPropertyChanged(nameof(ReportsTo));
			}
		}
		private string _PhotoPath;
		
		partial void OnPhotoPathChanging(string value);
		
		partial void OnPhotoPathChanged();
		
		public string PhotoPath
		{
			get
			{
				return this._PhotoPath;
			}
			set
			{
				this.OnPhotoPathChanging(value);
				this._PhotoPath = value;
				this.OnPhotoPathChanged();
				this.OnPropertyChanged(nameof(PhotoPath));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}