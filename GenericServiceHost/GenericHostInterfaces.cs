using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ServiceModel
{
   interface IEnableMetaDataExchange
   {
      bool EnableMetaDataExchange { get; set; }
      bool HasMexEndpoint { get; }
      void AddAllMexEndPoints();
   }
}
