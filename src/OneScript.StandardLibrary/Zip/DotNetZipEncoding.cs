/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using System.Text;
using Ionic.Zip;

namespace OneScript.StandardLibrary.Zip
{
    internal static class DotNetZipEncoding
    {
        private static volatile bool _encodingIsSet;
        private static readonly object _locker = new object();

        static DotNetZipEncoding()
        {
            _encodingIsSet = false;
        }

        public static void SetDefault(Encoding encoding)
        {
            if (_encodingIsSet)
                return;

            lock (_locker)
            {
                if (_encodingIsSet)
                    return;
                
                SetDefaultEncodingViaProperty(encoding);
                _encodingIsSet = true;
            }
        }

        private static void SetDefaultEncodingViaProperty(Encoding encoding)
        {
            ZipFile.DefaultEncoding = encoding;
        }
        
        private static void SetDefaultEncodingViaReflection(Encoding encoding)
        {
            const string fieldName = "_defaultEncoding";

            var field = typeof(ZipFile).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
                throw new InvalidOperationException("Field for default encoding not found");

            field.SetValue(null, encoding);

            if (!Equals(ZipFile.DefaultEncoding, encoding))
            {
                throw new InvalidOperationException(
                    "Field is set, but property DefaultEncoding hasn't changed. Probably another property implementation in library");
            }
        }
    }
}