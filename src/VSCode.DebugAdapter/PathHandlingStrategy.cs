/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace VSCode.DebugAdapter
{
    public class PathHandlingStrategy
    {
	    public bool DebuggerLinesStartAt1 { get; set; } = true;
	    public bool DebuggerPathsAreUri { get; set; } = false;
	    public bool ClientLinesStartAt1 { get; set; } = true;
	    public bool ClientPathsAreUri { get; set; } = true;

	    public int ConvertDebuggerLineToClient(int line)
        {
        	if (DebuggerLinesStartAt1) {
        		return ClientLinesStartAt1 ? line : line - 1;
        	}
        	else {
        		return ClientLinesStartAt1 ? line + 1 : line;
        	}
        }

        public int ConvertClientLineToDebugger(int line)
        {
        	if (DebuggerLinesStartAt1) {
        		return ClientLinesStartAt1 ? line : line + 1;
        	}
        	else {
        		return ClientLinesStartAt1 ? line - 1 : line;
        	}
        }

        public string ConvertDebuggerPathToClient(string path)
        {
        	if (DebuggerPathsAreUri) {
        		if (ClientPathsAreUri) {
        			return path;
        		}
        		else {
        			Uri uri = new Uri(path);
        			return uri.LocalPath;
        		}
        	}
        	else {
        		if (ClientPathsAreUri) {
        			try {
        				var uri = new System.Uri(path);
        				return uri.AbsoluteUri;
        			}
        			catch {
        				return null;
        			}
        		}
        		else {
        			return path;
        		}
        	}
        }

        public string ConvertClientPathToDebugger(string clientPath)
        {
        	if (clientPath == null) {
        		return null;
        	}

        	if (DebuggerPathsAreUri) {
        		if (ClientPathsAreUri) {
        			return clientPath;
        		}
        		else {
        			var uri = new System.Uri(clientPath);
        			return uri.AbsoluteUri;
        		}
        	}
        	else {
        		if (ClientPathsAreUri) {
        			if (Uri.IsWellFormedUriString(clientPath, UriKind.Absolute)) {
        				Uri uri = new Uri(clientPath);
        				return uri.LocalPath;
        			}
        			Console.Error.WriteLine("path not well formed: '{0}'", clientPath);
        			return null;
        		}
        		else {
        			return clientPath;
        		}
        	}
        }
    }
}