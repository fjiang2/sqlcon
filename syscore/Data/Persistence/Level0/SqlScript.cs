using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;

namespace Sys.Data
{

    public class SqlScript
    {
        public const string GO = "GO";

        private string scriptFile;
        private ConnectionProvider provider;

        public event EventHandler<SqlExecutionEventArgs> Reported;
        public event EventHandler<EventArgs> Completed;
        public event EventHandler<SqlExceptionEventArgs> Error;
        public int BatchSize { get; set; } = 1;

        private StringBuffer buffer;

        public SqlScript(ConnectionProvider provider, string scriptFile)
        {
            this.provider = provider;
            this.scriptFile = scriptFile;

            if (!File.Exists(scriptFile))
                throw new FileNotFoundException("cannot find file", scriptFile);
        }

        protected void OnReported(SqlExecutionEventArgs e)
        {
            if (Reported != null)
                Reported(this, e);
        }

        protected void OnCompleted(EventArgs e)
        {
            if (Completed != null)
                Completed(this, e);
        }

        protected void OnError(SqlExceptionEventArgs e)
        {
            if (Error != null)
                Error(this, e);
            else
                throw e.Exception;
        }

        public void Execute(Func<bool> stopOnError)
        {
            buffer = new StringBuffer(BatchSize);

            StringBuilder builder = new StringBuilder();
            var reader = new SqlScriptReader(scriptFile);

            while (reader.NextLine())
            {
                string line = reader.Line;

                string formatedLine = line.Trim();
                string upperLine = formatedLine.ToUpper();
                if (upperLine.StartsWith("INSERT")
                    || upperLine.StartsWith("UPDATE")
                    || upperLine.StartsWith("DELETE")
                    || upperLine.StartsWith("CREATE")
                    || upperLine.StartsWith("DROP")
                    || upperLine.StartsWith("ALTER")
                    || upperLine.StartsWith(GO)
                    )
                {
                    bool go = upperLine.StartsWith(GO);

                    if (!ExecuteSql(reader.LineNumber, builder, go))
                    {
                        if (stopOnError != null && stopOnError())
                        {
                            reader.Close();
                            return;
                        }
                    }

                    builder.Clear();
                    if (!upperLine.StartsWith(GO))
                        builder.AppendLine(line);
                }
                else
                {
                    builder.AppendLine(line);
                    while (reader.NextLine())
                    {
                        line = reader.Line;

                        formatedLine = line.Trim();

                        upperLine = formatedLine.ToUpper();

                        if (!upperLine.StartsWith(GO))
                            builder.AppendLine(line);
                        else
                        {
                            if (!ExecuteSql(reader.LineNumber, builder, commit: true))
                            {
                                if (stopOnError != null && stopOnError())
                                {
                                    reader.Close();
                                    return;
                                }
                            }

                            builder.Clear();
                            break;
                        }
                    }
                }

            }

            reader.Close();

            if (!ExecuteSql(reader.LineNumber, builder, commit: true))
                return;

            OnCompleted(new EventArgs());
        }

        private bool ExecuteSql(int line, StringBuilder builder, bool commit)
        {
            string sql = builder.ToString();

            if (buffer.Append(line, sql, commit))
                return true;

            bool result = ExecuteSql(buffer);
            buffer.Clear();
            return result;
        }

        private bool ExecuteSql(StringBuffer buffer)
        {
            if (buffer.BatchSize == 0)
                return true;

            string sql = buffer.AllText;
            try
            {
                var cmd = new SqlCmd(provider, sql);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                OnError(new SqlExceptionEventArgs(sql, ex) { Line = buffer.BatchLine });
                return false;
            }

            OnReported(new SqlExecutionEventArgs(sql)
            {
                BatchLine = buffer.BatchLine,
                BatchSize = buffer.BatchSize,
                Line = buffer.Line,
                TotalSize = buffer.TotalSize
            }); ;

            return true;
        }

        public void ExecuteTransaction(string[] clauses)
        {
            using (SqlConnection connection = (SqlConnection)provider.NewDbConnection)
            {
                connection.Open();

                // Start a local transaction.
                SqlTransaction sqlTran = connection.BeginTransaction();

                // Enlist a command in the current transaction.
                SqlCommand command = connection.CreateCommand();
                command.Transaction = sqlTran;

                try
                {
                    // Execute two separate commands.
                    foreach (string clause in clauses)
                    {
                        command.CommandText = clause;
                        command.ExecuteNonQuery();
                    }

                    // Commit the transaction.
                    sqlTran.Commit();

                    OnCompleted(new EventArgs());
                }
                catch (Exception ex)
                {
                    // Handle the exception if the transaction fails to commit.
                    OnError(new SqlExceptionEventArgs(command, ex));

                    try
                    {
                        // Attempt to roll back the transaction.
                        sqlTran.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        // Throws an InvalidOperationException if the connection 
                        // is closed or the transaction has already been rolled 
                        // back on the server.
                        OnError(new SqlExceptionEventArgs(command, exRollback));
                    }
                }
            }

        }

    }
}
