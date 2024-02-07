/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Localization;

namespace OneScript.Contexts
{
    /// <summary>
    /// Опции регистрации предопределенных аннотаций
    /// </summary>
    public class PredefinedInterfaceRegistration : IEquatable<PredefinedInterfaceRegistration>
    {
        public PredefinedInterfaceRegistration(
            MarkerLocation location,
            BilingualString annotation,
            BilingualString methodName)
        {
            Location = location;
            Annotation = annotation;
            MethodName = methodName;
        }

        public MarkerLocation Location { get; }
        public BilingualString Annotation { get; }
        public BilingualString MethodName { get; }
        
        public PredefinedInterfaceRegistration(MarkerLocation location, BilingualString annotation) : this(location, annotation, null)
        {
        }

        /// <summary>
        /// Хелпер для регистрации аннотаций модуля
        /// </summary>
        /// <param name="annotation">Регистрируемая аннотация</param>
        public static PredefinedInterfaceRegistration OnModule(BilingualString annotation)
        {
            return new PredefinedInterfaceRegistration(MarkerLocation.ModuleAnnotation, annotation);
        }

        /// <summary>
        /// Хелпер для регистрации аннотаций на методах
        /// </summary>
        /// <param name="annotation">Регистрируемая аннотация</param>
        /// <param name="methodName">Метод на котором должна быть расположена аннотация</param>
        public static PredefinedInterfaceRegistration OnMethod(BilingualString annotation, BilingualString methodName)
        {
            return new PredefinedInterfaceRegistration(MarkerLocation.SpecificMethodAnnotation, annotation, methodName);
        }

        public bool Equals(PredefinedInterfaceRegistration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Location == other.Location && Equals(Annotation, other.Annotation) && Equals(MethodName, other.MethodName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PredefinedInterfaceRegistration)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Location, Annotation, MethodName);
        }
    }

    public enum MarkerLocation
    {
        ModuleAnnotation,
        SpecificMethodAnnotation
    }
}