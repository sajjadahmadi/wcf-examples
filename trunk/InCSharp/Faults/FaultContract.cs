using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace CodeRunner.ServiceModel.Examples
{

    [DataContract]
    class MyFault
    {
        private string description;

        public MyFault(string description)
        { this.description = description; }

        [DataMember]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
