using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Refl = System.Reflection;

namespace ScriptEngine.Machine
{
    internal delegate IRuntimeContextInstance InstanceConstructor(IValue[] arguments);

    public class TypeFactory
    {
        private readonly Type _clrType;
        private Dictionary<int, InstanceConstructor> _constructorsCache = new Dictionary<int, InstanceConstructor>();

        public TypeFactory(Type clrType)
        {
            _clrType = clrType;
        }

        public IRuntimeContextInstance CreateInstance(IValue[] arguments)
        {
            if (_constructorsCache.TryGetValue(arguments.Length, out var constructor))
            {
                return constructor(arguments);
            }

            constructor = CreateConstructor(arguments);
            _constructorsCache[arguments.Length] = constructor;

            return constructor(arguments);

        }

        private InstanceConstructor CreateConstructor(IValue[] arguments)
        {
            var ctors = _clrType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute))
                .Select(x => new
                {
                    CtorInfo = x,
                    Parametrized = ((ScriptConstructorAttribute)x.GetCustomAttributes(typeof(ScriptConstructorAttribute), false)[0]).ParametrizeWithClassName
                });


            foreach (var ctor in ctors)
            {
                var parameters = ctor.CtorInfo.GetParameters();
                var processor = new ArgumentsProcessor(parameters);

                
            }

            throw new NotImplementedException();
        }
    }

    internal class ArgumentsProcessor
    {
        private Refl.ParameterInfo[] parameters;

        public ArgumentsProcessor(Refl.ParameterInfo[] parameters)
        {
            this.parameters = parameters;
        }
    }

}
