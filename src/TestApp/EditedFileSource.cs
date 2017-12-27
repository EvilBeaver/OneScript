/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ScriptEngine.Environment;

namespace TestApp
{
	internal class EditedFileSource : ICodeSource
	{
		private readonly string _code;

		private readonly string _path = "";

		public EditedFileSource(string code, string path)
		{
			if (path != "")
				_path = Path.GetFullPath(path);
			_code = code;
		}

		private string GetCodeString()
		{
			return _code;
		}

		#region ICodeSource Members

		string ICodeSource.Code => GetCodeString();

		string ICodeSource.SourceDescription => _path != "" ? _path : "<string>";

		#endregion
	}
}