using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class PagePurchases : Page
	{
		bool isPageLoaded = false;

		public PagePurchases()
		{
			this.InitializeComponent();
		}

		private void CalenderViewMain_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
		{
			int x = 0;
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (isPageLoaded == false) return;

			if (sender == RadioButtonRecent) StoryboardShowRecent.Begin();
			else if (sender == RadioButtonSingle) StoryboardShowSingle.Begin();
			else if (sender == RadioButtonRange) StoryboardShowRange.Begin();

		}

		private void ComboBoxPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			isPageLoaded = true;
		}
	}
}
