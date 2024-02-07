/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Commons;
using OneScript.Execution;

namespace OneScript.Contexts
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PredefinedInterfaceResolver
    {
        private readonly ISet<CheckerData> _checkers;
        
        private class CheckerData : PredefinedInterfaceRegistration
        {
            public IPredefinedInterfaceChecker Checker { get; }

            public CheckerData(
                IPredefinedInterfaceChecker checker,
                PredefinedInterfaceRegistration source) 
                : base(source.Location, source.Annotation, source.MethodName)
            {
                Checker = checker;
            }
        }
        
        public PredefinedInterfaceResolver(IEnumerable<IPredefinedInterfaceChecker> checkers)
        {
            _checkers = MapCheckers(checkers);
        }
        
        public void Resolve(IExecutableModule module)
        {
            var triggeredCheckers = new HashSet<IPredefinedInterfaceChecker>();
            
            _checkers
                .Where(check => !triggeredCheckers.Contains(check.Checker))
                .Where(check => 
                    check.Location == MarkerLocation.ModuleAnnotation 
                    && module.ModuleAttributes.Any(attr => check.Annotation.HasName(attr.Name)))
                .ForEach(check =>
                {
                    triggeredCheckers.Add(check.Checker);
                    check.Checker.Validate(module);
                });

            foreach (var methodInfo in module.Methods.Where(m => m is BslScriptMethodInfo).Cast<BslScriptMethodInfo>())
            {
                _checkers
                    .Where(check => !triggeredCheckers.Contains(check.Checker))
                    .Where(check =>
                        check.Location == MarkerLocation.SpecificMethodAnnotation &&
                        check.MethodName.HasName(methodInfo.Name) &&
                        methodInfo.HasBslAnnotation(check.Annotation))
                    .ForEach(check => 
                    {
                        triggeredCheckers.Add(check.Checker);
                        check.Checker.Validate(module);
                    });
            }
        }
        
        private ISet<CheckerData> MapCheckers(IEnumerable<IPredefinedInterfaceChecker> checkers)
        {
            var result = new HashSet<CheckerData>();

            foreach (var sourceCheck in checkers)
            {
                sourceCheck.GetRegistrations().ForEach(x => result.Add(new CheckerData(sourceCheck, x)));
            }

            return result;
        }
    }
}