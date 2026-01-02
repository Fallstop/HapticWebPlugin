namespace Loupedeck.HapticWebPlugin.Helpers
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json;
    using System.Threading.Tasks;

    using ICSharpCode.SharpZipLib.Zip;

    public class CertificateManager
    {
        private const String GitHubOwner = "fallstop";
        private const String GitHubRepo = "HapticWebPlugin";
        private const String CertsBranch = "certs";
        private const String ZipFileName = "local.jmw.nz.zip";
        private const String ZipPassword = "password";
        private const String CacheFileName = "certificate_cache.json";
        private const String CertFileName = "local.jmw.nz.pfx";
        private const Int32 CheckIntervalHours = 24;
        private const Int32 CertExpiryWarningDays = 14;

        private readonly String _cacheDirectory;
        private readonly HttpClient _httpClient;

        public X509Certificate2 Certificate { get; private set; }
        public CertificateStatus Status { get; private set; } = CertificateStatus.NotLoaded;
        public String StatusMessage { get; private set; } = "Certificate not loaded";
        public DateTime? CertificateExpiry { get; private set; }

        public CertificateManager(String cacheDirectory)
        {
            this._cacheDirectory = cacheDirectory;
            this._httpClient = new HttpClient();
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", "HapticWebPlugin/1.0");
            this._httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        }

        public async Task<Boolean> InitializeAsync()
        {
            try
            {
                IoHelpers.EnsureDirectoryExists(this._cacheDirectory);

                var cacheInfo = this.LoadCacheInfo();
                var needsRefresh = this.ShouldRefreshCertificate(cacheInfo);

                if (needsRefresh)
                {
                    PluginLog.Info("Checking for new certificate from GitHub Actions...");
                    var downloaded = await this.TryDownloadCertificateAsync();

                    if (!downloaded && !this.HasCachedCertificate())
                    {
                        this.Status = CertificateStatus.Error;
                        this.StatusMessage = "Can't download SSL certificate. Check your internet connection and try restarting the plugin.";
                        PluginLog.Error(this.StatusMessage);
                        return false;
                    }
                }

                return this.LoadCachedCertificate();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to initialize certificate manager");
                this.Status = CertificateStatus.Error;
                this.StatusMessage = $"SSL certificate setup failed. Try restarting the plugin.";
                return false;
            }
        }

        private CacheInfo LoadCacheInfo()
        {
            var cacheFilePath = Path.Combine(this._cacheDirectory, CacheFileName);

            if (!File.Exists(cacheFilePath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(cacheFilePath);
                return JsonSerializer.Deserialize<CacheInfo>(json);
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, "Failed to load cache info");
                return null;
            }
        }

        private void SaveCacheInfo(CacheInfo info)
        {
            var cacheFilePath = Path.Combine(this._cacheDirectory, CacheFileName);

            try
            {
                var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(cacheFilePath, json);
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, "Failed to save cache info");
            }
        }

        private Boolean ShouldRefreshCertificate(CacheInfo cacheInfo)
        {
            if (cacheInfo == null)
            {
                return true;
            }

            var hoursSinceLastCheck = (DateTime.UtcNow - cacheInfo.LastChecked).TotalHours;
            return hoursSinceLastCheck >= CheckIntervalHours;
        }

        private Boolean HasCachedCertificate()
        {
            var certPath = Path.Combine(this._cacheDirectory, CertFileName);
            return File.Exists(certPath);
        }

        private async Task<Boolean> TryDownloadCertificateAsync()
        {
            try
            {
                var downloaded = await this.DownloadFromGitHubRawAsync();
                if (downloaded)
                {
                    this.SaveCacheInfo(new CacheInfo
                    {
                        LastChecked = DateTime.UtcNow,
                        ArtifactId = 0,
                        DownloadedAt = DateTime.UtcNow
                    });
                    PluginLog.Info("Successfully downloaded new certificate from GitHub");
                    return true;
                }

                PluginLog.Warning("Failed to download certificate from GitHub");
                return false;
            }
            catch (HttpRequestException ex)
            {
                PluginLog.Warning(ex, "Network error while downloading certificate");
                return false;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to download certificate");
                return false;
            }
        }

        private async Task<Boolean> DownloadFromGitHubRawAsync()
        {
            var url = $"https://raw.githubusercontent.com/{GitHubOwner}/{GitHubRepo}/{CertsBranch}/{ZipFileName}";
            var zipPath = Path.Combine(this._cacheDirectory, ZipFileName);

            PluginLog.Info($"Downloading certificate from: {url}");

            try
            {
                using var response = await this._httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var fs = File.Create(zipPath))
                {
                    await response.Content.CopyToAsync(fs);
                }

                PluginLog.Info($"Certificate zip saved, extracting...");
                this.ExtractPasswordProtectedZip(zipPath);

                return true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                PluginLog.Warning("Certificate not found in repository. Waiting for first workflow run.");
                return false;
            }
            finally
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
            }
        }

        private void ExtractPasswordProtectedZip(String zipPath)
        {
            var extractDir = this._cacheDirectory;

            using var fileStream = File.OpenRead(zipPath);
            using var zipFile = new ZipFile(fileStream);
            zipFile.Password = ZipPassword;

            foreach (ZipEntry entry in zipFile)
            {
                if (!entry.IsFile)
                {
                    continue;
                }

                // Extract directly to cache directory, flattening the structure
                var fileName = Path.GetFileName(entry.Name);
                if (String.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                var destPath = Path.Combine(extractDir, fileName);

                using var inputStream = zipFile.GetInputStream(entry);
                using var outputStream = File.Create(destPath);
                inputStream.CopyTo(outputStream);

                PluginLog.Verbose($"Extracted: {fileName}");
            }

            PluginLog.Info($"Certificate extracted successfully");
        }

        private Boolean LoadCachedCertificate()
        {
            var certPath = Path.Combine(this._cacheDirectory, CertFileName);

            if (!File.Exists(certPath))
            {
                this.Status = CertificateStatus.Error;
                this.StatusMessage = "SSL certificate not found. Check your internet connection and restart the plugin.";
                return false;
            }

            try
            {
                this.Certificate = new X509Certificate2(certPath);
                this.CertificateExpiry = this.Certificate.NotAfter;

                if (DateTime.UtcNow > this.CertificateExpiry)
                {
                    this.Status = CertificateStatus.Expired;
                    this.StatusMessage = $"SSL certificate expired. Restart the plugin to download a new one.";
                    PluginLog.Warning(this.StatusMessage);
                    return true;
                }

                var daysUntilExpiry = (this.CertificateExpiry.Value - DateTime.UtcNow).TotalDays;
                if (daysUntilExpiry <= CertExpiryWarningDays)
                {
                    this.Status = CertificateStatus.ExpiringSoon;
                    this.StatusMessage = $"SSL certificate expires in {(Int32)daysUntilExpiry} days. It should have auto-renwed by now.";
                    PluginLog.Warning(this.StatusMessage);
                    return true;
                }

                this.Status = CertificateStatus.Valid;
                this.StatusMessage = null;
                PluginLog.Info($"SSL certificate valid until {this.CertificateExpiry:yyyy-MM-dd}");
                return true;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to load certificate");
                this.Status = CertificateStatus.Error;
                this.StatusMessage = $"Failed to load certificate: {ex.Message}";
                return false;
            }
        }

        public void Dispose()
        {
            this.Certificate?.Dispose();
            this._httpClient?.Dispose();
        }
    }

    public enum CertificateStatus
    {
        NotLoaded,
        Valid,
        ExpiringSoon,
        Expired,
        Error
    }

    public class CacheInfo
    {
        public DateTime LastChecked { get; set; }
        public Int64 ArtifactId { get; set; }
        public DateTime DownloadedAt { get; set; }
    }
}
