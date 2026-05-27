using System.Collections.ObjectModel;
using System.Text.Json;
using FitnessNutritionTracker.Repository;

namespace FitnessNutritionTracker;

public static class SharedData
{
    public static ObservableCollection<Goal> Goals { get; set; }

    public static decimal CurrentWeight { get; set; }
    public static int CurrentCalories { get; set; }
    public static int CurrentSteps { get; set; }
    public static double CurrentSleepHours { get; set; }

    public static double CurrentWorkout { get; set; }

    public static int CurrentFoodCalories { get; set; }

    public static event Action DataUpdated;

    public static event Action<decimal> WeightUpdated;



    // Call this method whenever you save a new activity
    public static void RefreshLatestValues()
    {
        using var db = new AppDbContext();
        var repo = new ActivityLogRepository(db);

        // Weight
        var weightLog = repo.GetLatestLogByType(LogType.Weight);
        if (weightLog != null)
            CurrentWeight = JsonSerializer.Deserialize<WeightActivity>(weightLog.Detail)?.Weight ?? CurrentWeight;

        // Workout / Calories
        var caloriesLog = repo.GetLatestLogByType(LogType.Workout);
        if (caloriesLog != null)
            CurrentCalories = JsonSerializer.Deserialize<WorkoutActivity>(caloriesLog.Detail)?.Calories ?? CurrentCalories;

        // Steps
        var stepsLog = repo.GetLatestLogByType(LogType.Steps);
        if (stepsLog != null)
            CurrentSteps = JsonSerializer.Deserialize<StepsActivity>(stepsLog.Detail)?.Steps ?? CurrentSteps;

        // Sleep
        var sleepLog = repo.GetLatestLogByType(LogType.Sleep);
        if (sleepLog != null)
            CurrentSleepHours = JsonSerializer.Deserialize<SleepActivity>(sleepLog.Detail)?.Hours ?? CurrentSleepHours;

        //food calories
        var foodlog = repo.GetLatestLogByType(LogType.Food);
        if (foodlog != null)
            CurrentSleepHours = JsonSerializer.Deserialize<FoodActivity>(foodlog.Detail)?.Calories ?? CurrentFoodCalories;

        // Notify subscribers
        DataUpdated?.Invoke();
    }


    public static void UpdateActivityData(string type, double value)
    {
        switch (type.ToLower())
        {
            case "steps":
                CurrentSteps += (int)value;
                break;
            case "sleep":
                CurrentSleepHours += value;
                break;
            case "workout":
            case "calories":
                CurrentCalories += (int)value;
                break;
            case "weight":
                CurrentWeight = (decimal)value;
                WeightUpdated?.Invoke(CurrentWeight);
                break;
            case "foodCalories":
                    CurrentFoodCalories += (int)value;
                break;
        }

        DataUpdated?.Invoke();
    }


    private static DateTime _lastReset = DateTime.Now;

    public static void ResetDailyStatsIfNeeded()
    {
        if ((DateTime.Now - _lastReset).TotalHours >= 24)
        {
            CurrentSteps = 0;
            CurrentCalories = 0;
            CurrentSleepHours = 0;
            _lastReset = DateTime.Now;
            DataUpdated?.Invoke();
        }
    }


}
