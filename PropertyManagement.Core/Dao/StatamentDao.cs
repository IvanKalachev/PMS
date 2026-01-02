using NHibernate;
using NHibernate.Linq;
using PropertyManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyManagement.Core.Dao
{
    public class StatamentDao : BaseDao<Statement, Int64>
    {
        public StatamentDao(ISession session)
            : base(session)
        {
        }

        public void CreateStatement(Detection detection, string data)
        {
            Statement statement = new Statement
            {
                Detection = detection,
                Data = data,
                InsertDate = DateTime.Now
            };

            SaveOrUpdate(statement);
        }

        public Statement GetStatementForDetection(Int64 detectionId)
        {
            return CurrentSession.Query<Statement>()
                   .Where(x => x.Detection.Id == detectionId)
                   .FirstOrDefault();
        }

        public List<Statement> GetAll()
        {
            return CurrentSession.Query<Statement>()
                    .OrderByDescending(x => x.Id)
                    .Fetch(x => x.Detection)
                    .ToList();
        }
    }
}
