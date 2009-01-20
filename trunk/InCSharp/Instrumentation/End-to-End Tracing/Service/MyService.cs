using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace WcfExamples.EndToEndTracing.Service
{
    public class MyService : IMyContract
    {
        public string GetHeader(string name, string ns)
        {
            Debug.Assert(name != null);
            Debug.Assert(ns != null);

            TraceSource ts = new TraceSource("MyServiceTraceSource");
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            ts.TraceEvent(TraceEventType.Start, 0, "GetHeader Activity");

            try
            {
                Message reqMessage = OperationContext.Current.RequestContext.RequestMessage;
                int headerIndex = reqMessage.Headers.FindHeader(name, ns);
                if (headerIndex != -1)
                    return reqMessage.Headers.GetHeader<string>(headerIndex);
                else
                    throw new FaultException<string>(reqMessage.Headers.Action, "Header not found.  See detail for Action header.");
            }
            finally
            {
                ts.TraceEvent(TraceEventType.Stop, 0, "GetHeader Activity");
            }
        }

    }
}
