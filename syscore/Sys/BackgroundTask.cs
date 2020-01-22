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
using System.ComponentModel;

namespace Sys
{
    public class UserState
    {
        public int Progress1;
        public int Progress2;
        public string Message;

        public UserState()
        {
        }
    }

    public class BackgroundTask : BackgroundWorker
    {
        private UserState state = new UserState();
        private bool cancelled = false;

        public BackgroundTask()
        {
            this.WorkerReportsProgress = true;
        }

        public void Cancel()
        {
            this.cancelled = true;
        }

        public bool SetProgress(int progress1, int progress2)
        {
            this.state.Progress1 = progress1;
            this.state.Progress2 = progress2;
            this.state.Message = "";
            this.ReportProgress(0, state);

            return this.cancelled;
        }


        public bool SetProgress(int progress2, string message)
        {
            this.state.Progress2 = progress2;
            this.state.Message = message;
            this.ReportProgress(0, state );

            return this.cancelled;
        }

        public bool SetProgress(int progress2)
        {
            this.state.Progress2 = progress2;
            this.state.Message = "";
            this.ReportProgress(0, state);

            return this.cancelled;
        }


        public bool SetProgress(string message)
        {
            this.state.Progress2 = 0;
            this.state.Message = message;
            this.ReportProgress(0, state);

            return this.cancelled;
        }

        public bool SetProgress(int progress1, int progress2, string message)
        {
            this.state.Progress1 = progress1;
            this.state.Progress2 = progress2;
            this.state.Message = message;
            this.ReportProgress(0, state);

            return this.cancelled;
        }

    }

}
