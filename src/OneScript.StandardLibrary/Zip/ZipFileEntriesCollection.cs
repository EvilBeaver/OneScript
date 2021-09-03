﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
{
    [ContextClass("ЭлементыZipФайла", "ZipFileEntries")]
    public class ZipFileEntriesCollection : AutoCollectionContext<ZipFileEntriesCollection, ZipFileEntryContext>
    {
        readonly List<ZipFileEntryContext> _entries;

        public ZipFileEntriesCollection(IEnumerable<ZipEntry> entries)
        {
            _entries = entries.Select(x => new ZipFileEntryContext(x)).ToList();
        }

        [ContextMethod("Количество", "Count")]
        public override int Count()
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

        public override IEnumerator<ZipFileEntryContext> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
    }
}
