using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;

[ServiceContract]
public interface IMyContract
{
    [OperationContract]
    string MyMethod();
}

public class MyService : IMyContract
{
    public string MyMethod() {
        return "Do something";
    }
}
