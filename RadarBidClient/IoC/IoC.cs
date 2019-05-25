using Autofac;
using Autofac.Core;
using log4net;
using Radar.Common;
using Radar.dm;
using Radar.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Radar.ioc
{
    public class IoC
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(IoC));

        public static readonly IoC me = new IoC();

        private IoC()
        {
            initContainer();
        }


        private IContainer Container;

        private void initContainer()
        {
            long s1 = KK.CurrentMills();

            var builder = new ContainerBuilder();
            
            // 为加快加载速度 - 过滤出本项目的Assembly

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            logger.DebugFormat("assemblies length is {0}", assemblies?.Length);

            Assembly[] projAsses = assemblies.Where(a => a.FullName.StartsWith("Radar")).ToArray<Assembly>();

            foreach (Assembly asse in projAsses)
            {
                logger.InfoFormat("Assembly FullName is {0}", asse.FullName);
            }

            var componentClasses = new List<Type>();

            builder.RegisterAssemblyTypes(projAsses)
                .Where(t => {
                    var arr = t.GetCustomAttributes(typeof(ComponentAttribute), false);
                    var ret = arr != null && arr.Length > 0;
                    
                    if (ret)
                    {
                        componentClasses.Add(t);
                    }
                    return ret;
                })
                .AsSelf()
                .SingleInstance()
                .OnActivated(e => {
                    // 这里的功能类似 IStartable 
                    object inst = e.Instance;
                    if ( typeof(InitializingBean).IsAssignableFrom(inst.GetType()) )
                    {
                        try
                        {
                            ((InitializingBean) inst).AfterPropertiesSet();
                        } catch(Exception ex)
                        {
                            logger.Error("call AfterPropertiesSet for class#" + inst + " error", ex);
                            throw ex;
                        }
                    }
                })
                ;


            Container = builder.Build();

            logger.InfoFormat("init IoC Container done. Components count#{0}. version is {1}. elasped {2}ms.", componentClasses.Count, Ver.ver, KK.CurrentMills() - s1);
            
            // 初始化 InitializingBean
            foreach (var t in componentClasses)
            {
                if (typeof(InitializingBean).IsAssignableFrom(t))
                {
                    Container.Resolve(t);
                }
            }


        }


        public T Get<T>() {
            return Container.Resolve<T>();
        }

    }

    //[MetadataAttribute]
    public class ComponentAttribute : Attribute
    {

    }

    public interface InitializingBean 
    {
        /// <summary>
        /// 初始化 Component 之后执行
        /// </summary>
        void AfterPropertiesSet();

    }
    
}
