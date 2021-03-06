﻿using System;
using System.ComponentModel;

namespace BeatBoxers
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName){
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler!=null){
				var e = new PropertyChangedEventArgs (propertyName);
				handler (this,e);
			}
		}

		#endregion
	}
}

