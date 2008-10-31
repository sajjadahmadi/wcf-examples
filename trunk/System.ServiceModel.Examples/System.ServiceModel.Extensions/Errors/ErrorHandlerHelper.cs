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
        public static void PromoteException(Type serviceType, Exception error, MessageVersion version, ref Message fault)
        {
            // TODO: Why are we checking this?
            if (error is FaultException && error.GetType().IsGenericType)
            {
                Debug.Assert(error.GetType().GetGenericTypeDefinition() == typeof(FaultException<>));
                return;
            }

            if (!ExceptionInContract(serviceType, error)) return;

            try
            {
                // TODO: Try to understand this.
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
                    attributes = GetFaults(methodInfo);
                    faultAttribs.AddRange(attributes);
                    bool faultExists = faultAttribs.Any<FaultContractAttribute>(fault => fault.DetailType == error.GetType());
                    return faultExists;
                }
            }
            return false;
        }

        static string GetServiceMethodName(Exception error)
        {
            const string WCFPrefix = "SyncInvoke";
            int start = error.StackTrace.IndexOf(WCFPrefix);
            if (start != -1) { Debug.Fail("Method not found."); return string.Empty; }

            string trimmed = error.StackTrace.Substring(start + WCFPrefix.Length);
            string[] parts = trimmed.Split('(');
            return parts[0];
        }
        static FaultContractAttribute[] GetFaults(MethodInfo methodInfo)
        {
            object[] attributes = methodInfo.GetCustomAttributes(typeof(FaultContractAttribute), false);
            return attributes as FaultContractAttribute[];
        }
    }
}
