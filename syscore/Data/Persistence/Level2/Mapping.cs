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
using System.Data;
using System.Reflection;

namespace Sys.Data
{
    enum MappingType
    {
        One2One,
        One2Many,
        Many2Many
    }

    class Mapping
    {
        private AssociationAttribute association;
        private MappingType mappingType;
        private PersistentObject dpoInstance;



        PropertyInfo propertyInfo1;              //fieldof(UserDpo._ID)
        PropertyInfo propertyInfo2;              //fieldof(DPCollection<RoleDpo>)  or fieldof(xxxDpo)

        
        private SqlBuilder clause1;     //A := SELECT UserRoles.Role_ID FROM UserRoles WHERE UserRoles.User_ID=@[User.ID]
        private SqlBuilder clause2;     //B := SELECT * FROM Roles WHERE Roles.Role_ID IN (A)

        public Mapping(PersistentObject dpo, PropertyInfo propertyInfo2)
        {
            this.association = Reflex.GetAssociationAttribute(propertyInfo2);

            if (association == null)
                return;

            this.dpoInstance = dpo;
            this.propertyInfo2 = propertyInfo2;

            Type dpoType2;            //typeof(RoleDpo)
            if (propertyInfo2.PropertyType.IsGenericType)
            {
                dpoType2 = PersistentObject.GetCollectionGenericType(propertyInfo2);

                if (this.association.TRelation == null)
                    mappingType = MappingType.One2Many;
                else
                    mappingType = MappingType.Many2Many;
            }
            else
            {
                dpoType2 = propertyInfo2.PropertyType;
                mappingType = MappingType.One2One;
            }

            

            this.propertyInfo1 = dpo.GetType().GetProperty(association.Column1);
           
           
            if (mappingType == MappingType.Many2Many)
            {
                this.clause1 = new SqlBuilder()
                    .SELECT.COLUMNS(association.Relation2)
                    .FROM(association.TRelation)
                    .WHERE(association.Relation1.ColumnName() == association.Column1.ParameterName());

                this.clause2 = new SqlBuilder()
                    .SELECT
                    .COLUMNS()
                    .FROM(dpoType2)
                    .WHERE(association.Relation2.ColumnName().IN(this.clause1));
                    
            }
            else
            {
                SqlExpr where = association.Column2.ColumnName() == association.Column1.ParameterName();
                if (association.Filter != null)
                    where = where.AND(association.Filter);

                this.clause2 = new SqlBuilder()
                    .SELECT
                    .COLUMNS()
                    .FROM(dpoType2)
                    .WHERE(where);

                if(association.OrderBy != null)
                    this.clause2 = clause2.ORDER_BY(association.OrderBy);
            }
        }


   

        /// <summary>
        /// return -> { UserRoles.Role1, UserRoles.Role2, ...}
        /// </summary>
        private object[] Relation
        {
            get
            {
                object value1 = propertyInfo1.GetValue(dpoInstance, null);
                SqlCmd cmd = new SqlCmd(this.clause1);
                cmd.AddParameter(association.Column1.SqlParameterName(), value1);
                return cmd.FillDataTable().ToArray<object>(association.Relation1);
            }
        }

        public void SetValue()
        {
            if (association == null)
                return ;

            object value1 = propertyInfo1.GetValue(dpoInstance, null);
            SqlCmd cmd = new SqlCmd(this.clause2);
            cmd.AddParameter(association.Column1.SqlParameterName(), value1);
            DataTable dataTable =  cmd.FillDataTable();

            if (mappingType == MappingType.One2One)
            {
                //if association object was not instatiated
                if (propertyInfo2.GetValue(this, null) == null)
                {
                    PersistentObject dpo = (PersistentObject)Activator.CreateInstance(propertyInfo2.PropertyType, null);
                    dpo.FillObject(dataTable.Rows[0]);
                    propertyInfo2.SetValue(this, dpo, null);
                }
                else
                {
                    IDPObject dpo = (IDPObject)propertyInfo2.GetValue(this, null);
                    dpo.FillObject(dataTable.Rows[0]);
                }
            }
            else
            {
                //if association collection was not instatiated
                if (propertyInfo2.GetValue(this, null) == null)
                    propertyInfo2.SetValue(this, Activator.CreateInstance(propertyInfo2.PropertyType, new object[] { dataTable }), null);
                else
                {
                    IPersistentCollection collection = (IPersistentCollection)propertyInfo2.GetValue(this, null);
                    collection.Table = dataTable;
                }
            }

        }


        private bool IsColumn1Identity()
        {
            ColumnAttribute[] attributes = CustomAttributeProvider.GetAttributes<ColumnAttribute>(propertyInfo1);
            if (attributes.Length == 0)
                return false;

            return attributes[0].Identity;
        }


        public void FillIdentity()
        {
            if (association == null)
                return;

            if (!IsColumn1Identity())
                return;

            if (mappingType == MappingType.One2Many)
            {
                object value1 = propertyInfo1.GetValue(dpoInstance, null);
                IDPCollection collection = (IDPCollection)propertyInfo2.GetValue(this, null);
                foreach (DataRow row in collection.Table.Rows)
                {
                    row[association.Column2] = value1;
                    IDPObject dpo = (IDPObject)collection.GetObject(row);
                    dpo.FillIdentity(row);
                }
            }
        }

    }
}
