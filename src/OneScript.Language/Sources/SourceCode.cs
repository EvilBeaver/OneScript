/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.LexicalAnalysis;

// ReSharper disable once CheckNamespace
namespace OneScript.Sources
{
    public class SourceCode
    {
        private readonly ICodeSource _source;
        
        internal SourceCode(string sourceName, ICodeSource source)
        {
            _source = source;
            Name = sourceName;
        }

        public SourceCodeIterator CreateIterator() => new SourceCodeIterator(this); 
        
        public string Location => _source.Location;
        
        public string Name { get; }

        public string GetSourceCode() => _source.GetSourceCode();
    }
}