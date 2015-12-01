//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace Sys
{
    /// <summary>
    /// Exception class 
    /// </summary>
    public class MessageException : Exception
    {
        
        private Message msg;


        /// <summary>
        /// construct Exception by message and level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public MessageException(MessageLevel level, string format, params object[] args)
            :base(string.Format(format, args))
        {
            this.msg = new Message(level, string.Format(format, args))
                .HasCode((int)MessageCode.None);
        }


        /// <summary>
        /// construct Exception by message
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public MessageException(string format, params object[] args)
            : this(MessageLevel.Error, format, args)
        {

        }


        /// <summary>
        /// construct Exception by message code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public MessageException(MessageCode code, string message)
            :base(message)
        {
            this.msg = new Message(MessageLevel.Error, message)
                .HasCode((int)code);
        }

        /// <summary>
        /// return message
        /// </summary>
        /// <returns></returns>
        public Message GetMessage()
        {
            return this.msg;
        }

        /// <summary>
        /// return message code
        /// </summary>
        public MessageCode MessageCode
        {
            get { return (MessageCode)this.msg.Code; }
        }


        /// <summary>
        /// return message
        /// </summary>
        /// <returns></returns>
        public override string  ToString()
        {
 	        return this.msg.ToString();
        }
    }
}
