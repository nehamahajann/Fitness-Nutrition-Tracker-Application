using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Media3D;
using FitnessNutritionTracker;
using Microsoft.EntityFrameworkCore;

namespace FitnessNutritionTracker.Repository
{
    public class ActivityLogRepository
    {
        private readonly AppDbContext _db;

        public ActivityLogRepository(AppDbContext db)
        {
            _db = db;

#if DEBUG
            _db.Database.EnsureCreated();
#endif
        }

        public IEnumerable<ActivityLog> GetAllArr()
        {
            return _db.ActivityLogs.AsNoTracking().OrderByDescending(e => e.Timestamp).ToArray();
        }


        public IEnumerable<ActivityLog> GetListByLogType(LogType logType)
        {
            return _db.ActivityLogs
                .AsNoTracking()
                .Where(e => e.LogType == logType)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        public ActivityLog? GetLatestLogByType(LogType logType)
        {
            return _db.ActivityLogs
                .AsNoTracking()
                .Where(e => e.LogType == logType)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();
        }

        public IEnumerable<ActivityLog> GetLatestLogByTypeLimitNum(LogType logType, int takeNum)
        {
            return _db.ActivityLogs
                .AsNoTracking()
                .Where(e => e.LogType == logType)
                .OrderByDescending(e => e.Timestamp)
                .Take(takeNum).ToList(); ;
        }


        public IEnumerable<ActivityLog> GetLimitNumLog(int takeNum)
        {
            return _db.ActivityLogs
                .AsNoTracking()
                .OrderByDescending(e => e.Timestamp)
                .Take(takeNum).ToList(); ;
        }


        public ActivityLog? GetById(int id)
        {
            return _db.ActivityLogs.Find(id);
        }

        public void Add(ActivityLog product)
        {
            _db.ActivityLogs.Add(product);
        }

        public void Update(ActivityLog product)
        {
            _db.ActivityLogs.Update(product);
        }

        public void Delete(int id)
        {
            var entity = _db.ActivityLogs.Find(id);
            if (entity != null)
                _db.ActivityLogs.Remove(entity);
        }

        public int Save()
        {
            return _db.SaveChanges();
        }


        public double GetLatestWeight()
        {
            var latestWeightLog = _db.ActivityLogs
                .Where(a => a.LogType == LogType.Weight) // or use enum: LogType.Weight
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();

            if (latestWeightLog != null)
            {
                try
                {
                    var weightActivity = JsonSerializer.Deserialize<WeightActivity>(latestWeightLog.Detail);
                    if (weightActivity != null)
                    {
                        return (double)weightActivity.Weight;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing weight JSON: {ex.Message}");
                }
            }

            return 0; // Default value if no valid weight found
        }



    }
}
