using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageSelectIngredents : Page, INotifyPropertyChanged
    {
		private ImplicitAnimationCollection _implicitAnimations;
		double _thumbnailSize = 160;

		// public properties
		public double ThumbnailSize { get => _thumbnailSize; set => SetField(ref _thumbnailSize, value); }

		// constructor
		public PageSelectIngredents()
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
					case "Accept": AcceptSelection();  break;
					case "Cancel": CancelSelection(); break;
				}
			}
		}
		private void AppBarToggleButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (sender is AppBarToggleButton tb)
			{
				ToggleGrouping(tb.IsChecked.GetValueOrDefault());
			}
		}
		private void ButtonBack_Tapped(object sender, TappedRoutedEventArgs e)
		{
			this.Frame.Tag = null;

			OnBackRequested();
		}
		private void GridViewMain_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (ApiInformation.IsTypePresent(typeof(ImplicitAnimationCollection).FullName))
			{
				var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
				elementVisual.ImplicitAnimations = _implicitAnimations;
			}
		}
		private void GridViewMain_ItemClick(object sender, ItemClickEventArgs e)
		{

		}

		// private methods
		private void ZoomInThumbnails()
		{
			if (ThumbnailSize < 300) ThumbnailSize += 40;
		}
		private void ZoomOutThumbnails()
		{
			if (ThumbnailSize > 100) ThumbnailSize -= 40;
		}
		private void ToggleGrouping(bool useGrouping)
		{
			// store existing selected items
			var list = GridViewMain.SelectedItems.Cast<Ingredient>().ToList();

			// update data source
			if (useGrouping)
			{
				GroupedSource.Source = MainModelView.Current.GroupedIngredients;
				GroupedSource.IsSourceGrouped = true;
			}
			else
			{
				GroupedSource.Source = MainModelView.Current.Ingredients;
				GroupedSource.IsSourceGrouped = false;
			}

			// update selected items
			foreach (var item in list)
				GridViewMain.SelectedItems.Add(item);
		}
		private void AcceptSelection()
		{
			this.Frame.Tag = GridViewMain.SelectedItems.Cast<Ingredient>().ToList();

			OnBackRequested();
		}
		private void CancelSelection()
		{
			this.Frame.Tag = null;

			OnBackRequested();
		}
		private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			this.Frame.Tag = null;

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

		// methods for grid animation
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
		protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
