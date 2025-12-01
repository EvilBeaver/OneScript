/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace VSCode.DebugAdapter
{
    public class WorkspaceMapper
    {

        private Workspace _localWorkspace;
        private Workspace _remoteWorkspace;

        [JsonProperty("localPath")]
        private string LocalPathForDeserialization
        {
            set { _localWorkspace = new Workspace(value ?? ""); }
        }

        [JsonProperty("remotePath")]
        private string RemotePathForDeserialization
        {
            set { _remoteWorkspace = new Workspace(value ?? ""); }
        }

        public WorkspaceMapper(){}
        
        public WorkspaceMapper(string localPath, string remotePath) {
            
			this._localWorkspace = new Workspace(localPath);
			this._remoteWorkspace = new Workspace(remotePath);
		}

        public string LocalToRemote(string path)
        {
           return ConvertPath(path, _localWorkspace, _remoteWorkspace); 
        }

        public string RemoteToLocal(string path)
        {
           return ConvertPath(path, _remoteWorkspace, _localWorkspace);
        }

        private string ConvertPath(string path, Workspace fromPrefix, Workspace toPrefix)
        {
            if (string.IsNullOrWhiteSpace(path) || 
                string.IsNullOrWhiteSpace(fromPrefix.Original) || 
                string.IsNullOrWhiteSpace(toPrefix.Original))
                return path;

            var normalizedPath = path.Replace('\\', '/');
            var normalizedFrom = fromPrefix.Normalized;
            
            var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (normalizedPath.StartsWith(toPrefix.Normalized, comparison) ||
                !normalizedPath.StartsWith(normalizedFrom, comparison))
                return path;

            var relativePath = normalizedPath.Substring(normalizedFrom.Length).TrimStart('/');          
            var result = toPrefix.Normalized + "/" + relativePath;
            
            return result;
        }
           
    }

    internal class Workspace
    {
        public string Original;
        public string Normalized;

        internal Workspace(string path)
        {
            this.Original = path;
            this.Normalized = path.Replace('\\', '/').TrimEnd('/');
        }

    }

}