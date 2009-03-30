using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Messages
{
    [MessageContract]
    class MailMessage
    {
        /// <summary>
        /// Specifies that the default wrapper element in the 
        /// SOAP message must NOT be written around array types in a header element.
        /// For example:
        /// <To>user1@company.com</To>
        /// <To>user2@company.com</To>
        /// <To>user3@company.com</To>
        /// </summary>
        [MessageHeaderArray]
        public string[] To;

        /// <summary>
        /// Specifies that a data member is a SOAP message header.
        /// </summary>
        [MessageHeader]
        public string From;

        /// <summary>
        /// Specifies that a member is serialized as an element inside the SOAP body.
        /// </summary>
        [MessageBodyMember]
        public string Subject;

        [MessageBodyMember]
        public string Body;
    }
}
