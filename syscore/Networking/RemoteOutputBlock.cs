using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Sys.Networking
{
    [DataContract]
    class RemoteOutputBlock
    {
        [DataMember]
        public string mem { get; set; }

        [DataMember]
        public string ret { get; set; }

        [DataMember]
        public string err { get; set; }
    }
}
