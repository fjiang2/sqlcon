namespace Sys.Data
{
    class DbSchemaColumn
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public short Length { get; set; }
        public bool Nullable { get; set; }
        public byte precision { get; set; }
        public byte scale { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        public string definition { get; set; }
        public string PKContraintName { get; set; }
        public string PK_Schema { get; set; }
        public string PK_Table { get; set; }
        public string PK_Column { get; set; }
        public string FKContraintName { get; set; }
    }
}