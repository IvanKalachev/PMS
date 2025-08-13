using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;

namespace PropertyManagement.Security
{
    public class PropertyManagementMembershipProvider : MembershipProvider
    {
        private const int newPasswordLength = 8;

        private string pApplicationName;
        private string connectionString;
        private bool pRequiresUniqueEmail;
        private bool pEnablePasswordReset;
        private bool pRequiresQuestionAndAnswer;
        private int pPasswordAttemptWindow;
        private int pMaxInvalidPasswordAttempts;
        private int pMinRequiredNonAlphanumericCharacters;
        private int pMinRequiredPasswordLength;
        private MembershipPasswordFormat pPasswordFormat;
        private string pPasswordStrengthRegularExpression;

        //
        // Used when determining encryption key values.
        //

        private MachineKeySection machineKey;

        public override string ApplicationName
        {
            get
            {
                return pApplicationName;
            }
            set
            {
                pApplicationName = value;
            }
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

        public override void Initialize(String name, NameValueCollection config)
        {
            //
            // Initialize values from web.config.
            //

            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(name))
                name = "HotelRegisterMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Sample ODBC Membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"],
                                              System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

            ConnectionType = Convert.ToString(GetConfigValue(config["connectionType"], "odbc"));
            pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));

            var temp_format = config["passwordFormat"] ?? "Hashed";

            switch (temp_format)
            {
                case "Hashed":
                    pPasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    pPasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    pPasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            //
            // Initialize OdbcConnection.
            //
            var ConnectionStringSettings =
                ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;

            // Get encryption and decryption key information from the configuration.
            var cfg =
                WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!ValidateUser(username, oldPassword))
                return false;

            var args =
                new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);


            if (args.Cancel)
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
                }

            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("UPDATE Users " +
                                          "SET Password = ?, LastPasswordChangeDate = ? " +
                                          "WHERE UserName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Password", "varchar").Value = EncodePassword(newPassword);
            Instance.AddParameter(cmd, ConnectionType, "@LastPasswordChangeDate", "datetime").Value = DateTime.Now;
            Instance.AddParameter(cmd, ConnectionType, "@UserName", "varchar").Value = username;

            int rowsAffected = 0;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                rowsAffected = cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
            }
            finally
            {
                conn.Close();
            }

            return rowsAffected > 0;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!ValidateUser(username, password))
            {
                return false;
            }

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("UPDATE Users " +
                                          "SET PasswordQuestion = ?, PasswordAnswer = ? " +
                                          "WHERE UserName = ?", conn);
            Instance.AddParameter(cmd, ConnectionType, "@Question", "varchar").Value = newPasswordQuestion;
            Instance.AddParameter(cmd, ConnectionType, "@QuestionAnswer", "varchar").Value = newPasswordAnswer;
            Instance.AddParameter(cmd, ConnectionType, "@UserName", "varchar").Value = username;

            int rowsAffected = 0;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                rowsAffected = cmd.ExecuteNonQuery();

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

            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var u = GetUser(username, false);

            if (u != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            var createDate = DateTime.Now;

            var uuid = Guid.NewGuid();
            if (providerUserKey == null)
            {
                providerUserKey = uuid.ToString();
            }
            else
            {
                if (providerUserKey is Guid)
                {
                    uuid = (Guid)providerUserKey;
                }

                if (!string.IsNullOrEmpty(providerUserKey.ToString()))
                {
                    try
                    {
                        var providerGuid = Guid.NewGuid();
                        uuid = providerGuid;
                    }
                    catch (FormatException ex)
                    {
                        // the providerUserKey is not gui representation - use the allready generated Guid then
                    }
                    catch (OverflowException)
                    {
                        // the providerUserKey is not gui representation - use the allready generated Guid then
                    }
                }
            }

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            IDbTransaction tran = null;

            var cmd = Instance.GetCommand("INSERT INTO Users " +
                                          "(UserName, ApplicationName, Email, Password, " +
                                          "PasswordQuestion, PasswordAnswer, IsApproved, CreationDate, IsDeleted, " +
                                          "ProviderUserKey, IsLockedOut)" +
                                          "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;
            Instance.AddParameter(cmd, ConnectionType, "@AppName", "varchar").Value = ApplicationName;
            Instance.AddParameter(cmd, ConnectionType, "@Email", "varchar").Value = email;
            Instance.AddParameter(cmd, ConnectionType, "@Password", "varchar").Value = EncodePassword(password);
            Instance.AddParameter(cmd, ConnectionType, "@PassQuest", "varchar").Value = passwordQuestion;
            Instance.AddParameter(cmd, ConnectionType, "@PassAnswer", "varchar").Value = passwordAnswer;
            Instance.AddParameter(cmd, ConnectionType, "@IsApproved", "bit").Value = isApproved;
            Instance.AddParameter(cmd, ConnectionType, "@CreationDate", "datetime").Value = createDate;
            Instance.AddParameter(cmd, ConnectionType, "@IsDeleted", "bit").Value = 0;
            Instance.AddParameter(cmd, ConnectionType, "@ProviderUserKey", "varchar").Value = providerUserKey.ToString();
            Instance.AddParameter(cmd, ConnectionType, "@IsLockedOut", "bit").Value = 0;

            int rowsAffected = 0;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                rowsAffected = cmd.ExecuteNonQuery();

                status = rowsAffected > 0 ? MembershipCreateStatus.Success : MembershipCreateStatus.UserRejected;
                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null) tran.Rollback();
                status = MembershipCreateStatus.ProviderError;
            }
            finally
            {
                conn.Close();
            }

            return GetUser(username, false);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("UPDATE Users " +
                                            "SET IsDeleted = 1 " +
                                            "WHERE UserName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@IsDeleted", "bit").Value = 1;

            var rowsAffected = 0;
            IDbTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                rowsAffected = cmd.ExecuteNonQuery();

                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.
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

            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool EnablePasswordReset
        {
            get { return pEnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return pRequiresQuestionAndAnswer; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("SELECT COUNT(*) FROM Users " +
                                           "WHERE Email LIKE ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@EmailSearch", "varchar").Value = "%" + emailToMatch + "%";

            var users = new MembershipUserCollection();

            DbDataReader reader = null;
            IDbTransaction tran = null;
            totalRecords = 0;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                totalRecords = (int)cmd.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }
                else
                {
                    cmd.CommandText = "SELECT ProviderUserKey, UserName, Email, PasswordQuestion, Comment " +
                                        "IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangeDate, LastLockedOutDate " +
                                        "FROM Users WHERE UserName LIKE ?";

                    reader = (DbDataReader)cmd.ExecuteReader();

                    int counter = 0;
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    while (reader.Read())
                    {
                        if (counter >= startIndex)
                        {
                            MembershipUser u = GetUserFromReader(reader);
                            users.Add(u);
                        }

                        if (counter >= endIndex)
                        {
                            cmd.Cancel();
                        }

                        counter++;
                    }
                    reader.Close();
                    tran.Commit();
                }
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

            return users;

        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("SELECT COUNT(*) FROM Users " +
                                            "WHERE UserName LIKE ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@UserNameSearch", "varchar").Value = "%" + usernameToMatch + "%";

            var users = new MembershipUserCollection();

            DbDataReader reader = null;
            IDbTransaction tran = null;

            totalRecords = 0;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                totalRecords = (int)cmd.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                cmd.CommandText = "SELECT ProviderUserKey, UserName, Email, PasswordQuestion, Comment " +
                                    "IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangeDate, LastLockedOutDate " +
                                    "FROM Users WHERE UserName LIKE ?";

                reader = (DbDataReader)cmd.ExecuteReader();

                int counter = 0;
                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                while (reader.Read())
                {
                    if (counter >= startIndex)
                    {
                        var u = GetUserFromReader(reader);
                        users.Add(u);
                    }

                    if (counter >= endIndex)
                    {
                        cmd.Cancel();
                    }

                    counter++;
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

            return users;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("SELECT COUNT(*) FROM Users " +
                                           "WHERE IsDeleted = 0", conn);

            var users = new MembershipUserCollection();

            DbDataReader reader = null;
            totalRecords = 0;
            IDbTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;
                totalRecords = (int)cmd.ExecuteScalar();

                if (totalRecords <= 0)
                {
                    return users;
                }

                cmd.CommandText = "SELECT ProviderUserKey, UserName, Email, PasswordQuestion, Comment " +
                                    "IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastPasswordChangeDate " +
                                    "FROM Users WHERE IsDeleted = 0";

                reader = (DbDataReader)cmd.ExecuteReader();

                int counter = 0;
                int startIndex = pageSize * pageIndex;
                int endIndex = startIndex + pageSize - 1;

                while (reader.Read())
                {
                    if (counter >= startIndex)
                    {
                        MembershipUser u = GetUserFromReader(reader);
                        users.Add(u);
                    }

                    if (counter >= endIndex)
                    {
                        cmd.Cancel();
                    }

                    counter++;
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

            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            var onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            var compareTime = DateTime.Now.Subtract(onlineSpan);

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT COUNT(*) FROM Users " +
                                            "WHERE IsDeleted = 0 " +
                                            "AND LastActivityDate > ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@CompareDate", "datetime").Value = compareTime;

            int numOnline = 0;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                numOnline = (int)cmd.ExecuteScalar();

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

            return numOnline;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd =
                Instance.GetCommand(
                    "SELECT Password, PasswordAnswer, IsLockedOut " +
                       "WHERE UserName = ?  AND IsDeleted = 0", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

            string password = "";
            string passwordAnswer = "";
            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.HasRows)
                {
                    reader.Read();

                    if (reader.GetBoolean(2))
                        throw new MembershipPasswordException("The supplied user is locked out.");

                    password = reader.GetString(0);
                    passwordAnswer = reader.GetString(1);
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
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


            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }


            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = DecodePassword(password);
            }

            return password;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("SELECT ProviderUserKey, UserName, Email, PasswordQuestion, \"Comment\", " +
                                            "IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangeDate, LastLockedOutDate " +
                                            "FROM Users WHERE UserName = ? " +
                                            "AND IsDeleted = 0", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;


            MembershipUser u = null;
            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    u = GetUserFromReader(reader);

                    if (userIsOnline)
                    {
                        var updateCmd = Instance.GetCommand("UPDATE Users " +
                                                        " SET LastActivityDate = ? " +
                                                        " WHERE Username = ?", conn);

                        Instance.AddParameter(updateCmd, ConnectionType, "@LastActivityDate", "datetime").Value = DateTime.Now;
                        Instance.AddParameter(updateCmd, ConnectionType, "@Username", "varchar").Value = username;

                        updateCmd.Transaction = tran;

                        updateCmd.ExecuteNonQuery();
                    }
                }
                reader.Close();
                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null)
                {
                    if (reader != null)
                        reader.Close();
                    tran.Rollback();
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                conn.Close();
            }

            return u;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null)
            {
                return null;
            }
            var keyGuid = providerUserKey.ToString();
            if (string.IsNullOrEmpty(keyGuid))
            {
                return null;
            }

            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("SELECT ProviderUserKey, UserName, Email, PasswordQuestion, Comment " +
                                            "IsApproved, IsLockedOut, CreationDate, LastLoginDate, LastActivityDate, LastPasswordChangeDate, LastLockedOutDate " +
                                            "FROM Users WHERE ProviderUserKey LIKE ?" +
                                            "AND IsDeleted = 0", conn);

            Instance.AddParameter(cmd, ConnectionType, "@ProviderUserKey", "varchar").Value = keyGuid;

            MembershipUser u = null;
            DbDataReader reader = null;
            IDbTransaction tran = null;


            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    u = GetUserFromReader(reader);
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

            return u;
        }

        public override string GetUserNameByEmail(string email)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT UserName" +
                                      " FROM Users WHERE Email LIKE ? " +
                                      " AND IsDeleted = 0", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Email", "varchar").Value = email;

            string username = "";

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                username = (string)cmd.ExecuteScalar();

                tran.Commit();
            }
            catch (Exception e)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
            }
            finally
            {
                conn.Close();
            }

            if (username == null)
            {
                username = "";
            }

            return username;
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return pMaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return pMinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return pMinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return pPasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return pPasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return pPasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return pRequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return pRequiresUniqueEmail; }
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            var newPassword = Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);

            var args = new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");


            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("SELECT PasswordAnswer, IsLockedOut FROM Users " +
                                        "WHERE IsDeleted=0 AND UserName=?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

            int rowsAffected = 0;
            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader(CommandBehavior.Default);

                string passwordAnswer;
                if (reader.HasRows)
                {
                    reader.Read();

                    if (reader.GetBoolean(1))
                        throw new MembershipPasswordException("The supplied user is locked out.");

                    passwordAnswer = reader.GetString(0);
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }

                if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                {
                    UpdateFailureCount(username, "passwordAnswer");

                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                var updateCmd = Instance.GetCommand("UPDATE Users " +
                                                " SET Password = ?, LastPasswordChangeDate = ?" +
                                                " WHERE Username = ? ", conn);

                Instance.AddParameter(updateCmd, ConnectionType, "@Password", "varchar").Value = EncodePassword(newPassword);
                Instance.AddParameter(updateCmd, ConnectionType, "@LastPasswordChangedDate", "datetime").Value = DateTime.Now;
                Instance.AddParameter(updateCmd, ConnectionType, "@Username", "varchar").Value = username;

                updateCmd.Transaction = tran;

                rowsAffected = updateCmd.ExecuteNonQuery();

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

            if (rowsAffected > 0)
            {
                return newPassword;
            }
            else
            {

                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }

        }

        public override bool UnlockUser(string userName)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("UPDATE Users " +
                                      " SET IsLockedOut = 0, LastLockedOutDate = ? " +
                                      " WHERE Username = ? )", conn);

            Instance.AddParameter(cmd, ConnectionType, "@LastLockedOutDate", "datetime").Value = DateTime.Now;
            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = userName;

            int rowsAffected = 0;

            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                rowsAffected = cmd.ExecuteNonQuery();

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

            if (rowsAffected > 0)
                return true;

            return false;
        }

        public override void UpdateUser(MembershipUser user)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd = Instance.GetCommand("UPDATE Users " +
                                      " SET Email = ?, Comment = ?," +
                                      " IsApproved = ?" +
                                      " WHERE Username = ? ", conn);

            Instance.AddParameter(cmd, ConnectionType, "@Email", "varchar").Value = user.Email;
            Instance.AddParameter(cmd, ConnectionType, "@Comment", "varchar").Value = user.Comment;
            Instance.AddParameter(cmd, ConnectionType, "@IsApproved", "bit").Value = user.IsApproved;
            Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = user.UserName;

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
        }

        public override bool ValidateUser(string username, string password)
        {
            var isValid = false;

            var conn = Instance.GetConnection(ConnectionType, connectionString);
            var cmd =
                Instance.GetCommand("SELECT Password, IsApproved FROM Users " +
                                " WHERE UserName = ? " +
                                " AND IsLockedOut = 0 AND IsDeleted=0", conn);

            var userNameParam = Instance.AddParameter(cmd, ConnectionType, "@Username", "nvarchar");
            userNameParam.Size = 255;
            userNameParam.Value = username;

            DbDataReader reader = null;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader();

                bool isApproved;
                string pwd;
                if (reader.HasRows)
                {
                    reader.Read();
                    pwd = reader.GetString(0);
                    isApproved = reader.GetBoolean(1);
                }
                else
                {
                    return false;
                }

                reader.Close();

                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        isValid = true;

                        var updateCmd = Instance.GetCommand("UPDATE Users SET LastLoginDate = ?" +
                                                        "WHERE UserName = ? " +
                                                        "AND IsDeleted=0", conn);

                        Instance.AddParameter(updateCmd, ConnectionType, "@LastLoginDate", "datetime").Value = DateTime.Now;
                        Instance.AddParameter(updateCmd, ConnectionType, "@Username", "varchar").Value = username;

                        updateCmd.Transaction = tran;

                        updateCmd.ExecuteNonQuery();
                    }
                    tran.Commit();
                }
                else
                {
                    tran.Commit();
                    conn.Close();

                    UpdateFailureCount(username, "password");
                }
            }
            catch (Exception e)
            {
                if (tran != null)
                {
                    tran.Rollback();
                    conn.Close();
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }

            return isValid;
        }

        private static string GetConfigValue(string configValue, string defaultValue)
        {
            return String.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }

        //
        // HexToByte
        //   Converts a hexadecimal string to a byte array. Used to convert encryption
        // key values from the configuration.
        //
        private static byte[] HexToByte(string hexString)
        {
            var returnBytes = new byte[hexString.Length / 2];
            for (var i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                        Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    var hash = new HMACSHA1 { Key = HexToByte(machineKey.ValidationKey) };
                    encodedPassword =
                        Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        //
        // GetUserFromReader
        //    A helper function that takes the current row from the DbDataReader
        // and hydrates a MembershiUser from the values. Called by the 
        // MembershipUser.GetUser implementation.
        //
        private MembershipUser GetUserFromReader(IDataRecord reader)
        {

            if (reader == null) throw new ArgumentNullException("reader");
            var providerUserKey = reader.GetString(0);
            var username = reader.GetString(1);
            var email = reader.GetString(2);

            var passwordQuestion = "";
            if (reader.GetValue(3) != DBNull.Value)
                passwordQuestion = reader.GetString(3);

            var comment = "";
            if (reader.GetValue(4) != DBNull.Value)
                comment = reader.GetString(4);

            var isApproved = reader.GetBoolean(5);
            var isLockedOut = reader.GetBoolean(6);
            var creationDate = new DateTime();
            if (reader.GetValue(7) != DBNull.Value)
            {
                creationDate = reader.GetDateTime(7);
            }

            var lastLoginDate = new DateTime();
            if (reader.GetValue(8) != DBNull.Value)
            {
                lastLoginDate = reader.GetDateTime(8);
            }

            var lastActivityDate = new DateTime();
            if (reader.GetValue(9) != DBNull.Value)
            {
                lastLoginDate = reader.GetDateTime(9);
            }

            var lastPasswordChangedDate = new DateTime();
            if (reader.GetValue(10) != DBNull.Value)
            {
                lastPasswordChangedDate = reader.GetDateTime(10);
            }

            var lastLockedOutDate = new DateTime();
            if (reader.GetValue(11) != DBNull.Value)
            {
                lastLockedOutDate = reader.GetDateTime(11);
            }

            var u = new MembershipUser(Name,
                                       username,
                                       providerUserKey,
                                       email,
                                       passwordQuestion,
                                       comment,
                                       isApproved,
                                       isLockedOut,
                                       creationDate,
                                       lastLoginDate,
                                       lastActivityDate,
                                       lastPasswordChangedDate,
                                       lastLockedOutDate);
            return u;
        }

        //
        // CheckPassword
        //   Compares password values based on the MembershipPasswordFormat.
        //
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    //pass1 = EncodePassword(password);
                    pass2 = DecodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        //
        // UnEncodePassword
        //   Decrypts or leaves the password clear based on the PasswordFormat.
        //
        private string DecodePassword(string encodedPassword)
        {
            var password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                        Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        //
        // UpdateFailureCount
        //   A helper method that performs the checks and updates associated with
        // password failure tracking.
        //
        private void UpdateFailureCount(string username, string failureType)
        {
            var conn = Instance.GetConnection(ConnectionType, connectionString);

            var cmd = Instance.GetCommand("SELECT FailedPswrdAttempt_WinStr, FailedPswrdAnswer_AtmptCount, FailedPswrdAnswer_AtmptWinStr " +
                                           "FROM Users WHERE UserName = ?", conn);

            Instance.AddParameter(cmd, ConnectionType, "@UserName", "varchar").Value = username;

            DbDataReader reader = null;
            var windowStart = new DateTime();
            Int64 failureCount = 0;
            IDbTransaction tran = null;

            try
            {
                conn.Open();
                tran = conn.BeginTransaction();
                cmd.Transaction = tran;

                reader = (DbDataReader)cmd.ExecuteReader(CommandBehavior.SingleRow);

                if (reader.HasRows)
                {
                    reader.Read();

                    if (failureType == "password")
                    {
                        if (!reader.IsDBNull(0))
                        {
                            failureCount = reader.GetInt64(0);
                        }
                        if (!reader.IsDBNull(1))
                        {
                            windowStart = reader.GetDateTime(1);
                        }
                    }

                    if (failureType == "passwordAnswer")
                    {
                        if (!reader.IsDBNull(2))
                        {
                            failureCount = reader.GetInt64(2);
                        }
                        if (!reader.IsDBNull(3))
                        {
                            windowStart = reader.GetDateTime(3);
                        }
                    }
                }

                reader.Close();

                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.

                    if (failureType == "password")
                    {
                        cmd.CommandText = "UPDATE Users " +
                                          "SET FailedPswrdAttemptCount = ?, " +
                                          "FailedPswrdAttempt_WinStr = ? " +
                                          "WHERE UserName = ?)";
                    }

                    if (failureType == "passwordAnswer")
                    {
                        cmd.CommandText = "UPDATE Users " +
                                          "SET FailedPswrdAnswer_AtmptCount = ?, " +
                                          "FailedPswrdAnswer_AtmptWinStr = ? " +
                                          "UserName = ?)";
                    }

                    cmd.Parameters.Clear();

                    Instance.AddParameter(cmd, ConnectionType, "@Count", "int").Value = 1;
                    Instance.AddParameter(cmd, ConnectionType, "@WindowStart", "datetime").Value = DateTime.Now;
                    Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

                    if (cmd.ExecuteNonQuery() < 0)
                    {
                        throw new ProviderException("Unable to update failure count and window start.");
                    }
                }
                else
                {
                    if (failureCount++ >= MaxInvalidPasswordAttempts)
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.

                        cmd.CommandText = "UPDATE Users " +
                                          "SET IsLockedOut = ?, LastLockedOutDate = ? " +
                                          "UserName = ?)";

                        cmd.Parameters.Clear();

                        Instance.AddParameter(cmd, ConnectionType, "@IsLockedOut", "bit").Value = 1;
                        Instance.AddParameter(cmd, ConnectionType, "@LastLockedOutDate", "datetime").Value = DateTime.Now;
                        Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

                        if (cmd.ExecuteNonQuery() < 0)
                            throw new ProviderException("Unable to lock out user.");
                    }
                    else
                    {
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.

                        failureCount++;

                        if (failureType == "password")
                            cmd.CommandText = "UPDATE Users " +
                                              "SET FailedPswrdAttemptCount = ?" +
                                              "WHERE U.Username = ?)";


                        if (failureType == "passwordAnswer")
                            cmd.CommandText = "UPDATE Users " +
                                              "SET FailedPswrdAnswer_AtmptCount = ?" +
                                              "WHERE Username = ?)";

                        cmd.Parameters.Clear();

                        Instance.AddParameter(cmd, ConnectionType, "@Count", "int").Value = failureCount;
                        Instance.AddParameter(cmd, ConnectionType, "@Username", "varchar").Value = username;

                        if (cmd.ExecuteNonQuery() < 0)
                            throw new ProviderException("Unable to update failure count.");
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
                if (reader != null)
                {
                    reader.Close();
                }
                conn.Close();
            }
        }
    }
}
