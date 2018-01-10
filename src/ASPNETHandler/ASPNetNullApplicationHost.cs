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
using System.Threading.Tasks;

using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

namespace OneScript.ASPNETHandler
{
    // Класс-заглушка для вывода Сообщить.
    // Поведение как на сервере 1С
    public class ASPNetNullApplicationHost : IHostApplication
    {
        public ASPNetNullApplicationHost()
        {

        }
        // Обработчик Сообщить, в HTTPСервисе ничего не делает, также поступаем и мы
        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {

        }
        // Непонятно что это, наверное аналог системного диалога, на сервере нет никаких диалогов
        public void ShowExceptionInfo(Exception exc)
        {

        }
        // Мы не можем вводить никаких строк на сервере в 1С это недоступно
        public bool InputString(out string result, int maxLen)
        {
            result = null; 
            return false;
        }
        // У нас нет никаких аргументов командной строки
        public string[] GetCommandLineArguments()
        {
            return new string[0]; // возвращаем массив из 0 аргументов т.к у нас их нет
        }
    }
}
