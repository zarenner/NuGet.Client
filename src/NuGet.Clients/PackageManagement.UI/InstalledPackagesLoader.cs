using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.VisualStudio;
using NuGet.Versioning;

namespace NuGet.PackageManagement.UI
{
    public class InstalledPackagesLoader
    {
        // The key is the package id, and the value is the list of installed versions,
        // ordered from the smallest to the biggest
        private Dictionary<string, List<NuGetVersion>> _installedPackages;

        private List<NuGetProject> _projects;

        // The task that is loading all installed packages
        private Task _loadInstalledPackagesTask;

        public InstalledPackagesLoader()
        {
            _installedPackages = new Dictionary<string, List<NuGetVersion>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<Dictionary<string, List<NuGetVersion>>> GetInstalledPackages()
        {
            await _loadInstalledPackagesTask;
            return _installedPackages;
        }

        public void StartLoadInstalledPackagesTask(IEnumerable<NuGetProject> projects)
        {
            _projects = projects.ToList();
            _loadInstalledPackagesTask = Task.Run(
                async () =>
                {
                    _installedPackages.Clear();
                    foreach (var project in _projects)
                    {
                        foreach (var package in (await project.GetInstalledPackagesAsync(CancellationToken.None)))
                        {
                            List<NuGetVersion> versions = null;
                            if (_installedPackages.TryGetValue(package.PackageIdentity.Id, out versions))
                            {
                                versions.Add(package.PackageIdentity.Version);
                            }
                            else
                            {
                                _installedPackages.Add(
                                    package.PackageIdentity.Id,
                                    new List<NuGetVersion> { package.PackageIdentity.Version });
                            }
                        }
                    }

                    foreach (var versions in _installedPackages.Values)
                    {
                        versions.Sort();
                    }
                });
        }
    }
}