using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
		ObservableCollection<IngredientPurchase> purchases = new ObservableCollection<IngredientPurchase>();

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
			//if (isPageLoaded == false) return;

			// Render basic day items.
			if (args.Phase == 0)
			{
				// Register callback for next phase.
				args.RegisterUpdateCallback(CalenderViewSingleDate_CalendarViewDayItemChanging);
			}

			if (args.Phase == 1)
			{
				Color[] barcolors = new Color[] { Colors.Red };
				if (MainModelView.Current.CountIngredientPurchase(args.Item.Date) > 0)
				{
					args.Item.SetDensityColors(barcolors);
				}
			}

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
		private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
		{

		}
		private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
		{
			this.DeleteFlyout.Hide();
		}


		private void UpdateRecentPurchases()
		{
			if (ComboBoxPeriod.SelectedItem == null)
			{
				FlyoutWarmingText.Text = "Please select a Period";
				FlyoutWarming.ShowAt(ComboBoxPeriod);
				return;
			}

			string periodText = (string)(ComboBoxPeriod.SelectedItem);
			if (periodText == "All records")
				TextBlockListHeaderDate.Text = "[All purchases]";
			else
				TextBlockListHeaderDate.Text = $"[Recent {periodText}]";

			int periodInMonths = 0;
			switch (periodText)
			{
				case "1 Month": periodInMonths = 1; break;
				case "3 Months": periodInMonths = 3; break;
				case "6 Months": periodInMonths = 6; break;
				case "1 Year": periodInMonths = 12; break;
				case "All records": periodInMonths = -1; break;
			}

			// read records from DB and update item source
			this.purchases = MainModelView.Current.GetIngredientPurchase(periodInMonths);
			this.ListViewPurchases.ItemsSource = this.purchases;
		}
		private void UpdateSingleDatePurchases()
		{
			if (CalenderViewSingleDate.SelectedDates.Count == 0)
			{
				FlyoutWarmingText.Text = "Please select a date";
				FlyoutWarming.ShowAt(CalenderViewSingleDate);
				return;
			}

			DateTimeOffset date = this.CalenderViewSingleDate.SelectedDates[0];
			TextBlockListHeaderDate.Text = $"[{date.ToString("dd MMM yyyy")}]";

			// read records from DB and update item source
			this.purchases = MainModelView.Current.GetIngredientPurchase(date);
			this.ListViewPurchases.ItemsSource = this.purchases;

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

			DateTimeOffset startDate = CalendarDatePickerStartDate.Date.Value;
			DateTimeOffset endDate = CalendarDatePickerEndDate.Date.Value;

			if (startDate >= endDate)
			{
				FlyoutWarmingText.Text = "Start date must be earlier than end date";
				FlyoutWarming.ShowAt(CalendarDatePickerStartDate);
				return;
			}

			TextBlockListHeaderDate.Text = "[" +
				startDate.ToString("dd MMM yyyy") + " - " +
				endDate.ToString("dd MMM yyyy") + "]";

			// read records from DB and update item source
			this.purchases = MainModelView.Current.GetIngredientPurchase(startDate, endDate);
			this.ListViewPurchases.ItemsSource = this.purchases;

		}

	}
}
