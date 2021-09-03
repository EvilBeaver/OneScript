/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

// ReSharper disable once CheckNamespace
namespace OneScript.Sources
{
    public class SourceCode : ISourceCodeIndexer
    {
        private readonly ICodeSource _source;
        private ISourceCodeIndexer _indexer;

        internal SourceCode(string sourceName, ICodeSource source)
        {
            _source = source;
            Name = sourceName;
        }

        public SourceCodeIterator CreateIterator()
        {
            var newIterator = new SourceCodeIterator(this);
            _indexer = newIterator;

            return newIterator;
        }

        public string Location => _source.Location;
        
        public string Name { get; }

        public string GetSourceCode() => _source.GetSourceCode();

        public string GetCodeLine(int index)
        {
            return _indexer?.GetCodeLine(index);
        }

        public override string ToString() => Name ?? Location;
    }
}