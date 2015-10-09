using System;
using System.Collections.Generic;
using NuGet.Frameworks;

namespace NuGet.Packaging.Core
{
    /// <summary>
    /// Framework specific group of build action properties for shared content items.
    /// </summary>
    public class SharedContentGroup : IFrameworkSpecific
    {
        /// <summary>
        /// Group target framework
        /// </summary>
        public NuGetFramework TargetFramework { get; }

        /// <summary>
        /// Shared content item entries
        /// </summary>
        public IReadOnlyList<SharedContentItem> SharedContentItems { get; }

        /// <summary>
        /// Default build action. This is applied to all items not in
        /// <see cref="SharedContentItems"/>.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Default copyToOutput value. This is applied to all items not in
        /// <see cref="SharedContentItems"/>.
        /// </summary>
        public bool CopyToOutput { get; }

        /// <summary>
        /// Default flatten value. This is applied to all items not in
        /// <see cref="SharedContentItems"/>.
        /// </summary>
        public bool Flatten { get; }

        /// <summary>
        /// Default target language. This is applied to all items not in
        /// <see cref="SharedContentItems"/>.
        /// </summary>
        /// <remarks>Optional</remarks>
        public string TargetLanguage { get; }

        /// <summary>
        /// Framework specific group of build action entries.
        /// </summary>
        /// <param name="targetFramework">Target framework</param>
        /// <param name="sharedItems">Build action entries</param>
        /// <param name="action"> default build action.</param>
        /// <param name="copyToOutput">default copyToOutput for the group.</param>
        /// <param name="targetLanguage">default targetLanguage for the group.</param>
        /// <param name="flatten">default flatten content value.</param>
        public SharedContentGroup(
            NuGetFramework targetFramework,
            IReadOnlyList<SharedContentItem> sharedItems,
            string action,
            string targetLanguage,
            bool copyToOutput,
            bool flatten)
        {
            if (targetFramework == null)
            {
                throw new ArgumentNullException(nameof(targetFramework));
            }

            if (sharedItems == null)
            {
                throw new ArgumentNullException(nameof(sharedItems));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            TargetFramework = targetFramework;
            SharedContentItems = sharedItems;
            Action = action;
            TargetLanguage = targetLanguage;
            Flatten = flatten;
            CopyToOutput = copyToOutput;
        }
    }
}
