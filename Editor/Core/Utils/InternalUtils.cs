using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wireframe
{
    public static class InternalUtils
    {
        private static List<AService> allServices = null;
        private static List<Type> allBuildSources = null;
        private static List<Type> allBuildDestinations = null;
        
        public static List<AService> AllServices()
        {
            if (allServices == null)
            {
                FetchAllTypes();
            }
            
            return allServices;
        }
        
        public static T GetService<T>() where T : AService
        {
            foreach (AService service in AllServices())
            {
                if (service is T t)
                {
                    return t;
                }
            }
            
            return null;
        }
        
        public static List<Type> AllBuildSources()
        {
            if (allBuildSources == null)
            {
                FetchAllTypes();
            }
            
            return allBuildSources;
        }
        
        public static List<Type> AllBuildDestinations()
        {
            if (allBuildDestinations == null)
            {
                FetchAllTypes();
            }
            
            return allBuildDestinations;
        }
        
        private static void FetchAllTypes()
        {
            allServices = new List<AService>();
            allBuildSources = new List<Type>();
            allBuildDestinations = new List<Type>();
            
            Type sourceType = typeof(ABuildSource);
            Type serviceType = typeof(AService);
            Type destinationType = typeof(ABuildDestination);
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass) continue;
                if (type.IsAbstract) continue;
                
                if (sourceType.IsAssignableFrom(type))
                {
                    allBuildSources.Add(type);
                }
                else if (serviceType.IsAssignableFrom(type))
                {
                    allServices.Add((AService)Activator.CreateInstance(type));
                }
                else if (destinationType.IsAssignableFrom(type))
                {
                    allBuildDestinations.Add(type);
                }
            }
        }
    }
}