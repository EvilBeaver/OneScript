/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Ionic.Zip;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [ContextClass("ЭлементыZipФайла", "ZipFileEntries")]
    public class ZipFileEntriesCollection : AutoContext<ZipFileEntriesCollection>, ICollectionContext
    {
        List<ZipFileEntryContext> _entries;

        public ZipFileEntriesCollection(IEnumerable<ZipEntry> entries)
        {
            _entries = entries.Select(x => new ZipFileEntryContext(x)).ToList();
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _entries.Count;
        }

        [ContextMethod("Получить", "Get")]
        public IValue Get(IValue index)
        {
            return GetIndexedValue(index);
        }

        [ContextMethod("Найти", "Find")]
        public IValue Find(string name)
        {
            var entry = _entries.FirstOrDefault(x => System.IO.Path.GetFileName(x.GetZipEntry().FileName) == name);

            if (entry == null)
                return ValueFactory.Create();

            return entry;
        }

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            int idx = (int)index.AsNumber();
            return _entries[idx];
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
