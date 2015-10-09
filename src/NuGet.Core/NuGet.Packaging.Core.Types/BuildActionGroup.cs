using System;
using System.Collections.Generic;
using NuGet.Frameworks;

namespace NuGet.Packaging.Core
{
    /// <summary>
    /// Framework specific group of build action entries.
    /// </summary>
    public class BuildActionGroup : IFrameworkSpecific
    {
        /// <summary>
        /// Group target framework
        /// </summary>
        public NuGetFramework TargetFramework { get; }

        /// <summary>
        /// Build action entries
        /// </summary>
        public IReadOnlyList<BuildActionEntry> BuildActions { get; }

        /// <summary>
        /// Default build action. This is applied to all items not in
        /// <see cref="BuildActions"/>.
        /// </summary>
        public string DefaultAction { get; }

        /// <summary>
        /// Framework specific group of build action entries.
        /// </summary>
        /// <param name="targetFramework">Target framework</param>
        /// <param name="buildActions">Build action entries</param>
        /// <param name="defaultAction">Default action to apply to 
        /// all other items.</param>
        public BuildActionGroup(
            NuGetFramework targetFramework,
            IReadOnlyList<BuildActionEntry> buildActions,
            string defaultAction)
        {
            if (targetFramework == null)
            {
                throw new ArgumentNullException(nameof(targetFramework));
            }

            if (buildActions == null)
            {
                throw new ArgumentNullException(nameof(buildActions));
            }

            if (defaultAction == null)
            {
                throw new ArgumentNullException(nameof(defaultAction));
            }

            TargetFramework = targetFramework;
            BuildActions = buildActions;
            DefaultAction = defaultAction;
        }
    }
}
