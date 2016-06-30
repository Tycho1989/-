using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Configuration;
using System.ServiceModel;

using ZT.FCL.SysService.Interface;


namespace ZT.FCL.Service
{
    /// <summary>
    /// 说明：服务代理工厂
    /// 作者：李浩
    /// 时间：2014-1-7 10：41：22
    /// </summary>
    public static class WCF
    {
        /// <summary>
        /// 连接线路类型
        /// </summary>
        public static string LineType
        {
            set;
            get;
        }
        /// <summary>
        /// 服务地址
        /// </summary>
        public static string ServerAddress
        {
            set;
            get;
        }
        /// <summary>
        /// 配置服务的地址
        /// </summary>
        public static string ServiceConfigAddress;
        /// <summary>
        /// 服务配置信息缓存
        /// </summary>
        private static Dictionary<string, ServiceConfigInfo> _serviceCache;



        /// <summary>
        /// 静态构造器
        /// </summary>
        static WCF()
        {
            WCF._serviceCache = new Dictionary<string, ServiceConfigInfo>();
            WCF.ServiceConfigAddress = ConfigurationManager.AppSettings["serviceConfigAddress"];
            WCF.InitServiceConfig();
        }
        /// <summary>
        /// 初始化服务
        /// </summary>
        private static void InitServiceConfig()
        {
            IList<ServiceConfigInfo> serviceList = null;

            if (string.IsNullOrEmpty(WCF.ServiceConfigAddress))
            {
                throw new Exception("配置服务的地址未配置");
            }

            try
            {
                serviceList = WCF.Create<IServiceConfig>(WCF.ServiceConfigAddress).GetSysServices();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("获取系统服务异常:{0}", ex.Message));
            }

            foreach (ServiceConfigInfo serviceConfig in serviceList)
            {
                WCF._serviceCache.Add(serviceConfig.InterfaceName, serviceConfig);
            }
        }
        /// <summary>
        /// 设置指定连接线路上的服务地址
        /// </summary>
        /// <param name="lineType">连接线路类型</param>
        public static void SetServerAddress(string lineType)
        {
            WCF.ServerAddress = WCF.Create<IServiceConfig>(WCF.ServiceConfigAddress).GetServerAddress(lineType);
        }
        /// <summary>
        /// 设置指定连接线路上的服务地址
        /// </summary>
        /// <param name="line">客户端选择的线路信息</param>
        public static void SetServerAddress(ServiceLine line)
        {
            WCF.ServerAddress = WCF.Create<IServiceConfig>(WCF.ServiceConfigAddress).GetServer(line);
        }


        /// <summary>
        /// 创建服务代理
        /// </summary>
        /// <typeparam name="T">接口类型</typeparam>
        /// <returns>服务代理</returns>
        public static T Create<T>()
        {
            return WCF.Create<T>(null, ServiceBindTypes.BasicHttpBinding, null);
        }
        /// <summary>
        /// 创建服务代理
        /// </summary>
        /// <typeparam name="T">接口类型</typeparam>
        /// <param name="endpointAddress">终结点地址</param>
        /// <returns>服务代理</returns>
        public static T Create<T>(string endpointAddress)
        {
            return WCF.Create<T>(endpointAddress, ServiceBindTypes.BasicHttpBinding);
        }
        /// <summary>
        /// 创建服务代理
        /// </summary>
        /// <typeparam name="T">接口类型</typeparam>
        /// <returns>服务代理</returns>
        public static T CreateDuplex<T>(object callback, bool isPush = false)
        {
            return WCF.Create<T>(null, ServiceBindTypes.NetTcpBinding, callback, isPush);
        }
        /// <summary>
        /// 创建服务代理
        /// </summary>
        /// <typeparam name="T">接口类型</typeparam>
        /// <param name="endpointAddress">终结点地址</param>
        /// <param name="bindType">绑定类型</param>
        /// <param name="callback">回调函数</param>
        /// <param name="isPush"></param>
        /// <returns>服务代理</returns>
        public static T Create<T>(string endpointAddress = null, ServiceBindTypes bindType = ServiceBindTypes.BasicHttpBinding, object callback = null, bool isPush = false)
        {
            ServiceConfigInfo serviceConfig = null;

            if (string.IsNullOrEmpty(endpointAddress))
            {
                //从服务信息缓存中取服务
                WCF._serviceCache.TryGetValue(typeof(T).FullName, out serviceConfig);
                if (serviceConfig == null)
                {
                    throw new Exception(string.Format("请求的服务[{0}]地址不存在,无法访问.", typeof(T).FullName));
                }

                //指定了服务器地址
                if (!string.IsNullOrEmpty(WCF.ServerAddress))
                {
                    //替换指定连接线路的服务端地址
                    endpointAddress = serviceConfig.ReplaceHost(WCF.ServerAddress);
                }
                else
                {
                    endpointAddress = serviceConfig.EndPointAddress;
                }
                bindType = serviceConfig.BindType;
            }
            else
            {
                //指定了服务器地址
                if (!string.IsNullOrEmpty(WCF.ServerAddress))
                {
                    string host = endpointAddress.Substring(endpointAddress.IndexOf("//") + 2, endpointAddress.LastIndexOf(':') - endpointAddress.IndexOf("//") - 2);

                    //替换指定连接线路的服务端地址
                    endpointAddress = endpointAddress.Replace(host, WCF.ServerAddress);
                }
            }

            return (T)new ServiceRealProxy<T>(endpointAddress, bindType).GetTransparentProxy();
        }
    }
}
