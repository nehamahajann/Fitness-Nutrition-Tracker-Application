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
    public class ActivityViewModel : INotifyPropertyChanged
    {
        // Click Commands
        public ICommand ClickAnalyzeFoodPhoto { get; }
        public ICommand ClickWeightDecrese { get; }
        public ICommand ClickWeightIncr { get; }
        public ICommand ClickSaveWeightChange { get; }
        public ICommand ClickSaveCaloriesChange { get; }
        public ICommand ClickSaveSteps { get; }
        public ICommand ClickSaveSleepHours { get; }



        public static Array MealTypes => Enum.GetValues(typeof(MealType));
        public static Array LogTypes => Enum.GetValues(typeof(LogType));
        public static Array WorkoutTypes => Enum.GetValues(typeof(WorkoutType));


        public MealType SelectedMealType { get; set; } = MealType.Breakfast;
        public LogType SelectedLogType { get; set; } = LogType.Food;
        public WorkoutType SelectedWorkoutType { get; set; } = WorkoutType.Running;


        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }


        private decimal _currentWeigh = 50.0m;
        public decimal CurrentWeight
        {
            get => _currentWeigh;
            set
            {
                if (_currentWeigh != value)
                {
                    _currentWeigh = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _inputCalories = string.Empty;
        public string InputCalories
        {
            get => _inputCalories;
            set
            {
                var cleaned = Regex.Replace(value ?? "", @"[^0-9]", "");
                if (_inputCalories != cleaned)
                {
                    _inputCalories = cleaned;
                    OnPropertyChanged(nameof(InputCalories));
                }
            }
        }


        private string _inputSteps = string.Empty;
        public string InputSteps
        {
            get => _inputSteps;
            set
            {
                var cleaned = Regex.Replace(value ?? "", @"[^0-9]", "");
                if (_inputSteps != cleaned)
                {
                    _inputSteps = cleaned;
                    OnPropertyChanged(nameof(InputSteps));
                }
            }
        }

        private string _inputSleepHours = string.Empty;
        public string InputSleepHours
        {
            get => _inputSleepHours;
            set
            {
                var cleaned = Regex.Replace(value ?? "", @"[^0-9]", "");
                if (_inputSleepHours != cleaned)
                {
                    _inputSleepHours = cleaned;
                    OnPropertyChanged(nameof(InputSleepHours));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public ActivityViewModel()
        {
            UpdateData();

            ClickAnalyzeFoodPhoto = new RelayCommand(AnalyzeFoodPhoto_Click);
            ClickWeightDecrese = new RelayCommand(WeightDecrese_Click);
            ClickWeightIncr = new RelayCommand(WeightIncr_Click);
            ClickSaveWeightChange = new RelayCommand(SaveWeightChange_Click);
            ClickSaveCaloriesChange = new RelayCommand(SaveCalories_Click);
            ClickSaveSteps = new RelayCommand(SaveSteps_Click);
            ClickSaveSleepHours = new RelayCommand(SaveSleepHours_Click);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private void UpdateData()
        {
            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);

            ActivityLog? ActivityLog = rep.GetLatestLogByType(LogType.Weight);

            if (ActivityLog != null)
            {
                WeightActivity? WeightActivityObj = JsonSerializer.Deserialize<WeightActivity>(ActivityLog.Detail);

                if (WeightActivityObj != null)
                {
                    this.CurrentWeight = WeightActivityObj.Weight;
                }
            }
        }


        private async void AnalyzeFoodPhoto_Click(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter for image files
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*";

            // select file
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string SelectedImagePath = openFileDialog.FileName;

                    var client = new GeminiApiClient();

                    MessageBox.Show("Upload successful, please wait for system analysis");

                    string result = await client.RequestGemini(SelectedImagePath);

                    GeminiObject? GeminiResp = JsonSerializer.Deserialize<GeminiObject>(result);

                    if (GeminiResp == null)
                    {
                        MessageBox.Show("Unexpected error AddActivity:43");
                        return;
                    }

                    string answerString = GeminiResp.Candidates[0].Content.Parts[0].Text;

                    MealNutrition? AnswerObj = JsonSerializer.Deserialize<MealNutrition>(answerString);

                    if (AnswerObj == null)
                    {
                        MessageBox.Show("Unexpected error AddActivity:53");
                        return;
                    }

                    using var db = new AppDbContext();

                    ActivityLogRepository rep = new ActivityLogRepository(db);

                    FoodActivity logDetail = new FoodActivity
                    {
                        MealType = SelectedMealType,
                        FoodName = AnswerObj.MealName,
                        Protein = AnswerObj.Protein,
                        Carbs = AnswerObj.Carbs,
                        Fat = AnswerObj.Fat,
                        Fiber = AnswerObj.Fiber,
                        Calories = AnswerObj.Calories,
                    };

                    string jsonString = JsonSerializer.Serialize(logDetail);

                    rep.Add(new ActivityLog
                    {
                        LogType = LogType.Food,
                        Detail = jsonString,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });

                    try
                    {
                        rep.Save();
                        FitnessNutritionTracker.SharedData.RefreshLatestValues();

                        FitnessNutritionTracker.SharedData.UpdateActivityData("foodCalories", AnswerObj.Calories);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Unable to add to database: {ex.Message}");
                    }

                    MessageBox.Show("Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to load photo: {ex.Message}");
                }
            }
        }


        private void WeightDecrese_Click(object parameter)
        {
            this.CurrentWeight -= 0.5m;
        }


        private void WeightIncr_Click(object parameter)
        {
            this.CurrentWeight += 0.5m;
        }


        private void SaveWeightChange_Click(object parameter)
        {
            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);


            ActivityLog? latestWeight = rep.GetLatestLogByType(LogType.Weight);

            var change = 0.0m;

            if (latestWeight != null)
            {
                WeightActivity? latestWeightObj = JsonSerializer.Deserialize<WeightActivity>(latestWeight.Detail);

                if (latestWeightObj != null)
                {
                    change = this.CurrentWeight - latestWeightObj.Weight;
                }
            }

            string jsonString = JsonSerializer.Serialize(new WeightActivity
            {
                Weight = this.CurrentWeight,
                Change = change
            });

            rep.Add(new ActivityLog
            {
                LogType = LogType.Weight,
                Detail = jsonString,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            try
            {

                rep.Save();

                FitnessNutritionTracker.SharedData.UpdateActivityData("weight", (double)this.CurrentWeight);
                DashboardSharedData.UpdateWeight((double)this.CurrentWeight);

                var weightGoal = db.GoalSettings.FirstOrDefault(g => g.Name == "Weight");
                if (weightGoal != null)
                {
                    weightGoal.Current = (double)this.CurrentWeight;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save data: {ex.Message}");
            }

            MessageBox.Show("Success");
        }


        private void SaveCalories_Click(object parameter)
        {
            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);

            string jsonString = JsonSerializer.Serialize(new WorkoutActivity
            {
                WorkoutType = this.SelectedWorkoutType,
                Calories = int.TryParse(this.InputCalories, out var calories) ? calories : 0,
            });

            rep.Add(new ActivityLog
            {
                LogType = LogType.Workout,
                Detail = jsonString,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            try
            {
                rep.Save();
                FitnessNutritionTracker.SharedData.RefreshLatestValues();
                int updatedcalories = int.TryParse(this.InputCalories, out var c) ? c : 0;
                FitnessNutritionTracker.SharedData.UpdateActivityData("calories", updatedcalories);


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save data: {ex.Message}");
            }

            MessageBox.Show("Success");
        }



        private void SaveSteps_Click(object parameter)
        {
            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);

            string jsonString = JsonSerializer.Serialize(new StepsActivity
            {
                Steps = int.TryParse(this.InputSteps, out var s) ? s : 0,
            });

            rep.Add(new ActivityLog
            {
                LogType = LogType.Steps,
                Detail = jsonString,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            try
            {
                rep.Save();
                FitnessNutritionTracker.SharedData.RefreshLatestValues();
                int updatedsteps = int.TryParse(this.InputSteps, out var steps) ? steps: 0;
                FitnessNutritionTracker.SharedData.UpdateActivityData("steps", updatedsteps);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save data: {ex.Message}");
            }

            List<ActivityLog> ActivityList = rep.GetListByLogType(LogType.Steps).ToList();

            MessageBox.Show("Success");

        }


        private void SaveSleepHours_Click(object parameter)
        {
            using var db = new AppDbContext();

            ActivityLogRepository rep = new ActivityLogRepository(db);

            string jsonString = JsonSerializer.Serialize(new SleepActivity
            {
                Hours = int.TryParse(this.InputSleepHours, out var s) ? s : 0,
            });

            rep.Add(new ActivityLog
            {
                LogType = LogType.Sleep,
                Detail = jsonString,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            try
            {
                rep.Save();
                FitnessNutritionTracker.SharedData.RefreshLatestValues();
                double sleepHours = double.TryParse(this.InputSleepHours, out var sh) ? sh : 0;
                FitnessNutritionTracker.SharedData.UpdateActivityData("sleep", sleepHours);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save data: {ex.Message}");
            }

            List<ActivityLog> ActivityList = rep.GetListByLogType(LogType.Sleep).ToList();

            MessageBox.Show("Success");
        }
    }
}