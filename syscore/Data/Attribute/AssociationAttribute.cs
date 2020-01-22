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
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class AssociationAttribute : NonValizedAttribute
    {
    

        private Locator locator = null;
        public string OrderBy;
        public string Filter;

    
        public Locator Locator
        {
            get
            {
                return this.locator;
            }
        }

        
        #region One-to-One mapping / One-to-Many mapping 

        public readonly string Column1;
        public readonly string Column2;
        public AssociationAttribute(string column1, string column2)
        {
            this.Column1 = column1;
            this.Column2 = column2;
        }

        #endregion



        #region Many-to-Many Mapping

        //public readonly string Column1;       //Users.ID
        //public readonly string Column2;       //Roles.ID
        public readonly string Relation1;    //UserRoles.User_ID
        public readonly string Relation2;    //UserRoles.Role_ID
        public readonly Type TRelation;      //typeof(UserRoles)
        public AssociationAttribute(string column1, string column2, Type relationDpo, string relation1, string relation2)
        {
            this.Column1 = column1;
            this.Column2 = column2;

            this.Relation1 = relation1;
            this.Relation2 = relation2;
            this.TRelation = relationDpo;
        }

        public AssociationAttribute(string column1, string column2, Type relationDpo)
            : this(column1, column2, relationDpo, column1, column2)
        {

        }

        #endregion

    }

   
}
