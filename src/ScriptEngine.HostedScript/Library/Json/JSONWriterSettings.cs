/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;



/// <summary>
/// 
/// Определяет набор параметров, используемых при записи JSON.
/// </summary>
[ContextClass("ПараметрыЗаписиJSON", "JSONWriterSettings")]
public class JSONWriterSettings : AutoContext<JSONWriterSettings>
{

    private bool _useDoubleQuotes;

    private IValue _newLines;

    private string _paddingSymbols;

    private IValue _escapeCharacters;

    private bool _escapeAmpersand;

    private bool _escapeSingleQuotes;

    private bool _escapeLineTerminators;

    private bool _escapeSlash;

    private bool _escapeAngleBrackets;

public JSONWriterSettings(IValue NewLines = null, string PaddingSymbols = null, bool UseDoubleQuotes = true, IValue EscapeCharacters = null, bool EscapeAngleBrackets = false, bool EscapeLineTerminators = false, bool EscapeAmpersand = false, bool EscapeSingleQuotes = false, bool EscapeSlash = false)
{
        _newLines = NewLines;
        _paddingSymbols = PaddingSymbols;
        _useDoubleQuotes = UseDoubleQuotes;
        _escapeCharacters = EscapeCharacters;
        _escapeAngleBrackets = EscapeAngleBrackets;
        _escapeLineTerminators = EscapeLineTerminators;
        _escapeAmpersand = EscapeAmpersand;
        _escapeSingleQuotes = EscapeSingleQuotes;
        _escapeSlash = EscapeSlash;
}


