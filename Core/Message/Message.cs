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

namespace Sys
{
    /// <summary>
    /// message description with message level
    /// </summary>
    public class Message
    {
        /// <summary>
        /// message level
        /// </summary>
        public readonly MessageLevel Level;

        /// <summary>
        /// message description
        /// </summary>
        public readonly string Description;

        private int code;
        private MessageLocation location;

        /// <summary>
        /// Message's owner
        /// </summary>
        private object sender;


        /// <summary>
        /// define new message
        /// </summary>
        /// <param name="level"></param>
        /// <param name="description"></param>
        public Message(MessageLevel level, string description)
        {
            this.Level = level;
            this.Description = description;
        }

        /// <summary>
        /// has code in this message?
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Message HasCode(int code)
        {
            this.code = code;
            return this;
        }
        
        /// <summary>
        /// assign sender to message
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public Message From(object sender)
        {
            this.sender = sender;
            return this;
        }
        
        /// <summary>
        /// assign location to message 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Message At(MessageLocation location)
        {
            this.location = location;
            return this;
        }

        /// <summary>
        /// return message code
        /// </summary>
        public int Code
        {
            get { return this.code; }
        }

      
        /// <summary>
        /// return message location
        /// </summary>
        public MessageLocation Location
        {
            get { return this.location;}
        }

        /// <summary>
        /// message sender
        /// </summary>
        public object Sender
        {
            get { return this.sender; }
        }

        #region GetHashCode/Equals/ToString

        public override int GetHashCode()
        {
            return location.GetHashCode() + Level.GetHashCode() + Description.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Message message = (Message)obj;
            return this.code == message.code && this.location == message.location && this.Level == message.Level && this.Description == message.Description;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (Code != 0)
                builder.AppendFormat("{0}({1})", this.Level, this.code);
            else
                builder.Append(this.Level);

            builder.Append(" : ");

            if(this.location == null)
                builder.Append(this.Description);
            else
                builder.AppendFormat("{0} @ {1}", this.Description, this.location);

            return builder.ToString();
        }
        
        #endregion

        /// <summary>
        /// emit error message
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Message Error(string description)
        {
            return Error(0, description);
        }

        /// <summary>
        /// emit error message with code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Message Error(int code, string description)
        {
            Message message = new Message(MessageLevel.Error, description);
            message.code = code;
            return message;
        }


        /// <summary>
        /// emit warning message
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Message Warning(string description)
        {
            return Warning(0, description);
        }

        /// <summary>
        /// emit warning message with code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Message Warning(int code, string description)
        {
            Message message = new Message(MessageLevel.Warning, description);
            message.code = code;
            return message;
        }

        /// <summary>
        /// emit information message
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Message Information(string description)
        {
            return Information(0, description);
        }


        /// <summary>
        /// emit information message with code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Message Information(int code, string description)
        {
            Message message = new Message(MessageLevel.Information, description);
            message.code = code;
            return message;
        }
    }
}
