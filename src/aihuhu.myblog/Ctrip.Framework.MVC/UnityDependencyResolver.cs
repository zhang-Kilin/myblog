using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace Ctrip.Framework.MVC
{
    public class UnityDependencyResolver : IDependencyResolver
    {
        private IUnityContainer m_Container = null;

        public UnityDependencyResolver(IUnityContainer container)
        {
            this.m_Container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                object instance = this.m_Container.Resolve(serviceType);

                return instance;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                IEnumerable<object> instances = this.m_Container.ResolveAll(serviceType);
                return instances;
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }
    }
}