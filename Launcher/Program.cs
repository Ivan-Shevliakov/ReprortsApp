using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static string repoUrl = "https://github.com/Ivan-Shevliakov/ReprortsApp";
    static string rawUrl = "https://raw.githubusercontent.com/Ivan-Shevliakov/ReprortsApp/main";
    static string localPath = "ReportsApp";
    static string windowsPath = "ReportsApp/Windows"; 
    static string versionFile = "version.txt";
    static string exeName = "Raports.exe"; // Или какое имя у вашего EXE?

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== ReportsApp Launcher ===");
        Console.WriteLine("Checking for updates...");

        try
        {
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Local path: {Path.GetFullPath(localPath)}");
            Console.WriteLine($"Windows path: {Path.GetFullPath(windowsPath)}");

            string localVersion = await GetLocalVersion();
            string remoteVersion = await GetRemoteVersion();

            Console.WriteLine($"Local version: '{localVersion}'");
            Console.WriteLine($"Remote version: '{remoteVersion}'");

            if (localVersion != remoteVersion || !Directory.Exists(localPath))
            {
                Console.WriteLine("Update found! Downloading...");
                await DownloadAndExtractUpdate();
                await SaveLocalVersion(remoteVersion);
                Console.WriteLine("Update completed!");
            }
            else
            {
                Console.WriteLine("No updates found.");
            }

            LaunchApp();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task<string> GetLocalVersion()
    {
        string path = Path.Combine(localPath, versionFile);
        return File.Exists(path) ? (await File.ReadAllTextAsync(path)).Trim() : "0.0.0";
    }

    static async Task<string> GetRemoteVersion()
    {
        using var client = new HttpClient();
        try
        {
            return (await client.GetStringAsync($"{rawUrl}/{versionFile}")).Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting remote version: {ex.Message}");
            return "0.0.0";
        }
    }

    static async Task DownloadAndExtractUpdate()
    {
        using var client = new HttpClient();

        Console.WriteLine("Downloading update...");
        var response = await client.GetAsync($"{repoUrl}/archive/refs/heads/main.zip");
        using var stream = await response.Content.ReadAsStreamAsync();

        string tempPath = "temp_update";
        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);

        Console.WriteLine("Extracting files...");
        using var archive = new ZipArchive(stream);
        archive.ExtractToDirectory(tempPath);

        string sourcePath = Path.Combine(tempPath, "ReprortsApp-main");
        if (Directory.Exists(localPath))
            Directory.Delete(localPath, true);

        CopyDirectory(sourcePath, localPath);

        Directory.Delete(tempPath, true);
    }

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            if (subDir.Name == ".git") continue;

            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    static async Task SaveLocalVersion(string version)
    {
        string path = Path.Combine(localPath, versionFile);
        await File.WriteAllTextAsync(path, version);
    }

    static void LaunchApp()
    {

       
            string workingDir = Path.GetFullPath(windowsPath);
            string exePath = Path.Combine(workingDir, exeName);

            Console.WriteLine($"Launching: {exePath}");

            if (File.Exists(exePath))
            {
                try
                {
                    // Самый простой способ - Process.Start с абсолютным путем
                    Process.Start(exePath);
                    Console.WriteLine("✅ Application launched!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to launch: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"❌ File not found: {exePath}");
            }
        

    }
}