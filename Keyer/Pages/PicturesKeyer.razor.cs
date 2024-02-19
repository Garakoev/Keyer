using CommunityToolkit.Maui.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor.Utilities;
using SkiaSharp;

namespace Keyer.Pages;

public partial class PicturesKeyer
{
    private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full z-10";
    private string _dragClass = DefaultDragClass;
    private string _imagePath;
    private string processedImageUrl = null;
    private MudColor keyingColor; // зелёный цвет по умолчаню
    private byte[] image;
    [Inject] private IFileSaver FileSaver { get; set; }

    private async Task Clear()
    {
        _imagePath = null;
        ClearDragClass();
        await Task.Delay(100);
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            var fileName = Path.GetFileName(file.Name);
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.OpenReadStream().CopyToAsync(fileStream);
            }

            using var memoryStream = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(memoryStream);
            image = memoryStream.ToArray();
            _imagePath = $"data:{file.ContentType};base64,{Convert.ToBase64String(image)}";
        }
    }

    private async Task ProcessImage()
    {
        if (image == null)
        {
            return;
        }

        using var inputStream = new SKManagedStream(new MemoryStream(image));
        using var originalBitmap = SKBitmap.Decode(inputStream);

        using var processedBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
        originalBitmap.CopyTo(processedBitmap);
        var editingColor = ConvertColorFormat(keyingColor.Value);
        await Task.Run(() =>
        {
            for (int x = 0; x < processedBitmap.Width; x++)
            {
                for (int y = 0; y < processedBitmap.Height; y++)
                {
                    if (editingColor == processedBitmap.GetPixel(x, y).ToString())
                    {
                        processedBitmap.SetPixel(x, y, SKColor.Empty);
                    }
                }
            }
        });

        using var outputStream = new MemoryStream();
        processedBitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);
        processedImageUrl = $"data:image/png;base64,{Convert.ToBase64String(outputStream.ToArray())}";
    }

    private void SetDragClass()
        => _dragClass = $"{DefaultDragClass} mud-border-primary";

    private void ClearDragClass()
        => _dragClass = DefaultDragClass;

    private static string ConvertColorFormat(string color)
    {
        if (color.Length != 9 || color[0] != '#')
        {
            throw new ArgumentException("Input color string must be in the format '#rrggbbaa'.");
        }

        string alpha = color.Substring(7, 2);
        string red = color.Substring(1, 2);
        string green = color.Substring(3, 2);
        string blue = color.Substring(5, 2);

        return "#" + alpha + red + green + blue;
    }

    private async Task SaveImage()
    {
        if (processedImageUrl != null)
        {
            var defaultFileName = "processed_image.png";
            var imageData = processedImageUrl.Split(',')[1];
            var bytes = Convert.FromBase64String(imageData);
            var stream = new MemoryStream(bytes);

            await FileSaver.SaveAsync(defaultFileName, stream, CancellationToken.None);
        }
    }
}