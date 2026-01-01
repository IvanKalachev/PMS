using PropertyManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class CurrencyDao : BaseDao<Currency, Int64>
    {
        public CurrencyDao(ISession session) : base(session)
        {
        }

        public Currency GetDefaultCurrency()
        {
            return CurrentSession.Query<Currency>()
                     .Where(x => x.IsDefault == true)
                     .FirstOrDefault();
        }
    }
}
