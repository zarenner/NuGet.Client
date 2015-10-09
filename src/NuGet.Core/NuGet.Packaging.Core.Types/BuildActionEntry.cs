using System;

namespace NuGet.Packaging.Core
{
    /// <summary>
    /// Represents a BuildAction entry from a nuspec file.
    /// </summary>
    public class BuildActionEntry
    {
        /// <summary>
        /// Relative file path or wild card path.
        /// </summary>
        /// <remarks>Does not contain the TFM folder.</remarks>
        public string File { get; }

        /// <summary>
        /// Build action
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Build action
        /// </summary>
        /// <param name="file">Relative file path.</param>
        /// <param name="action">Build action.</param>
        public BuildActionEntry(string file, string action)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            File = file;
            Action = action;
        }
    }
}
