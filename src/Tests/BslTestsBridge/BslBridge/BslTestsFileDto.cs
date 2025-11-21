/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace BslTestsBridge.BslBridge
{
    public class BslTestsFileDto
    {
        public BslTestsFileDto(string testsFile, string fixtureName)
        {
            Path = testsFile;
            FixtureName = fixtureName;
        }

        public string Path { get; }
        
        public string FixtureName { get; }
    }
}

