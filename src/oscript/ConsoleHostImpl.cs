/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;
using OneScript.StandardLibrary;

namespace oscript
{
	internal static class ConsoleHostImpl
	{
		public static void Echo(string text, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
			if (status == MessageStatusEnum.Ordinary)
			{
				Output.WriteLine(text);
			}
			else
			{
				var oldColor = Output.TextColor;
				ConsoleColor newColor;

				switch (status)
				{
					case MessageStatusEnum.Information:
						newColor = ConsoleColor.Green;
						break;
					case MessageStatusEnum.Attention:
						newColor = ConsoleColor.Yellow;
						break;
					case MessageStatusEnum.Important:
					case MessageStatusEnum.VeryImportant:
						newColor = ConsoleColor.Red;
						break;
					default:
						newColor = oldColor;
						break;
				}

				try
				{
					Output.TextColor = newColor;
					Output.WriteLine(text);
				}
				finally
				{
					Output.TextColor = oldColor;
				}
			}
		}

		public static void ShowExceptionInfo(Exception exc)
		{ 
			if (exc is ScriptException exception)
			{
			    var rte = exception;
			    Echo(rte.MessageWithoutCodeFragment);
			}
		    else
		         Echo(exc.ToString());
		}

		public static bool InputString(out string result, string prompt, int maxLen, bool multiline)
		{
			if( !String.IsNullOrEmpty(prompt) )
				Console.Write(prompt);

			result = multiline ? ReadMultilineString() : Console.ReadLine();
			
			if (result == null)
				return false;

			if (maxLen > 0 && maxLen < result.Length)
				result = result.Substring(0, maxLen);

			return true;
		}
		
		private static string ReadMultilineString()
        {
			string read;
			System.Text.StringBuilder text = null;

			while (true)
			{
				read = Console.ReadLine();

				if (read == null)
					break;

				if (text == null)
				{
					text = new System.Text.StringBuilder(read);
				}
				else
				{
					text.Append("\n");
					text.Append(read);
				}
			}

			return text?.ToString();
		}

	}
}