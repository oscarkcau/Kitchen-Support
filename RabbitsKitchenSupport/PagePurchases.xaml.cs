using System;
using System.Collections.Generic;
using System.Globalization;
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
		// enum
		enum BrowseMode { Recent, SingleDate, Range }

		// private fields
		BrowseMode browseMode = BrowseMode.Recent;
		bool isPageLoaded = false;

		// constructor
		public PagePurchases()
		{
			this.InitializeComponent();
		}

		// event handlers
		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			isPageLoaded = true;
		}
		private void CalenderViewSingleDate_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
		{

		}
		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (isPageLoaded == false) return;

			if (sender == RadioButtonRecent)
			{
				browseMode = BrowseMode.Recent;
				StoryboardShowRecent.Begin();
			}
			else if (sender == RadioButtonSingleDate)
			{
				browseMode = BrowseMode.SingleDate;
				StoryboardShowSingle.Begin();
			}
			else if (sender == RadioButtonRange)
			{
				browseMode = BrowseMode.Range;
				StoryboardShowRange.Begin();
			}
		}
		private void ComboBoxPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
		private void ButtonRefresh_Tapped(object sender, TappedRoutedEventArgs e)
		{
			switch (browseMode)
			{
				case BrowseMode.Recent:
					UpdateRecentPurchases();
					break;
				case BrowseMode.SingleDate:
					UpdateSingleDatePurchases();
					break;
				case BrowseMode.Range:
					UpdateRangePurchases();
					break;
			}
		}

		private void ListViewPurchases_ItemClick(object sender, ItemClickEventArgs e)
		{

		}

		private void UpdateRecentPurchases()
		{
			if (ComboBoxPeriod.SelectedItem == null)
			{
				FlyoutWarmingText.Text = "Please select a Period";
				FlyoutWarming.ShowAt(ComboBoxPeriod);
				return;
			}

			if ((string)(ComboBoxPeriod.SelectedItem) == "All records")
				TextBlockListHeader1.Text = "All purchases";
			else
				TextBlockListHeader1.Text = "Purchases in recent " + this.ComboBoxPeriod.SelectedItem;

			TextBlockListHeaderDate1.Text = "";
			TextBlockListHeader2.Text = "";
			TextBlockListHeaderDate2.Text = "";
		}
		private void UpdateSingleDatePurchases()
		{
			if (CalenderViewSingleDate.SelectedDates.Count == 0)
			{
				FlyoutWarmingText.Text = "Please select a date";
				FlyoutWarming.ShowAt(CalenderViewSingleDate);
				return;
			}

			DateTime date = this.CalenderViewSingleDate.SelectedDates[0].Date;

			TextBlockListHeader1.Text = "Purchases on ";
			TextBlockListHeaderDate1.Text = date.ToString("ddd dd MMM yyyy");
			TextBlockListHeader2.Text = "";
			TextBlockListHeaderDate2.Text = "";
		}
		private void UpdateRangePurchases()
		{
			if (CalendarDatePickerStartDate.Date == null)
			{
				FlyoutWarmingText.Text = "Please select a date";
				FlyoutWarming.ShowAt(CalendarDatePickerStartDate);
				return;
			}
			if (CalendarDatePickerEndDate.Date == null)
			{
				FlyoutWarmingText.Text = "Please select a date";
				FlyoutWarming.ShowAt(CalendarDatePickerEndDate);
				return;
			}

			DateTime startDate = CalendarDatePickerStartDate.Date.Value.Date;
			DateTime endDate = CalendarDatePickerEndDate.Date.Value.Date;

			if (startDate >= endDate)
			{
				FlyoutWarmingText.Text = "Start date must be earlier than end date";
				FlyoutWarming.ShowAt(CalendarDatePickerStartDate);
				return;
			}

			TextBlockListHeader1.Text = "Purchases between ";
			TextBlockListHeaderDate1.Text = startDate.ToString("ddd dd MMM yyyy");
			TextBlockListHeader2.Text = " and ";
			TextBlockListHeaderDate2.Text = endDate.ToString("ddd dd MMM yyyy");
		}
	}
}
