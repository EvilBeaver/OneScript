# OneScript #

[![Join telegram chat](https://img.shields.io/badge/chat-telegram-blue?style=flat&logo=telegram)](https://t.me/oscript_library) [![DEV Build Status](https://build.oscript.io/buildStatus/icon?job=1Script%2Fdevelop&style=flat-square&subject=dev)](https://build.oscript.io/job/1Script/job/develop/) [![STABLE Build Status](https://build.oscript.io/buildStatus/icon?job=1Script%2Fmaster&style=flat-square&subject=stable)](https://build.oscript.io/job/1Script/job/master/)

## Проект является независимой кросс-платформенной реализацией виртуальной машины, исполняющей скрипты на языке 1С:Предприятие ##

![Logo](.github/logo-small.png)

При этом библиотеки системы 1С:Предприятие не используются и не требуется установка системы 1С:Предприятие на целевой машине.

Иными словами, это инструмент для написания и выполнения программ на языке 1С без использования платформы 1С:Предприятие.

## Название и произношение ##

Проект носит название OneScript, может быть сокращен при написании до названия 1Script. Произносится как `[уанскрипт]`.

OneScript позволяет создавать и выполнять текстовые сценарии, написанные на языке, знакомом любому специалисту по системе 1С:Предприятие. Применение знакомого языка для скриптовой автоматизации позволяет значительно повысить продуктивность специалиста за счет более простой автоматизации ручных операций.

## Сайт проекта ##

Основная информация о проекте, релизы и техдокументация расположены на официальном сайте

[https://oscript.io](https://oscript.io)

## Библиотека полезных скриптов ##

В поставку OneScript уже входит набор наиболее часто используемых пакетов. Эти, а также другие пакеты находятся в репозитории [oscript-library](https://github.com/oscript-library) и доступны всем желающим. Имеется пакетный менеджер [opm](https://github.com/oscript-library/opm).

## Установка ##

### Windows ###

- (интерактивно) скачать c [официального сайта](https://oscript.io) или установщик из раздела [Releases](https://github.com/EvilBeaver/OneScript/releases) и запустить его. Далее, Далее, Готово.

### Linux ###

- (интерактивно) скачать нужный пакет [официального сайта](https://oscript.io) или установщик из раздела [Releases](https://github.com/EvilBeaver/OneScript/releases) и установить его.

### MacOS ###

Интерактивного установщика нет, но движок можно установить из командной строки:

- установить [homebrew](https://brew.sh/index_ru)
- установить mono командой `brew install mono`
- скачать [ovm](https://github.com/oscript-library/ovm/releases)
- выполнить команду `mono ovm.exe install stable`
- выполнить команду `mono ovm.exe use stable`
- перезапустить терминал

# Ручная локальная сборка

## Подготовка

Ниже приведены ссылки на дистрибутивы, однако, учтите, что ссылки могут меняться со временем и их актуальность не гарантируется. Нужен dotnet SDK и компилятор C++, скачать можно из любого места, которое нагуглится.

* Установить [MS BuildTools](https://visualstudio.microsoft.com/ru/thank-you-downloading-visual-studio/?sku=buildtools&rel=16), при установке включить таргетинг на .net6, .net4.8, установить компилятор C++.
* Установить [InnoSetup](https://jrsoftware.org/isdl.php)
* Скачать [OneScriptDocumenter](https://github.com/EvilBeaver/OneScriptDocumenter/releases) и установить в произвольный каталог на диске
* Создать произвольный каталог библиотек и разместить в нем библиотеки, которые нужно будет включить в поставку. Проще всего создать пустой каталог и установить в него пакеты менеджером opm

```bat
opm install -d E:\my_libraries asserts
opm install -d E:\my_libraries gitsync
opm install -d E:\my_libraries fs
```

## Сборка

Запустить Developer Command Prompt (появится в меню Пуск после установки MSBuildTools или Visual Studio). Перейти в каталог репозитория OneScript. Далее приведены команды в консоли Developer Command Prompt
Сборка выполняется с помощью msbuild. Таргеты:

* CleanAll - очистка результатов предыдущих сборок
* PrepareDistributionContent - сборка файлов для поставки в один каталог
* CreateDistributions - упаковка файлов в разные типы дистрибутивов (zip, exe, nuget)

**Параметры сборки**

* ReleaseNumber - номер релиза, который будет прописан в файлах
* OneScriptDocumenter - путь к exe файлу OneScriptDocumenter.exe (если не указать, документация не собирается)
* StandardLibraryPacks - путь к каталогу, который будет являться поставляемым каталогом библиотек (библиотеки оттуда будут размещены в дистрибутиве в подпапке lib). Если не указан, библиотеки в дистрибутив не включаются.
* InnoSetupPath - путь к каталогу установки InnoSetup. Обязателен, если собираем инсталлятор (таргет CreateDistributions)

Все поставляемые файлы будут размещены в каталоге `built` в корне репозитория 1Script

### Сборка содержимого дистрибутивов в отдельном каталоге

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionContent
```

### Сборка с ручным указанием версии

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionContent /p:ReleaseNumber=1.99.6
```

#### Сборка библиотек и документации

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionContent /p:ReleaseNumber=1.99.6 /p:OneScriptDocumenter=path-to-documenter.exe /p:StandardLibraryPacks=E:\my_libraries
```

#### Сборка библиотек, документации и инсталлятора

```bat
msbuild Build.csproj /t:CleanAll;PrepareDistributionContent;CreateDistributions /p:ReleaseNumber=1.99.6 /p:OneScriptDocumenter=path-to-documenter.exe /p:StandardLibraryPacks=E:\my_libraries /p:InnoSetupPath=path-to-innosetup-install-dir
```

