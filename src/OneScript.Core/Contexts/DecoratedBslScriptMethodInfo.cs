/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OneScript.Contexts.Internal;

namespace OneScript.Contexts
{
    /// <summary>
    /// Декоратор для BslScriptMethodInfo, позволяющий добавлять дополнительные аннотации
    /// при сохранении иммутабельности оригинального метода
    /// </summary>
    public class DecoratedBslScriptMethodInfo : BslScriptMethodInfo, IBuildableMember
    {
        private readonly BslScriptMethodInfo _originalMethod;

        private DecoratedBslScriptMethodInfo(BslScriptMethodInfo originalMethod)
        {
            _originalMethod = originalMethod ?? throw new ArgumentNullException(nameof(originalMethod));
        }

        /// <summary>
        /// Создает декоратор для указанного метода
        /// </summary>
        /// <param name="original">Оригинальный метод для декорирования</param>
        /// <param name="decorator">Опциональный делегат для достройки метода через билдер</param>
        /// <returns>Декорированный метод</returns>
        public static DecoratedBslScriptMethodInfo Create(
            BslScriptMethodInfo original, 
            Action<BslMethodBuilder<DecoratedBslScriptMethodInfo>> decorator = null)
        {
            var decorated = new DecoratedBslScriptMethodInfo(original);
            
            // Инициализируем декоратор из оригинального метода
            var buildable = (IBuildableMethod)decorated;
            buildable.SetName(original.Name);
            buildable.SetDataType(original.ReturnType);
            buildable.SetDeclaringType(original.DeclaringType);
            buildable.SetDispatchIndex(original.DispatchId);
            buildable.SetExportFlag(!original.Attributes.HasFlag(MethodAttributes.Private));
            
            // Аннотации будут объединены при первом вызове GetCustomAttributes

            // Применяем декоратор через билдер, если он передан
            if (decorator != null)
            {
                var builder = new BslMethodBuilder<DecoratedBslScriptMethodInfo>(decorated, () => new BslParameterInfo());
                decorator(builder);
                builder.Build();
            }

            return decorated;
        }

        // Делегируем все свойства в оригинальный метод
        public override Type ReturnType => _originalMethod.ReturnType;
        public override Type DeclaringType => _originalMethod.DeclaringType;
        public override string Name => _originalMethod.Name;
        public override string Alias => _originalMethod.Alias;
        public override Type ReflectedType => _originalMethod.ReflectedType;
        public override MethodImplAttributes GetMethodImplementationFlags() => _originalMethod.GetMethodImplementationFlags();
        public override MethodAttributes Attributes => _originalMethod.Attributes;
        public override RuntimeMethodHandle MethodHandle => _originalMethod.MethodHandle;
        public override MethodInfo GetBaseDefinition() => _originalMethod.GetBaseDefinition();
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => _originalMethod.ReturnTypeCustomAttributes;

        // Параметры всегда делегируются в оригинальный метод
        public override ParameterInfo[] GetParameters() => _originalMethod.GetParameters();
        public new BslParameterInfo[] GetBslParameters() => _originalMethod.GetBslParameters();

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return _originalMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public new bool HasBslAnnotation(Localization.BilingualString name)
        {
            return _originalMethod.HasBslAnnotation(name);
        }

        void IBuildableMember.SetDeclaringType(Type declaringType)
        {
            ((IBuildableMember)_originalMethod).SetDeclaringType(declaringType);
        }
        
        // Переопределяем SetAnnotations для объединения аннотаций
        void IBuildableMember.SetAnnotations(IEnumerable<object> annotations)
        {
            // Получаем аннотации из оригинального метода
            var originalAnnotations = _originalMethod.GetCustomAttributes(false);
            
            // Получаем новые аннотации
            var newAnnotations = annotations.ToArray();
            
            // Объединяем: если тип аннотации уже есть в оригинальных, заменяем все аннотации того же типа на новые
            var merged = new List<object>(originalAnnotations);
            
            // Группируем новые аннотации по типу
            var newAnnotationsByType = newAnnotations.GroupBy(a => a.GetType()).ToDictionary(g => g.Key, g => g.ToArray());
            
            // Удаляем все старые аннотации типов, которые есть в новых
            foreach (var annotationType in newAnnotationsByType.Keys)
            {
                merged.RemoveAll(a => a.GetType() == annotationType);
            }
            
            // Добавляем все новые аннотации
            merged.AddRange(newAnnotations);
            
            base.SetAnnotations(new AnnotationHolder(merged.ToArray()));
        }

        // Переопределяем RetrieveAnnotations для объединения, если аннотации еще не были установлены
        protected override AnnotationHolder RetrieveAnnotations()
        {
            // Если аннотации еще не были установлены через SetAnnotations, 
            // просто возвращаем аннотации из оригинального метода
            var originalAnnotations = _originalMethod.GetCustomAttributes(false);
            return new AnnotationHolder(originalAnnotations);
        }
    }
}

