using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
	public sealed partial class PageIngredientDetails : Page
	{
		// private fields
		Ingredient ingredient;
		ObservableCollection<IngredientPurchase> purchases = new ObservableCollection<IngredientPurchase>();
		int purchasePeriod = 1; // one month by default

		// constructor
		public PageIngredientDetails()
		{
			this.InitializeComponent();

			// register keyboard events for back action
			KeyboardAccelerator GoBack = new KeyboardAccelerator();
			GoBack.Key = VirtualKey.GoBack;
			GoBack.Invoked += BackInvoked;
			KeyboardAccelerator AltLeft = new KeyboardAccelerator();
			AltLeft.Key = VirtualKey.Left;
			AltLeft.Modifiers = VirtualKeyModifiers.Menu;
			AltLeft.Invoked += BackInvoked;
			this.KeyboardAccelerators.Add(GoBack);
			this.KeyboardAccelerators.Add(AltLeft);
		}

		// event handlers
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			this.ingredient = (Ingredient)e.Parameter;
			this.DataContext = this.ingredient;

			this.purchases = MainModelView.Current.GetIngredientPurchase(this.ingredient, 1);
			this.ListViewPurchases.ItemsSource = this.purchases;

			this.TextBlockAvgCost1M.Text = GetAverageCost(1);
			this.TextBlockAvgCost3M.Text = GetAverageCost(3);
			this.TextBlockAvgCost6M.Text = GetAverageCost(6);
			this.TextBlockAvgCost1Y.Text = GetAverageCost(12);
		}
		private void ButtonBack_Tapped(object sender, TappedRoutedEventArgs e)
		{
			OnBackRequested();
		}
		private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (sender is AppBarButton b)
			{
				switch (b.Tag as string)
				{
					case "Edit Basic Info":
						EditBasicInfo();
						break;
					case "Add Purchase":
						AddPurchase();
						break;
					case "Edit Purchase":
						EditPurchase();
						break;
				}
			}
		}
		private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
		{
			RemovePurchase();
		}
		private void ListViewIngredients_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem == this.ListViewPurchases.SelectedItem)
			{
				EditPurchase();
			}
		}
		private void ComboBoxPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// get the new period in months
			// note that -1 means all records
			string value = (string)(e.AddedItems[0]);
			int newPeriod = 0;
			switch (value)
			{
				case "1 Month": newPeriod = 1; break;
				case "3 Months": newPeriod = 3; break;
				case "6 Months": newPeriod = 6; break;
				case "1 Year": newPeriod = 12; break;
				case "All records": newPeriod = -1; break;
			}

			// if new period is different from existing one
			if (newPeriod != purchasePeriod)
			{
				// update period field
				purchasePeriod = newPeriod;

				// read records from DB and update item source
				this.purchases = MainModelView.Current.GetIngredientPurchase(this.ingredient, purchasePeriod);
				this.ListViewPurchases.ItemsSource = this.purchases;
			}
		}

		// private methods
		private string GetAverageCost(int month)
		{
			double? cost = MainModelView.Current.GetAverageIngredientPurchaseCost(this.ingredient, month);

			if (cost == null) return "No Value";

			return cost.Value.ToString("N2") + " HKD";
		}
		private async void EditBasicInfo()
		{
			var dialog = new ContentDialogIngredient("Edit Ingredient", ingredient);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.UpdateIngredient(ingredient);
			}
		}
		private async void AddPurchase()
		{
			// prepare and show dialog
			IngredientPurchase purchase = new IngredientPurchase();
			purchase.Ingredient = this.ingredient;
			purchase.IngredientID = this.ingredient.ID;
			purchase.Date = DateTime.Now;
			var dialog = new ContentDialogIngredientPurchase("Add Ingredient Purchase", purchase);
			var result = await dialog.ShowAsync();

			// while Add New Category button is pressed
			while (dialog.Tag is string && (string)(dialog.Tag) == "Add New Provider")
			{
				await AddProvier(); // show add category dialog 
				dialog.Tag = null; // remember to reset Tag property
				result = await dialog.ShowAsync(); // show ingredient dialog again
			}

			// if ok button is pressed
			if (result == ContentDialogResult.Primary)
			{
				purchases.Add(purchase);
				MainModelView.Current.AddIngredientPurchase(purchase);
			}
		}
		private async void EditPurchase()
		{
			if (ListViewPurchases.SelectedItem is IngredientPurchase purchase)
			{
				// prepare and show dialog
				var dialog = new ContentDialogIngredientPurchase("Edit Ingredient Purchase", purchase);
				var result = await dialog.ShowAsync();


				// while Add New Category button is pressed
				while (dialog.Tag is string && (string)(dialog.Tag) == "Add New Provider")
				{
					await AddProvier(); // show add category dialog 
					dialog.Tag = null; // remember to reset Tag property
					result = await dialog.ShowAsync(); // show ingredient dialog again
				}

				// if ok button is pressed
				if (result == ContentDialogResult.Primary)
				{
					MainModelView.Current.UpdateIngredientPurchase(purchase);
				}
			}
		}
		private void RemovePurchase()
		{
			if (ListViewPurchases.SelectedItem is IngredientPurchase purchase)
			{
				purchases.Remove(purchase);

				MainModelView.Current.DeleteIngredientPurchase(purchase);
			}
		}
		private async Task AddProvier()
		{
			var category = new Category();
			var dialog = new ContentDialogCategories("Add Ingredient Provider", category);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.AddCategory(category, MainModelView.Current.IngredientProviders);
			}
		}

		// methods for back navigation
		private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			OnBackRequested();
			args.Handled = true;
		}
		private bool OnBackRequested()
		{
			if (this.Frame.CanGoBack)
			{
				this.Frame.GoBack();
				return true;
			}
			return false;
		}


	}
}
