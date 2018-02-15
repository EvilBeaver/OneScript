/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
	public static class Program
	{

		public static Encoding ConsoleOutputEncoding
		{
			get { return Output.ConsoleOutputEncoding; }
			set { Output.ConsoleOutputEncoding = value; }
		}

        public enum Loc { en, ru };

        public static Loc LocaleMessage = Loc.en;

		public static int Main(string[] args)
		{
			int returnCode;

			var behavior = BehaviorSelector.Select(args);
			try
			{
				returnCode = behavior.Execute();
			}
			catch (Exception e)
			{
				// сюда при выполнении скрипта мы попадать не должны
				// исключения времени выполнения выводятся в IApplicationHost.ShowExceptionInfo
				// здесь мы пишем только если случилось что-то совсем плохое
				Output.WriteLine(e.ToString());
				returnCode = 1;
			}

			return returnCode;
		}
	}
}