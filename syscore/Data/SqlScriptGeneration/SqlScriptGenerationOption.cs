namespace Sys.Data
{
    public class SqlScriptGenerationOption
    {
        /// <summary>
        /// Generate SQL INSERT
        /// if HasIfExists 
        ///   "IF NOT EXISTS(SELECT * WHERE ...)
        ///      INSERT INTO ...."
        /// else generate SQL
        ///    "INSERT INTO ..."
        /// </summary>
        public bool HasIfExists { get; set; }

        /// <summary>
        /// Generate SQL INSERT
        /// if InsertWithoutColumns
        ///   "INSERT INTO table-name VALUES (...)"
        /// else  
        ///   "INSERT INTO table-name(column,....) VALUES (...)"
        /// </summary>
        public bool InsertWithoutColumns { get; set; }


        public bool IncludeIdentity { get; set; }
    }
}
