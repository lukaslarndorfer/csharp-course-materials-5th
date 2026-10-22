using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PuppyAdoption.Util;
using PuppyAdoption.ViewModels;

namespace PuppyAdoption.Views;

public partial class PuppyAdmission : ScopedControl
{
    public PuppyAdmission()
    {
        InitializeComponent();
    }

    private async void HandlePickImage(object? sender, RoutedEventArgs e)
    {
        try
        {
            string? filePath = await GetImageFilePathAsync();
            if (filePath is not null && DataContext is PuppyAdmissionViewModel viewModel)
            {
                byte[]? imageData = GetImageData(filePath);
                if (imageData is not null)
                {
                    viewModel.SetImageData(imageData);
                }
            }
        }
        catch (Exception ex)
        {
            // within async void due to XAML event handler limitations - at least log the error
            await Console.Error.WriteLineAsync($"Error picking image: {ex.Message}");
        }
    }

    private static byte[]? GetImageData(string imageFilePath)
    {
        try
        {
            using var image = new Bitmap(imageFilePath);
            using var stream = new MemoryStream();
            image.Save(stream, PngBitmapEncoderOptions.Default);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading image file '{imageFilePath}': {ex.Message}");

            return null;
        }
    }

    private async ValueTask<string?> GetImageFilePathAsync()
    {
        var owner = TopLevel.GetTopLevel(this);
        if (owner is null)
        {
            throw new InvalidOperationException("No top-level window found");
        }

        IEnumerable<IStorageFile> files = await PickFilesAsync(owner);
        var first = files.FirstOrDefault();

        return first?.TryGetLocalPath();
    }

    private static async Task<IEnumerable<IStorageFile>> PickFilesAsync(TopLevel owner)
    {
        var imageFiles = new FilePickerFileType("Images")
        {
            Patterns = ["*.png", "*.jpg", "*.jpeg"]
        };

        IReadOnlyList<IStorageFile> files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose puppy image",
            AllowMultiple = false,
            FileTypeFilter = [imageFiles]
        });

        return files;
    }
}
