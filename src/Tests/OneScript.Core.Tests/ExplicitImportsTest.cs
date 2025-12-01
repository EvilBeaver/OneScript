/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Language.Sources;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Debugger;
using Xunit;

namespace OneScript.Core.Tests
{
    [Collection("SystemLogger")]
    public class ExplicitImportsTest : IDisposable
    {
        private readonly List<string> _messages;
        private readonly ISystemLogWriter _logWriter;
        
        // Названия для тестовых скриптов
        private const string LibraryModuleName = "СкриптБиблиотеки";
        private const string LibraryPackageId = "testlib";
        private const string LibraryShortName = "TestLib";

        public ExplicitImportsTest()
        {
            var mock = new Mock<ISystemLogWriter>();
            mock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(str => _messages.Add(str));
            
            _messages = new List<string>();
            _logWriter = mock.Object;
            SystemLogger.SetWriter(_logWriter);
        }

        public void Dispose()
        {
            // Очищаем логгер после каждого теста
            SystemLogger.Reset();
        }

        [Fact]
        public void ExplicitImports_Disabled_NoWarnings()
        {
            var code = CompileClientScript(ExplicitImportsBehavior.Disabled, out var shouldCompile);
            
            shouldCompile.Should().BeTrue();
            _messages.Should().BeEmpty("в режиме Disabled не должно быть предупреждений");
        }

        [Fact]
        public void ExplicitImports_Enabled_CompilationError()
        {
            Action act = () => CompileClientScript(ExplicitImportsBehavior.Enabled, out _);

            act.Should().Throw<CompilerException>()
                .WithMessage("*" + LibraryModuleName + " принадлежит пакету " + LibraryShortName + "*", "должна быть ошибка о неявном импорте");
            
            _messages.Should().BeEmpty("в режиме Enabled выбрасывается исключение, а не пишется в лог");
        }

        [Fact]
        public void ExplicitImports_Warn_HasWarning()
        {
            var code = CompileClientScript(ExplicitImportsBehavior.Warn, out var shouldCompile);
            
            shouldCompile.Should().BeTrue();
            _messages.Should().HaveCount(1, "должно быть одно предупреждение")
                .And.Contain(x => 
                    x.Contains(LibraryModuleName, StringComparison.InvariantCultureIgnoreCase) &&
                    x.Contains(LibraryShortName, StringComparison.InvariantCultureIgnoreCase),
                    "предупреждение должно содержать имя библиотеки и символа");
        }

        [Fact]
        public void ExplicitImports_Development_WithDebugger_CompilationError()
        {
            var debuggerMock = new Mock<IDebugger>();
            debuggerMock.Setup(x => x.IsEnabled).Returns(true);
            
            Action act = () => CompileClientScript(
                ExplicitImportsBehavior.Development, 
                out _, 
                debuggerMock.Object);
            
            act.Should().Throw<CompilerException>()
                .WithMessage("*" + LibraryModuleName + " принадлежит пакету " + LibraryShortName + "*", 
                    "в режиме Development с включенным отладчиком должна быть ошибка");
        }

        [Fact]
        public void ExplicitImports_Development_WithoutDebugger_NoWarnings()
        {
            var debuggerMock = new Mock<IDebugger>();
            debuggerMock.Setup(x => x.IsEnabled).Returns(false);
            
            var code = CompileClientScript(
                ExplicitImportsBehavior.Development, 
                out var shouldCompile, 
                debuggerMock.Object);
            
            shouldCompile.Should().BeTrue();
            _messages.Should().BeEmpty("в режиме Development без отладчика не должно быть предупреждений");
        }

        [Fact]
        public void ExplicitImports_Development_NoDebuggerRegistered_NoWarnings()
        {
            var code = CompileClientScript(
                ExplicitImportsBehavior.Development, 
                out var shouldCompile);
            
            shouldCompile.Should().BeTrue();
            _messages.Should().BeEmpty("в режиме Development без зарегистрированного отладчика не должно быть предупреждений");
        }

        [Fact]
        public void ExplicitImports_WithUseDirective_NoWarnings()
        {
            // Скрипт с явным импортом через директиву #Использовать
            var codeWithImport = $"#Использовать {LibraryShortName}\n" +
                                $"А = {LibraryModuleName}.Метод();";
            
            var instance = CompileUserScript(
                codeWithImport, 
                ExplicitImportsBehavior.Enabled, 
                out var shouldCompile);
            
            shouldCompile.Should().BeTrue();
            _messages.Should().BeEmpty("при явном импорте через #Использовать не должно быть предупреждений");
        }

