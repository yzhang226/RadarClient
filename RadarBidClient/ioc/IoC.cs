using Autofac;
using Autofac.Core;
using log4net;
using RadarBidClient.dm;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RadarBidClient.ioc
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
            long s1 = KK.currentTs();

            var builder = new ContainerBuilder();
            
            // 为加快加载速度 - 过滤出本项目的Assembly

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            logger.DebugFormat("assemblies length is {0}", assemblies?.Length);

            Assembly[] projAsses = assemblies.Where(a => a.FullName.StartsWith("Radar")).ToArray<Assembly>();

            foreach (Assembly asse in projAsses)
            {
                logger.InfoFormat("Assembly FullName is {0}", asse.FullName);
            }


            builder.RegisterAssemblyTypes(projAsses)
                .Where(t => {
                    var arr = t.GetCustomAttributes(typeof(ComponentAttribute), false);
                    var ret = arr != null && arr.Length > 0;
                    return ret; })
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
                
                // .AutoActivate() // 参考 https://autofaccn.readthedocs.io/en/latest/lifetime/startup.html
                ;


            Container = builder.Build();

            logger.InfoFormat("init IoC Container done, elasped {0}ms", KK.currentTs() - s1);
  
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
