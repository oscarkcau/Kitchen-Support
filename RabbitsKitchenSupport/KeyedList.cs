using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitsKitchenSupport
{
	class KeyedList : ObservableCollection<object>
	{
		public string Key {
			get; set;
		}

		public KeyedList(string key) : base()
		{
			Key = key;
		}
	}
}
