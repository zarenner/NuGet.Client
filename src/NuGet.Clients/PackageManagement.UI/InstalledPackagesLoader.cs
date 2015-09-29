using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI
{
    public class InstalledPackages
    {
        // The key is the package id, and the value is the list of installed versions,
        // ordered from the smallest to the biggest
        private Dictionary<string, List<NuGetVersion>> _installedPackages;

        public InstalledPackages()
        {
            _installedPackages = new Dictionary<string, List<NuGetVersion>>();
        }

        public void Clear()
        {
            _installedPackages.Clear();
        }

        public void AddPackage(PackageIdentity packageIdentity)
        {
            List<NuGetVersion> versions = null;
            if (_installedPackages.TryGetValue(packageIdentity.Id, out versions))
            {
                versions.Add(packageIdentity.Version);
            }
            else
            {
                _installedPackages.Add(
                    packageIdentity.Id,
                    new List<NuGetVersion> { packageIdentity.Version });
            }
        }

        public void SortVersions()
        {
            foreach (var versions in _installedPackages.Values)
            {
                versions.Sort();
            }
        }

        // Returns null when packageId is not found.
        public IList<NuGetVersion> GetVersions(string packageId)
        {
            List<NuGetVersion> versions = null;
            if (_installedPackages.TryGetValue(packageId, out versions))
            {
                return versions;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<string> GetPackageIds()
        {
            return _installedPackages.Keys;
        }
    }

    public class InstalledPackagesLoader
    {
        // The task that is loading all installed packages
        private Task _loadInstalledPackagesTask;
        private InstalledPackages _installedPackages;

        public InstalledPackagesLoader()
        {
            _installedPackages = new InstalledPackages();
        }

        public async Task<InstalledPackages> GetInstalledPackages()
        {
            await _loadInstalledPackagesTask;
            return _installedPackages;
        }

        public void StartLoadInstalledPackagesTask(IEnumerable<NuGetProject> projects)
        {
            var projectList = projects.ToList();
            _loadInstalledPackagesTask = Task.Run(
                async () =>
                {
                    _installedPackages.Clear();
                    foreach (var project in projectList)
                    {
                        foreach (var package in (await project.GetInstalledPackagesAsync(CancellationToken.None)))
                        {
                            _installedPackages.AddPackage(package.PackageIdentity);
                        }
                    }
                    _installedPackages.SortVersions();
                });
        }
    }
}