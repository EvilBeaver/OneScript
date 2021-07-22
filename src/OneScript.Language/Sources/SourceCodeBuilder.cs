/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

// ReSharper disable once CheckNamespace
namespace OneScript.Sources
{
    public class SourceCodeBuilder
    {
        private ICodeSource _source;
        private string _moduleName;

        private SourceCodeBuilder()
        {
        }

        public SourceCodeBuilder FromSource(ICodeSource source)
        {
            _source = source;
            return this;
        }
        
        public SourceCodeBuilder WithName(string name)
        {
            _moduleName = name;
            return this;
        }

        public SourceCode Build()
        {
            if (_source == default)
                throw new InvalidOperationException("Source is not set");

            if (_moduleName == default)
                _moduleName = _source.Location;

            return new SourceCode(_moduleName, _source);
        }

        public static SourceCodeBuilder Create() => new SourceCodeBuilder();
    }
}