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
using System.Reflection;
using System.Data;
using Tie;

namespace Sys.Data
{

    /// <summary>
    /// represents a record in the table
    /// </summary>
    public abstract class PersistentObject : PersistentValue, IDPObject, IValizable, IDataContractRow
    {
        public event DataRowChangeEventHandler AfterLoaded;
        public event RowChangedHandler BeforeSaving;

        enum SaveMode
        {
            Insert,
            Update,
            Save,
            Validate
        }


        private TableAttribute dataTableAttribute;
        private PropertyInfo[] columnProperties;

        private bool insertIdentityOn = false;      //used for re-create table data during SQL Server updating


        protected PersistentObject()
        {
            Type type = this.GetType();

            TableAttribute[] attributes = GetAttributes<TableAttribute>();  
            if (attributes.Length > 0)
                dataTableAttribute = attributes[0];
            else
                dataTableAttribute = new TableAttribute(type.Name, Level.Application);

            this.columnProperties = Reflex.GetColumnProperties(type);   

        }

        protected PersistentObject(DataRow dataRow)
            : this()
        {
            this.UpdateObject(dataRow);
        }

        /// <summary>
        /// Update object by unique identityId
        /// </summary>
        /// <param name="identityId"></param>
        public void UpdateObject(int identityId)
        {
            if (this.Identity.Length == 0)
                throw new MessageException("no identity columns found");

            if (this.Identity.Length > 1)
                throw new MessageException("multiple identity columns defined {0}", this.Identity);

            UpdateObject(this.Identity.ColumnNames[0].ColumnName() == identityId);
        }

        /// <summary>
        /// Instantiate an instant from select a record from database
        /// </summary>
        /// <param name="where"></param>
        public void UpdateObject(SqlExpr where)
        {
            DataRow row = DataExtension.FillDataRow(this.TableName.Provider, new SqlBuilder().SELECT.COLUMNS().FROM(TableName).WHERE(where).Clause);
            this.exists = row != null;
            
            if(exists)
                this.UpdateObject(row);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(PersistentObject source)
        {

            if (source == null)
                throw new MessageException("source cannot be null");

            Type t1 = this.GetType();
            Type t2 = source.GetType();

            if (t1 != t2 && !t2.IsSubclassOf(t1))
                throw new MessageException("class type {0} is not matched to {1}.", t2.FullName, t1.FullName);

            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
                object value = propertyInfo.GetValue(source, null);
                propertyInfo.SetValue(this, value, null);
            }
        }


        /// <summary>
        /// Get values from persistentObject instance and refresh dataRow
        /// </summary>
        /// <param name="dataRow"></param>
        public void UpdateDataRow(DataRow dataRow)
        {
            Collect(dataRow);
        }


        /// <summary>
        /// Get values from dataRow and update persistentObject instance
        /// </summary>
        /// <param name="dataRow"></param>
        public void UpdateObject(DataRow dataRow)
        {
            Fill(dataRow);

            //RowLoaded(dataRow);
            if (AfterLoaded != null)
                AfterLoaded(this, new DataRowChangeEventArgs(dataRow, DataRowAction.Nothing));
        }



        /// <summary>
        /// DataRow <==> persistentObject instance, return new row
        /// </summary>
        public DataRow Row
        {
            get
            {
                DataRow dataRow = this.NewRow;
                Collect(dataRow);
                return dataRow;
            }
            set
            {
                UpdateObject(value);
            }
        }



        public bool HasAttribute<T>() where T : Attribute
        {
            return GetAttributes<T>().Length != 0;
        }

        public T[] GetAttributes<T>() where T : Attribute
        {
            return CustomAttributeProvider.GetAttributes<T>(this.GetType());
        }

        public bool BeforeSavingHooked
        {
            get { return BeforeSaving != null; }
        }

        public override string ToString()
        {
            return string.Format("Table={0} Locator={1}", TableName, Locator);
        }


        #region Exists/Changed

        private bool exists = false;
        public virtual bool Exists { get { return exists; } }


        /// <summary>
        /// this dpo Compares to the Reocrd in the SQL Server
        /// </summary>
        public bool Changed(params string[] ignoredColumns)
        {
            RowObjectAdapter d = new RowObjectAdapter(this);
            d.Apply();
            bool exists = d.Load();
            if (!exists)
                return true;

            return !d.Row.EqualTo(this.Row, ignoredColumns);
        }

        #endregion



    


        #region Locator/TableName/TableId/DataRow

