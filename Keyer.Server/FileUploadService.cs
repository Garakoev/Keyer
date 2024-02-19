using Microsoft.AspNetCore.Components.Forms;

namespace Keyer.Server;

public class FileUploadService
{
    public static async Task<string> UploadAndGetPath(IBrowserFile file)
    {
        // Save the file to a location on the server
        var path = Path.Combine("wwwroot", "uploads", file.Name);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.OpenReadStream().CopyToAsync(stream);
        }
        // Return the full path
        return path;
    }
}