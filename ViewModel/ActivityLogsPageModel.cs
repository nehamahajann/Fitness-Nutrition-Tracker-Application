using FitnessNutritionTracker.Repository;
using System.Collections.ObjectModel;


namespace FitnessNutritionTracker.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Windows;

    public class ActivityLogsPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<ActivityLog>? _activityLogList = new ObservableCollection<ActivityLog>();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ObservableCollection<ActivityLog>? ActivityLogList
        {
            get => _activityLogList;
            set
            {
                _activityLogList = value;
                OnPropertyChanged();
            }
        }


        public ActivityLogsPageModel()
        {
            LoadData();

            Messenger.UpdateRequested += LoadData;
        }


        public void LoadData()
        {
            using var db = new AppDbContext();
            ActivityLogRepository rep = new ActivityLogRepository(db);

            var logs = rep.GetAllArr().ToList();

            if (logs == null)
            {
                return;
            }

            if (ActivityLogList == null)
            {
                ActivityLogList = new ObservableCollection<ActivityLog>();
            }
            else
            {
                ActivityLogList.Clear();
            }

            foreach (var log in logs)
            {
                switch (log.LogType)
                {
                    case LogType.Weight:
                        var weightActivity = JsonSerializer.Deserialize<WeightActivity>(log.Detail);

                        if (weightActivity == null)
                        {
                            break;
                        }

                        var changeType = "increased";

                        if (weightActivity.Change < 0)
                        {
                            changeType = "decreased";
                        }

                        log.DetailDisplay = $"Your weight has {changeType}: {weightActivity.Change}";

                        break;
                    case LogType.Workout:
                        var actDetail = JsonSerializer.Deserialize<WorkoutActivity>(log.Detail);

                        if (actDetail == null)
                        {
                            break;
                        }

                        var WorkoutTypeDesc = actDetail.WorkoutType.ToString();

                        log.DetailDisplay = $"You burned {actDetail.Calories} Calories by {WorkoutTypeDesc}";

                        break;
                    case LogType.Food:
                        var foodActivity = JsonSerializer.Deserialize<FoodActivity>(log.Detail);

                        if (foodActivity == null)
                        {
                            break;
                        }

                        log.DetailDisplay = $"{foodActivity.MealType}: {foodActivity.FoodName}\nCalories: {foodActivity.Calories} Protein: {foodActivity.Protein} Carbs: {foodActivity.Carbs} Fat: {foodActivity.Fat} Fiber: {foodActivity.Fiber}";

                        break;
                    case LogType.Steps:
                        var StepsActivity = JsonSerializer.Deserialize<StepsActivity>(log.Detail);

                        if (StepsActivity == null)
                        {
                            break;
                        }

                        log.DetailDisplay = $"Wakled {StepsActivity.Steps} Steps";

                        break;
                    case LogType.Sleep:
                        var SleepActivity = JsonSerializer.Deserialize<SleepActivity>(log.Detail);

                        if (SleepActivity == null)
                        {
                            break;
                        }

                        log.DetailDisplay = $"Sleeped {SleepActivity.Hours} Hours";

                        break;
                    default:

                        break;
                }

                ActivityLogList.Add(log);
            }
        }
    }
}