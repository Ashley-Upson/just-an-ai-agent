using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text;
using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.MCP.Files;

public class FileHandler
{
    private static readonly HttpClient HttpClient = new();

    public async ValueTask<FileHandlerResult> CreateFileAsync(ToolExecutionContext context, string path, string content, bool overwrite = false)
    {
        string scopedPath = ResolveFilePath(context, path);
        Directory.CreateDirectory(Path.GetDirectoryName(scopedPath)!);

        if (File.Exists(scopedPath) && !overwrite)
            throw new ValidationException("The target file already exists.");

        await File.WriteAllTextAsync(scopedPath, content ?? string.Empty, Encoding.UTF8);

        return new()
        {
            Message = "File created."
        };
    }

    public async ValueTask<FileHandlerResult> ReadFileAsync(ToolExecutionContext context, string path)
    {
        string scopedPath = ResolveFilePath(context, path);

        if (!File.Exists(scopedPath))
            throw new ValidationException("The requested file does not exist.");

        return new()
        {
            Content = await File.ReadAllTextAsync(scopedPath, Encoding.UTF8),
            Message = "File read."
        };
    }

    public async ValueTask<FileHandlerResult> UpdateFileAsync(ToolExecutionContext context, string path, string content, bool createIfMissing = false)
    {
        string scopedPath = ResolveFilePath(context, path);

        if (!File.Exists(scopedPath) && !createIfMissing)
            throw new ValidationException("The requested file does not exist.");

        Directory.CreateDirectory(Path.GetDirectoryName(scopedPath)!);
        await File.WriteAllTextAsync(scopedPath, content ?? string.Empty, Encoding.UTF8);

        return new()
        {
            Message = "File updated."
        };
    }

    public async ValueTask<FileHandlerResult> DownloadFileFromUrlAsync(ToolExecutionContext context, string url, string path, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ValidationException("Parameter 'url' is required.");

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            throw new ValidationException("Parameter 'url' must be an absolute URL.");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ValidationException("Only HTTP and HTTPS URLs can be downloaded.");

        string scopedPath = ResolveFilePath(context, path);
        Directory.CreateDirectory(Path.GetDirectoryName(scopedPath)!);

        if (File.Exists(scopedPath) && !overwrite)
            throw new ValidationException("The target file already exists.");

        string temporaryPath = Path.Combine(
            Path.GetDirectoryName(scopedPath)!,
            $".just-an-ai-agent-download-{Guid.NewGuid():N}.tmp");

        try
        {
            using HttpResponseMessage response = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using (Stream responseStream = await response.Content.ReadAsStreamAsync())
            await using (FileStream fileStream = File.Create(temporaryPath))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            File.Move(temporaryPath, scopedPath, overwrite);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return new()
        {
            Message = "File downloaded."
        };
    }

    public ValueTask<FileHandlerResult> DeleteAsync(ToolExecutionContext context, string path, bool recursive = false)
    {
        string scopedPath = ResolvePath(context, path);

        if (File.Exists(scopedPath))
        {
            File.Delete(scopedPath);

            return Completed(new()
            {
                Message = "File deleted."
            });
        }

        if (Directory.Exists(scopedPath))
        {
            Directory.Delete(scopedPath, recursive);

            return Completed(new()
            {
                Message = "Directory deleted."
            });
        }

        throw new ValidationException("The requested file or directory does not exist.");
    }

    public ValueTask<FileHandlerResult> RenameAsync(ToolExecutionContext context, string path, string newName, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ValidationException("Parameter 'newName' is required.");

        if (newName.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) >= 0)
            throw new ValidationException("Parameter 'newName' must be a name, not a path.");

        string scopedPath = ResolvePath(context, path);
        string parentPath = Path.GetDirectoryName(scopedPath)
            ?? throw new ValidationException("Cannot rename the project root.");
        string destinationPath = ResolvePath(context, Path.Combine(GetRelativePath(context, parentPath), newName));

        MoveFileSystemEntry(scopedPath, destinationPath, overwrite);

        return Completed(new()
        {
            Message = "Entry renamed."
        });
    }

    public ValueTask<FileHandlerResult> MoveAsync(ToolExecutionContext context, string sourcePath, string destinationPath, bool overwrite = false)
    {
        string scopedSourcePath = ResolvePath(context, sourcePath);
        string scopedDestinationPath = ResolvePath(context, destinationPath);

        MoveFileSystemEntry(scopedSourcePath, scopedDestinationPath, overwrite);

        return Completed(new()
        {
            Message = "Entry moved."
        });
    }

