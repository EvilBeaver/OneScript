/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Подключение DLL-файлов библиотеки внешних компонент Native API
    /// </summary>
    public class NativeApiKernel
    {
        public static bool IsLinux
        {
            get => System.Environment.OSVersion.Platform == PlatformID.Unix;
        }

        internal static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Операционная система в манифесте библиотеки внешних компонент
        /// </summary>
        internal static string OsName
        {
            get
            {
                if (IsMacOS)
                    return "MacOS";
                return IsLinux ? "Linux" : "Windows";
            }
        }

        internal static string LibraryExtension
        {
            get
            {
                if (IsMacOS)
                    return ".dylib";
                return IsLinux ? ".so" : ".dll";
            }
        }

        public static IntPtr LoadLibrary(string filename)
        {
            return NativeLibrary.TryLoad(filename, out var module) ? module : IntPtr.Zero;
        }

        public static IntPtr GetProcAddress(IntPtr module, string procName)
        {
            return NativeLibrary.TryGetExport(module, procName, out var pointer)
                ? pointer
                : throw new ApplicationException($"Function pointer for {procName} not obtained.");
        }

        public static bool FreeLibrary(IntPtr module)
        {
            NativeLibrary.Free(module);
            return true;
        }

        /// <summary>
        /// Освобождает все подключённые библиотеки Native API.
        /// Идемпотентен; повторный вызов безопасен.
        /// </summary>
        public static void Shutdown()
        {
            NativeApiFactory.Shutdown();
        }
    }
}