    /// <summary>
    /// 
    /// Создает объект параметров записи JSON.
    /// </summary>
    ///
    /// <param name="NewLines">
    /// Определяет способ переноса строк, который будет использован при записи данных JSON.
    /// Значение по умолчанию: Авто. </param>
    /// <param name="PaddingSymbols">
    /// Определяет символы отступа, используемые при записи данных JSON.
    /// Применяется только, если значение ПереносСтрокJSON отлично от Нет.
    /// Значение по умолчанию: " ". </param>
    /// <param name="UseDoubleQuotes">
    /// Определяет, будут ли при записи имена свойств JSON записываться в двойных кавычках.
    /// Значение по умолчанию: Истина. </param>
    /// <param name="EscapeCharacters">
    /// Определяет используемый способ экранирования (замены) символов при записи данных JSON.
    /// Значение по умолчанию: Нет. </param>
    /// <param name="EscapeAngleBrackets">
    /// Определяет, будут ли при записи экранироваться символы '&lt;' и '&gt;'.
    /// Значение по умолчанию: Ложь. </param>
    /// <param name="EscapeLineTerminators">
    /// Определяет, будут ли экранироваться разделители строк - U+2028 (line-separator) и U+2029 (page-separator).
    /// Значение по умолчанию: Истина. </param>
    /// <param name="EscapeAmpersand">
    /// Определяет, будет ли при записи экранироваться символ амперсанда '&amp;'.
    /// Значение по умолчанию: Ложь. </param>
    /// <param name="EscapeSingleQuotes">
    /// Определяет, будут ли экранироваться одинарные кавычки.
    /// Устанавливается в значение Истина, если ИспользоватьДвойныеКавычки установлено в Ложь.
    /// Значение по умолчанию: Ложь. </param>
    /// <param name="EscapeSlash">
    /// Определяет, будет ли экранироваться слеш (косая черта) при записи значения.
    /// Значение по умолчанию: Ложь. </param>
    [ScriptConstructor]
    public static IRuntimeContextInstance Constructor(IValue NewLines = null, IValue PaddingSymbols = null, IValue UseDoubleQuotes = null, IValue EscapeCharacters = null, IValue EscapeAngleBrackets = null, IValue EscapeLineTerminators = null, IValue EscapeAmpersand = null, IValue EscapeSingleQuotes = null, IValue EscapeSlash = null)
{
	return new JSONWriterSettings(NewLines,
        (PaddingSymbols == null ? null : PaddingSymbols.AsString()),
        (UseDoubleQuotes == null?true:UseDoubleQuotes.AsBoolean()),
        EscapeCharacters,
        (EscapeAngleBrackets == null ? true : EscapeAngleBrackets.AsBoolean()),
        (EscapeLineTerminators == null ? true : EscapeLineTerminators.AsBoolean()),
        (EscapeAmpersand == null ? true : EscapeAmpersand.AsBoolean()),
        (EscapeSingleQuotes == null ? true : EscapeSingleQuotes.AsBoolean()),
        (EscapeSlash == null ? true : EscapeSlash.AsBoolean()));
    }

/// <summary>
/// 
/// 
/// </summary>
///
[ScriptConstructor]
public static IRuntimeContextInstance Constructor()
{
	return new JSONWriterSettings();
}

/// <summary>
/// 
/// Определяет использование двойных кавычек при записи свойств и значений JSON.
/// После создания объекта данное свойство имеет значение Истина.
/// </summary>
/// <value>Булево (Boolean)</value>
[ContextProperty("ИспользоватьДвойныеКавычки", "UseDoubleQuotes")]
public bool UseDoubleQuotes
{
	get { return _useDoubleQuotes; }
	
}


/// <summary>
/// 
/// Управляет размещением начала и конца объектов и массивов, ключей и значений на новой строке.
/// Также установка данного свойства в значение, отличное от Нет, добавляет пробел между именем свойства, двоеточием и значением.
/// После создания объекта данное свойство имеет значение Авто.
/// </summary>
/// <value>ПереносСтрокJSON (JSONLineBreak)</value>
[ContextProperty("ПереносСтрок", "NewLines")]
public IValue NewLines
{
	get { return _newLines; }
	
}


/// <summary>
/// 
/// Определяет символы отступа, используемые при записи документа JSON.
/// Свойство не используется, если свойство ПереносСтрокJSON установлено в значение Нет.
/// После создания объекта данное свойство имеет значение - один пробельный символ.
/// </summary>
/// <value>Строка (String)</value>
[ContextProperty("СимволыОтступа", "PaddingSymbols")]
public string PaddingSymbols
{
	get { return _paddingSymbols; }
	
}


/// <summary>
/// 
/// Определяет способ экранирования символов при записи документа JSON.
/// После создания объекта данное свойство имеет значение СимволыВнеASCII.
/// </summary>
/// <value>ЭкранированиеСимволовJSON (JSONCharactersEscapeMode)</value>
[ContextProperty("ЭкранированиеСимволов", "EscapeCharacters")]
public IValue EscapeCharacters
{
	get { return _escapeCharacters; }
	
}


/// <summary>
/// 
/// Определяет, будет ли экранироваться знак амперсанда при записи документа JSON.
/// После создания объекта данное свойство имеет значение Ложь.
/// </summary>
/// <value>Булево (Boolean)</value>
[ContextProperty("ЭкранироватьАмперсанд", "EscapeAmpersand")]
public bool EscapeAmpersand
{
	get { return _escapeAmpersand; }
	
}


/// <summary>
/// 
/// Определяет, будет ли экранирован знак одинарной кавычки при записи документа JSON.
/// Имеет значение Истина, если ИспользоватьДвойныеКавычки установлен Ложь.
/// После создания объекта данное свойство имеет значение Ложь
/// </summary>
/// <value>Булево (Boolean)</value>
[ContextProperty("ЭкранироватьОдинарныеКавычки", "EscapeSingleQuotes")]
public bool EscapeSingleQuotes
{
	get { return _escapeSingleQuotes; }	
}


/// <summary>
/// 
/// Определяет экранирование символов "U+2028" (разделитель строк) и "U+2029" (разделитель абзацев) для совместимости с JavaScript.
/// После создания объекта данное свойство имеет значение Истина.
/// </summary>
/// <value>Булево (Boolean)</value>
[ContextProperty("ЭкранироватьРазделителиСтрок", "EscapeLineTerminators")]
public bool EscapeLineTerminators
{
	get { return _escapeLineTerminators; }
	
}


/// <summary>
/// 
/// Определяет, будет ли экранироваться слеш (косая черта) при записи значения.
/// После создания объекта данное свойство имеет значение Ложь.
/// </summary>
/// <value>Булево (Boolean)</value>
[ContextProperty("ЭкранироватьСлеш", "EscapeSlash")]
public bool EscapeSlash
{
	get { return _escapeSlash; }
	
}


/// <summary>
/// 
/// Определяет, будут ли экранированы знаки угловых скобок при записи документа JSON.
/// После создания объекта данное свойство имеет значение Ложь.
/// </summary>
/// <value>Булево (Boolean)</value>
[ContextProperty("ЭкранироватьУгловыеСкобки", "EscapeAngleBrackets")]
public bool EscapeAngleBrackets
{
	get { return _escapeAngleBrackets; }
	
}

}
