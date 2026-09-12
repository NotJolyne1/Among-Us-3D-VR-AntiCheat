using AntiCheat.Config;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using UnityEngine;

namespace AntiCheat.Managers
{
    internal static class AutoUpdateManager
    {
        private const string VersionUrl = "https://jolynesbackend.xyz/anticheat/version";
        private const string ReleaseUrl = "https://api.github.com/repos/NotJolyne1/AntiCheat/releases/latest";
        internal static bool Checking { get; private set; }
        internal static bool Updating { get; private set; }
        internal static string LatestVersion { get; private set; } = "";
        internal static string Status { get; private set; } = "";

        internal static async void Check()
        {
            if (Checking) return;
            Checking = true;

            try
            {
                using HttpClient Client = CreateClient();
                LatestVersion = (await Client.GetStringAsync(VersionUrl)).Trim().TrimStart('v', 'V');

                if (!Version.TryParse(Settings.Version.Trim().TrimStart('v', 'V'), out Version Current)) throw new InvalidDataException("Invalid current version");
                if (!Version.TryParse(LatestVersion, out Version Latest)) throw new InvalidDataException("Invalid server version");

                Settings.Outdated = Current < Latest;
                Status = Settings.Outdated ? $"Version {LatestVersion} is available" : "Anti-Cheat is up to date";
            }
            catch (Exception Exception)
            {
                Status = $"Update check failed: {Exception.Message}";
                MelonLogger.Error(Status);
            }
            finally
            {
                Checking = false;
            }
        }

        internal static async void Update()
        {
            if (Updating) return;
            Updating = true;
            Status = "Preparing update";

            try
            {
                using HttpClient Client = CreateClient();
                using JsonDocument Release = JsonDocument.Parse(await Client.GetStringAsync(ReleaseUrl));
                string DownloadUrl = "";
                int DllCount = 0;

                foreach (JsonElement Asset in Release.RootElement.GetProperty("assets").EnumerateArray())
                {
                    string Name = Asset.GetProperty("name").GetString() ?? "";
                    if (!Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;
                    DownloadUrl = Asset.GetProperty("browser_download_url").GetString() ?? "";
                    DllCount++;
                }

                if (DllCount != 1 || string.IsNullOrEmpty(DownloadUrl)) throw new InvalidDataException($"Expected one DLL release asset but found {DllCount}");

                int ProcessId = Process.GetCurrentProcess().Id;
                string GameExecutable = Process.GetCurrentProcess().MainModule.FileName;
                string ModPath = Path.Combine(MelonEnvironment.ModsDirectory, "AntiCheat.dll");
                string TemporaryPath = ModPath + ".update";
                string BatchPath = Path.Combine(MelonEnvironment.UserDataDirectory, "AntiCheatUpdate.bat");

                Directory.CreateDirectory(MelonEnvironment.UserDataDirectory);
                File.WriteAllText(BatchPath, BuildBatch(ProcessId, DownloadUrl, ModPath, TemporaryPath, GameExecutable));
                Process.Start(new ProcessStartInfo { FileName = BatchPath, WorkingDirectory = Path.GetDirectoryName(GameExecutable), UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
                Application.Quit();
            }
            catch (Exception Exception)
            {
                Updating = false;
                Status = $"Update failed: {Exception.Message}";
                MelonLogger.Error(Status);
            }
        }

        private static HttpClient CreateClient()
        {
            HttpClient Client = new HttpClient();
            Client.DefaultRequestHeaders.UserAgent.ParseAdd("Jolyne-AntiCheat-Updater");
            Client.Timeout = TimeSpan.FromSeconds(30);
            return Client;
        }

        private static string BuildBatch(int ProcessId, string DownloadUrl, string ModPath, string TemporaryPath, string GameExecutable)
        {
            return $"@echo off\r\ntaskkill /PID {ProcessId} /F >nul 2>&1\r\n:wait\r\ntasklist /FI \"PID eq {ProcessId}\" | find \"{ProcessId}\" >nul\r\nif not errorlevel 1 timeout /t 1 /nobreak >nul & goto wait\r\ncurl.exe -L --fail --silent --show-error \"{DownloadUrl}\" -o \"{TemporaryPath}\"\r\nif errorlevel 1 goto failed\r\nif not exist \"{TemporaryPath}\" goto failed\r\nmove /Y \"{TemporaryPath}\" \"{ModPath}\" >nul\r\nstart \"\" \"{GameExecutable}\"\r\ndel \"%~f0\"\r\nexit\r\n:failed\r\ndel \"{TemporaryPath}\" >nul 2>&1\r\npause\r\ndel \"%~f0\"\r\nexit";
        }
    }
}