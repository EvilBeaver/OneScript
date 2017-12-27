using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;


namespace MarshallerTest
{
    [ContextClass("СредстваТелеграм", "TelegramMeans")]
    public class TelegramMeansImpl : AutoContext<TelegramMeansImpl>
    {

        public TelegramMeansImpl()
        {

        }

        [ScriptConstructor(Name = "Без параметров")]
        public static IRuntimeContextInstance Constructor()
        {
            return new TelegramMeansImpl();
        }

        [ContextMethod("НовыйКлиентТелеграмБот", "NewClientTelegramBot")]
        public TelegramBotClientImpl NewClientTelegramBot(string token)
        {
            return new TelegramBotClientImpl(token);
        }
    }

    [ContextClass("КлиентТелеграмБот", "ClientTelegramBot")]
    public class TelegramBotClientImpl : AutoContext<TelegramBotClientImpl>
    {
        public TelegramBotClientImpl(string token)
        {

        }

        [ContextMethod("ПолучитьМоегоПользователя", "GetMyUser")]
        public UserImpl GetMe()
        {
            return new UserImpl();
        }

    }

    [ContextClass("ПользовательТелеграмБот", "UserTelegramBot")]
    public class UserImpl : AutoContext<UserImpl>
    {
        string firstName;
        [ContextProperty("Имя", "Name")]
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value;
            }
        }

        [ContextProperty("Пользователь", "User")]
        public UserImpl User
        {
            get
            {
                return null;
            }
        }

        [ContextMethod("ПолучитьМоегоПользователя", "GetMyUser")]
        public UserImpl GetMyUser(IValue arg1, UserImpl arg2, string arg3)
        {
            return null;
        }
        public UserImpl()
        {

        }
    }

}
