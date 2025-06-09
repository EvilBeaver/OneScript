/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace OneScriptDocumenter.Primary
{
    public class PrimaryDocumentation : IEnumerable<PrimaryBslDocument>
    {
        private readonly Dictionary<Type, PrimaryBslDocument> _documents =
            new Dictionary<Type, PrimaryBslDocument>();

        public ReferenceCollector ReferenceCollector { get; } = new ReferenceCollector();
        
        public void Add(PrimaryBslDocument doc)
        {
            _documents.Add(doc.Owner, doc);
        }

        public PrimaryBslDocument Get(Type key) => _documents[key];
        
        public IEnumerator<PrimaryBslDocument> GetEnumerator()
        {
            return _documents.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}