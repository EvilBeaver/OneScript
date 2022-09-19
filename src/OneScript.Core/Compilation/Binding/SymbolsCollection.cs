using System;
using System.Collections;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Compilation.Binding
{
    public class SymbolsCollection<T> : IReadOnlyList<T>
        where T : ISymbol
    {
        private readonly IndexedNameValueCollection<T> _storage = new IndexedNameValueCollection<T>();

        public int Add(T symbol)
        {
            CodeError SelectMessage(T item, string text)
            {
                return item switch
                {
                    IVariableSymbol v => LocalizedErrors.DuplicateVarDefinition(text),
                    IMethodSymbol m => LocalizedErrors.DuplicateMethodDefinition(text),
                    _ => throw new InvalidOperationException()
                };
            }

            var index = AddItem(symbol);
            if (index == -1)
                throw new BindingException(SelectMessage(symbol, symbol.Name));

            if (!AddAlias(index, symbol))
                throw new BindingException(SelectMessage(symbol, symbol.Alias));

            return index;
        }
        
        public IEnumerator<T> GetEnumerator() => _storage.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _storage.Count;

        public T this[int index] => _storage[index];
        
        public T this[string index] => _storage[index];

        public int IndexOf(string name) => _storage.IndexOf(name);

        public bool IsDefined(string name) => _storage.IndexOf(name) >= 0;
        
        private int AddItem(T item)
        {
            try
            {
                return _storage.Add(item, item.Name);
            }
            catch (InvalidOperationException)
            {
                return -1;
            }
        }
        
        private bool AddAlias(int index, T item)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.Alias))
                    return true;
                
                _storage.AddName(index, item.Alias);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}