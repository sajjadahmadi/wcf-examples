using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;

namespace WcfExamples.EndToEndTracing.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create activity to contain the other activities
            var ts = new TraceSource("MyServiceTraceSource");
            Guid mainActivityId = Guid.NewGuid();
            Trace.CorrelationManager.ActivityId = mainActivityId;
            ts.TraceEvent(TraceEventType.Start, 0, "Main Activity");

            // Create new activity and transfer to it
            Guid newActivityId = Guid.NewGuid();
            ts.TraceTransfer(0, "Transferring...", newActivityId);
            Trace.CorrelationManager.ActivityId = newActivityId;
            ts.TraceEvent(TraceEventType.Start, 0, "GetHeader Activity");

            // Call operation
            using (MyServiceClient proxy = new MyServiceClient("WSHttpBinding_MyService"))
            {
                try
                {
                    Console.WriteLine(proxy.GetHeader("MyHeader", "http://tempuri.org"));
                    ts.TraceEvent(TraceEventType.Information, 0, "GetHeader succeeded!");
                }
                catch (FaultException<string> ex)
                {
                    Console.WriteLine(ex.Reason);
                    Console.WriteLine(ex.Detail);
                    ts.TraceEvent(TraceEventType.Error, 0,
                        "GetHeader failed.\nReason:{0}\nDetail{1}",
                        ex.Reason, ex.Detail);
                }
            }

            // Transfer back to main activity
            ts.TraceTransfer(666, "Transferring...", mainActivityId);
            ts.TraceEvent(TraceEventType.Stop, 0, "GetHeader Activity");
            Trace.CorrelationManager.ActivityId = mainActivityId;

            // Stop the main activity
            ts.TraceEvent(TraceEventType.Stop, 0, "Main Activity");

            Console.ReadKey();
        }
    }
}
