using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using NHibernate;

namespace PropertyManagement.Web
{
    public class OrakSybaseDialect : SybaseSQLAnywhere10Dialect
    {
        protected override void RegisterStringFunctions()
        {
            RegisterFunction("ascii", new StandardSQLFunction("ascii", NHibernateUtil.Int32));
            RegisterFunction("byte64_decode", new StandardSQLFunction("byte64_decode", NHibernateUtil.StringClob));
            RegisterFunction("byte64_encode", new StandardSQLFunction("byte64_encode", NHibernateUtil.StringClob));
            RegisterFunction("byte_length", new StandardSQLFunction("byte_length", NHibernateUtil.Int32));
            RegisterFunction("byte_substr", new VarArgsSQLFunction(NHibernateUtil.String, "byte_substr(", ",", ")"));
            RegisterFunction("char", new StandardSQLFunction("char", NHibernateUtil.String));
            RegisterFunction("charindex", new StandardSQLFunction("charindex", NHibernateUtil.Int32));
            RegisterFunction("char_length", new StandardSQLFunction("char_length", NHibernateUtil.Int32));
            RegisterFunction("compare", new VarArgsSQLFunction(NHibernateUtil.Int32, "compare(", ",", ")"));
            RegisterFunction("compress", new VarArgsSQLFunction(NHibernateUtil.BinaryBlob, "compress(", ",", ")"));
            RegisterFunction("concat", new VarArgsSQLFunction(NHibernateUtil.String, "(", "||", ")"));
            RegisterFunction("csconvert", new VarArgsSQLFunction(NHibernateUtil.StringClob, "csconvert(", ",", ")"));
            RegisterFunction("decompress", new VarArgsSQLFunction(NHibernateUtil.BinaryBlob, "decompress(", ",", ")"));
            RegisterFunction("decrypt", new VarArgsSQLFunction(NHibernateUtil.BinaryBlob, "decrypt(", ",", ")"));
            RegisterFunction("difference", new StandardSQLFunction("difference", NHibernateUtil.Int32));
            RegisterFunction("encrypt", new VarArgsSQLFunction(NHibernateUtil.BinaryBlob, "encrypt(", ",", ")"));
            RegisterFunction("hash", new VarArgsSQLFunction(NHibernateUtil.String, "hash(", ",", ")"));
            RegisterFunction("insertstr", new StandardSQLFunction("insertstr", NHibernateUtil.String));
            RegisterFunction("lcase", new StandardSQLFunction("lcase", NHibernateUtil.String));
            RegisterFunction("left", new StandardSQLFunction("left", NHibernateUtil.String));
            RegisterFunction("length", new StandardSQLFunction("length", NHibernateUtil.Int32));
            RegisterFunction("locate", new VarArgsSQLFunction(NHibernateUtil.Int32, "locate(", ",", ")"));
            RegisterFunction("lower", new StandardSQLFunction("lower", NHibernateUtil.String));
            RegisterFunction("ltrim", new StandardSQLFunction("ltrim", NHibernateUtil.String));
            RegisterFunction("patindex", new StandardSQLFunction("patindex", NHibernateUtil.Int32));
            RegisterFunction("repeat", new StandardSQLFunction("repeat", NHibernateUtil.String));
            RegisterFunction("replace", new StandardSQLFunction("replace", NHibernateUtil.String));
            RegisterFunction("replicate", new StandardSQLFunction("replicate", NHibernateUtil.String));
            RegisterFunction("reverse", new StandardSQLFunction("reverse", NHibernateUtil.String));
            RegisterFunction("right", new StandardSQLFunction("right", NHibernateUtil.String));
            RegisterFunction("rtrim", new StandardSQLFunction("rtrim", NHibernateUtil.String));
            RegisterFunction("similar", new StandardSQLFunction("rtrim", NHibernateUtil.Int32));
            RegisterFunction("sortkey", new VarArgsSQLFunction(NHibernateUtil.Binary, "sortkey(", ",", ")"));
            RegisterFunction("soundex", new StandardSQLFunction("soundex", NHibernateUtil.Int32));
            RegisterFunction("space", new StandardSQLFunction("space", NHibernateUtil.String));
            RegisterFunction("str", new VarArgsSQLFunction(NHibernateUtil.String, "str(", ",", ")"));
            RegisterFunction("string", new VarArgsSQLFunction(NHibernateUtil.String, "string(", ",", ")"));
            RegisterFunction("strtouuid", new StandardSQLFunction("strtouuid"));
            RegisterFunction("stuff", new StandardSQLFunction("stuff", NHibernateUtil.String));

            // In SQL Anywhere 10, substr() semantics depends on the ANSI_substring option

            RegisterFunction("substr", new VarArgsSQLFunction(NHibernateUtil.String, "substr(", ",", ")"));
            RegisterFunction("substring", new VarArgsSQLFunction(NHibernateUtil.String, "substr(", ",", ")"));
            RegisterFunction("to_char", new VarArgsSQLFunction(NHibernateUtil.String, "to_char(", ",", ")"));
            RegisterFunction("to_nchar", new VarArgsSQLFunction(NHibernateUtil.String, "to_nchar(", ",", ")"));

            RegisterFunction("trim", new StandardSQLFunction("trim", NHibernateUtil.String));
            RegisterFunction("ucase", new StandardSQLFunction("ucase", NHibernateUtil.String));
            RegisterFunction("unicode", new StandardSQLFunction("unicode", NHibernateUtil.Int32));
            RegisterFunction("unistr", new StandardSQLFunction("unistr", NHibernateUtil.String));
            RegisterFunction("upper", new StandardSQLFunction("upper", NHibernateUtil.String));
            RegisterFunction("uuidtostr", new StandardSQLFunction("uuidtostr", NHibernateUtil.String));
        }
    }
}