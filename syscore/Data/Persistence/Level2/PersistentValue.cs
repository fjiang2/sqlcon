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
using System.Text;
using Tie;


namespace Sys.Data
{
    public class PersistentValue : IValizable 
    {
        protected Memory memory;
        private string scope;
        private string name;


        //base: scope
        //this: scope.name
        // val: class fields
        public PersistentValue(Memory memory, string scope, string name)
        {
            this.memory = memory;
            this.scope = scope;
            this.name = name;
        }
        
        protected PersistentValue()
        {
            this.memory = null;
            this.scope = null;
            this.name = null;
        }

        protected PersistentValue(VAL val)
        {
            Conversion.VAL2Class(val, this);
        }

        public void SetVAL(VAL val)
        {
            Conversion.VAL2Class(val, this);
        }
     
        public Memory Memory { get { return this.memory; }}
        public string ValScope { get { return this.scope; } set { this.scope = value; } }
        public string ValName { get { return this.name; } set { this.name = value; } }

        public string ValPath { get { return this.scope + "." + this.name; } }


        public VAL GetField(string fieldName)
        {
            VAL BASE = Script.Evaluate(scope, memory);
            return BASE[name][fieldName];
        }

        public void SetField(string fieldName, VAL value)
        {
            VAL BASE = Script.Evaluate(scope, memory);
            BASE[name][fieldName] = value;
        }


        public virtual VAL OverlapObject()
        {
            VAL BASE = Script.Evaluate(scope, memory);
            if (BASE[name].Undefined)
                BASE[name] = this.GetVAL();
            else
            {
                VAL THIS = BASE[name];
                VAL val = this.GetVAL();
                for (int i = 0; i < val.Size; i++)
                {
                    VAL v = val[i];
                    THIS[v[0].Str] = v[1];
                }
            }

            return BASE[name];
        }



        public virtual VAL OverwriteObject()
        {
            VAL BASE = Script.Evaluate(scope, memory);
            BASE[name] = this.GetVAL();

            return BASE[name];
        }


        public virtual VAL GetVAL()
        {
            return Conversion.Class2VAL(this);
        }

        public override string ToString()
        {
            VAL BASE = Script.Evaluate(scope, memory);
            return BASE[name].ToJson();
        }


        public string PersistentString
        {
            get 
            {
                return VAL.Boxing(this.memory).ToJson();
            }

            set
            {
                VAL v = Script.Evaluate(value);
                this.memory = (Memory)v;
            }
        }
    }
}
