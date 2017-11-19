# README #

[![Join the chat at https://gitter.im/EvilBeaver/OneScript](https://badges.gitter.im/EvilBeaver/OneScript.svg)](https://gitter.im/EvilBeaver/OneScript?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Настоящий проект является расширением исходного, реализующим HTTP сервисы в среде OneScript.

Использование настоящего расширения, позволяет создавать HTTP сервисы в среде OneScript, аналогично тому, как это делается в 1С:Предприятие.

Расширение создано на основе технологии ASP.NET и позволяет создавать HTTP сервисы на операционных системах Windows и Linux.

## Состав расширения

Расширение состоит из файла HTTPServiceContext.cs, который расположен в папке Library\HTTPServiceContext, проекта ScriptEngine.HostedScript, а также проекта ASPNetHandler, который является частью исходного решения.

Файл HTTPServiceContext.cs содержит реализацию типов HTTPСервисЗапрос и HTTPСервисОтвет, которые используются в работе с HTTP сервисами в 1С:Предприятие. Проект ASPNETHandler реализует обработчик WEB запросов в среде OneScript. 

## Сайт проекта

Основная информация о проекте, релизы и техдокументация расположены на официальном сайте

http://oscript.io

## Библиотека полезных скриптов

В поставку OneScript уже входит набор наиболее часто используемых пакетов. Эти пакеты разрабатываются в едином репозитарии на github https://github.com/oscript-library и доступны для всем желающим. 
