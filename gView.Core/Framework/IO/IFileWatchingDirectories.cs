using System.Collections.Generic;
using System.IO;

namespace gView.Framework.IO
{
    public interface IFileWatchingDirectories
    {
        /// <summary>
        /// Gets the directories.
        /// </summary>
        /// <value>The directories.</value>
        List<DirectoryInfo> Directories { get; }

        /// <summary>
        /// Adds the directory.
        /// </summary>
        /// <param name="di">The di.</param>
        void AddDirectory(DirectoryInfo di);

        /// <summary>
        /// Removes the directory.
        /// </summary>
        /// <param name="di">The di.</param>
        void RemoveDirectory(DirectoryInfo di);
    }
}
