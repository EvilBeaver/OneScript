/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Xunit;

namespace OneScript.Language.Tests
{
    public class TrieTests
    {
        [Fact]
        public void LexemTrieAdd()
        {
            var t = new LexemTrie<int>();
            t.Add("Иван", 0);
            t.Add("Иволга", 1);

            Assert.Equal(1, t.Get("Иволга"));
            Assert.Equal(1, t.Get("ивОлга"));
            Assert.Equal(0, t.Get("Иван"));
            Assert.Equal(0, t.Get("иван"));
            Assert.Equal(0, t.Get("ивАн"));
        }

        [Fact]
        public void Tokens()
        {
            var t = new LexemTrie<int>();
            t.Add("иначе", 1);
            t.Add("иначеесли", 2);

            Assert.Equal(2, t.Get("ИначеЕсли"));
            Assert.Equal(1, t.Get("Иначе"));
        }
    }
}