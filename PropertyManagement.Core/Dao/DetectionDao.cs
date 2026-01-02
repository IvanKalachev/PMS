using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using PropertyManagement.Core.Entities;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class DetectionDao : BaseDao<Detection, Int64>
    {
        public DetectionDao(ISession session)
            : base(session)
        {
        }

        public bool Create(Detection detection)
        {
            if (!CheckForExsistion(detection))
            {
                using (var transaction = CurrentSession.BeginTransaction())
                {
                    CurrentSession.Save(detection);
                    transaction.Commit();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckForExsistion(Detection detection)
        {
            var sameDetection = (from c in CurrentSession.Query<Detection>()
                                 where c.Month == detection.Month
                                 && c.Year == detection.Year
                                 select c).Count();

            if (sameDetection != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Detection> GetAll()
        {
            return CurrentSession.Query<Detection>()
                   .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                   .ToList();
        }

        public Int64 GetLastDetectionId()
        {
            return CurrentSession.Query<Detection>()
                   .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                   .Select(x => x.Id).FirstOrDefault();
        }

        public List<Detection> GetDetectionsWithoutStatements()
        {
            var result = new List<Detection>();

            var detections = GetAll();
            if (detections != null && detections.Any())
            {
                var statements = CurrentSession.Query<Statement>()
                                               .Fetch(x => x.Detection)
                                               .ToList();

                if (statements != null && statements.Any())
                {
                    foreach(var detection in detections)
                    {
                        var detectionStatement = statements.FirstOrDefault(x => x.Detection.Id == detection.Id);
                        if (detectionStatement == null)
                        {
                            result.Add(detection);
                        }
                    }
                }
                else
                {
                    result = detections;
                }
            }

            return result.OrderByDescending(x => x.Id).ToList();
        }
    }
}
