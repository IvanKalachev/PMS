using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.Common;

namespace PropertyManagement.Security
{
    public class Instance
    {

        public static IDbConnection GetConnection(string connectionType, string connectionString)
        {
            switch (connectionType.ToLower())
            {
                case "odbc":
                    {
                        return new OdbcConnection(connectionString);
                    }
                case "oledb":
                    {
                        return new OleDbConnection(connectionString);
                    }
                case "sql":
                    {
                        return new SqlConnection(connectionString);
                    }
                default:
                    {
                        throw new ArgumentException("No connection type specified!");
                    }

            }
        }

        public static IDbCommand GetCommand(string command, IDbConnection connection)
        {
            if (connection is OdbcConnection)
                return new OdbcCommand(command, (OdbcConnection)connection);
            if (connection is OleDbConnection)
                return new OleDbCommand(command, (OleDbConnection)connection);
            if (connection is SqlConnection)
                return new SqlCommand(command, (SqlConnection)connection);
            throw new ArgumentException("Not implemented connection type!");

        }

        public static DbParameter AddParameter(IDbCommand command,
                                                string connectionType,
                                                string parameterName,
                                                string parameterType)
        {
            switch (connectionType.ToLower())
            {
                case "odbc":
                    {
                        var com = (OdbcCommand)command;
                        return com.Parameters.Add(parameterName, (OdbcType)GetDbType("odbc", parameterType));

                    }
                case "oledb":
                    {
                        var com = (OleDbCommand)command;
                        return com.Parameters.Add(parameterName, (OleDbType)GetDbType("oledb", parameterType));

                    }
                case "sql":
                    {
                        var com = (SqlCommand)command;
                        var param = com.Parameters.Add(parameterName, (SqlDbType)GetDbType("sql", parameterType));
                        ParseSqlCommand(com, param);
                        return param;
                    }
                default:
                    throw new ArgumentException(
                        String.Format("Database type not implemented in " +
                        "Membership/Role provider! Type: {0}",
                                      connectionType));
            }

        }


        private static object GetDbType(string dbType, string parameterType)
        {
            switch (dbType.ToLower())
            {
                case "odbc":
                    {
                        switch (parameterType.ToLower())
                        {
                            case "int":
                                {
                                    return OdbcType.Int;
                                }
                            case "varchar":
                                {
                                    return OdbcType.VarChar;
                                }
                            case "nvarchar":
                                {
                                    return OdbcType.NVarChar;
                                }
                            case "bit":
                                {
                                    return OdbcType.Bit;
                                }
                            case "datetime":
                                {
                                    return OdbcType.DateTime;
                                }
                            case "uniqueidentifier":
                                {
                                    return OdbcType.UniqueIdentifier;
                                }
                            default:
                                throw new ArgumentException(String.Format("Not implemented parameter type:{0}", parameterType));
                        }
                    }
                case "oledb":
                    {
                        switch (parameterType.ToLower())
                        {
                            case "int":
                                {
                                    return OleDbType.Integer;
                                }
                            case "varchar":
                                {
                                    return OleDbType.VarChar;
                                }
                            case "nvarchar":
                                {
                                    return OleDbType.VarWChar;
                                }
                            case "bit":
                                {
                                    return OleDbType.Boolean;
                                }
                            case "datetime":
                                {
                                    return OleDbType.DBDate;
                                }
                            case "uniqueidentifier":
                                {
                                    return OleDbType.Guid;
                                }
                            default:
                                throw new ArgumentException(String.Format("Not implemented parameter type:{0}", parameterType));
                        }
                    }
                case "sql":
                    {
                        switch (parameterType.ToLower())
                        {
                            case "int":
                                {
                                    return SqlDbType.Int;
                                }
                            case "varchar":
                                {
                                    return SqlDbType.VarChar;
                                }
                            case "nvarchar":
                                {
                                    return SqlDbType.NVarChar;
                                }
                            case "bit":
                                {
                                    return SqlDbType.Bit;
                                }
                            case "datetime":
                                {
                                    return SqlDbType.DateTime;
                                }
                            case "uniqueidentifier":
                                {
                                    return SqlDbType.UniqueIdentifier;
                                }
                            default:
                                throw new ArgumentException(String.Format("Not implemented parameter type: {0}", parameterType));
                        }
                    }
                default:
                    throw new ArgumentException(String.Format("Not implemented database type: {0}", dbType));
            }

        }

        private static void ParseSqlCommand(SqlCommand command, SqlParameter param)
        {
            var query = command.CommandText;
            var index = query.IndexOf("?");

            query = query.Remove(index, 1);
            query = query.Insert(index, param.ParameterName);


            command.CommandText = query;
            return;
        }
    }
}