        [Fact]
        public void ExplicitImports_SamePackage_NoWarnings()
        {
            // Библиотека использует себя - не должно быть предупреждений
            var libraryCode = $"Функция Метод() Экспорт\n" +
                            $"    А = {LibraryModuleName};\n" +
                            $"    Возврат А;\n" +
                            $"КонецФункции";
            
            var instance = CompileLibraryScript(
                libraryCode,
                ExplicitImportsBehavior.Enabled,
                out var shouldCompile);
            
            shouldCompile.Should().BeTrue();
            _messages.Should().BeEmpty("модули из той же библиотеки не требуют явного импорта");
        }

        private IRuntimeContextInstance CompileClientScript(
            ExplicitImportsBehavior behavior, 
            out bool success,
            IDebugger debugger = null)
        {
            // Код клиентского скрипта, который использует библиотеку неявно
            var clientCode = $"А = {LibraryModuleName}.Метод();";
            
            var instance = CompileUserScript(clientCode, behavior, out success, debugger);
            return instance;
        }

        private IRuntimeContextInstance CompileLibraryScript(
            string code,
            ExplicitImportsBehavior behavior,
            out bool success,
            IDebugger debugger = null)
        {
            var instance = CompileUserScript(
                code, 
                behavior, 
                out success, 
                debugger, 
                ownerPackageId: LibraryPackageId);
            
            return instance;
        }

        private IRuntimeContextInstance CompileUserScript(
            string code, 
            ExplicitImportsBehavior behavior,
            out bool success,
            IDebugger debugger = null,
            string ownerPackageId = null)
        {
            success = true;
            
            try
            {
                var configValue = behavior switch
                {
                    ExplicitImportsBehavior.Enabled => "on",
                    ExplicitImportsBehavior.Disabled => "off",
                    ExplicitImportsBehavior.Warn => "warn",
                    ExplicitImportsBehavior.Development => "dev",
                    _ => throw new ArgumentException(nameof(behavior))
                };

                var builder = DefaultEngineBuilder
                    .Create()
                    .SetupConfiguration(p =>
                    {
                        p.Add(() => new Dictionary<string, string>
                        {
                            { "lang.explicitImports", configValue }
                        });
                    })
                    .SetDefaultOptions();

                // Регистрируем отладчик если передан
                if (debugger != null)
                {
                    builder.WithDebugger(debugger);
                }

                // Регистрируем мок резолвера зависимостей
                builder.SetupServices(services =>
                {
                    services.UseImports(_ => new TestDependencyResolver());
                });

                var engine = builder.Build();
                
                // Регистрируем библиотеку как глобальное свойство
                RegisterLibrary(engine);

                var compiler = engine.GetCompilerService();
                var source = ownerPackageId != null 
                    ? SourceCodeBuilder.Create().FromString(code).WithOwnerPackage(ownerPackageId).Build()
                    : engine.Loader.FromString(code);
                
                engine.Initialize();
                
                var module = engine.AttachedScriptsFactory.CompileModuleFromSource(
                    compiler, 
                    source, 
                    null, 
                    engine.NewProcess());
                
                var instance = engine.NewObject(module, engine.NewProcess(), null);
                return instance;
            }
            catch (CompilerException)
            {
                success = false;
                throw;
            }
        }

        private void RegisterLibrary(ScriptingEngine engine)
        {
            // Создаем простой модуль библиотеки
            var libraryCode = "Функция Метод() Экспорт\n" +
                            "    Возврат 42;\n" +
                            "КонецФункции";
            
            var compiler = engine.GetCompilerService();
            var source = SourceCodeBuilder.Create()
                .FromString(libraryCode)
                .WithOwnerPackage(LibraryPackageId)
                .Build();
            engine.Initialize();
            
            var libraryModule = engine.AttachedScriptsFactory.CompileModuleFromSource(
                compiler, 
                source, 
                null, 
                engine.NewProcess());
            
            var libraryInstance = engine.CreateUninitializedSDO(libraryModule);
            
            // Регистрируем библиотеку как глобальное свойство с PackageInfo
            var packageInfo = new PackageInfo(LibraryPackageId, LibraryShortName);
            engine.Environment.InjectGlobalProperty(libraryInstance, LibraryModuleName, packageInfo);
            
            // Инициализируем библиотеку
            engine.InitializeSDO(libraryInstance, engine.NewProcess());
        }

        /// <summary>
        /// Мок резолвера зависимостей для поддержки директивы #Использовать
        /// </summary>
        private class TestDependencyResolver : IDependencyResolver
        {
            public void Initialize(ScriptingEngine engine)
            {
                // Ничего не делаем
            }

            public PackageInfo Resolve(SourceCode module, string libraryName, IBslProcess process)
            {
                // Возвращаем PackageInfo для нашей тестовой библиотеки
                if (libraryName == LibraryShortName)
                {
                    return new PackageInfo(LibraryPackageId, LibraryShortName);
                }
                
                return null;
            }
        }
    }
}

