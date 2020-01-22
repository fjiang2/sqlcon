using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Sys.Networking
{
    [DataContract]
    class RemoteInputBlock
    {
        [DataMember]
        public string method { get; set; }

        [DataMember]
        public string code { get; set; }

        [DataMember]
        public string mem { get; set; }
    }
}
