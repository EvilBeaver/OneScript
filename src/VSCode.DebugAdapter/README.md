# Сервер отладки 1Script

Данное расширение предоставляет возможность отладки приложений 1Script. На данный момент находится в статусе ознакомительной версии.

## Запуск отладки

* Откройте папку с проектом oscript
* Нажмите F5
* Будет создан файл launch.json с параметрами отладки.

В файле launch.json можно задать несколько так называемых "профилей" отладки - наборов комбинаций параметров, под которым будет запускаться отлаживаемое приложение.

Каждый профиль отладки представляет собой настройку запуска отлаживаемого приложения, его аргументы командной строки и версию интерпретатора 1script, которая будет выполнять приложение.

### Подробное описание каждого параметра выводится при наведении мышки на опцию в файле launch.json.

При возникновении вопросов напишите обращение на https://github.com/EvilBeaver/OneScript/issues. Мы обязательно вам поможем.

## Примеры конфигураций запуска

### Запуск 1Script, файл my-program.os с передачей аргументов командной строки и установкой переменных окружения

```json
{
    "name": "Отладка 1Script",
    "type": "oscript",
    "request": "launch",
    "cwd": "${workspaceRoot}/src",
    "program": "my-program.os",
    "args": ["arg1", "arg2"],
    "env": {
        "OSCRIPT_CONFIG": "lib.system=D:/myOsLibraries",
        "JAVA_HOME": "D:/MyJava/JDK_29_Full"
    },
    "debugPort": 5051
}
```

### Запуск сервера 1Script.Web, установленного по пути e:/osweb на порту 5051

```json
{
    "name": "Отладка 1Script.Web",
    "type": "oscript.web",
    "request": "launch",
    "appDir": "${workspaceRoot}/src",
    "runtimeExecutable": "e:/osweb/OneScript.WebHost.exe",
    "debugPort": 5051
}
```

### Подключение к работающему процессу 1Script.Web на порту 5051

```json
{
    "name": "Отладка 1Script.Web (attach)",
    "type": "oscript.web",
    "request": "attach",
    "debugPort": 5051
}
```