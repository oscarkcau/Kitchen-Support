using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace RabbitsKitchenSupport
{
	class ThumbnailManager
	{
		public static readonly uint DefaultWidth = 160;

		// public function
		public static async Task<SoftwareBitmap> LoadImageAsync(StorageFile file)
		{
			SoftwareBitmap softwareBitmap = null;

			// load file to SoftwareBitmap
			using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
			{
				// Create the decoder from the stream
				BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

				// Get the SoftwareBitmap representation of the file
				softwareBitmap = await decoder.GetSoftwareBitmapAsync();

				// convert the format to be displayable in Image control
				if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
					softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
				{
					softwareBitmap = SoftwareBitmap.Convert(
						softwareBitmap,
						BitmapPixelFormat.Bgra8,
						BitmapAlphaMode.Premultiplied
						);
				}
			}

			return softwareBitmap;
		}
		public static async Task<(string, SoftwareBitmap)> AddThumbnailAsync(StorageFile file)
		{
			// load image from external file path
			var softwareBitmap = await LoadImageAsync(file);

			// create new image file in local storage folder
			string thumbnailFilename = DateTime.Now.ToFileTime() + ".png";
			StorageFolder thumbnailFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
				"thumbnails",
				CreationCollisionOption.OpenIfExists
				);
			StorageFile thumbnailFile = await thumbnailFolder.CreateFileAsync(thumbnailFilename);

			// save resampled image to local storage folder
			await SaveSoftwareBitmap(softwareBitmap, thumbnailFile);

			// reload resampled image from local storage folder
			return (thumbnailFilename, await LoadImageAsync(thumbnailFile));
		}

		// private methods
		private static async Task SaveSoftwareBitmap(SoftwareBitmap softwareBitmap, StorageFile file)
		{
			using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
			{
				// Create an encoder with the desired format
				BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

				// Set the software bitmap
				encoder.SetSoftwareBitmap(softwareBitmap);

				// Set additional encoding parameters, if needed
				int inputImageWidth = softwareBitmap.PixelWidth;
				int inputImageHeight = softwareBitmap.PixelHeight;
				encoder.BitmapTransform.ScaledWidth = DefaultWidth;
				encoder.BitmapTransform.ScaledHeight = (uint)((inputImageHeight * DefaultWidth) / inputImageWidth);
				encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.None;
				encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
				encoder.IsThumbnailGenerated = true;

				try
				{
					await encoder.FlushAsync();
				}
				catch (Exception err)
				{
					const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
					switch (err.HResult)
					{
						case WINCODEC_ERR_UNSUPPORTEDOPERATION:
							// If the encoder does not support writing a thumbnail, then try again
							// but disable thumbnail generation.
							encoder.IsThumbnailGenerated = false;
							break;
						default:
							throw;
					}
				}

				if (encoder.IsThumbnailGenerated == false)
				{
					await encoder.FlushAsync();
				}
			}
		}

	}
}
