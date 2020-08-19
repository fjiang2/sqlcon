﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
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
            startLine = 0;
            queue.Clear();

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

        private int startLine = 0;
        private List<string> queue = new List<string>();
        private bool ExecuteSql(int line, StringBuilder builder, bool commit)
        {
            string sql = builder.ToString();
            if (string.IsNullOrEmpty(sql))
                return true;

            if (commit)
                return ExecuteSql(line, 1, sql);

            if (queue.Count == 0)
                startLine = line;

            queue.Add(sql);

            if (queue.Count < BatchSize)
                return true;

            sql = string.Join(string.Empty, queue);
            bool result = ExecuteSql(startLine, queue.Count, sql);
            queue.Clear();
            return result;
        }

        private bool ExecuteSql(int line, int count, string sql)
        {
            try
            {
                var cmd = new SqlCmd(provider, sql);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                OnError(new SqlExceptionEventArgs(sql, ex) { Line = line });
                return false;
            }

            OnReported(new SqlExecutionEventArgs(sql) { Line = line, BatchSize = count });
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
