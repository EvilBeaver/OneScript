# README #

[![Join the chat at https://gitter.im/EvilBeaver/OneScript](https://badges.gitter.im/EvilBeaver/OneScript.svg)](https://gitter.im/EvilBeaver/OneScript?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Настоящий проект является расширением исходного, реализующим HTTP сервисы в среде OneScript.

Настоящее расширение, позволяет создавать HTTP сервисы в среде OneScript, аналогично тому, как это делается в 1С:Предприятие. Поведение и функционал, аналогичны платформе 1С:Предприятие 8.3 (8.3.10.2252), с незначительными отличиями. 

Расширение создано на основе технологии ASP.NET и позволяет создавать HTTP сервисы в операционных системах Windows и Linux.

## Отличия от 1С:Предприятие

В отличии от HTTP сервисов 1С:Предприятия, где построение URL запроса строится следующим образом: http(s)://ИмяСервера:Порт//ИмяИнформационнойБазы/hs/КорневойURL/Шаблон[?ПараметрыЗапроса], в настоящем расширении, URL запроса строится также, как при обращении к обычному сайту (http(s):Порт//ПутьКФайлуOS/ФайлСкрипта.os[?ПараметрыЗапроса]).
Данное отличие связано с отсутствием в OneScript информационной базы и каких-либо объектов, соответствующих шаблонам 1С.
В отличии от 1С:Предприятия, где все обработчики, связанные с определенным WEB-сервисом располагаются в одном модуле, в настоящем расширении, для обработки вызова сервиса используется одна предопределенная функция на скрипт. При необходимости эмуляции поведения платформы, необходимо создать несколько скриптов, которые будут соответствовать шаблону 1С, разместить в них функции обработки соответствующих методов и вызывать их из предопределенной функции.  
В связи с отсутствием шаблонов HTTP сервиса, Свойство ПараметрыURL не будет содержать каких-либо элементов.

## Состав расширения

Расширение состоит из файла HTTPServiceContext.cs, который расположен в папке Library\HTTPServiceContext, проекта ScriptEngine.HostedScript, а также проекта ASPNetHandler, который является частью исходного решения.

Файл HTTPServiceContext.cs содержит реализацию типов HTTPСервисЗапрос и HTTPСервисОтвет, которые используются в работе с HTTP сервисами в 1С:Предприятие. Проект ASPNETHandler реализует обработчик WEB запросов в среде OneScript. 

## Создание скрипта HTTP сервиса

Скрипт HTTP сервиса, представляет собой обычный текстовый файл, содержащий код на языке OneScript, который содержит предопределенную экспортную функцию ОбработкаВызоваHTTPСервиса, которая аналогично обработчику 1С:Предприятие имеет один параметр, типа HTTPСервисЗапрос и должна возвратить объект типа HTTPСервисОтвет.
Ниже представлен типовой фрагмент кода в скрипте HTTP сервиса:

```c++
// Методы, аналогичные обработчикам запросов в 1С:Предприятие. 
// Могут быть перенесены из конфигурации с учетом совместимости с объектами OneScript 
Функция МетодОбработкиGET(Запрос)
	Возврат Новый HTTPСервисОтвет(200);
КонецФункции 

Функция МетодОбработкиPOST(Запрос)
	Возврат Новый HTTPСервисОтвет(200);
КонецФункции

// Предопределенная функция. Она вызывается для обработки запроса
Функция ОбработкаВызоваHTTPСервиса(Запрос) Экспорт

	Если Запрос.HTTPМетод = "GET" Тогда
		Возврат МетодОбработкиGET(Запрос);
	ИначеЕсли Запрос.HTTPМетод = "POST" Тогда
		Возврат МетодОбработкиPOST(Запрос);
	Иначе
		Возврат Новый HTTPСервисОтвет(200);
	КонецЕсли;
		
КонецФункции
```

## Размещение сервиса на WEB-сервере

### Windows


### Linux


## Сайт исходного проекта

Основная информация о проекте, релизы и техдокументация расположены на официальном сайте

http://oscript.io

## Библиотека полезных скриптов

В поставку OneScript уже входит набор наиболее часто используемых пакетов. Эти пакеты разрабатываются в едином репозитарии на github https://github.com/oscript-library и доступны для всем желающим. 
