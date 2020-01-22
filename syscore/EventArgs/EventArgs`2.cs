using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class EventArgs<T1, T2> : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly T1 Value1;

        /// <summary>
        /// 
        /// </summary>
        public readonly T2 Value2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        public EventArgs(T1 value1, T2 value2)
        {
            this.Value1 = value1;
            this.Value2 = value2;
        }
    }
}
