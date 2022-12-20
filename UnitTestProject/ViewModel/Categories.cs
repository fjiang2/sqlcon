using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Categories
		: INotifyPropertyChanged
	{
		public Categories()
		{
		}
		private int _CategoryID;
		
		partial void OnCategoryIDChanging(int value);
		
		partial void OnCategoryIDChanged();
		
		public int CategoryID
		{
			get
			{
				return this._CategoryID;
			}
			set
			{
				this.OnCategoryIDChanging(value);
				this._CategoryID = value;
				this.OnCategoryIDChanged();
				this.OnPropertyChanged(nameof(CategoryID));
			}
		}
		private string _CategoryName;
		
		partial void OnCategoryNameChanging(string value);
		
		partial void OnCategoryNameChanged();
		
		public string CategoryName
		{
			get
			{
				return this._CategoryName;
			}
			set
			{
				this.OnCategoryNameChanging(value);
				this._CategoryName = value;
				this.OnCategoryNameChanged();
				this.OnPropertyChanged(nameof(CategoryName));
			}
		}
		private string _Description;
		
		partial void OnDescriptionChanging(string value);
		
		partial void OnDescriptionChanged();
		
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this.OnDescriptionChanging(value);
				this._Description = value;
				this.OnDescriptionChanged();
				this.OnPropertyChanged(nameof(Description));
			}
		}
		private byte[] _Picture;
		
		partial void OnPictureChanging(byte[] value);
		
		partial void OnPictureChanged();
		
		public byte[] Picture
		{
			get
			{
				return this._Picture;
			}
			set
			{
				this.OnPictureChanging(value);
				this._Picture = value;
				this.OnPictureChanged();
				this.OnPropertyChanged(nameof(Picture));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}