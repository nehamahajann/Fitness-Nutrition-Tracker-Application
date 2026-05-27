using System.Collections.ObjectModel;
using System.Windows;
using FitnessNutritionTracker.Repository;

namespace FitnessNutritionTracker.ViewModel
{
    public class GoalSettingsViewModel
    {
        public ObservableCollection<FitnessNutritionTracker.Goal> Goals { get; set; }

        private static GoalSettingsViewModel _instance;
        public static GoalSettingsViewModel Instance => _instance ??= new GoalSettingsViewModel();


        private GoalSettingsViewModel()
        {
            using var db = new AppDbContext();
            var repo = new ActivityLogRepository(db);
            double currentWeight = repo.GetLatestWeight(); // This method must be non-static

            //for Weight

            var latestWeightGoal = db.GoalSettings.Where(g => g.Name == "Weight")
            .OrderByDescending(g => g.Timestamp)
            .FirstOrDefault();

            double targetWeight = latestWeightGoal?.Target ?? 70;


            //for Food calories

            var latestFoodCaloriesGoal = db.GoalSettings.Where(g => g.Name == "Calories")
           .OrderByDescending(g => g.Timestamp)
           .FirstOrDefault();

            double targetFoodCaloriesgoal = latestFoodCaloriesGoal?.Target ?? 2000;

            // Sleep
            var latestSleepGoal = db.GoalSettings.Where(g => g.Name == "Sleep")
                                                .OrderByDescending(g => g.Timestamp)
                                                .FirstOrDefault();
            double targetSleep = latestSleepGoal?.Target ?? 8;
            double currentSleep = latestSleepGoal?.Current ?? 0;

            // Steps
            var latestStepsGoal = db.GoalSettings.Where(g => g.Name == "Steps")
                                                .OrderByDescending(g => g.Timestamp)
                                                .FirstOrDefault();
            int targetSteps = (int)(latestStepsGoal?.Target ?? 8000);
            int currentSteps = (int)(latestStepsGoal?.Current ?? 0);

            // Workout
            var latestWorkoutGoal = db.GoalSettings.Where(g => g.Name == "Workout")
                                                  .OrderByDescending(g => g.Timestamp)
                                                  .FirstOrDefault();
            double targetWorkout = latestWorkoutGoal?.Target ?? 800;
            double currentWorkout = latestWorkoutGoal?.Current ?? 0;    //needed or not Omkar

            Goals = new ObservableCollection<FitnessNutritionTracker.Goal>
    {
        new FitnessNutritionTracker.Goal { Name = "Weight", Start = currentWeight, Current = currentWeight,Target = targetWeight, Unit = "kg", Type = FitnessNutritionTracker.GoalType.Gain},
        new FitnessNutritionTracker.Goal { Name = "Calories", Start = 0, Current = SharedData.CurrentCalories, Target = targetFoodCaloriesgoal, Unit = "kcal" },
        new FitnessNutritionTracker.Goal { Name = "Sleep", Start = 0, Current = SharedData.CurrentSleepHours, Target = targetSleep, Unit = "hrs" },
        new FitnessNutritionTracker.Goal { Name = "Steps", Start = 0, Current = SharedData.CurrentSteps, Target = targetSteps, Unit = "steps" },
        new FitnessNutritionTracker.Goal { Name = "Workout", Start = 0, Current = SharedData.CurrentWorkout, Target = targetWorkout, Unit = "kcal" }, 
    };

            FitnessNutritionTracker.SharedData.Goals = Goals;
        }


        public void RefreshGoals()
        {
            using var db = new AppDbContext();
            var repo = new ActivityLogRepository(db);
            double currentWeight = repo.GetLatestWeight();

            var weightGoal = Goals.FirstOrDefault(g => g.Name == "Weight");
            if (weightGoal != null)
            {
                weightGoal.Current = currentWeight;
            }
        }

        public void SaveGoalsToDatabase()
        {
            try
            {
                using var db = new AppDbContext();

                // Get all existing goals
                var existingGoals = db.GoalSettings.ToList();

                foreach (var goal in Goals)
                {
                    // Try to find an existing goal by name
                    var existing = existingGoals.FirstOrDefault(g => g.Name == goal.Name);

                    if (existing != null)
                    {
                        // Update existing
                        existing.Current = goal.Current;
                        existing.Target = goal.Target;
                        existing.Timestamp = DateTime.Now;
                        db.Update(existing);
                    }
                    else
                    {
                        // Add new goal
                        db.GoalSettings.Add(new GoalSetting
                        {
                            Name = goal.Name,
                            Start = goal.Start,
                            Current = goal.Current,
                            Target = goal.Target,
                            Unit = goal.Unit,
                            Timestamp = DateTime.Now
                        });
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving goals: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
