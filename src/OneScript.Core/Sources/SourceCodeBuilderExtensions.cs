/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;

namespace OneScript.Sources
{
    public static class SourceCodeBuilderExtensions
    {
        public static SourceCodeBuilder FromString(this SourceCodeBuilder builder, string code)
        {
            builder.FromSource(new StringCodeSource(code));
            return builder;
        }
        
        public static SourceCodeBuilder FromFile(this SourceCodeBuilder builder, string path)
        {
            builder.FromSource(new FileCodeSource(path));
            return builder;
        }
        
        public static SourceCodeBuilder FromFile(this SourceCodeBuilder builder, string path, Encoding encoding)
        {
            builder.FromSource(new FileCodeSource(path, encoding));
            return builder;
        }
    }
}