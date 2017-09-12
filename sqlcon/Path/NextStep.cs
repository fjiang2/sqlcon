using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    enum NextStep
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
