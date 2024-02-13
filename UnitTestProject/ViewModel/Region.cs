using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Region
		: INotifyPropertyChanged
	{
		public Region()
		{
		}
		private int _RegionID;
		
		partial void OnRegionIDChanging(int value);
		
		partial void OnRegionIDChanged();
		
		public int RegionID
		{
			get
			{
				return this._RegionID;
			}
			set
			{
				this.OnRegionIDChanging(value);
				this._RegionID = value;
				this.OnRegionIDChanged();
				this.OnPropertyChanged(nameof(RegionID));
			}
		}
		private string _RegionDescription;
		
		partial void OnRegionDescriptionChanging(string value);
		
		partial void OnRegionDescriptionChanged();
		
		public string RegionDescription
		{
			get
			{
				return this._RegionDescription;
			}
			set
			{
				this.OnRegionDescriptionChanging(value);
				this._RegionDescription = value;
				this.OnRegionDescriptionChanged();
				this.OnPropertyChanged(nameof(RegionDescription));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}