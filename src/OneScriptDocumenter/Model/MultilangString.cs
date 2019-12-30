using System;
using System.Collections.Generic;

namespace OneScriptDocumenter.Model
{
    class MultilangString : IEnumerable<KeyValuePair<string, string>>
    {
        public const string DEFAULT_LANG = "ru";

        private readonly Dictionary<string, string> _variants = new Dictionary<string,string>();

        public MultilangString(string content)
        {
            if (content == null)
                throw new ArgumentNullException();
            
            Set(DEFAULT_LANG, content);
        }

        public MultilangString(IDictionary<string, string> strings)
        {
            if (strings == null)
                throw new ArgumentNullException();

            _variants = new Dictionary<string, string>(strings);
        }

        private void Set(string lang, string content)
        {
            if (lang == null || content == null)
                throw new ArgumentNullException();

            _variants[lang] = content;
        }

        public string DefaultString 
        { 
            get
            {
                return _variants[DEFAULT_LANG];
            }
            set
            {
                _variants[DEFAULT_LANG] = value;
            }
        }

        public override string ToString()
        {
            return DefaultString;
        }

        public string ToString(string lang)
        {
            return _variants[lang];
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _variants.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static MultilangString New(string content)
        {
            return new MultilangString(content);
        }
    }
}
