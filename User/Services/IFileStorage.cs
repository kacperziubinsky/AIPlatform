namespace User.Services;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string extension);
    Task<Stream?> OpenReadAsync(string storedFileName);
    Task DeleteAsync(string storedFileName);
}