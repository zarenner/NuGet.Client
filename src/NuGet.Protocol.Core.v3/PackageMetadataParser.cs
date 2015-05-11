﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGet.Protocol.Core.v3
{
    public static class PackageMetadataParser
    {
        public static ServerPackageMetadata ParseMetadata(JObject metadata)
        {
            var version = NuGetVersion.Parse(metadata.Value<string>(Properties.Version));
            DateTimeOffset? published = null;
            var publishedToken = metadata[Properties.Published];
            if (publishedToken != null)
            {
                published = publishedToken.ToObject<DateTimeOffset>();
            }

            var id = metadata.Value<string>(Properties.PackageId);
            var title = metadata.Value<string>(Properties.Title);
            var summary = metadata.Value<string>(Properties.Summary);
            var description = metadata.Value<string>(Properties.Description);
            var authors = GetFieldAsArray(metadata, Properties.Authors);
            var owners = GetFieldAsArray(metadata, Properties.Owners);
            var iconUrl = GetUri(metadata, Properties.IconUrl);
            var licenseUrl = GetUri(metadata, Properties.LicenseUrl);
            var projectUrl = GetUri(metadata, Properties.ProjectUrl);
            var tags = GetFieldAsArray(metadata, Properties.Tags);
            var dependencySets = (metadata.Value<JArray>(Properties.DependencyGroups) ?? Enumerable.Empty<JToken>()).Select(obj => LoadDependencySet((JObject)obj));
            var requireLicenseAcceptance = metadata[Properties.RequireLicenseAcceptance] == null ? false : metadata[Properties.RequireLicenseAcceptance].ToObject<bool>();
            IEnumerable<string> types = metadata.Value<string>(Properties.Type).Split(' ');

            //Uri reportAbuseUrl =
            //    _reportAbuseResource != null ?
            //    _reportAbuseResource.GetReportAbuseUrl(id, version) :
            //    null;

            if (String.IsNullOrEmpty(title))
            {
                // If no title exists, use the Id
                title = id;
            }

            // TODO: populate these
            NuGetVersion minClientVersion = null;
            var downloadCount = 0;
            var downloadCountForVersion = 0;

            return new ServerPackageMetadata(new PackageIdentity(id, version), title, summary, description,
                authors, iconUrl, licenseUrl, projectUrl, tags, published, dependencySets,
                requireLicenseAcceptance, minClientVersion, downloadCount, downloadCountForVersion, owners, types);
        }

        private static IEnumerable<string> GetFieldAsArray(JObject json, string property)
        {
            var value = json[property];

            if (value == null)
            {
                return Enumerable.Empty<string>();
            }

            var array = value as JArray;

            if (array != null)
            {
                return array.Select(e => e.ToString());
            }
            else
            {
                return new string[] { value.ToString() };
            }
        }

        /// <summary>
        /// Returns a field value or the empty string. Arrays will become comma delimited strings.
        /// </summary>
        private static string GetField(JObject json, string property)
        {
            var value = json[property];

            if (value == null)
            {
                return string.Empty;
            }

            var array = value as JArray;

            if (array != null)
            {
                return String.Join(", ", array.Select(e => e.ToString()));
            }

            return value.ToString();
        }

        private static Uri GetUri(JObject json, string property)
        {
            if (json[property] == null)
            {
                return null;
            }
            var str = json[property].ToString();
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }
            return new Uri(str);
        }

        private static PackageDependencyGroup LoadDependencySet(JObject set)
        {
            var fxName = set.Value<string>(Properties.TargetFramework);

            var framework = NuGetFramework.AnyFramework;

            if (!String.IsNullOrEmpty(fxName))
            {
                framework = NuGetFramework.Parse(fxName);
                fxName = framework.GetShortFolderName();
            }

            return new PackageDependencyGroup(framework,
                (set.Value<JArray>(Properties.Dependencies) ?? Enumerable.Empty<JToken>()).Select(obj => LoadDependency((JObject)obj)));
        }

        private static PackageDependency LoadDependency(JObject dep)
        {
            var ver = dep.Value<string>(Properties.Range);
            return new PackageDependency(
                dep.Value<string>(Properties.PackageId),
                String.IsNullOrEmpty(ver) ? null : VersionRange.Parse(ver));
        }
    }
}