        private Locator locator = null;
        public virtual Locator Locator
        {
            get
            {
                if (locator != null)
                    return locator;

                LocatorAttribute[] attributes = this.GetAttributes<LocatorAttribute>();
                if (attributes.Length > 0)
                    return attributes[0].Locator;

                if(this.Primary.Length != 0)
                    return new Locator(this.Primary);

                throw new MessageException("There is no locator defined.");
            }
        }

        /// <summary>
        /// set new locator and return old locator
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Locator SetLocator(string[] columns)
        {
            Locator old = this.locator;
            this.locator = new Locator(columns);
            
            return old;
        }

        /// <summary>
        /// User defined locator(where clause), use exact ColumnName(case sensitive)
        /// </summary>
        /// <param name="where"></param>
        public void SetLocator(Locator locator)
        {
            this.locator = locator;
        }


        public virtual TableName TableName
        {
            get
            {
                if (dataTableAttribute != null)
                    return dataTableAttribute.TableName;

                throw new MessageException("There is no table name defined.");
            }
        }


        public virtual Level Level
        {
            get
            {
                if (dataTableAttribute != null)
                    return dataTableAttribute.Level;

                return Level.Fixed;
            }
        }

        public bool IsPack
        {
            get
            {
                if (dataTableAttribute != null)
                    return dataTableAttribute.Pack;

                return true;
            }
        }

        public bool HasProvider
        {
            get
            {
                if (dataTableAttribute != null)
                    return !TableName.Provider.Equals(ConnectionProviderManager.DefaultProvider);

                return false;
            }
        }
      
        public virtual int TableId
        {
            get 
            {
               return TableName.Id;
            }
        }

        public bool DefaultValueUsed
        {
            get
            {
                return dataTableAttribute.DefaultValueUsed;
            }
            set
            {
                dataTableAttribute.DefaultValueUsed = value;
            }
        }


        public virtual IIdentityKeys Identity
        {
            get
            {
                return schema.Identity;
            }
        }

        public virtual IPrimaryKeys Primary
        {
            get
            {
                return schema.PrimaryKeys;
            }
        }


        public virtual ComputedColumns ComputedColumns
        {
            get
            {
                return schema.ComputedColumns;
            }
        }

        protected virtual string CreateTableString
        {
            get
            {
                string f = "";
                foreach (PropertyInfo propertyInfo in this.columnProperties)
                {
                    ColumnAttribute attr = Reflex.GetColumnAttribute(propertyInfo);
                    if (attr != null)
                    {
                        if (f != "")
                            f += ",\r\n";

                        ColumnSchema column = new ColumnSchema(attr);
                        f += "\t" + ColumnSchema.GetSQLField(column);
                    }
                }

                return TableClause.CREATE_TABLE(f, this.Primary); 
            }
        }


        #endregion


        #region Fill/Collect

        protected T GetField<T>(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return default(T);

            object value = row[columnName];

            if (value == System.DBNull.Value)
            {
                return default(T);
            }

            return (T)value;
        }

        protected void SetField(DataRow row, string columnName, object value)
        {
            if (row.Table.Columns.Contains(columnName))
            {
                if (value == null)
                    row[columnName] = System.DBNull.Value;
                else
                    row[columnName] = value;
            }
        }

        /// <summary>
        /// Fill object properties
        /// </summary>
        /// <param name="dataRow"></param>
        public virtual void Fill(DataRow dataRow)
        {

            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
                if (Reflex.FillField(this, propertyInfo, dataRow, dataTableAttribute.DefaultValueUsed) == null)
                { 
                    Mapping mapping = new Mapping(this, propertyInfo);
                    mapping.SetValue();
                }
            }
        }


        /// <summary>
        /// Collect property values and save to DataRow
        /// </summary>
        /// <param name="dataRow"></param>
        public virtual void Collect(DataRow dataRow)
        {
            foreach (PropertyInfo propertyInfo in Reflex.GetColumnProperties(this))
            {
                ColumnAttribute attribute = Reflex.GetColumnAttribute(dataRow, propertyInfo);

                if (attribute != null && dataRow.Table.Columns.Contains(attribute.ColumnNameSaved))
                {
                    if (propertyInfo.GetValue(this, null) == null)
                        dataRow[attribute.ColumnNameSaved] = System.DBNull.Value;
                    else
                        dataRow[attribute.ColumnNameSaved] = propertyInfo.GetValue(this, null);
                }
            }
        }

     


