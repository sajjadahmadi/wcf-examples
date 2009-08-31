open System
open System.Diagnostics
Console.Clear()

let ts = new TraceSource("ExampleTraceSource")
ts.Listeners.Add(new ConsoleTraceListener())
ts.Switch <- new SourceSwitch("ExampleSourceSwitch", "ActivityTracing, Information")

let firstActivityId = Guid.NewGuid()

Trace.CorrelationManager.ActivityId <- firstActivityId

ts.TraceEvent(TraceEventType.Start, 0, "Start Activity")

let newActivityId = Guid.NewGuid()

ts.TraceTransfer(0, sprintf "Transferring %A to %A..." firstActivityId newActivityId, newActivityId)

Trace.CorrelationManager.ActivityId <- newActivityId

ts.TraceEvent(TraceEventType.Start, 0, "Get Header")

ts.TraceEvent(TraceEventType.Information, 0, "Stuff happened")

ts.TraceTransfer(666, "Transferring...", firstActivityId)

ts.TraceEvent(TraceEventType.Stop, 0, "Get Header")

Trace.CorrelationManager.ActivityId <- firstActivityId

ts.Flush()
