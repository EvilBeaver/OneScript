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
        public void IdentifiersTrieAdd()
        {
            var t = new IdentifiersTrie<int>();
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
            var t = new IdentifiersTrie<int>();
            t.Add("иначе", 1);
            t.Add("иначеесли", 2);

            Assert.Equal(2, t.Get("ИначеЕсли"));
            Assert.Equal(1, t.Get("Иначе"));
        }

        [Fact]
        public void IdentifiersTrie_Inclusive_Test_ContainsKey()
        {
            var t = new IdentifiersTrie<bool>();
            
            t.Add("ЕслиИначе", true);
            Assert.False(t.ContainsKey("Если"));
            Assert.True(t.ContainsKey("ЕслиИначе"));
        }
        
        [Fact]
        public void IdentifiersTrie_Inclusive_Test_TryGetValue()
        {
            var t = new IdentifiersTrie<int>();
            
            t.Add("МетодОдин", 1);
            t.Add("МетодОдинИДва", 2);

            var exist = t.TryGetValue("Метод", out _);
            
            Assert.False(exist);
        }
    }
}