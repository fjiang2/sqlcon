using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class CustomerCustomerDemo
		: INotifyPropertyChanged
	{
		public CustomerCustomerDemo()
		{
		}
		private string _CustomerID;
		
		partial void OnCustomerIDChanging(string value);
		
		partial void OnCustomerIDChanged();
		
		public string CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this.OnCustomerIDChanging(value);
				this._CustomerID = value;
				this.OnCustomerIDChanged();
				this.OnPropertyChanged(nameof(CustomerID));
			}
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
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}