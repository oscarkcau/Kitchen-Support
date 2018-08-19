using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
	public sealed partial class PageRecipeDetails : Page
	{
		// private field
		Recipe recipe = null;
        ObservableCollection<RecipeIngredientItem> ingredientItems = null;

		// constructor
		public PageRecipeDetails()
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

			this.recipe = (Recipe)e.Parameter;
			this.DataContext = this.recipe;

            if (Frame.Tag == null)
            {
                this.ingredientItems = MainModelView.Current.GetIngredientItems(this.recipe);
            }

			this.ListViewIngredients.ItemsSource = this.ingredientItems;
			
			if (Frame.Tag is List<Ingredient> items)
			{
				AddIngrientItems(items);
                Frame.Tag = null;
            }
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
					case "Add Ingredients":
						{
							var frame = Window.Current.Content as Frame;
							frame.Navigate(typeof(PageSelectIngredents));
						}
						break;
					case "Edit Quantity":
						EditIngrientQuantity();
						break;
				}
			}
		}
		private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
		{
			RemoveIngrientItem();

			DeleteFlyout.Hide();
		}
		private void ButtonBack_Tapped(object sender, TappedRoutedEventArgs e)
		{
			OnBackRequested();
		}
        private void ListViewIngredients_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem == this.ListViewIngredients.SelectedItem)
            {
                EditIngrientQuantity();
            }
        }

        // private methods
        private async void EditBasicInfo()
		{
			var dialog = new ContentDialogRecipe("Edit Recipe", recipe);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.UpdateRecipe(recipe);
			}
		}
		private void AddIngrientItems(List<Ingredient> ingredients)
		{
			foreach (var ingredient in ingredients)
			{
				// skip ingredient if already in the list
				if (this.ingredientItems.Any(x => x.Ingredient == ingredient)) continue;

				// create new RecipeIngredientItem instance
				RecipeIngredientItem item = new RecipeIngredientItem();
				item.Ingredient = ingredient;
				item.IngredientID = ingredient.ID;

				// add to the list
				this.ingredientItems.Add(item);

                // add to db
                MainModelView.Current.AddIngredientItem(item);
			}
		}
		private void RemoveIngrientItem()
		{
			if (this.ListViewIngredients.SelectedItem is RecipeIngredientItem item)
			{
				ingredientItems.Remove(item);

                MainModelView.Current.DeleteIngredientItem(item);
			}
		}
		private async void EditIngrientQuantity()
		{
			if (!(this.ListViewIngredients.SelectedItem is RecipeIngredientItem item)) return;

			var dialog = new ContentDialogIngredientQuantity(item);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.UpdateIngredientItem(item);
			}
		}
        
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
