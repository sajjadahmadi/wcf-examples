using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Reflection;

namespace System.ServiceModel.Errors
{
    public static class ErrorHandlerHelper
    {
        /// <summary>
        /// Promote a CLR exception to a contracted fault if the exception type
        /// matches the detailing type of any of the defined fault contracts.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="error"></param>
        /// <param name="version"></param>
        /// <param name="fault"></param>
        public static void PromoteException(Type serviceType, Exception error, MessageVersion version, ref Message fault)
        {
            if (error is FaultException && error.GetType().IsGenericType)
            {
                // No need to promote... Already FaultException<T>
                Debug.Assert(error.GetType().GetGenericTypeDefinition() == typeof(FaultException<>));
                return; 
            }

            bool inContract = ExceptionInContract(serviceType, error);
            if (!inContract) return;

            try
            {
                Type faultUnboundedType = typeof(FaultException<>);
                Type faultBoundedType = faultUnboundedType.MakeGenericType(error.GetType());
                Exception newException = (Exception)Activator.CreateInstance(error.GetType(), error.Message);
                FaultException faultException = (FaultException)Activator.CreateInstance(faultBoundedType, newException);
                MessageFault messageFault = faultException.CreateMessageFault();
                fault = Message.CreateMessage(version, messageFault, faultException.Action);
            }
            catch
            { }
        }

        static bool ExceptionInContract(Type serviceType, Exception error)
        {
            List<FaultContractAttribute> faultAttribs = new List<FaultContractAttribute>();
            Type[] interfaces = serviceType.GetInterfaces();

            string serviceMethod = GetServiceMethodName(error);
            FaultContractAttribute[] attributes;

            foreach (Type interfaceType in interfaces)
            {
                MethodInfo[] methods = interfaceType.GetMethods();
                foreach (MethodInfo methodInfo in methods)
                {
                    attributes = methodInfo.GetCustomAttributes<FaultContractAttribute>(false);
                    faultAttribs.AddRange(attributes);
                    bool faultExists = faultAttribs.Any<FaultContractAttribute>(fault => fault.DetailType == error.GetType());
                    return faultExists;
                }
            }
            return false;
        }

        static string GetServiceMethodName(Exception error)
        {
            // TODO: Couldn't we get the method name some other way?
            const string WCFPrefix = "SyncInvoke";
            int start = error.StackTrace.IndexOf(WCFPrefix);
            
            Debug.Assert(start != -1, "Method not found. Did they change the prefix?");

            string trimmed = error.StackTrace.Substring(start + WCFPrefix.Length);
            string[] parts = trimmed.Split('(');
            return parts[0];
        }
        
        static T[] GetCustomAttributes<T>(this MethodInfo methodInfo, bool inherit)
        {
            object[] attributes = methodInfo.GetCustomAttributes(typeof(T), inherit);
            return attributes as T[];
        }
    }
}
