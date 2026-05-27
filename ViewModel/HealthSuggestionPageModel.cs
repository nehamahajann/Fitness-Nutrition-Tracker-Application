using FitnessNutritionTracker;
using FitnessNutritionTracker.Helpers;
using FitnessNutritionTracker.Repository;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessNutritionTracker.ViewModel
{
    public class HealthSuggestionPageModel : INotifyPropertyChanged
    {
        // Click Commands
        public ICommand DietarySuggestionCommands { get; }
        public ICommand FitnessTipCommands { get; }


        private string _userQuestion = "";
        public string UserQuestion
        {
            get => _userQuestion;
            set
            {
                if (_userQuestion != value)
                {
                    _userQuestion = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _aiRespText = "";
        public string AIRespText
        {
            get => _aiRespText;
            set
            {
                if (_aiRespText != value)
                {
                    _aiRespText = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public HealthSuggestionPageModel()
        {
            DietarySuggestionCommands = new RelayCommand(DietarySuggestion_Click);
            FitnessTipCommands = new RelayCommand(FitnessTip_Click);
        }


        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void DietarySuggestion_Click(object parameter)
        {
            this.UserQuestion = "Would you be able to give me some tailored dietary suggestions?";
            this.AIRespText = "...";

            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);

            IEnumerable<ActivityLog> foodLogs = rep.GetLatestLogByTypeLimitNum(LogType.Food, 10);

            var textfoodLogs = "";

            foreach (var log in foodLogs)
            {
                var foodActivity = JsonSerializer.Deserialize<FoodActivity>(log.Detail);

                if (foodActivity == null)
                {
                    continue;
                }

                textfoodLogs += $"{foodActivity.MealType}: {foodActivity.FoodName} Calories: {foodActivity.Calories} Protein: {foodActivity.Protein} Carbs: {foodActivity.Carbs} Fat: {foodActivity.Fat} Fiber: {foodActivity.Fiber}\n";
            }

            var promptText = $"Based on the following recent food intake logs, please provide dietary suggestions to improve nutrition and health. Consider factors such as balanced macronutrients, portion sizes, and meal timing. (Directly give suggestion, no irrelevent words, no markdown fomat in the text! do not over 200 words and be polite) Here are the logs:\n{textfoodLogs}";

            var client = new GeminiApiClient();


            client.RequestTextResp(promptText).ContinueWith((task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var response = task.Result;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GeminiObject? GeminiResp = JsonSerializer.Deserialize<GeminiObject>(response);

                        if (GeminiResp == null)
                        {
                            MessageBox.Show("Remote server has no response. Please try again.");
                            return;
                        }

                        this.AIRespText = $"Dietary Suggestions: {GeminiResp.Candidates[0].Content.Parts[0].Text}";
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Error getting dietary suggestions: " + task.Exception?.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }


        private void FitnessTip_Click(object parameter)
        {
            this.UserQuestion = "Would you be able to give me some tailored fitness tips?";
            this.AIRespText = "...";

            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);

            IEnumerable<ActivityLog> foodLogs = rep.GetLimitNumLog(10);

            var textLogs = "";

            foreach (var log in foodLogs)
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

                        textLogs += $"weight has {changeType}: {weightActivity.Change}\n";

                        break;
                    case LogType.Workout:
                        var actDetail = JsonSerializer.Deserialize<WorkoutActivity>(log.Detail);

                        if (actDetail == null)
                        {
                            break;
                        }

                        var WorkoutTypeDesc = actDetail.WorkoutType.ToString();

                        textLogs += $"burned {actDetail.Calories} Calories by {WorkoutTypeDesc}\n";

                        break;
                    case LogType.Food:
                        var foodActivity = JsonSerializer.Deserialize<FoodActivity>(log.Detail);

                        if (foodActivity == null)
                        {
                            break;
                        }

                        textLogs += $"{foodActivity.MealType}: {foodActivity.FoodName}\nCalories: {foodActivity.Calories} Protein: {foodActivity.Protein} Carbs: {foodActivity.Carbs} Fat: {foodActivity.Fat} Fiber: {foodActivity.Fiber}\n";

                        break;
                    case LogType.Steps:
                        var StepsActivity = JsonSerializer.Deserialize<StepsActivity>(log.Detail);

                        if (StepsActivity == null)
                        {
                            break;
                        }

                        textLogs += $"Wakled {StepsActivity.Steps} Steps \n";

                        break;
                    case LogType.Sleep:
                        var SleepActivity = JsonSerializer.Deserialize<SleepActivity>(log.Detail);

                        if (SleepActivity == null)
                        {
                            break;
                        }

                        textLogs += $"Sleeped {SleepActivity.Hours} Hours\n";

                        break;
                    default:

                        break;
                }
            }

            var promptText = $"Based on the following recent food intake and workout logs, please provide fitness suggestions to improve health. (Directly give suggestion, no irrelevent words, no markdown fomat in the text! do not over 200 words and be polite) Here are the logs:\n{textLogs}";
            var client = new GeminiApiClient();

            MessageBox.Show(promptText);


            client.RequestTextResp(promptText).ContinueWith((task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var response = task.Result;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GeminiObject? GeminiResp = JsonSerializer.Deserialize<GeminiObject>(response);

                        if (GeminiResp == null)
                        {
                            MessageBox.Show("Remote server has no response. Please try again.");
                            return;
                        }

                        this.AIRespText = $"Dietary Suggestions: {GeminiResp.Candidates[0].Content.Parts[0].Text}";
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Error getting dietary suggestions: " + task.Exception?.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }


    }



}