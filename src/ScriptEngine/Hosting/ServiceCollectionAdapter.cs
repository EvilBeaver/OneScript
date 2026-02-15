/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ScriptEngine.Hosting
{
    public static class ServiceCollectionAdapter
    {
        public static void PopulateContainer(IServiceCollection services, TinyIocImplementation container)
        {
            // Register the container itself as IServiceContainer
            container.RegisterSingleton(typeof(OneScript.DependencyInjection.IServiceContainer), container);
            
            // Group descriptors by service type to detect enumerable registrations
            var groupedDescriptors = services
                .GroupBy(d => d.ServiceType)
                .ToList();
            
            foreach (var group in groupedDescriptors)
            {
                var serviceType = group.Key;
                var descriptors = group.ToList();
                
                // Check if this is an enumerable registration (multiple DIFFERENT implementation types)
                var isEnumerable = IsEnumerableRegistration(descriptors);
                
                if (isEnumerable)
                {
                    // Register all unique implementations as enumerable
                    var processedTypes = new HashSet<Type>();
                    foreach (var descriptor in descriptors)
                    {
                        if (descriptor.ImplementationType != null && 
                            !processedTypes.Contains(descriptor.ImplementationType))
                        {
                            container.RegisterEnumerable(serviceType, descriptor.ImplementationType);
                            processedTypes.Add(descriptor.ImplementationType);
                        }
                    }
                }
                else
                {
                    // Not enumerable - use the last registration (standard DI behavior)
                    var descriptor = descriptors.Last();
                    
                    if (descriptor.Lifetime == ServiceLifetime.Singleton)
                    {
                        if (descriptor.ImplementationInstance != null)
                        {
                            container.RegisterSingleton(descriptor.ServiceType, descriptor.ImplementationInstance);
                        }
                        else if (descriptor.ImplementationFactory != null)
                        {
                            container.RegisterSingleton(descriptor.ServiceType, descriptor.ImplementationFactory);
                        }
                        else if (descriptor.ImplementationType != null)
                        {
                            container.RegisterSingleton(descriptor.ServiceType, descriptor.ImplementationType);
                        }
                    }
                    else if (descriptor.Lifetime == ServiceLifetime.Scoped)
                    {
                        if (descriptor.ImplementationType != null)
                        {
                            container.RegisterScoped(descriptor.ServiceType, descriptor.ImplementationType);
                        }
                    }
                    else // Transient
                    {
                        if (descriptor.ImplementationFactory != null)
                        {
                            container.RegisterTransient(descriptor.ServiceType, descriptor.ImplementationFactory);
                        }
                        else if (descriptor.ImplementationType != null)
                        {
                            container.RegisterTransient(descriptor.ServiceType, descriptor.ImplementationType);
                        }
                    }
                }
            }
            
            container.FinalizeRegistrations();
        }

        private static bool IsEnumerableRegistration(List<ServiceDescriptor> descriptors)
        {
            // Enumerable registration means multiple DIFFERENT implementation types for the same service type
            // If all descriptors have implementation types and there are multiple distinct ones, it's enumerable
            var implementationTypes = descriptors
                .Where(d => d.ImplementationType != null)
                .Select(d => d.ImplementationType)
                .Distinct()
                .ToList();
            
            return implementationTypes.Count > 1;
        }
    }
}
