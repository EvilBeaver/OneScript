/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Reflection;
using OneScript.Contexts;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TypeReflectionTests
    {
        private ScriptingEngine host;

        public TypeReflectionTests()
        {
            var builder = DefaultEngineBuilder
                .Create()
                .SetDefaultOptions();
            
            host = builder.Build();
            host.Environment = new RuntimeEnvironment();
        }
        
        private LoadedModule LoadFromString(string code)
        {
            var codeSrc = host.Loader.FromString(code);
            var cmp = host.GetCompilerService();
            var image = cmp.Compile(codeSrc);
            var module = host.LoadModuleImage(image);

            return module;
        }

        [Fact]
        public void CheckIfTypeHasReflectedWithName()
        {
            string script = "Перем А;";

            var reflected = CreateDummyType(script);
            Assert.Equal("Dummy", reflected.Name);
            Assert.Equal("ScriptEngine.Machine.Reflection.dyn.Dummy", reflected.FullName);

        }

        private Type CreateDummyType(string script)
        {
            var module = LoadFromString(script);
            var builder = new ClassBuilder(typeof(UserScriptContextInstance));
            var reflected = builder.SetModule(module)
                                   .SetTypeName("Dummy")
                                   .ExportDefaults()
                                   .Build();

            return reflected;
        }

        [Fact]
        public void CheckNonExportVarsArePrivateFields()
        {
            string script = "Перем А; Перем Б Экспорт;";

            var reflected = CreateDummyType(script);

            var props = reflected.GetFields(BindingFlags.NonPublic);
            Assert.Single(props);
            Assert.Equal("А", props[0].Name);
            Assert.Equal(typeof(IValue), props[0].FieldType);
        }

        [Fact]
        public void CheckExportVarsArePublicFields()
        {
            string script = "Перем А; Перем Б Экспорт;";

            var reflected = CreateDummyType(script);

            var props = reflected.GetFields(BindingFlags.Public);
            Assert.Single(props);
            Assert.Equal("Б", props[0].Name);
            Assert.Equal(typeof(IValue), props[0].FieldType);

        }

        [Fact]
        public void CheckDefaultGetMethodsArePublic()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods();
            Assert.Single(defaultGet);
            Assert.Equal("Внешняя", defaultGet[0].Name);
        }

        [Fact]
        public void CheckExplicitPublicMethodsCanBeRetrieved()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.Public);
            Assert.Single(defaultGet);
            Assert.Equal("Внешняя", defaultGet[0].Name);
        }

        [Fact]
        public void CheckPrivateMethodsCanBeRetrieved()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.NonPublic);
            Assert.Single(defaultGet);
            Assert.Equal("Внутренняя", defaultGet[0].Name);
        }

        [Fact]
        public void CheckAllMethodsCanBeRetrieved()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.Public|BindingFlags.NonPublic);
            Assert.Equal(2, defaultGet.Length);
        }

        [Fact]
        public void ClassCanBeCreatedViaConstructor()
        {
            var cb = new ClassBuilder(typeof(UserScriptContextInstance));
            var module = LoadFromString("");
            cb.SetTypeName("testDrive")            
                .SetModule(module)
                .ExportDefaults()
                .ExportConstructor((parameters => new UserScriptContextInstance(module)));
            var type = cb.Build();

            var instance = type.GetConstructors()[0].Invoke(new object[0]);
            Assert.IsAssignableFrom<UserScriptContextInstance>(instance);
        }

        [Fact]
        public void ClassCanExposeNativeMethodByName()
        {
            var cb = new ClassBuilder(typeof(UserScriptContextInstance));
            var module = LoadFromString("");
            cb.SetTypeName("testDrive")
              .SetModule(module)
              .ExportClassMethod("GetMethodsCount")
              .ExportConstructor((parameters => new UserScriptContextInstance(module)));
            var type = cb.Build();

            Assert.NotNull(type.GetMethod("GetMethodsCount"));
        }

        [Fact]
        public void ClassCanExposeNativeMethodDirectly()
        {
            var cb = new ClassBuilder(typeof(UserScriptContextInstance));
            var module = LoadFromString("");
            var nativeMethod = typeof(UserScriptContextInstance).GetMethod("AddProperty",
                                                                           BindingFlags.Public | BindingFlags.Instance,
                                                                           null,
                                                                           new Type[]
                                                                           {
                                                                               typeof(string),
                                                                               typeof(IValue)
                                                                           }, null);
            cb.SetTypeName("testDrive")
              .SetModule(module)
              .ExportClassMethod(nativeMethod)
              .ExportConstructor((parameters => new UserScriptContextInstance(module)));
            var type = cb.Build();

            Assert.NotNull(type.GetMethod("AddProperty"));
        }

        [Fact]
        public void CheckMethodBodyIsNotReflected()
        {
            string script = "Процедура Внутренняя()\n" +
                            "КонецПроцедуры\n\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры\n" +
                            "ТелоМодуля = 2;";

            var reflected = CreateDummyType(script);

            var defaultGet = reflected.GetMethods(BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(2, defaultGet.Length);
        }

        [Fact]
        public void CheckMethodAnnotationsReflected()
        {
            string script = "&Аннотация\n" +
                            "&ДругаяАннотация\n" +
                            "Процедура Внешняя() Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);
            var method = reflected.GetMethod("Внешняя");
            Assert.NotNull(method);
            Assert.Equal(2, method.GetCustomAttributes(typeof(BslAnnotationAttribute), false).Length);

            var first = (BslAnnotationAttribute)method.GetCustomAttributes(typeof(BslAnnotationAttribute), false)[0];
            Assert.Equal("Аннотация", first.Name);
        }

        [Fact]
        public void CheckParametersAnnotationsReflected()
        {
            string script = "Процедура Внешняя(&Аннотация Параметр, ПараметрБезАннотации) Экспорт\n" +
                            "КонецПроцедуры";

            var reflected = CreateDummyType(script);
            var method = reflected.GetMethod("Внешняя");
            Assert.NotNull(method);
            var param = method.GetParameters()[0];

            var first = (BslAnnotationAttribute)param.GetCustomAttributes(typeof(BslAnnotationAttribute), false)[0];
            Assert.Equal("Аннотация", first.Name);
        }
    }
}
