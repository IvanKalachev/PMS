using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class ServiceDao : BaseDao<Service, Int64>
    {
        public ServiceDao(ISession session)
            : base(session)
        {
        }

        public IList<Service> LoadAll()
        {
            return CurrentSession.Query<Service>()
                                 .Fetch(x => x.ChargeType)
                                 .OrderByDescending(x => x.Id)
                                 .ToList();
        }
    }
}
