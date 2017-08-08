/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
	[ContextClass("Символы", "Chars")]
	public sealed class SymbolsContext : AutoContext<SymbolsContext>
	{
		/// <summary>
		/// Символ перевода строки.
		/// </summary>
		/// <value>Символ перевода строки.</value>
		[ContextProperty("ПС")]
		public string LF
		{
			get
			{
				return "\n";
			}
		}

		/// <summary>
		/// Символ возврата каретки.
		/// </summary>
		/// <value>Символ возврата каретки.</value>
		[ContextProperty("ВК")]
		public string CR
		{
			get
			{
				return "\r";
			}
		}

		/// <summary>
		/// Символ вертикальной табуляции.
		/// </summary>
		/// <value>Символ вертикальной табуляции.</value>
		[ContextProperty("ВТаб")]
		public string VTab
		{
			get
			{
				return "\v";
			}
		}

		/// <summary>
		/// Символ табуляции.
		/// </summary>
		/// <value>Символ горизонтальной табуляции.</value>
		[ContextProperty("Таб")]
		public string Tab
		{
			get
			{
				return "\t";
			}
		}

		/// <summary>
		/// Символ промотки.
		/// </summary>
		/// <value>Символ промотки.</value>
		[ContextProperty("ПФ")]
		public string FF
		{
			get
			{
				return "\f";
			}
		}

		/// <summary>
		/// Символ неразрывного пробела.
		/// </summary>
		/// <value>Символ неразрывного пробела.</value>
		[ContextProperty("НПП")]
		public string Nbsp
		{
			get
			{
				return "\u00A0";
			}
		}

	}
}
