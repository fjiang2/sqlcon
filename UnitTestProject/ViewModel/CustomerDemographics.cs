using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class CustomerDemographics
		: INotifyPropertyChanged
	{
		public CustomerDemographics()
		{
		}
		private string _CustomerTypeID;
		
		partial void OnCustomerTypeIDChanging(string value);
		
		partial void OnCustomerTypeIDChanged();
		
		public string CustomerTypeID
		{
			get
			{
				return this._CustomerTypeID;
			}
			set
			{
				this.OnCustomerTypeIDChanging(value);
				this._CustomerTypeID = value;
				this.OnCustomerTypeIDChanged();
				this.OnPropertyChanged(nameof(CustomerTypeID));
			}
		}
		private string _CustomerDesc;
		
		partial void OnCustomerDescChanging(string value);
		
		partial void OnCustomerDescChanged();
		
		public string CustomerDesc
		{
			get
			{
				return this._CustomerDesc;
			}
			set
			{
				this.OnCustomerDescChanging(value);
				this._CustomerDesc = value;
				this.OnCustomerDescChanged();
				this.OnPropertyChanged(nameof(CustomerDesc));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}