        public void FillIdentity(DataRow dataRow)
        {

            Type type = this.GetType();
            foreach (string key in Identity.ColumnNames)
            {
                PropertyInfo propertyInfo = type.GetProperty(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(propertyInfo != null)
                {
                   object value = dataRow[key];
                   if(value != System.DBNull.Value)
                       propertyInfo.SetValue(this, value, null);
                }
            }

            foreach (string key in ComputedColumns.ColumnNames)
            {
                PropertyInfo propertyInfo = type.GetProperty(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (propertyInfo != null)
                {
                    object value = dataRow[key];
                    if (value != System.DBNull.Value)
                        propertyInfo.SetValue(this, value, null);
                    else
                        propertyInfo.SetValue(this, null, null);
                }
            }


            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
              Mapping mapping = new Mapping(this, propertyInfo);
              mapping.FillIdentity();
            }
        }

        #endregion


     


        
        #region Load/Save/Delete/Clear

        protected SqlTrans transaction = null;
        internal SqlTrans Transaction
        {
            get { return transaction; }    //used in SqlObject
        }

        public void SetTransaction(SqlTrans transaction)
        {
            this.transaction = transaction;
        }


        public virtual DataRow Save()
        {
            return save(new Selector(), SaveMode.Save);
        }


        public virtual DataRow Save(string[] columnNames)
        {
            return save(new Selector(columnNames), SaveMode.Save);
        }

        public virtual DataRow Insert()
        {
            return save(new Selector(), SaveMode.Insert);
        }

        public virtual DataRow Update()
        {
            return save(new Selector(), SaveMode.Update);
        }

        /// <summary>
        /// Update record(s) based on WHERE condition
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual DataRow Update(SqlExpr where)
        {
            Locator temp = this.locator;
            
            this.locator = new Locator(where);
            DataRow row = save(new Selector(), SaveMode.Update);
            
            //restore Locator
            this.locator = temp;
            return row;
        }

        public virtual DataRow Validate()
        {
            return save(new Selector(), SaveMode.Validate);
        }

        public virtual DataRow Update(string[] columnNames)
        {
            return save(new Selector(columnNames), SaveMode.Update);
        }


        private DataRow save(Selector columnNames, SaveMode mode)
        {
            RowObjectAdapter d = new RowObjectAdapter(this, columnNames);
            d.Apply();

            d.InsertIdentityOn = this.insertIdentityOn;

            d.ValueChangedHandler = ValueChanged;
            d.RowChanged += RowChanged;
            if (BeforeSaving != null)
                d.RowChanged += BeforeSaving;

            switch (mode)
            {
                case SaveMode.Insert:
                    d.Insert();
                    break;

                case SaveMode.Update:
                    d.Update();
                    break;

                case SaveMode.Save:
                    d.Save();
                    break;

                case SaveMode.Validate:
                    d.Validate();
                    break;
            }

            FillIdentity(d.Row);    //because of Identity retrieved

            return d.Row;
        }


      

        protected virtual void RowChanged(object sender, RowChangedEventArgs e)
        {

        }

        protected virtual void ValueChanged(object sender, ValueChangedEventArgs e)
        {

        }

        protected virtual void RowLoaded(DataRow dataRow)
        {
            return ;
        }

        public virtual void SaveAssociations()
        {
            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
                Attribute[] association = CustomAttributeProvider.GetAttributes<AssociationAttribute>(propertyInfo);

                if (association.Length != 0)
                {
                    SaveAssociationCollection(propertyInfo);
                }
            }
        }



        public virtual DataRow Load()
        {
            RowObjectAdapter d = new RowObjectAdapter(this);
            d.Apply();

            exists = d.Load();      
            Fill(d.Row);
            
            //RowLoaded(d.Row);

            if (AfterLoaded != null)
                AfterLoaded(this, new DataRowChangeEventArgs(d.Row, DataRowAction.Nothing));


            return d.Row;
        }



        public virtual bool Delete()
        {
            RowObjectAdapter d = new RowObjectAdapter(this);
            d.Apply();

            //check 1..many table dependency
            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
                AssociationAttribute association = Reflex.GetAssociationAttribute(propertyInfo);
                if (association == null)
                    continue;

                if (!propertyInfo.PropertyType.IsGenericType)
                    continue;

                IDPCollection collection = (IDPCollection)propertyInfo.GetValue(this, null);
                if (collection.Count == 0)
                    continue ;

                Type[] typeParameters = propertyInfo.PropertyType.GetGenericArguments();
                if (typeParameters.Length == 1 && typeParameters[0].IsSubclassOf(typeof(PersistentObject)))
                {
                    throw new ApplicationException(string.Format(
                        "Error: data persistent object on Table {0} cannot be deleted because Table {1} depends on it.", 
                        TableName,
                        collection.TableName));
                }
                    
                
            }

