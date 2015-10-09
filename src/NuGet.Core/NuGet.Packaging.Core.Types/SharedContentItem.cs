using System;

namespace NuGet.Packaging.Core
{
    /// <summary>
    /// Represents a SharedContent entry from a nuspec file.
    /// </summary>
    public class SharedContentItem
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
        /// If true the item will be copied to the output folder.
        /// </summary>
        public bool CopyToOutput { get; }

        /// <summary>
        /// If true the content items will keep the same folder structure in the output
        /// folder.
        /// </summary>
        public bool Flatten { get; }

        /// <summary>
        /// Programming language this content is specific to.
        /// </summary>
        /// <remarks>Optional</remarks>
        public string TargetLanguage { get; }

        /// <summary>
        /// Item from SharedContent in the nuspec file.
        /// </summary>
        /// <param name="file">Relative file path.</param>
        /// <param name="action">Build action.</param>
        /// <param name="copyToOutput">Copy the item to the build output directory.</param>
        /// <param name="flatten">Copy the item to the build output directory without 
        /// using the package sub folders.</param>
        public SharedContentItem(
            string file, 
            string action, 
            string targetLanguage, 
            bool copyToOutput, 
            bool flatten)
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
            CopyToOutput = copyToOutput;
            Flatten = flatten;
            TargetLanguage = targetLanguage;
        }
    }
}
