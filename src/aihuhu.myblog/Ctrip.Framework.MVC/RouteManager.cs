using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Ctrip.Framework.MVC.Configuration;
using System.Xml;
using System.Web.Routing;
using System.Web.Mvc;
using Ctrip.Framework.MVC.CodeDom;

namespace Ctrip.Framework.MVC
{
    public class RouteManager
    {
        /// <summary>
        /// 默认的配置文件路径
        /// </summary>
        private static readonly string CONFIGURATION_PATH = "router.config";

        /// <summary>
        /// 监控配置文件的变化，以及时刷新缓存
        /// </summary>
        private static FileSystemWatcher m_Watcher = null;
        private static object SyncObject = new object();
        private static object SyncObject1 = new object();
        
        private static RouterConfiguration m_Router;
        private static RouterConfiguration Router
        {
            get
            {
                if (m_Router == null)
                {
                    lock (SyncObject1)
                    {
                        if (m_Router == null)
                        {
                            Configure(CONFIGURATION_PATH, false);
                        }
                    }
                }
                return m_Router;
            }
        }

        /// <summary>
        /// 所有的route配置
        /// </summary>
        public static RouteConfigurationCollection Routes
        {
            get
            {
                return Router.Routes;
            }
        }

        /// <summary>
        /// 所有的ignore route配置
        /// </summary>
        public static IgnoreRouteConfigurationCollection IgnoreRoutes
        {
            get
            {
                return Router.IgnoreRoutes;
            }
        }

        /// <summary>
        /// route配置文件所在目录 ,  默认为应用程序根目录
        /// </summary>
        public static string BaseDirectory
        {
            get
            {
                return Router.BaseDirectory;
            }
        }

        /// <summary>
        /// route 配置文件所在的url地址
        /// </summary>
        public static string BasePath
        {
            get
            {
                return Router.BasePath;
            }
        }

        /// <summary>
        /// 用于配置默认 Router
        /// </summary>
        public static void Configure()
        {
            Configure(CONFIGURATION_PATH);
        }

        /// <summary>
        /// 用于配置Router
        /// </summary>
        /// <param name="routeConfigPath">router配置文件路径</param>
        public static void Configure(string routeConfigPath)
        {
            Configure(routeConfigPath, true);
        }

        /// <summary>
        /// 用于配置Router
        /// </summary>
        /// <param name="routeConfigureElement">配置文件内容</param>
        public static void Configure(XmlElement routeConfigureElement)
        {
            lock (SyncObject)
            {
                m_Router = new RouterConfiguration(routeConfigureElement);
                //如果采用element配置，则说明用户自行监控配置文件，无需重复监测，释放文件监控
                if (m_Watcher != null)
                {
                    m_Watcher.Dispose();
                    m_Watcher = null;
                }
            }
        }


        private static void Configure(string routeConfigPath, bool hasLock)
        {
            if (hasLock)
            {
                lock (SyncObject)
                {
                    // 读取配置
                    m_Router = new RouterConfiguration(routeConfigPath);
                    MapRoutes();
                    //监控配置文件
                    WatcheConfiguration(routeConfigPath);
                }
            }
            else
            {
                // 读取配置
                m_Router = new RouterConfiguration(routeConfigPath);
                MapRoutes();
                //监控配置文件
                WatcheConfiguration(routeConfigPath);
            }
        }

        private static void WatcheConfiguration(string filePath)
        {
            string fullPath = Utility.ConvertToFullPath(filePath);
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(filePath);

            if (m_Watcher != null   //当前文件监控可用
                && (
                    m_Watcher.Path.ToLower() != directory.ToLower()  //文件夹发生变化
                    || m_Watcher.Filter.ToLower() != fileName.ToLower() //文件发生变化
                ))
            {
                //需要重新设定watcher
                m_Watcher.Dispose();
                m_Watcher = null;
            }

            if (m_Watcher == null)
            {
                m_Watcher = new FileSystemWatcher(directory);
                m_Watcher.EnableRaisingEvents = true;
                m_Watcher.Filter = fileName;
                m_Watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

                m_Watcher.Changed += new FileSystemEventHandler((s, e) =>
                {
                    Configure(e.FullPath, true);
                });
            }
        }

        private static void MapRoutes()
        {
            using (RouteTable.Routes.GetWriteLock())
            {
                RouteTable.Routes.Clear();

                MapRoutes(RouteTable.Routes, IgnoreRoutes);

                MapRoutes(RouteTable.Routes, Router);
            }
        }

        private static void MapRoutes(RouteCollection routes, RouterConfiguration router)
        {
            if (router != null
                && router.Routes != null)
            {
                IDictionary<string, object> parameterInstances = new DynamicCreateRouteParameterInstances().CreateRouteParametersInstance(router);
                foreach (RouteConfiguration config in router.Routes)
                {
                    if (parameterInstances.ContainsKey(config.Name))
                    {
                        object parameter = parameterInstances[config.Name];
                        routes.MapRoute(config.Name, config.Url, parameter);
                    }
                    else
                    {
                        routes.MapRoute(config.Name, config.Url);
                    }
                }
            }
        }

        private static void MapRoutes(RouteCollection routes, IgnoreRouteConfigurationCollection ignoreRouteConfigCollection)
        {
            if (ignoreRouteConfigCollection != null)
            {
                foreach (IgnoreRouteConfiguration config in ignoreRouteConfigCollection)
                {
                    routes.IgnoreRoute(config.Path);
                }
            }
        }
    }
}