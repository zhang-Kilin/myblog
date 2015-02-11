using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using Ctrip.Framework.MVC;

namespace aihuhu.myblog.web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        protected void Application_Start()
        {
            //注册Unity
            IUnityContainer container = new UnityContainer();
            InitUnity(container);
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            container.RegisterType<IControllerActivator, ControllerActivator>();

            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);

            //注册静态资源
            SourceManager.Configure(ConfigurationManager.AppSettings["StaticSourceConfigurationFilePath"]);

            //注册路由器
            RouteManager.Configure(ConfigurationManager.AppSettings["RouterConfigurationFilePath"]);

        }

        private void InitUnity(IUnityContainer container)
        {
            UnityConfigurationSection section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            if (section != null)
            {
                section.Configure(container, "containerOne");
            }
        }
    }
}