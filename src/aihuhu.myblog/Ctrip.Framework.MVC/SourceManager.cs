using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Ctrip.Framework.MVC.Configuration;
using System.IO;

namespace Ctrip.Framework.MVC
{
    public class SourceManager
    {
        /// <summary>
        /// 默认的配置文件路径
        /// </summary>
        private static readonly string SOURCE_CONFIGURATION_PATH = "staticsources.config";
        /// <summary>
        /// 监控配置文件的变化，以及时刷新缓存
        /// </summary>
        private static FileSystemWatcher m_Watcher = null;
        private static object SyncObject = new object();
        private static object SyncObject1 = new object();

        private static SourceConfiguration m_Source;
        private static SourceConfiguration Source
        {
            get
            {
                if (m_Source == null)
                {
                    lock (SyncObject1)
                    {
                        if (m_Source == null)
                        {
                            Configure(SOURCE_CONFIGURATION_PATH, false);
                        }
                    }
                }
                return m_Source;
            }
        }

        /// <summary>
        /// 所有的资源package配置
        /// </summary>
        public static SourceCollection Sources
        {
            get
            {
                return Source.Sources;
            }
        }

        /// <summary>
        /// package配置文件所在目录 ,  默认为应用程序根目录
        /// </summary>
        public static string BaseDirectory
        {
            get
            {
                return Source.BaseDirectory;
            }
        }

        /// <summary>
        /// package 配置文件所在的url地址
        /// </summary>
        public static string BasePath
        {
            get
            {
                return Source.BasePath;
            }
        }

        /// <summary>
        /// 配置资源文件
        /// </summary>
        public static void Configure()
        {
            Configure(SOURCE_CONFIGURATION_PATH);
        }

        /// <summary>
        /// 配置资源文件
        /// </summary>
        /// <param name="packageConfigPath"></param>
        public static void Configure(string packageConfigPath)
        {
            Configure(packageConfigPath, true);
        }

        /// <summary>
        /// 配置资源文件
        /// </summary>
        /// <param name="configuElement"></param>
        public static void Configure(XmlElement configuElement)
        {
            lock (SyncObject)
            {
                m_Source = new SourceConfiguration(configuElement);
                //如果采用element配置，则说明用户自行监控配置文件，无需重复监测，释放文件监控
                if (m_Watcher != null)
                {
                    m_Watcher.Dispose();
                    m_Watcher = null;
                }
            }
        }
        
        private static void Configure(string packageConfigPath, bool hasLock)
        {
            if (hasLock)
            {
                lock (SyncObject)
                {
                    // 读取配置
                    m_Source = new SourceConfiguration(packageConfigPath);
                    //监控配置文件
                    WatcheConfiguration(packageConfigPath);
                }
            }
            else
            {
                // 读取配置
                m_Source = new SourceConfiguration(packageConfigPath);
                //监控配置文件
                WatcheConfiguration(packageConfigPath);
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
    }
}