using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.IO.Compression;

class Launcher
{
    static string repoUrl = "https://github.com/Ivan-Shevliakov/ReprortsApp";
    static string rawUrl = "https://raw.githubusercontent.com/Ivan-Shevliakov/ReprortsApp/main";
    static string localPath = "ReportsApp";
    static string versionFile = "version.txt";
    static string buildZipUrl = "https://github.com/Ivan-Shevliakov/ReprortsApp/archive/refs/heads/main.zip";

    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("Checking for updates...");

        try
        {
            // Проверяем текущую версию
            string localVersion = await GetLocalVersion();
            string remoteVersion = await GetRemoteVersion();

            Console.WriteLine($"Local version: {localVersion}");
            Console.WriteLine($"Remote version: {remoteVersion}");

            if (localVersion != remoteVersion || !Directory.Exists(localPath))
            {
                Console.WriteLine("Update found! Downloading...");
                await DownloadAndExtractUpdate();
                await File.WriteAllTextAsync(Path.Combine(localPath, versionFile), remoteVersion);
            }

            // Запускаем приложение
            LaunchApp();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static async Task<string> GetLocalVersion()
    {
        string path = Path.Combine(localPath, versionFile);
        return File.Exists(path) ? await File.ReadAllTextAsync(path) : "0.0.0";
    }

    static async Task<string> GetRemoteVersion()
    {
        using var client = new HttpClient();
        return await client.GetStringAsync($"{rawUrl}/{versionFile}");
    }

    static async Task DownloadAndExtractUpdate()
    {
        using var client = new HttpClient();

        // Скачиваем ZIP
        var response = await client.GetAsync(buildZipUrl);
        using var stream = await response.Content.ReadAsStreamAsync();

        // Создаем временную папку
        string tempPath = "temp_update";
        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);

        // Распаковываем
        using var archive = new ZipArchive(stream);
        archive.ExtractToDirectory(tempPath);

        // Копируем файлы из Builds/ в целевую папку
        string sourceBuildPath = FindBuildPath(tempPath);
        if (Directory.Exists(localPath))
            Directory.Delete(localPath, true);

        Directory.Move(sourceBuildPath, localPath);

        // Чистим временные файлы
        Directory.Delete(tempPath, true);
    }

    static string FindBuildPath(string tempPath)
    {
        // Ищем путь к билдам в распакованной структуре
        var possiblePaths = new[]
        {
            Path.Combine(tempPath, "ReprortsApp-main", "Builds", "Windows"),
            Path.Combine(tempPath, "Builds", "Windows"),
            Path.Combine(tempPath, "Windows")
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
                return path;
        }

        throw new DirectoryNotFoundException("Build directory not found in archive");
    }

    static void LaunchApp()
    {
        string exePath = Path.Combine(localPath, "Raports.exe"); // Замените на имя вашего exe
        if (File.Exists(exePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = localPath,
                UseShellExecute = true
            });
        }
        else
        {
            Console.WriteLine($"Application not found: {exePath}");
        }
    }
}
