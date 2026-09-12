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
        private const string ReleaseUrl = "https://api.github.com/repos/NotJolyne1/AntiCheat/releases?per_page=1";
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
                using JsonDocument Releases = JsonDocument.Parse(await Client.GetStringAsync(ReleaseUrl));
                if (Releases.RootElement.GetArrayLength() == 0) throw new InvalidDataException("No GitHub releases were found");

                JsonElement Release = Releases.RootElement[0];
                string DownloadUrl = "";
                int DllCount = 0;

                foreach (JsonElement Asset in Release.GetProperty("assets").EnumerateArray())
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
                string TemporaryPath = Path.Combine(Path.GetTempPath(), "AntiCheat.dll.update");
                string BatchPath = Path.Combine(Path.GetTempPath(), "AntiCheatUpdate.bat");

                byte[] Data = await Client.GetByteArrayAsync(DownloadUrl);
                await File.WriteAllBytesAsync(TemporaryPath, Data);

                File.WriteAllText(BatchPath, BuildBatch(ProcessId, DownloadUrl, ModPath, TemporaryPath, GameExecutable, LatestVersion));
                Process.Start(new ProcessStartInfo { FileName = BatchPath, UseShellExecute = true });
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
            Client.DefaultRequestHeaders.UserAgent.ParseAdd("AntiCheat-Updater");
            Client.Timeout = TimeSpan.FromSeconds(30);
            return Client;
        }

        // not malware, just script to update the mod after authorization
        private static string BuildBatch(int ProcessId, string DownloadUrl, string ModPath, string TemporaryPath, string GameExecutable, string Version)
        {
            return $"@echo off\r\ntitle AU3D Anti-Cheat Updater\r\ncolor 0B\r\necho.\r\necho AU3D Anti-Cheat Updater\r\necho Updating the anti-cheat to version {Version}.\r\necho Among Us 3D will close while the newest version is being downloaded. Don't worry if your anti virus blocks this, it can be mistaken as malware as this is a batch file.\r\necho Downloading source: {DownloadUrl}\r\necho.\r\necho [1/5] Closing Among Us 3D...\r\ntaskkill /PID {ProcessId} /F >nul 2>&1\r\n:wait\r\ntasklist /FI \"PID eq {ProcessId}\" /NH | find \"{ProcessId}\" >nul\r\nif errorlevel 1 goto download\r\ntimeout /t 1 /nobreak >nul\r\ngoto wait\r\n:download\r\necho [2/5] Downloading AntiCheat.dll...\r\ncurl.exe -L --fail --show-error \"{DownloadUrl}\" -o \"{TemporaryPath}\"\r\nif errorlevel 1 goto failed\r\nif not exist \"{TemporaryPath}\" goto failed\r\necho [3/5] Replacing the old anti cheat file...\r\nmove /Y \"{TemporaryPath}\" \"{ModPath}\" >nul\r\nif errorlevel 1 goto permission\r\necho [4/5] Update installed successfully!\r\necho [5/5] Starting Among Us 3D\r\ntimeout /t 2 /nobreak >nul\r\nstart \"\" \"{GameExecutable}\"\r\necho.\r\necho Update complete! This window will close shortly\r\ntimeout /t 6 /nobreak >nul\r\nexit /b\r\n:permission\r\necho.\r\necho The update was downloaded, but Windows denied access to the Mods folder.\r\necho Move this file manually:\r\necho {TemporaryPath}\r\necho To:\r\necho {ModPath}\r\npause\r\nexit /b\r\n:failed\r\necho.\r\necho The update could not be downloaded. Your existing DLL was not changed.\r\necho Download it manually from:\r\necho https://github.com/NotJolyne1/AntiCheat/releases\r\npause\r\nexit /b";
        }
    }
}