using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.Commands.Test
{
    public class SharedContentTests : IDisposable
    {
        [Fact]
        public async Task SharedContent_VerifyLockFile()
        {
            // Arrange
            var workingDir = TestFileSystemUtility.CreateRandomTestFolder();
            _testFolders.Add(workingDir);

            var repository = Path.Combine(workingDir, "repository");
            Directory.CreateDirectory(repository);
            var projectDir = Path.Combine(workingDir, "project");
            Directory.CreateDirectory(projectDir);
            var packagesDir = Path.Combine(workingDir, "packages");
            Directory.CreateDirectory(packagesDir);

            // Create a shared content package
            CreateSharedContentPackage(repository);

            var sources = new List<PackageSource>();
            sources.Add(new PackageSource(repository));

            var configJson = JObject.Parse(@"{
                  ""dependencies"": {
                    ""packageA"": ""1.0.0""
                  },
                  ""frameworks"": {
                    ""net46"": {}
                  }
                }");

            var specPath = Path.Combine(projectDir, "TestProject", "project.json");
            var spec = JsonPackageSpecReader.GetPackageSpec(configJson.ToString(), "TestProject", specPath);

            var request = new RestoreRequest(spec, sources, packagesDir);
            request.MaxDegreeOfConcurrency = 1;
            request.LockFilePath = Path.Combine(projectDir, "project.lock.json");

            var lockFileFormat = new LockFileFormat();
            var logger = new TestLogger();
            var command = new RestoreCommand(logger, request);

            // Act
            var result = await command.ExecuteAsync();
            result.Commit(logger);

            // Assert
            Assert.Equal(0, result.CompatibilityCheckResults.Sum(checkResult => checkResult.Issues.Count));
            Assert.Equal(0, logger.Errors);
            Assert.Equal(0, logger.Warnings);
        }

        private static FileInfo CreateSharedContentPackage(string repositoryDir)
        {
            var file = new FileInfo(Path.Combine(repositoryDir, "packageA.1.0.0.nupkg"));

            using (var zip = new ZipArchive(File.Create(file.FullName), ZipArchiveMode.Create))
            {
                zip.AddEntry("shared.any/config/config.xml", new byte[] { 0 });
                zip.AddEntry("shared.any/images/image.jpg", new byte[] { 0 });
                zip.AddEntry("shared/net45/config/config.xml", new byte[] { 0 });
                zip.AddEntry("shared/net45/images/image.jpg", new byte[] { 0 });
                zip.AddEntry("shared/net45/code/helpers.cs", new byte[] { 0 });
                zip.AddEntry("shared/net45/code/helpers.pp", new byte[] { 0 });
                zip.AddEntry("shared/uap10.0/images/image.jpg", new byte[] { 0 });
                zip.AddEntry("shared/win8/_._", new byte[] { 0 });

                zip.AddEntry("packageA.nuspec", @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <package xmlns=""http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd"">
                        <metadata>
                        <id>packageA</id>
                        <version>1.0.0</version>
                        <title />
                        <buildActions>
                        <group targetFramework=""net45""  defaultAction=""EmbeddedResource"">
                        <buildAction file=""config/config.xml"" action=""CopyToOutput"" />
                        </group>
                        <group defaultAction=""EmbeddedResource"" />
                        </buildActions>
                        </metadata>
                        </package>", Encoding.UTF8);
            }

            return file;
        }

        public void Dispose()
        {
            // Clean up
            foreach (var folder in _testFolders)
            {
                TestFileSystemUtility.DeleteRandomTestFolders(folder);
            }
        }

        private ConcurrentBag<string> _testFolders = new ConcurrentBag<string>();
    }
}