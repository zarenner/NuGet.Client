using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI
{
    internal class BrowseLoader : ILoader
    {
        public string LoadingMessage
        {
            get;
            private set;
        }

        private static int PageSize = 25;
        private SourceRepository _sourceRepository;
        private string _searchText;
        private List<NuGetProject> _projects;
        private Dictionary<string, List<NuGetVersion>> _installedPackages;
        private bool _includePrerelease;
        private InstalledPackagesLoader _installedPackageLoader;

        public void SetOptions(
            bool includePrerelease,
            SourceRepository sourceRepository,
            IEnumerable<NuGetProject> projects,
            InstalledPackagesLoader installedPackageLoader,
            string searchText)
        {
            _includePrerelease = includePrerelease;
            _sourceRepository = sourceRepository;
            _searchText = searchText;
            _projects = projects.ToList();
            _installedPackageLoader = installedPackageLoader;

            LoadingMessage = string.IsNullOrWhiteSpace(searchText) ?
                Resources.Text_Loading :
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Text_Searching,
                    searchText);
        }

        public async Task<LoadResult> LoadItemsAsync(int startIndex, CancellationToken cancellationToken)
        {
            var packages = new List<SearchResultPackageMetadata>();
            var results = await SearchAsync(startIndex, cancellationToken);            
            
            if (_installedPackages == null)
            {
                _installedPackages = await _installedPackageLoader.GetInstalledPackages();
            }

            int resultCount = 0;
            foreach (var package in results.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                resultCount++;

                var searchResultPackage = new SearchResultPackageMetadata();
                searchResultPackage.Id = package.Identity.Id;
                searchResultPackage.Version = package.Identity.Version;
                searchResultPackage.IconUrl = package.IconUrl;
                searchResultPackage.Author = package.Author;
                searchResultPackage.DownloadCount = package.DownloadCount;

                if (_installedPackages.ContainsKey(searchResultPackage.Id))
                {
                    var installedVersions = _installedPackages[searchResultPackage.Id];
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
                packages.Add(searchResultPackage);
            }

            cancellationToken.ThrowIfCancellationRequested();
            NuGetEventTrigger.Instance.TriggerEvent(NuGetEvent.PackageLoadEnd);
            return new LoadResult()
            {
                Items = packages,
                HasMoreItems = results.HasMoreItems,
                NextStartIndex = startIndex + resultCount
            };
        }

        // Load info in the background
        private async Task<BackgroundLoaderResult> BackgroundLoad(string id, Lazy<Task<IEnumerable<VersionInfo>>> versions)
        {
            if (_installedPackages.ContainsKey(id))
            {
                var versionsUnwrapped = await versions.Value;
                var highestAvailableVersion = versionsUnwrapped
                    .Select(v => v.Version)
                    .Max();
                var lowestInstalledVersion = _installedPackages[id].First();

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

        private async Task<SearchResult> SearchAsync(int startIndex, CancellationToken ct)
        {
            // normal search
            var searchResource = await _sourceRepository.GetResourceAsync<UISearchResource>();

            // search in source
            if (searchResource == null)
            {
                return SearchResult.Empty;
            }
            else
            {
                var searchFilter = new SearchFilter();
                searchFilter.IncludePrerelease = _includePrerelease;
                searchFilter.SupportedFrameworks = GetSupportedFrameworks();

                var searchResults = await searchResource.Search(
                    _searchText,
                    searchFilter,
                    startIndex,
                    PageSize + 1,
                    ct);

                var items = searchResults.ToList();
                var hasMoreItems = items.Count > PageSize;
                if (hasMoreItems)
                {
                    items.RemoveAt(items.Count - 1);
                }

                return new SearchResult
                {
                    Items = items,
                    HasMoreItems = hasMoreItems,
                };
            }
        }

        // Returns the list of frameworks that we need to pass to the server during search
        private IEnumerable<string> GetSupportedFrameworks()
        {
            var frameworks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var project in _projects)
            {
                NuGetFramework framework;
                if (project.TryGetMetadata(NuGetProjectMetadataKeys.TargetFramework,
                    out framework))
                {
                    if (framework != null
                        && framework.IsAny)
                    {
                        // One of the project's target framework is AnyFramework. In this case,
                        // we don't need to pass the framework filter to the server.
                        return Enumerable.Empty<string>();
                    }

                    if (framework != null
                        && framework.IsSpecificFramework)
                    {
                        frameworks.Add(framework.DotNetFrameworkName);
                    }
                }
                else
                {
                    // we also need to process SupportedFrameworks
                    IEnumerable<NuGetFramework> supportedFrameworks;
                    if (project.TryGetMetadata(
                        NuGetProjectMetadataKeys.SupportedFrameworks,
                        out supportedFrameworks))
                    {
                        foreach (var f in supportedFrameworks)
                        {
                            if (f.IsAny)
                            {
                                return Enumerable.Empty<string>();
                            }

                            frameworks.Add(f.DotNetFrameworkName);
                        }
                    }
                }
            }

            return frameworks;
        }
    }
}