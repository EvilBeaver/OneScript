/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScriptDocumenter.Primary
{
    public class PrimaryDocsCollector
    {
        public PrimaryDocumentation Collect(IReadOnlyCollection<string> inputAssemblies)
        {
            ConsoleLogger.Info("Collecting primary documentation");
            
            var documentation = new PrimaryDocumentation();
            
            using var assemblyReader = new AssemblyLoader();

            var assemblies = assemblyReader.LoadAssemblies(inputAssemblies);
            foreach (var keyAndValue in assemblies)
            {
                var assembly = keyAndValue.Value;

                var collector = new AssemblyDocsCollector(assembly);

                if (!collector.CollectDocumentation(documentation))
                {
                    ConsoleLogger.Warning($"XmlDoc file for assembly {assembly.Location} not found");
                }
            }

            return documentation;
        }
    }
}