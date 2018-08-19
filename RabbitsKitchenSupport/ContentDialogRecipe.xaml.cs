using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Extensions;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	public sealed partial class ContentDialogRecipe : ContentDialog
	{
		// private fields
		Recipe recipe = null;
		string thumbnailFilename = null;

		// constructor
		public ContentDialogRecipe(string title, Recipe recipe)
		{
			this.InitializeComponent();

			// initialize customized title
			this.Title = title ?? throw new ArgumentException();

			// initialize data context
			this.recipe = recipe ?? throw new ArgumentException();
			this.DataContext = recipe;
			this.thumbnailFilename = recipe.ThumbnailFilename;

			// initialize combo box with predefined item sources
			ComboBoxCategory.ItemsSource = MainModelView.Current.RecipeCategories;
		}

		// event handlers
		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			// make sure name and unit are not empty string
			if (string.IsNullOrWhiteSpace(TextBoxName.Text))
			{
				FlyoutWarmingText.Text = "Name cannot be empty!";
				FlyoutWarming.ShowAt(TextBoxName);
				args.Cancel = true;
				return;
			}
			if (ComboBoxCategory.SelectedIndex == -1)
			{
				FlyoutWarmingText.Text = "Category cannot be empty!";
				FlyoutWarming.ShowAt(ComboBoxCategory);
				args.Cancel = true;
				return;
			}
			if (string.IsNullOrWhiteSpace(TextBoxUnitPrice.Text))
			{
				FlyoutWarmingText.Text = "Unit Price cannot be empty!";
				FlyoutWarming.ShowAt(TextBoxUnitPrice);
				args.Cancel = true;
				return;
			}
			if (TextBoxRegex.GetIsValid(TextBoxUnitPrice) == false)
			{
				FlyoutWarmingText.Text = "Wrong numerical value!";
				FlyoutWarming.ShowAt(TextBoxUnitPrice);
				args.Cancel = true;
				return;
			}

			// perform real update
			TextBoxName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			ComboBoxCategory.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
			TextBoxUnitPrice.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			ImageThumbnail.GetBindingExpression(Image.SourceProperty).UpdateSource();
			this.recipe.ThumbnailFilename = this.thumbnailFilename;
		}
		private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
		{
			// show picker for load image file
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");

			StorageFile file = await picker.PickSingleFileAsync();
			if (file == null) return;

			var (thumbnailFilename, softwareBitmap) = await ThumbnailManager.AddThumbnailAsync(file);

			SoftwareBitmapSource source = new SoftwareBitmapSource();
			await source.SetBitmapAsync(softwareBitmap);
			this.ImageThumbnail.Source = source;
			this.thumbnailFilename = thumbnailFilename;
		}
		private void borderThumbnail_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			this.borderThumbnail.BorderThickness = new Thickness(4);
		}
		private void borderThumbnail_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			this.borderThumbnail.BorderThickness = new Thickness(1);
		}

	}
}
