/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;

namespace NUnitTests.Bsl
{
    internal static class RepositoryRootLocator
    {
        private static readonly Lazy<string> RepoRoot = new Lazy<string>(FindRoot);

        public static string ResolvePath(string relativePath)
        {
            if (relativePath == null)
                throw new ArgumentNullException(nameof(relativePath));

            var combined = Path.Combine(RepoRoot.Value, relativePath);
            return Path.GetFullPath(combined);
        }

        private static string FindRoot()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "tests"))
                    && Directory.Exists(Path.Combine(directory.FullName, "src")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Не удалось определить корень репозитория для поиска .os тестов.");
        }
    }
}

