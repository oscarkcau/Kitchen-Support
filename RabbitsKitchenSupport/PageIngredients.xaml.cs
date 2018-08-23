using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml.Hosting;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class PageIngredients : Page, INotifyPropertyChanged
	{
		private ImplicitAnimationCollection _implicitAnimations;
		double _thumbnailSize = 160;

		// public properties
		public double ThumbnailSize { get => _thumbnailSize; set => SetField(ref _thumbnailSize, value); }

		// constructor
		public PageIngredients()
		{
			InitializeComponent();

			GroupedSource.Source = MainModelView.Current.GroupedIngredients;

			EnsureImplicitAnimations();
		}

		// event handlers
		private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (sender is AppBarButton b)
			{
				switch (b.Tag as string)
				{
					case "Zoom In": ZoomInThumbnails(); break;
					case "Zoom Out": ZoomOutThumbnails(); break;
					case "Add": AddIngredient(); break;
					case "Edit": EditIngredient(); break;
					case "Edit Categories":
						{
							var frame = Window.Current.Content as Frame;
							frame.Navigate(typeof(PageCategories), 
								("Ingredient Categories", "Ingredient Category", MainModelView.Current.IngredientCategories));
						}
						break;
					case "Edit Providers":
						{
							var frame = Window.Current.Content as Frame;
							frame.Navigate(typeof(PageCategories), 
								("Ingredient Providers", "Ingredient Providers", MainModelView.Current.IngredientProviders));
						}
						break;
				}
			}
		}
		private void AppBarToggleButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (sender is AppBarToggleButton tb)
			{
				if (tb.IsChecked == true)
				{
					GroupedSource.Source = MainModelView.Current.GroupedIngredients;
					GroupedSource.IsSourceGrouped = true;
				}
				else
				{
					GroupedSource.Source = MainModelView.Current.Ingredients;
					GroupedSource.IsSourceGrouped = false;
				}
			}
		}
		private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
		{
			DeleteIngredient();

			DeleteFlyout.Hide();
		}
		private void GridViewMain_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem == this.GridViewMain.SelectedItem)
			{
				var frame = Window.Current.Content as Frame;
				frame.Navigate(typeof(PageIngredientDetails), e.ClickedItem);
			}
		}

		// private methods
		private async void AddIngredient()
		{
			// prepare and show dialog
			var ingredient = new Ingredient();
			var dialog = new ContentDialogIngredient("Add Ingredient", ingredient);
			var result = await dialog.ShowAsync();

			// while Add New Category button is pressed
			while (dialog.Tag is string && (string)(dialog.Tag) == "Add New Category")
			{
				await AddCategory(); // show add category dialog 
				dialog.Tag = null; // remember to reset Tag property
				result = await dialog.ShowAsync(); // show ingredient dialog again
			}

			// if ok button is pressed
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.AddIngredient(ingredient);
			}

		}
		private async void EditIngredient()
		{
			if (!(this.GridViewMain.SelectedItem is Ingredient ingredient)) return;

			// prepare and show dialog
			var dialog = new ContentDialogIngredient("Edit Ingredient", ingredient);
			var result = await dialog.ShowAsync();

			// while Add New Category button is pressed
			while (dialog.Tag is string && (string)(dialog.Tag) == "Add New Category")
			{
				await AddCategory(); // show add category dialog 
				dialog.Tag = null; // remember to reset Tag property
				result = await dialog.ShowAsync(); // show ingredient dialog again
			}

			// if ok button is pressed
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.UpdateIngredient(ingredient);
			}
		}
		private void DeleteIngredient()
		{
			if (!(this.GridViewMain.SelectedItem is Ingredient ingredient)) return;

			MainModelView.Current.DeleteIngredient(ingredient);
		}
		private void ZoomInThumbnails()
		{
			if (ThumbnailSize < 300) ThumbnailSize += 40;
		}
		private void ZoomOutThumbnails()
		{
			if (ThumbnailSize > 100) ThumbnailSize -= 40;
		}
		private async Task AddCategory()
		{
			var category = new Category();
			var dialog = new ContentDialogCategories("Add Ingredient Category", category);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.AddCategory(category, MainModelView.Current.IngredientCategories);
			}
		}

		// methods for grid animation
		private void GridViewMain_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (ApiInformation.IsTypePresent(typeof(ImplicitAnimationCollection).FullName))
			{
				var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
				elementVisual.ImplicitAnimations = _implicitAnimations;
			}
		}
		private void EnsureImplicitAnimations()
		{
			if (ApiInformation.IsTypePresent(typeof(ImplicitAnimationCollection).FullName) == false)
				return;

			if (_implicitAnimations == null)
			{
				var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

				var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
				offsetAnimation.Target = nameof(Visual.Offset);
				offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
				offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);


				var rotationAnimation = compositor.CreateVector2KeyFrameAnimation();
				rotationAnimation.Target = nameof(Visual.Size);
				rotationAnimation.InsertExpressionKeyFrame(1f, "this.FinalValue");
				rotationAnimation.Duration = TimeSpan.FromSeconds(500);

				var animationGroup = compositor.CreateAnimationGroup();
				animationGroup.Add(offsetAnimation);
				animationGroup.Add(rotationAnimation);

				_implicitAnimations = compositor.CreateImplicitAnimationCollection();
				_implicitAnimations[nameof(Visual.Offset)] = animationGroup;
			}
		}

		// INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
