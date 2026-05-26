/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.StandardLibrary;

namespace ScriptEngine.HostedScript
{
    /// <summary>
    /// Интерфейс взаимодействия с приложением, которое хостит движок скрипта.
    /// Хост-приложение может быть не консольным, а 3D-игрой или GUI-окном, поэтому движок не знает
    /// куда выводить текст в каждом конкретном случае. Это определяет реализация IHostApplication.
    /// </summary>
    public interface IHostApplication
    {
		/// <summary>
		/// Вывод пользовательских сообщений методом Сообщить.
		/// </summary>
		/// <param name="str">Строка</param>
		/// <param name="status">Статус сообщения (второй параметр метода Сообщить)</param>
        void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary);
		
        /// <summary>
        /// Показать информацию о неперехваченном исключении.
        /// </summary>
        /// <param name="exc">Неперехваченное исключение</param>
		void ShowExceptionInfo(Exception exc);
        
        /// <summary>
        /// Ввод данных методом ВвестиСтроку. Хост должен предоставить средства ввода текста.
        /// </summary>
        bool InputString(out string result, string prompt, int maxLen, bool multiline);
        
        /// <summary>
        /// Аргументы командной строки, доступные скрипту в коллекции АргументыКоманднойСтроки.
        /// Хост может при запуске получать собственные аргументы, которые не нужны или не должны быть видны в скриптах,
        /// поэтому хост может отфильтровать строку запуска и предоставить скрипту только аргументы непосредственно скрипта.
        /// </summary>
        string[] GetCommandLineArguments();
    }
}
