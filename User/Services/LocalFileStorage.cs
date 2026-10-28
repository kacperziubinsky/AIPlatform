namespace User.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IConfiguration config)
    {
        _root = config["Storage:CvPath"] ?? Path.Combine(AppContext.BaseDirectory, "cv-files");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string extension)
    {
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(_root, storedName);

        await using var fs = new FileStream(path, FileMode.CreateNew);
        await content.CopyToAsync(fs);
        return storedName;
    }

    public Task<Stream?> OpenReadAsync(string storedFileName)
    {
        var path = Path.Combine(_root, storedFileName);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(string storedFileName)
    {
        var path = Path.Combine(_root, storedFileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}