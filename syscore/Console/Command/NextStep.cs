using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Stdio
{
    public enum NextStep
    {
        NEXT,

        /// <summary>
        /// multiple lines command incompleting
        /// </summary>
        CONTINUE,  

        /// <summary>
        /// command completed
        /// </summary>
        COMPLETED,

        /// <summary>
        /// command exit received
        /// </summary>
        EXIT,

        /// <summary>
        /// error occurs
        /// </summary>
        ERROR,
    };

}
