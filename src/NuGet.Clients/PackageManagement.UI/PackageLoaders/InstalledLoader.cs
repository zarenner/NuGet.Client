using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI
{
    /*
    internal class InstalledLoader : ILoader
    {
        public string LoadingMessage
        {
            get;
            private set;
        }

        private readonly string LogEntrySource = "NuGet Package Manager";

        private SourceRepository _sourceRepository;
        private string _searchText;
        private InstalledPackages _installedPackages;
        private bool _includePrerelease;
        private InstalledPackagesLoader _installedPackageLoader;
        private NuGetPackageManager _packageManager;

        public void SetOptions(
            bool includePrerelease,
            SourceRepository sourceRepository,
            NuGetPackageManager packageManager,
            InstalledPackagesLoader installedPackageLoader,
            string searchText)
        {
            _includePrerelease = includePrerelease;
            _packageManager = packageManager;
            _sourceRepository = sourceRepository;
            _searchText = searchText;
            _installedPackageLoader = installedPackageLoader;

            LoadingMessage = string.IsNullOrWhiteSpace(searchText) ?
                Resources.Text_Loading :
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Text_Searching,
                    searchText);
        }

        private Task _loadTask;
        private List<SearchResultPackageMetadata> _searchResult;

        public void StartLoadTask()
        {
            _loadTask = Task.Run(
                async () =>
                {
                    _searchResult = new List<SearchResultPackageMetadata>();
                    var results = await SearchInstalledAsync(CancellationToken.None);
                    foreach (var package in results.Items)
                    {
                        var searchResultPackage = new SearchResultPackageMetadata();
                        searchResultPackage.Id = package.Identity.Id;
                        searchResultPackage.Version = package.Identity.Version;
                        searchResultPackage.IconUrl = package.IconUrl;
                        searchResultPackage.Author = package.Author;
                        searchResultPackage.DownloadCount = package.DownloadCount;

                        var installedVersions = _installedPackages.GetVersions(searchResultPackage.Id);
                        if (installedVersions != null)
                        {
                            if (installedVersions.Count > 1)
                            {
                                //!!! what should we do here?
                            }
                            else
                            {
                                searchResultPackage.InstalledVersion = installedVersions.Last();
                            }
                        }

                        var versionList = new Lazy<Task<IEnumerable<VersionInfo>>>(async () =>
                        {
                            var versions = await package.Versions.Value;

                            var filteredVersions = versions
                                    .Where(v => !v.Version.IsPrerelease || _includePrerelease)
                                    .ToList();

                            if (!filteredVersions.Any(v => v.Version == searchResultPackage.Version))
                            {
                                filteredVersions.Add(new VersionInfo(searchResultPackage.Version, downloadCount: null));
                            }

                            return filteredVersions;
                        });

                        searchResultPackage.Versions = versionList;

                        searchResultPackage.BackgroundLoader = new Lazy<Task<BackgroundLoaderResult>>(
                            () => BackgroundLoad(searchResultPackage.Id, versionList));

                        // filter out prerelease version when needed.
                        if (searchResultPackage.Version.IsPrerelease &&
                            !_includePrerelease)
                        {
                            var value = await searchResultPackage.BackgroundLoader.Value;

                            if (value.Status == PackageStatus.NotInstalled)
                            {
                                continue;
                            }
                        }

                        searchResultPackage.Summary = package.Summary;
                        _searchResult.Add(searchResultPackage);
                    }
                });
        }

        public async Task GetInstalledPackages()
        {
            _installedPackages = await _installedPackageLoader.GetInstalledPackages();
        }

        List<UISearchMetadata> _installedPackagesWithMetadata;

        public async Task GetMetadata(
            SourceRepository sourceRepository,
            NuGetPackageManager packageManager)
        {
            var results = new List<UISearchMetadata>();

            // UIMetadataResource may not be available
            // Given that this is the 'Installed' filter, we ignore failures in reaching the remote server
            // Instead, we will use the local UIMetadataResource
            UIMetadataResource metadataResource;
            try
            {
                metadataResource =
                    sourceRepository == null ?
                    null :
                    await sourceRepository.GetResourceAsync<UIMetadataResource>();
            }
            catch (Exception ex)
            {
                metadataResource = null;
                // Write stack to activity log
                Microsoft.VisualStudio.Shell.ActivityLog.LogError(LogEntrySource, ex.ToString());
            }

            var localResource = await packageManager
                .PackagesFolderSourceRepository
                .GetResourceAsync<UIMetadataResource>();
            var tasks = new List<Task<UISearchMetadata>>();
            foreach (var id in _installedPackages.GetPackageIds())
            {
                var packageIdentity = new PackageIdentity(
                    id,
                    _installedPackages.GetVersions(id).Last());
                tasks.Add(
                    Task.Run(() =>
                        GetPackageMetadataAsync(localResource,
                                                metadataResource,
                                                packageIdentity,
                                                CancellationToken.None)));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                results.Add(task.Result);
            }

            _installedPackagesWithMetadata = results;
        }

        private async Task<SearchResult> SearchInstalledAsync(CancellationToken cancellationToken)
        {
            if (_installedPackages == null)
            {
                _installedPackages = await _installedPackageLoader.GetInstalledPackages();
            }

            var installedPackages = _installedPackages
                .GetPackageIds()
                .Where(id => id.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) != -1)
                .OrderBy(id => id)
                .ToList();

            var results = new List<UISearchMetadata>();

            // UIMetadataResource may not be available
            // Given that this is the 'Installed' filter, we ignore failures in reaching the remote server
            // Instead, we will use the local UIMetadataResource
            UIMetadataResource metadataResource;
            try
            {
                metadataResource =
                _sourceRepository == null ?
                null :
                await _sourceRepository.GetResourceAsync<UIMetadataResource>();
            }
            catch (Exception ex)
            {
                metadataResource = null;
                // Write stack to activity log
                Microsoft.VisualStudio.Shell.ActivityLog.LogError(LogEntrySource, ex.ToString());
            }

            var localResource = await _packageManager
                .PackagesFolderSourceRepository
                .GetResourceAsync<UIMetadataResource>();
            var tasks = new List<Task<UISearchMetadata>>();
            foreach (var installedPackage in installedPackages)
            {
                var packageIdentity = new PackageIdentity(
                    installedPackage,
                    _installedPackages.GetVersions(installedPackage).Last());
                tasks.Add(
                    Task.Run(() =>
                        GetPackageMetadataAsync(localResource,
                                                metadataResource,
                                                packageIdentity,
                                                cancellationToken)));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                results.Add(task.Result);
            }

            return new SearchResult
            {
                Items = results,
                HasMoreItems = false,
            };
        }

        /// <summary>
        /// Get the metadata of an installed package.
        /// </summary>
        /// <param name="localResource">The local resource, i.e. the package folder of the solution.</param>
        /// <param name="metadataResource">The remote metadata resource.</param>
        /// <param name="identity">The installed package.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The metadata of the package.</returns>
        private async Task<UISearchMetadata> GetPackageMetadataAsync(
            UIMetadataResource localResource,
            UIMetadataResource metadataResource,
            PackageIdentity identity,
            CancellationToken cancellationToken)
        {
            if (metadataResource == null)
            {
                return await GetPackageMetadataWhenRemoteSourceUnavailable(
                    localResource,
                    identity,
                    cancellationToken);
            }

            try
            {
                var metadata = await GetPackageMetadataFromMetadataResourceAsync(
                    metadataResource,
                    identity,
                    cancellationToken);

                // if the package does not exist in the remote source, NuGet should
                // try getting metadata from the local resource.
                if (String.IsNullOrEmpty(metadata.Summary) && localResource != null)
                {
                    return await GetPackageMetadataWhenRemoteSourceUnavailable(
                        localResource,
                        identity,
                        cancellationToken);
                }
                else
                {
                    return metadata;
                }
            }
            catch
            {
                // When a v2 package source throws, it throws an InvalidOperationException or WebException
                // When a v3 package source throws, it throws an HttpRequestException

                // The remote source is not available. NuGet should not fail but
                // should use the local resource instead.
                if (localResource != null)
                {
                    return await GetPackageMetadataWhenRemoteSourceUnavailable(
                        localResource,
                        identity,
                        cancellationToken);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<UISearchMetadata> GetPackageMetadataFromMetadataResourceAsync(
            UIMetadataResource metadataResource,
            PackageIdentity identity,
            CancellationToken cancellationToken)
        {
            var uiPackageMetadatas = await metadataResource.GetMetadata(
                identity.Id,
                _includePrerelease,
                includeUnlisted: false,
                token: cancellationToken);
            var packageMetadata = uiPackageMetadatas.FirstOrDefault(p => p.Identity.Version == identity.Version);

            string summary = string.Empty;
            string title = identity.Id;
            string author = string.Empty;
            if (packageMetadata != null)
            {
                summary = packageMetadata.Summary;
                if (string.IsNullOrEmpty(summary))
                {
                    summary = packageMetadata.Description;
                }
                if (!string.IsNullOrEmpty(packageMetadata.Title))
                {
                    title = packageMetadata.Title;
                }

                author = string.Join(", ", packageMetadata.Authors);
            }

            var versions = uiPackageMetadatas.OrderByDescending(m => m.Identity.Version)
                .Select(m => new VersionInfo(m.Identity.Version, m.DownloadCount));
            return new UISearchMetadata(
                identity,
                title: title,
                summary: summary,
                author: author,
                downloadCount: packageMetadata == null ? null : packageMetadata.DownloadCount,
                iconUrl: packageMetadata == null ? null : packageMetadata.IconUrl,
                versions: ToLazyTask(versions),
                latestPackageMetadata: packageMetadata);
        }

        // Gets the package metadata from the local resource when the remote source
        // is not available.
        private static async Task<UISearchMetadata> GetPackageMetadataWhenRemoteSourceUnavailable(
            UIMetadataResource localResource,
            PackageIdentity identity,
            CancellationToken cancellationToken)
        {
            UIPackageMetadata packageMetadata = null;
            if (localResource != null)
            {
                var localMetadata = await localResource.GetMetadata(
                    identity.Id,
                    includePrerelease: true,
                    includeUnlisted: true,
                    token: cancellationToken);
                packageMetadata = localMetadata.FirstOrDefault(p => p.Identity.Version == identity.Version);
            }

            string summary = string.Empty;
            string title = identity.Id;
            string author = string.Empty;
            if (packageMetadata != null)
            {
                summary = packageMetadata.Summary;
                if (string.IsNullOrEmpty(summary))
                {
                    summary = packageMetadata.Description;
                }
                if (!string.IsNullOrEmpty(packageMetadata.Title))
                {
                    title = packageMetadata.Title;
                }
                author = string.Join(", ", packageMetadata.Authors);
            }

            var versions = new List<VersionInfo>
            {
                new VersionInfo(identity.Version, downloadCount: null)
            };

            return new UISearchMetadata(
                identity,
                title: title,
                summary: summary,
                author: author,
                downloadCount: packageMetadata.DownloadCount,
                iconUrl: packageMetadata == null ? null : packageMetadata.IconUrl,
                versions: ToLazyTask(versions),
                latestPackageMetadata: null);
        }

        private static Lazy<Task<IEnumerable<VersionInfo>>> ToLazyTask(IEnumerable<VersionInfo> versions)
        {
            return new Lazy<Task<IEnumerable<VersionInfo>>>(() => Task.FromResult(versions));
        }

        public async Task<LoadResult> LoadItemsAsync(int startIndex, CancellationToken cancellationToken)
        {
            await _loadTask;

            return new LoadResult()
            {
                Items = _searchResult,
                HasMoreItems = false,
                NextStartIndex = 0
            };
        }

        // Load info in the background
        private async Task<BackgroundLoaderResult> BackgroundLoad(string id, Lazy<Task<IEnumerable<VersionInfo>>> versions)
        {
            var installedVersions = _installedPackages.GetVersions(id);
            if (installedVersions != null)
            {
                var versionsUnwrapped = await versions.Value;
                var highestAvailableVersion = versionsUnwrapped
                    .Select(v => v.Version)
                    .Max();
                var lowestInstalledVersion = installedVersions.First();

                if (VersionComparer.VersionRelease.Compare(lowestInstalledVersion, highestAvailableVersion) < 0)
                {
                    return new BackgroundLoaderResult()
                    {
                        LatestVersion = highestAvailableVersion,
                        Status = PackageStatus.UpdateAvailable
                    };
                }

                return new BackgroundLoaderResult()
                {
                    Status = PackageStatus.Installed
                };
            }

            return new BackgroundLoaderResult()
            {
                Status = PackageStatus.NotInstalled
            };
        }
    } */

    internal class InstalledLoader : ILoader
    {
        private readonly string LogEntrySource = "NuGet Package Manager";

        public string LoadingMessage
        {
            get;
            private set;
        }

        public async Task<LoadResult> LoadItemsAsync(int startIndex, CancellationToken ct)
        {
            await _loadTask;
            return new LoadResult()
            {
                Items = _searchResult,
                HasMoreItems = false,
                NextStartIndex = 0
            };
        }

        private Task _loadTask;

        private InstalledPackages _installedPackages;
        private List<SearchResultPackageMetadata> _installedPackagesWithMetadata;
        private List<SearchResultPackageMetadata> _searchResult;

        // Starts the load task in the background.
        public void StartLoadTask(
            InstalledPackagesLoader installedPackagesLoader,
            SourceRepository sourceRepository,
            NuGetPackageManager packageManager,
            bool includePrerelease,
            string searchText)
        {
            _loadTask = Task.Run(
                async () =>
                {
                    _installedPackages = await installedPackagesLoader.GetInstalledPackages();
                    _installedPackagesWithMetadata = await GetMetadata(
                        sourceRepository,
                        packageManager,
                        includePrerelease);
                    _searchResult = GetSearchResult(searchText);
                });
        }

        // Starts the load task in the background. This is called after active source or
        // include prerelease is changed.
        public void StartLoadTask(
            SourceRepository sourceRepository,
            NuGetPackageManager packageManager,
            bool includePrerelease,
            string searchText)
        {
            _loadTask = Task.Run(
                async () =>
                {
                    _installedPackagesWithMetadata = await GetMetadata(
                        sourceRepository,
                        packageManager,
                        includePrerelease);
                    _searchResult = GetSearchResult(searchText);
                });
        }

        // Starts the load task in the background. This is called after search text is 
        // changed.
        public void StartLoadTask(string searchText)
        {
            _loadTask = Task.Run(
                () =>
                {
                    _searchResult = GetSearchResult(searchText);
                });
        }

        private async Task<List<UISearchMetadata>> GetUIMetadata(
            SourceRepository sourceRepository,
            NuGetPackageManager packageManager,
            bool includePrerelease)
        {
            var results = new List<UISearchMetadata>();

            // UIMetadataResource may not be available
            // Given that this is the 'Installed' filter, we ignore failures in reaching the remote server
            // Instead, we will use the local UIMetadataResource
            UIMetadataResource metadataResource;
            try
            {
                metadataResource =
                    sourceRepository == null ?
                    null :
                    await sourceRepository.GetResourceAsync<UIMetadataResource>();
            }
            catch (Exception ex)
            {
                metadataResource = null;
                // Write stack to activity log
                Microsoft.VisualStudio.Shell.ActivityLog.LogError(LogEntrySource, ex.ToString());
            }

            var localResource = await packageManager
                .PackagesFolderSourceRepository
                .GetResourceAsync<UIMetadataResource>();
            var tasks = new List<Task<UISearchMetadata>>();
            foreach (var id in _installedPackages.GetPackageIds())
            {
                var packageIdentity = new PackageIdentity(
                    id,
                    _installedPackages.GetVersions(id).Last());
                tasks.Add(
                    Task.Run(() =>
                        GetPackageMetadataAsync(localResource,
                                                metadataResource,
                                                packageIdentity,
                                                includePrerelease,
                                                CancellationToken.None)));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                results.Add(task.Result);
            }

            return results;
        }

        private async Task<List<SearchResultPackageMetadata>> GetMetadata(
            SourceRepository sourceRepository,
            NuGetPackageManager packageManager,
            bool includePrerelease)
        {
            var uiMetadata = await GetUIMetadata(sourceRepository, packageManager, includePrerelease);
            var result = CreateSearchResultFromUIMetadata(uiMetadata);
            return result;
        }

        private List<SearchResultPackageMetadata> CreateSearchResultFromUIMetadata(
             List<UISearchMetadata> searchMetadata)
        {
            var result = new List<SearchResultPackageMetadata>();
            foreach (var package in searchMetadata)
            {
                var searchResultPackage = new SearchResultPackageMetadata();
                searchResultPackage.Id = package.Identity.Id;
                searchResultPackage.Version = package.Identity.Version;
                searchResultPackage.IconUrl = package.IconUrl;
                searchResultPackage.Author = package.Author;
                searchResultPackage.DownloadCount = package.DownloadCount;
                searchResultPackage.Summary = package.Summary;

                var installedVersions = _installedPackages.GetVersions(searchResultPackage.Id);
                if (installedVersions != null)
                {
                    if (installedVersions.Count > 1)
                    {
                        //!!! what should we do here?
                    }
                    else
                    {
                        searchResultPackage.InstalledVersion = installedVersions.Last();
                    }
                }

                // The version list returned from the server might not contain the 
                // version of searchResultPackage. In this case, the version of searchResultPackage
                // needs to be added to the version list.
                var versionList = new Lazy<Task<IEnumerable<VersionInfo>>>(async () =>
                {
                    var versions = (await package.Versions.Value).ToList();
                    if (!versions.Any(v => v.Version == searchResultPackage.Version))
                    {
                        versions.Add(new VersionInfo(searchResultPackage.Version, downloadCount: null));
                    }

                    return versions;
                });
                searchResultPackage.Versions = versionList;

                searchResultPackage.BackgroundLoader = new Lazy<Task<BackgroundLoaderResult>>(
                    () => BackgroundLoad(searchResultPackage.Id, versionList));

                result.Add(searchResultPackage);
            }

            return result;
        }

        private List<SearchResultPackageMetadata> GetSearchResult(string searchText)
        {
            var searchResult = _installedPackagesWithMetadata
                .Where(p => p.Id.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1)
                .OrderBy(p => p.Id)
                .ToList();
            return searchResult;
        }

        // Load BackgroundLoaderResult in the background
        private async Task<BackgroundLoaderResult> BackgroundLoad(string id, Lazy<Task<IEnumerable<VersionInfo>>> versions)
        {
            var installedVersions = _installedPackages.GetVersions(id);
            if (installedVersions != null)
            {
                var versionsUnwrapped = await versions.Value;
                var highestAvailableVersion = versionsUnwrapped
                    .Select(v => v.Version)
                    .Max();
                var lowestInstalledVersion = installedVersions.First();

                if (VersionComparer.VersionRelease.Compare(lowestInstalledVersion, highestAvailableVersion) < 0)
                {
                    return new BackgroundLoaderResult()
                    {
                        LatestVersion = highestAvailableVersion,
                        Status = PackageStatus.UpdateAvailable
                    };
                }

                return new BackgroundLoaderResult()
                {
                    Status = PackageStatus.Installed
                };
            }

            return new BackgroundLoaderResult()
            {
                Status = PackageStatus.NotInstalled
            };
        }

        /// <summary>
        /// Get the metadata of an installed package.
        /// </summary>
        /// <param name="localResource">The local resource, i.e. the package folder of the solution.</param>
        /// <param name="metadataResource">The remote metadata resource.</param>
        /// <param name="identity">The installed package.</param>
        /// <param name="includePrerelease">Include prerelease version.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The metadata of the package.</returns>
        private async Task<UISearchMetadata> GetPackageMetadataAsync(
            UIMetadataResource localResource,
            UIMetadataResource metadataResource,
            PackageIdentity identity,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            if (metadataResource == null)
            {
                return await GetPackageMetadataWhenRemoteSourceUnavailable(
                    localResource,
                    identity,
                    cancellationToken);
            }

            try
            {
                var metadata = await GetPackageMetadataFromMetadataResourceAsync(
                    metadataResource,
                    identity,
                    includePrerelease,
                    cancellationToken);

                // if the package does not exist in the remote source, NuGet should
                // try getting metadata from the local resource.
                if (String.IsNullOrEmpty(metadata.Summary) && localResource != null)
                {
                    return await GetPackageMetadataWhenRemoteSourceUnavailable(
                        localResource,
                        identity,
                        cancellationToken);
                }
                else
                {
                    return metadata;
                }
            }
            catch
            {
                // When a v2 package source throws, it throws an InvalidOperationException or WebException
                // When a v3 package source throws, it throws an HttpRequestException

                // The remote source is not available. NuGet should not fail but
                // should use the local resource instead.
                if (localResource != null)
                {
                    return await GetPackageMetadataWhenRemoteSourceUnavailable(
                        localResource,
                        identity,
                        cancellationToken);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<UISearchMetadata> GetPackageMetadataFromMetadataResourceAsync(
            UIMetadataResource metadataResource,
            PackageIdentity identity,
            bool includePrerelease,
            CancellationToken cancellationToken)
        {
            var uiPackageMetadatas = await metadataResource.GetMetadata(
                identity.Id,
                includePrerelease,
                includeUnlisted: false,
                token: cancellationToken);
            var packageMetadata = uiPackageMetadatas.FirstOrDefault(p => p.Identity.Version == identity.Version);

            string summary = string.Empty;
            string title = identity.Id;
            string author = string.Empty;
            if (packageMetadata != null)
            {
                summary = packageMetadata.Summary;
                if (string.IsNullOrEmpty(summary))
                {
                    summary = packageMetadata.Description;
                }
                if (!string.IsNullOrEmpty(packageMetadata.Title))
                {
                    title = packageMetadata.Title;
                }

                author = string.Join(", ", packageMetadata.Authors);
            }

            var versions = uiPackageMetadatas.OrderByDescending(m => m.Identity.Version)
                .Select(m => new VersionInfo(m.Identity.Version, m.DownloadCount));

            return new UISearchMetadata(
                identity,
                title: title,
                summary: summary,
                author: author,
                downloadCount: packageMetadata == null ? null : packageMetadata.DownloadCount,
                iconUrl: packageMetadata == null ? null : packageMetadata.IconUrl,
                versions: ToLazyTask(versions),
                latestPackageMetadata: packageMetadata);
        }

        // Gets the package metadata from the local resource when the remote source
        // is not available.
        private static async Task<UISearchMetadata> GetPackageMetadataWhenRemoteSourceUnavailable(
            UIMetadataResource localResource,
            PackageIdentity identity,
            CancellationToken cancellationToken)
        {
            UIPackageMetadata packageMetadata = null;
            if (localResource != null)
            {
                var localMetadata = await localResource.GetMetadata(
                    identity.Id,
                    includePrerelease: true,
                    includeUnlisted: true,
                    token: cancellationToken);
                packageMetadata = localMetadata.FirstOrDefault(p => p.Identity.Version == identity.Version);
            }

            string summary = string.Empty;
            string title = identity.Id;
            string author = string.Empty;
            if (packageMetadata != null)
            {
                summary = packageMetadata.Summary;
                if (string.IsNullOrEmpty(summary))
                {
                    summary = packageMetadata.Description;
                }
                if (!string.IsNullOrEmpty(packageMetadata.Title))
                {
                    title = packageMetadata.Title;
                }
                author = string.Join(", ", packageMetadata.Authors);
            }

            var versions = new List<VersionInfo>
            {
                new VersionInfo(identity.Version, downloadCount: null)
            };

            return new UISearchMetadata(
                identity,
                title: title,
                summary: summary,
                author: author,
                downloadCount: packageMetadata.DownloadCount,
                iconUrl: packageMetadata == null ? null : packageMetadata.IconUrl,
                versions: ToLazyTask(versions),
                latestPackageMetadata: null);
        }

        private static Lazy<Task<IEnumerable<VersionInfo>>> ToLazyTask(IEnumerable<VersionInfo> versions)
        {
            return new Lazy<Task<IEnumerable<VersionInfo>>>(() => Task.FromResult(versions));
        }
    }
}