            d.RowChanged += RowChanged;

            return d.Delete();
        }


        /// <summary>
        /// delete record based on WHERE condition
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual bool Delete(SqlExpr where)
        {
            Locator temp = this.locator;

            this.locator = new Locator(where);
            bool result = Delete();

            //restore Locator
            this.locator = temp;
            return result;
        }

        public virtual void Clear()
        {
            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
                object value = null;

                if (propertyInfo.PropertyType.IsValueType)
                    value = DefaultRowValue.SystemDefaultValue(propertyInfo.PropertyType);

                propertyInfo.SetValue(this, value, null);
            }

        }

        public bool InsertIdentityOn
        {
            get { return this.insertIdentityOn; }
            set { this.insertIdentityOn = value; }
        }


        public RowAdapter NewRowAdapter(Selector columnNames)
        {
            RowObjectAdapter d = new RowObjectAdapter(this, columnNames);
            d.InsertIdentityOn = this.insertIdentityOn;
            return d;
        }


        private void Apply(RowAdapter d)
        {
            foreach (PropertyInfo propertyInfo in this.columnProperties)
            {
                ColumnAttribute a = Reflex.GetColumnAttribute(propertyInfo);
                if (a != null && d.Row.Table.Columns.Contains(a.ColumnNameSaved))
                {
                    if (propertyInfo.GetValue(this, null) == null)
                        d.Row[a.ColumnNameSaved] = System.DBNull.Value;
                    else
                        d.Row[a.ColumnNameSaved] = propertyInfo.GetValue(this, null);
                }

            }

            d.Fill();
        }
        #endregion


        
        
        #region Assoication Field

        internal static Type GetCollectionGenericType(PropertyInfo propertyInfo)
        {
            Type fieldType = propertyInfo.PropertyType;
            if (!fieldType.IsGenericType)
                throw new MessageException("DPCollection is not generic type");

            Type[] typeParameters = fieldType.GetGenericArguments();
            if (typeParameters.Length != 1)
                throw new MessageException("Too many generic parameters, DPCollection must declare like DPCollection<T> Children");
            
            if(typeParameters[0].IsSubclassOf(typeof(PersistentObject)))
                return  typeParameters[0];

            return null;
        }

       
     


        //Invoked by methodname(string), don't change method name
        private void SaveAssociationCollection(PropertyInfo propertyInfo)
        {
            Type fieldType = propertyInfo.PropertyType;

            if (!fieldType.IsGenericType)
                return;

            IDPCollection collection = (IDPCollection)propertyInfo.GetValue(this, null);
            collection.Save();

        }

        #endregion

     


        #region Collection

        protected IPersistentCollection collection = null;

        //called by DataPersistentCollection
        public void SetCollection(IPersistentCollection collection)
        {
            this.collection = collection;
        }

        /// <summary>
        /// Update DataPersistentCollection(DPC)'s DataRow when DataPersistentObject(DPO) is from DPC
        /// </summary>
        public void AcceptChanges()
        {
            if (this.collection == null)
                return;

            this.collection.UpdateDataRow(this);
        }

        #endregion


        /// <summary>
        /// Create Table in the SQL Server if the table does not exist
        /// </summary>
        /// <returns></returns>
        public bool CreateTable()
        {
            if (this.TableName.Exists())
                return false;

            string SQL = string.Format("USE [{0}];", TableName.DatabaseName.Name) + string.Format(this.CreateTableString, TableName.FormalName);
            DataExtension.ExecuteNonQuery(this.TableName.Provider, SQL);

            return true;
        }
        
      

        private TableSchema schema
        {
            get
            {
                return this.TableName.GetTableSchema();
            }
        }

        public DataRow NewRow
        {
            get 
            {
                if (this.collection != null)
                {
                    return collection.Table.NewRow();
                }

                //FROM SQL SERVER
                return schema.NewRow(); 

                //or FROM ColumnAttribute
                //return Reflex.GetEmptyDataTable(this.GetType()).NewRow();
            }
        }

        /// <summary>
        /// return value of column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public T GetColumnValue<T>(string columnName)
        {
            foreach (PropertyInfo field in this.columnProperties)
            {
                if (field.Name == columnName)
                    return (T)field.GetValue(this, null);
            }

            throw new MessageException("Column name ({0}) not found.");
        }

        public object GetColumnValue(string columnName)
        {
            foreach (PropertyInfo field in this.columnProperties)
            {
                if (field.Name == columnName)
                    return field.GetValue(this, null);
            }

            throw new MessageException("Column name ({0}) not found.");
        }
      
    }
}
