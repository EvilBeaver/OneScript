/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.Sources;

// ReSharper disable once CheckNamespace
namespace OneScript.Sources
{
    public class SourceCode : ISourceCodeIndexer
    {
        private readonly ICodeSource _source;
        private ISourceCodeIndexer _indexer;

        private string _code = null;

        internal SourceCode(string sourceName, ICodeSource source, string ownerPackageId = null)
        {
            _source = source;
            Name = sourceName;
            OwnerPackageId = ownerPackageId;
        }

        public SourceCodeIterator CreateIterator()
        {
            var newIterator = new SourceCodeIterator(this);
            _indexer = newIterator;

            return newIterator;
        }

        public string Location => _source.Location;
        
        public string Name { get; }
        
        /// <summary>
        /// Идентификатор пакета-владельца. null если модуль не принадлежит библиотеке.
        /// </summary>
        public string OwnerPackageId { get; }

        public string GetSourceCode()
        {
            // Однократное считывание того, что отдано на компиляцию
            // При изменении источника (напр. файла) в любом случае потребуется перекомпиляция и смена номеров строк.
            return _code ??= _source.GetSourceCode();
        }

        public string GetCodeLine(int lineNumber)
        {
            return _indexer?.GetCodeLine(lineNumber);
        }

        public override string ToString() => Name ?? Location;
    }
}
