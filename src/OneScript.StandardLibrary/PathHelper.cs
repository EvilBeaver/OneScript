/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Utility methods for working with file paths
    /// </summary>
    internal static class PathHelper
    {
        /// <summary>
        /// Strips trailing null characters from a path string.
        /// This is needed because Windows WebDAV client can add null characters to the end of paths,
        /// which causes ArgumentException in System.IO methods.
        /// Only trailing null characters are removed to avoid masking potential security issues
        /// with null characters in the middle of paths (e.g., "file.txt\0.exe").
        /// </summary>
        /// <param name="path">Path that may contain trailing null characters</param>
        /// <returns>Path with trailing null characters removed, or null if input was null</returns>
        public static string StripNullCharacters(string path)
        {
            if (path == null)
                return null;
                
            return path.TrimEnd('\0');
        }
    }
}