    public ValueTask<FileHandlerResult> ZipAsync(ToolExecutionContext context, string sourcePath, string zipPath, bool overwrite = false)
    {
        string scopedSourcePath = ResolvePath(context, sourcePath);
        string scopedZipPath = ResolveFilePath(context, zipPath);

        if (!Directory.Exists(scopedSourcePath) && !File.Exists(scopedSourcePath))
            throw new ValidationException("The source file or directory does not exist.");

        if (Directory.Exists(scopedSourcePath) && IsInsideRoot(scopedSourcePath, scopedZipPath))
            throw new ValidationException("The target zip file cannot be created inside the source directory.");

        if (File.Exists(scopedZipPath))
        {
            if (!overwrite)
                throw new ValidationException("The target zip file already exists.");

            File.Delete(scopedZipPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(scopedZipPath)!);

        if (Directory.Exists(scopedSourcePath))
        {
            ZipFile.CreateFromDirectory(scopedSourcePath, scopedZipPath);
        }
        else
        {
            using ZipArchive archive = ZipFile.Open(scopedZipPath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(scopedSourcePath, Path.GetFileName(scopedSourcePath));
        }

        return Completed(new()
        {
            Message = "Zip file created."
        });
    }

    public ValueTask<FileHandlerResult> UnzipAsync(ToolExecutionContext context, string zipPath, string destinationPath, bool overwrite = false)
    {
        string scopedZipPath = ResolveFilePath(context, zipPath);
        string scopedDestinationPath = ResolvePath(context, destinationPath);

        if (!File.Exists(scopedZipPath))
            throw new ValidationException("The requested zip file does not exist.");

        Directory.CreateDirectory(scopedDestinationPath);

        using ZipArchive archive = ZipFile.OpenRead(scopedZipPath);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryDestinationPath = ResolvePath(context, Path.Combine(GetRelativePath(context, scopedDestinationPath), entry.FullName));

            if (entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal))
            {
                Directory.CreateDirectory(entryDestinationPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(entryDestinationPath)!);
            entry.ExtractToFile(entryDestinationPath, overwrite);
        }

        return Completed(new()
        {
            Message = "Zip file extracted."
        });
    }

    private static void MoveFileSystemEntry(string sourcePath, string destinationPath, bool overwrite)
    {
        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            throw new ValidationException("The source file or directory does not exist.");

        if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
        {
            if (!overwrite)
                throw new ValidationException("The destination file or directory already exists.");

            DeleteExistingDestination(destinationPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        if (File.Exists(sourcePath))
            File.Move(sourcePath, destinationPath);
        else
            Directory.Move(sourcePath, destinationPath);
    }

    private static ValueTask<FileHandlerResult> Completed(FileHandlerResult result) =>
        new(result);

    private static void DeleteExistingDestination(string destinationPath)
    {
        if (File.Exists(destinationPath))
            File.Delete(destinationPath);
        else
            Directory.Delete(destinationPath, recursive: true);
    }

    private static string ResolveFilePath(ToolExecutionContext context, string path)
    {
        string resolvedPath = ResolvePath(context, path);

        if (Directory.Exists(resolvedPath))
            throw new ValidationException("The requested path is a directory.");

        return resolvedPath;
    }

    private static string ResolvePath(ToolExecutionContext context, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ValidationException("A path parameter is required.");

        if (Path.IsPathRooted(path))
            throw new ValidationException("Path parameters must be relative to the current tool workspace.");

        string projectPath = context.ProjectPath
            ?? throw new ValidationException("File execution requires an orchestration-provided project path.");
        Directory.CreateDirectory(projectPath);

        string normalizedRoot = Path.GetFullPath(projectPath);
        string resolvedPath = Path.GetFullPath(Path.Combine(normalizedRoot, path));

        if (!IsInsideRoot(normalizedRoot, resolvedPath))
            throw new ValidationException("The requested path is outside the current tool workspace.");

        return resolvedPath;
    }

    private static string GetRelativePath(ToolExecutionContext context, string scopedPath)
    {
        string projectPath = context.ProjectPath
            ?? throw new ValidationException("File execution requires an orchestration-provided project path.");

        return Path.GetRelativePath(Path.GetFullPath(projectPath), scopedPath);
    }

    private static bool IsInsideRoot(string root, string path)
    {
        string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return string.Equals(normalizedRoot, normalizedPath, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
