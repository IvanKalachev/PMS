using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Web.Security;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;

namespace PropertyManagement.Security
{
    public class PropertyManagementRoleProvider : RoleProvider
    {
        //
        // Global connection string, generic exception message, event log info.
        //

        private ConnectionStringSettings pConnectionStringSettings;
        private string connectionString;


        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //

        private bool pWriteExceptionsToEventLog;

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }

        //
        // System.Configuration.Provider.ProviderBase.Initialize Method
        //

        public override void Initialize(string name, NameValueCollection config)
        {
            //
            // Initialize values from web.config.
            //

            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(name))
                name = "OrakRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Sample ODBC Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);


            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }


            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                {
                    pWriteExceptionsToEventLog = true;
                }
            }


            //
            // Initialize OdbcConnection.
            //

            pConnectionStringSettings = ConfigurationManager.
                ConnectionStrings[config["connectionStringName"]];

            if (pConnectionStringSettings == null || pConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = pConnectionStringSettings.ConnectionString;

            ConnectionType = Convert.ToString(config["connectionType"]);
            //DbInit();
        }

        //
        // System.Web.Security.RoleProvider properties.
        //

        private Int64 _roleId;

        public Int64 RoleId
        {
            get { return _roleId; }
            set { _roleId = value; }
        }

        private Int64 _applicationId;

        public Int64 ApplicationId
        {
            get { return _applicationId; }
            set { _applicationId = value; }
        }

        private string pApplicationName;


        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        private string _connectionType;

        public string ConnectionType
        {
            get
            {
                return _connectionType;
            }
            set
            {
                _connectionType = value;
            }
        }

        //
        // System.Web.Security.RoleProvider methods.
        //

        //
        // RoleProvider.AddUsersToRoles
        //

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }


            foreach (string username in usernames)
            {
                if (!UserExists(username))
                {
                    throw new ProviderException("User name not found.");
                }

                if (username.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }


                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }


            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("INSERT INTO UsersInRoles " +
                                      " (UserId, RoleId) " +
                                      " Values(?, ?)", conn);

            var userParmId = Instance.AddParameter(cmd, ConnectionType, "@UserId", "int");
            var roleParmId = Instance.AddParameter(cmd, ConnectionType, "@RoleId", "int");


            var cmdUserId = Instance.GetCommand("SELECT Id FROM Users " +
                                            "WHERE UserName=? " +
                                            "AND IsDeleted=0 ", conn);

            var userParm = Instance.AddParameter(cmdUserId, ConnectionType, "@Username", "varchar");


            var cmdRoleId = Instance.GetCommand("SELECT Id Roles R " +
                                            " WHERE Rolename = ?", conn);

            var roleParm = Instance.AddParameter(cmdRoleId, ConnectionType, "@Rolename", "varchar");

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                cmdUserId.Transaction = tran;
                cmdRoleId.Transaction = tran;

                foreach (var username in usernames)
                {
                    foreach (var rolename in rolenames)
                    {
                        userParm.Value = username;
                        roleParm.Value = rolename;

                        var userId = (int)cmdUserId.ExecuteScalar();
                        var roleId = (int)cmdRoleId.ExecuteScalar();

                        userParmId.Value = userId;
                        roleParmId.Value = roleId;

                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch (Exception)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }


        //
        // RoleProvider.CreateRole
        //

        public override void CreateRole(string rolename)
        {
            if (rolename.Contains(","))
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(rolename))
            {
                throw new ProviderException("Role name already exists.");
            }

            var conn = Instance.GetConnection(ConnectionType, connectionString);

            IDbTransaction tran = null;

            var cmd = Instance.GetCommand("INSERT INTO Roles " +
                                      "(RoleName) " +
                                      "Values(?)", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Rolename", "varchar").Value = rolename;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                conn.Close();
            }
        }


        //
        // RoleProvider.DeleteRole
        //

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("DELETE FROM Roles " +
                                           "WHERE RoleName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Rolename", "varchar").Value = rolename;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return true;
        }


        //
        // RoleProvider.GetAllRoles
        //

        public override string[] GetAllRoles()
        {
            var tmpRoleNames = "";

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT Rolename FROM Roles", conn);

            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpRoleNames += reader.GetString(0) + ",";
                }
                reader.Close();
                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            if (tmpRoleNames.Length > 0)
            {
                // Remove trailing comma.
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);
                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }


        //
        // RoleProvider.GetRolesForUser
        //

        public override string[] GetRolesForUser(string username)
        {
            string tmpRoleNames = "";

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT RoleName FROM Roles R, Users U, UsersInRoles UR " +
                                            "WHERE R.Id = UR.RoleId " +
                                            "AND U.Id = UR.UserId " +
                                            "AND U.UserName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpRoleNames += reader.GetString(0) + ",";
                }
                reader.Close();
                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            if (tmpRoleNames.Length > 0)
            {
                // Remove trailing comma.
                tmpRoleNames = tmpRoleNames.Substring(0, tmpRoleNames.Length - 1);
                return tmpRoleNames.Split(',');
            }

            return new string[0];
        }


        //
        // RoleProvider.GetUsersInRole
        //

        public override string[] GetUsersInRole(string rolename)
        {
            string tmpUserNames = "";

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT UserName FROM Users U, Roles R, UsersInRoles UR " +
                                            "WHERE U.Id = UR.UserId " +
                                            "AND R.Id = UR.RoleId " +
                                            "AND R.RoleName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Rolename", "varchar").Value = rolename;

            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpUserNames += reader.GetString(0) + ",";
                }
                reader.Close();
                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }


        //
        // RoleProvider.IsUserInRole
        //

        public override bool IsUserInRole(string username, string rolename)
        {
            bool userIsInRole = false;

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT COUNT(*) FROM UsersInRoles UR, Users U, Roles R " +
                                            "WHERE UR.UserId = U.Id " +
                                            "AND UR.RoleId = R.Id " +
                                            "AND U.UserName = ? " +
                                            "AND R.RoleName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Rolename", "varchar").Value = rolename;
            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;


                var numRecs = (int)cmd.ExecuteScalar();

                if (numRecs > 0)
                {
                    userIsInRole = true;
                }

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return userIsInRole;
        }


        //
        // RoleProvider.RemoveUsersFromRoles
        //

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }


            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("DELETE FROM UsersInRoles " +
                                          "WHERE UserID = (SELECT Id FROM Users WHERE UserName = ? AND IsDeleted = 0) " +
                                          "AND RoleId = (SELECT Id FROM Roles WHERE RoleName = ?)", conn);

            var userParm = Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar");
            var roleParm = Instance.AddParameter(cmd, ConnectionType, "@Rolename", "varchar");

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        userParm.Value = username;
                        roleParm.Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();

            }
            finally
            {
                conn.Close();
            }
        }


        //
        // RoleProvider.RoleExists
        //

        public override bool RoleExists(string rolename)
        {
            var exists = false;

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT COUNT(*) Roles R " +
                                      " WHERE RoleName = ? ", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Rolename", "varchar").Value = rolename;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                var numRecs = (int)cmd.ExecuteScalar();

                if (numRecs > 0)
                {
                    exists = true;
                }

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return exists;
        }

        //
        // RoleProvider.UserExists 
        //

        public bool UserExists(string username)
        {
            var exists = false;

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT COUNT(*) Users " +
                                      " WHERE Username = ? " +
                                      " AND IsDeleted=0", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                var numRecs = (int)cmd.ExecuteScalar();

                if (numRecs > 0)
                {
                    exists = true;
                }

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return exists;
        }


        //
        // RoleProvider.FindUsersInRole
        //

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT UserName FROM Users U, UsersInRoles UR, Roles R " +
                                            "WHERE UR.UserId = U.Id " +
                                            "AND UR.RoleId = R.Id " +
                                            "AND U.UserName LIKE ? " +
                                            "AND U.IsDeleted = 0 " +
                                            "AND R.RoleName = ?", conn);
            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = "%" + usernameToMatch + "%";
            Instance.AddParameter(cmd, ConnectionType, "@RoleName", "varchar").Value = rolename;

            var tmpUserNames = "";
            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                while (reader.Read())
                {
                    tmpUserNames += reader.GetString(0) + ",";
                }
                reader.Close();
                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            if (tmpUserNames.Length > 0)
            {
                // Remove trailing comma.
                tmpUserNames = tmpUserNames.Substring(0, tmpUserNames.Length - 1);
                return tmpUserNames.Split(',');
            }

            return new string[0];
        }
    }
}
