using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;

namespace PropertyManagement.Core.Dao
{
    public class ChargeTypeDao : BaseDao<ChargeType, Int64>
    {
        public ChargeTypeDao(ISession session)
            : base(session)
        {
        }
    }
}
