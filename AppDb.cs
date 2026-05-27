using FitnessNutritionTracker;
using Microsoft.EntityFrameworkCore;


public class AppDbContext : DbContext
{
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<GoalSetting> GoalSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=db.sqlite");
    }
}
