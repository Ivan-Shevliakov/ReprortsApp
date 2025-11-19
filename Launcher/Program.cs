using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class Program
{
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_SHOW = 5;

    static string repoUrl = "https://github.com/Ivan-Shevliakov/ReprortsApp";
    static string rawUrl = "https://raw.githubusercontent.com/Ivan-Shevliakov/ReprortsApp/main";

    static string appDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "ReportsApp/report");

    static string installPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "ReportsApp");

    static string windowsPath = Path.Combine(appDataPath, "Windows");
    static string versionFile = "version.txt";
    static string exeName = "Raports.exe";
    static string launcherExeName = "ReportsAppLauncher.exe";

    static async Task Main(string[] args)
    {
        bool showConsole = args.Contains("-debug") || args.Contains("-console");

        if (showConsole)
        {
            ShowConsoleWindow();
            Console.WriteLine("=== ReportsApp Launcher ===");
        }

        try
        {
            AllocConsole();
            if (!IsLauncherInstalled() || showConsole)
            {
                InstallLauncher();
                CreateDesktopShortcut();
            }

            if (showConsole) Console.WriteLine("Checking for updates...");

            string localVersion = await GetLocalVersion();
            string remoteVersion = await GetRemoteVersion();

            if (showConsole)
            {
                Console.WriteLine($"Local version: '{localVersion}'");
                Console.WriteLine($"Remote version: '{remoteVersion}'");
            }

            if (localVersion != remoteVersion || !Directory.Exists(appDataPath))
            {
                if (showConsole) Console.WriteLine("Update found! Downloading...");
                await DownloadAndExtractUpdate();
                await SaveLocalVersion(remoteVersion);
                if (showConsole) Console.WriteLine("Update completed!");
            }
            else
            {
                if (showConsole) Console.WriteLine("No updates found.");
            }

            LaunchApp();
            Console.WriteLine("✅ Application launched successfully!");
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();

            if (showConsole)
            {
                Console.WriteLine("✅ Application launched successfully!");
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            ShowConsoleWindow();
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static bool IsLauncherInstalled()
    {
        string installedLauncherPath = Path.Combine(installPath, launcherExeName);
        return File.Exists(installedLauncherPath);
    }

    static void InstallLauncher()
    {
        try
        {
            // Создаем папку установки
            if (!Directory.Exists(installPath))
            {
                Directory.CreateDirectory(installPath);
            }

            // Копируем лаунчер в папку установки
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
            string installedLauncherPath = Path.Combine(installPath, launcherExeName);

            if (!File.Exists(installedLauncherPath) ||
                File.GetLastWriteTime(currentExePath) > File.GetLastWriteTime(installedLauncherPath))
            {
                File.Copy(currentExePath, installedLauncherPath, true);
                Console.WriteLine($"✅ Launcher installed to: {installPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not install launcher: {ex.Message}");
        }
    }

    static void CreateDesktopShortcut()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, "ReportsApp.lnk");
            string installedLauncherPath = Path.Combine(installPath, launcherExeName);
            string iconPath = installedLauncherPath;
            string vbsScript = $"""
                Set oWS = WScript.CreateObject("WScript.Shell")
                Set oLink = oWS.CreateShortcut("{shortcutPath}")
                oLink.TargetPath = "{installedLauncherPath}"
                oLink.WorkingDirectory = "{installPath}"
                oLink.Description = "ReportsApp Launcher"
                oLink.IconLocation = "{iconPath}, 0"
                oLink.Save
                """;

            string vbsPath = Path.Combine(Path.GetTempPath(), "create_shortcut.vbs");
            File.WriteAllText(vbsPath, vbsScript);

            Process.Start(new ProcessStartInfo
            {
                FileName = "wscript.exe",
                Arguments = $"\"{vbsPath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            }).WaitForExit(3000);

            File.Delete(vbsPath);
            Console.WriteLine($"✅ Desktop shortcut created: {shortcutPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not create desktop shortcut: {ex.Message}");
        }
    }

    static void ShowConsoleWindow()
    {
        var handle = GetConsoleWindow();
        if (handle == IntPtr.Zero)
        {
            AllocConsole();
        }
        else
        {
            ShowWindow(handle, SW_SHOW);
        }
    }

    static IntPtr GetConsoleWindow()
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        return GetConsoleWindow();
    }

    static async Task<string> GetLocalVersion()
    {
        string path = Path.Combine(appDataPath, versionFile);
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
            throw new Exception($"Failed to get remote version: {ex.Message}");
        }
    }

    static async Task DownloadAndExtractUpdate()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"{repoUrl}/archive/refs/heads/main.zip");
        using var stream = await response.Content.ReadAsStreamAsync();

        string tempPath = Path.Combine(Path.GetTempPath(), "ReportsApp_update");
        if (Directory.Exists(tempPath))
            Directory.Delete(tempPath, true);

        using var archive = new ZipArchive(stream);
        archive.ExtractToDirectory(tempPath);

        string sourcePath = Path.Combine(tempPath, "ReprortsApp-main");
        if (Directory.Exists(appDataPath))
            Directory.Delete(appDataPath, true);

        CopyDirectory(sourcePath, appDataPath);
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
        string path = Path.Combine(appDataPath, versionFile);
        await File.WriteAllTextAsync(path, version);
    }

    static void LaunchApp()
    {
        AllocConsole();
        string workingDir = Path.GetFullPath(windowsPath);
        string exePath = Path.Combine(workingDir, exeName);

        if (File.Exists(exePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDir,
                UseShellExecute = true
            });
        }
        else
        {
            throw new Exception($"Application not found: {exePath}");
        }
    }
}