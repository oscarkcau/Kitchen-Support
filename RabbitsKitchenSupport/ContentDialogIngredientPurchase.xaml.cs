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
using Microsoft.Toolkit.Uwp.UI.Extensions;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	public sealed partial class ContentDialogIngredientPurchase : ContentDialog
	{
		// private fields
		private IngredientPurchase purchase;

		// constructor
		public ContentDialogIngredientPurchase(string title, IngredientPurchase purchase)
		{
			this.InitializeComponent();

			// initialize customized title
			this.Title = title ?? throw new ArgumentException();

			// initialize data source
			this.purchase = purchase;
			this.DataContext = purchase;
			this.ComboBoxProvider.ItemsSource = MainModelView.Current.IngredientProviders;

		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			// make sure quantity is in correct format
			if (TextBoxRegex.GetIsValid(TextBoxQuantity) == false)
			{
				FlyoutWarmingText.Text = "Wrong numerical value!";
				FlyoutWarming.ShowAt(TextBoxQuantity);
				args.Cancel = true;
				return;
			}

			if (TextBoxRegex.GetIsValid(TextBoxCost) == false)
			{
				FlyoutWarmingText.Text = "Wrong numerical value!";
				FlyoutWarming.ShowAt(TextBoxCost);
				args.Cancel = true;
				return;
			}

			// perform real update
			TextBoxQuantity.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			TextBoxCost.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			DatePickerPurchaseDate.GetBindingExpression(DatePicker.DateProperty).UpdateSource();
			ComboBoxProvider.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
		}

		private void ButtonAddNewProvider_Tapped(object sender, TappedRoutedEventArgs e)
		{
			// indicate user pressed Add New Category button
			this.Tag = "Add New Provider";

			// hide the dialog
			this.Hide();
		}
	}
}
