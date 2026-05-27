using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using FitnessNutritionTracker.ViewModel;


namespace FitnessNutritionTracker;

/// <summary>
/// Interaction logic for Dashboard.xaml
/// </summary>
public partial class Dashboard : UserControl, INotifyPropertyChanged
{
    // Properties 
    private decimal _currentWeight;
    public decimal CurrentWeight { get => _currentWeight; set { _currentWeight = value; OnPropertyChanged(); } }

    private int _currentCalories;
    public int CurrentCalories { get => _currentCalories; set { _currentCalories = value; OnPropertyChanged(); } }

    private int _currentSteps;
    public int CurrentSteps { get => _currentSteps; set { _currentSteps = value; OnPropertyChanged(); } }

    private double _currentSleep;
    public double CurrentSleep { get => _currentSleep; set { _currentSleep = value; OnPropertyChanged(); } }

    private double _workoutGoal; 
    public double WorkoutGoal { get => _workoutGoal; set {_workoutGoal = value; OnPropertyChanged(); OnPropertyChanged(nameof(WorkoutProgress)); }}

    private DateTime _lastResetDate = DateTime.Now.Date;

    private double _currentWorkoutCalories;
    public double CurrentWorkoutCalories
    {
        get => _currentWorkoutCalories;
        set { 
                _currentWorkoutCalories = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(WorkoutProgress));
            }
    }

    private double _goalWorkoutCalories;
    public double GoalWorkoutCalories
    {
        get => _goalWorkoutCalories;
        set
        {
            _goalWorkoutCalories = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WorkoutProgress));
        }
    }

    private string _lastUpdated;
    public string LastUpdated
    {
        get => _lastUpdated;
        set { _lastUpdated = value; OnPropertyChanged(); }
    }

    private decimal _goalWeight;    
    public decimal GoalWeight
    {
        get => _goalWeight;
        set { _goalWeight = value; OnPropertyChanged(); } 
    }

    private int _goalSteps;  
    public int GoalSteps
    {
        get => _goalSteps;
        set
        {
            _goalSteps = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StepsProgress));
        }
    }

    private double _goalSleep;  
    public double GoalSleep
    {
        get => _goalSleep;
        set
        {
            _goalSleep = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SleepProgress));
        }

    }

    // --- Food Tracker ---
    private int _goalFoodCalories; 
    public int GoalFoodCalories
    {
        get => _goalFoodCalories;
        set
        {
            _goalFoodCalories = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FoodProgress));
        }
    }

    private int _currentFoodCalories; 
    public int CurrentFoodCalories
    {
        get => _currentFoodCalories;
        set
        {
            _currentFoodCalories = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FoodProgress));
        }
    }

    // Computed Properties
    public string FoodProgress => $"{CurrentFoodCalories} of {GoalFoodCalories} Cal eaten";

    public string StepsProgress => $"{CurrentSteps} of {GoalSteps} steps";

    public string SleepProgress => $"{CurrentSleep} of {GoalSleep} hrs";

    public string WorkoutProgress => $"{CurrentWorkoutCalories} of {GoalWorkoutCalories} cal";

    private ActivityViewModel _vm;

    // Properties to store Protein, carb, fat, fiber in food item

    private int _proteinPercent;
    public int ProteinPercent
    {
        get => _proteinPercent;
        set { _proteinPercent = value; OnPropertyChanged(); }
    }

    private int _fatPercent;
    public int FatPercent
    {
        get => _fatPercent;
        set { _fatPercent = value; OnPropertyChanged(); }
    }

    private int _carbPercent;
    public int CarbPercent
    {
        get => _carbPercent;
        set { _carbPercent = value; OnPropertyChanged(); }
    }

    private int _fibrePercent;
    public int FibrePercent
    {
        get => _fibrePercent;
        set { _fibrePercent = value; OnPropertyChanged(); }
    }

    public Dashboard()
    {
        InitializeComponent();
        this.DataContext = this;

        LoadLatestValues();

        // Subscribe to live updates
        SharedData.DataUpdated += LoadLatestValues;

        DashboardSharedData.WeightUpdated += () =>
        {
            CurrentWeight = (decimal)DashboardSharedData.CurrentWeight;
        };

        DashboardSharedData.GoalWeightUpdated += () =>
        {
            GoalWeight = (decimal)DashboardSharedData.GoalWeight;
        };

        DashboardSharedData.GoalWorkoutUpdated += () =>
        {
            GoalWorkoutCalories = DashboardSharedData.GoalWorkout; 
        };

        DashboardSharedData.GoalStepsUpdated += () =>
        {
            GoalSteps = DashboardSharedData.GoalSteps;
        };

        DashboardSharedData.GoalSleepUpdated += () =>
        {
            GoalSleep = DashboardSharedData.GoalSleep;
        };

        DashboardSharedData.GoalFoodUpdated += () =>
        {
            GoalFoodCalories = DashboardSharedData.GoalFoodCalories;
        };

    }
    private void LoadLatestValues()
    {
        using var db = new AppDbContext();
       db.Database.EnsureCreated();
        var weightGoal = db.GoalSettings.FirstOrDefault(g => g.Name == "Weight");
        if (weightGoal != null)
        {
            CurrentWeight = (decimal)weightGoal.Current; // Actual weight
            GoalWeight = (decimal)weightGoal.Target;     // Target weight
        }


        // --- Load workout goal & current calories dynamically ---
        var workoutGoal = db.GoalSettings.FirstOrDefault(g => g.Name == "Workout");
        if (workoutGoal != null)
        {
            GoalWorkoutCalories = workoutGoal.Target;

            // Sum calories from today's workout logs
            DateTime today = DateTime.Now.Date;

            // Pull workout logs from DB and evaluate timestamp on client
            var todaysWorkoutLogs = db.ActivityLogs
                .Where(a => a.LogType == LogType.Workout)   // filter by workout first
                .AsEnumerable()                             // switch to client-side
                .Where(a => DateTimeOffset.FromUnixTimeSeconds(a.Timestamp).ToLocalTime().Date >= today)
                .ToList();

            // Sum the calories
            CurrentWorkoutCalories = todaysWorkoutLogs
                .Select(log => JsonSerializer.Deserialize<WorkoutActivity>(log.Detail))
                .Where(w => w != null)
                .Sum(w => w!.Calories);
        }

    
        // Load steps goal & sum today's steps
        var stepsGoal = db.GoalSettings.FirstOrDefault(g => g.Name == "Steps");
        if (stepsGoal != null)
        {
            GoalSteps = (int)stepsGoal.Target;

            // Pull today's step logs
            DateTime today = DateTime.Now.Date;

            var todaysStepLogs = db.ActivityLogs
                .Where(a => a.LogType == LogType.Steps)
                .AsEnumerable() // switch to client-side evaluation
                .Where(a => DateTimeOffset.FromUnixTimeSeconds(a.Timestamp).ToLocalTime().Date >= today)
                .ToList();

            // Sum steps
            CurrentSteps = todaysStepLogs
                .Select(log => JsonSerializer.Deserialize<StepsActivity>(log.Detail))
                .Where(s => s != null)
                .Sum(s => s!.Steps);

            // Also update SharedData
            SharedData.CurrentSteps = CurrentSteps;
        }


        // Load sleep goal & sum today's sleep hours
        var sleepGoal = db.GoalSettings.FirstOrDefault(g => g.Name == "Sleep");
        if (sleepGoal != null)
        {
            GoalSleep = sleepGoal.Target;

            // Pull today's sleep logs
            DateTime today = DateTime.Now.Date;

            var todaysSleepLogs = db.ActivityLogs
                .Where(a => a.LogType == LogType.Sleep)
                .AsEnumerable() // client-side evaluation for timestamp
                .Where(a => DateTimeOffset.FromUnixTimeSeconds(a.Timestamp).ToLocalTime().Date >= today)
                .ToList();

            // Sum sleep hours
            CurrentSleep = todaysSleepLogs
                .Select(log => JsonSerializer.Deserialize<SleepActivity>(log.Detail))
                .Where(s => s != null)
                .Sum(s => s!.Hours);

            // Also update SharedData
            SharedData.CurrentSleepHours = CurrentSleep;
        }

        // Load food goal
        var foodGoal = db.GoalSettings.FirstOrDefault(g => g.Name == "Calories");
        if (foodGoal != null)
        {
            GoalFoodCalories = (int)foodGoal.Target;

            // Pull today's food logs
            DateTime today = DateTime.Now.Date;

            var todaysFoodcaloriesLogs = db.ActivityLogs
                .Where(a => a.LogType == LogType.Food)
                .AsEnumerable() // client-side evaluation for timestamp
                .Where(a => DateTimeOffset.FromUnixTimeSeconds(a.Timestamp).ToLocalTime().Date >= today)
                .ToList();

            // Sum food hours
            CurrentFoodCalories = todaysFoodcaloriesLogs
                .Select(log => JsonSerializer.Deserialize<FoodActivity>(log.Detail))
                .Where(s => s != null)
                .Sum(s => s!.Calories);

            // Also update SharedData
            SharedData.CurrentFoodCalories = CurrentFoodCalories;

            // Calculate Protein, carb, fat, fiber using below formulas// ---
            var foods = todaysFoodcaloriesLogs
                .Select(log => JsonSerializer.Deserialize<FoodActivity>(log.Detail))  
                .Where(f => f != null)
                .ToList();

            int totalProtein = foods.Sum(f => f!.Protein);  //sum up all protein values of all foods in
            int totalFat = foods.Sum(f => f!.Fat);
            int totalCarb = foods.Sum(f => f!.Carbs);
            int totalFibre = foods.Sum(f => f!.Fiber);

            // Optional: calculate percentage of daily goal
            int proteinGoal = 100;    // daily goal values
            int fatGoal = 100;
            int carbGoal = 200;
            int fibreGoal = 30;

            ProteinPercent = totalProtein * 100 / proteinGoal;
            FatPercent = totalFat * 100 / fatGoal;
            CarbPercent = totalCarb * 100 / carbGoal;
            FibrePercent = totalFibre * 100 / fibreGoal;

        }

        SharedData.ResetDailyStatsIfNeeded();
    }


    private void CheckAndResetDailyStats()
    {
        if (_lastResetDate.Date < DateTime.Now.Date)
        {
            CurrentSteps = 0;
            CurrentWorkoutCalories = 0;
            CurrentSleep = 0;
            _lastResetDate = DateTime.Now.Date;
        }
    }

    public void AddActivityData(string type, double value)
    {
        CheckAndResetDailyStats(); // Reset if it's a new day

        switch (type.ToLower())
        {
            case "steps":
                CurrentSteps += (int)value;
                OnPropertyChanged(nameof(StepsProgress));
                break;

            case "workout":
                CurrentWorkoutCalories += value;
                OnPropertyChanged(nameof(WorkoutProgress));

                break;
            case "sleep":
                CurrentSleep += value;
                break;

            case "food":
                CurrentCalories += (int)value;
                OnPropertyChanged(nameof(FoodProgress));
                break;
         
            case "weight":
                CurrentWeight = (decimal)value; 
                DashboardSharedData.UpdateWeight(value); 
                break;
        }
        LastUpdated = DateTime.Now.ToString("hh:mm tt");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void TrackFoodButton_Click(object sender, RoutedEventArgs e)
    {

        var mainWindow = (MainWindow)Application.Current.MainWindow;

        // Find the "Add Activity" tab
        foreach (TabItem tab in mainWindow.tabControl.Items)
        {
            if (tab.Header.ToString() == "Add Activity")
            {
                mainWindow.tabControl.SelectedItem = tab;
                break;
            }
        }

    }

    public void TrackWeightButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;

        // Find the "Add Activity" tab
        foreach (TabItem tab in mainWindow.tabControl.Items)
        {
            if (tab.Header.ToString() == "Add Activity")
            {
                mainWindow.tabControl.SelectedItem = tab;
                break;
            }
        }
    }

    public void TrackWorkoutButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;

        // Find the "Add Activity" tab
        foreach (TabItem tab in mainWindow.tabControl.Items)
        {
            if (tab.Header.ToString() == "Add Activity")
            {
                mainWindow.tabControl.SelectedItem = tab;
                break;
            }
        }
    }

    public void TrackStepsButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;

        // Find the "Add Activity" tab
        foreach (TabItem tab in mainWindow.tabControl.Items)
        {
            if (tab.Header.ToString() == "Add Activity")
            {
                mainWindow.tabControl.SelectedItem = tab;
                break;
            }
        }

    }

    public void TrackSleepButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = (MainWindow)Application.Current.MainWindow;

        // Find the "Add Activity" tab
        foreach (TabItem tab in mainWindow.tabControl.Items)
        {
            if (tab.Header.ToString() == "Add Activity")
            {
                mainWindow.tabControl.SelectedItem = tab;
                break;
            }
        }
    }
}
public static class DashboardSharedData
     {
        public static event Action? WeightUpdated;
        public static event Action? GoalWeightUpdated;
        public static event Action? GoalWorkoutUpdated;
        public static event Action? GoalStepsUpdated;
        public static event Action? GoalSleepUpdated;
        public static event Action? GoalFoodUpdated;

    public static double CurrentWeight { get; private set; }
        public static double GoalWeight { get; private set; }
        public static double GoalWorkout { get; private set; }
        public static int GoalSteps { get; private set; }
        public static double GoalSleep { get; private set; }
        public static int GoalFoodCalories { get; private set; }

    public static void UpdateWeight(double newWeight)
        {
            CurrentWeight = newWeight;
            WeightUpdated?.Invoke();
        }

        public static void UpdateGoalWeight(double newGoal)
        {
            GoalWeight = newGoal;
            GoalWeightUpdated?.Invoke();
        }
        public static void UpdateGoalWorkout(double newGoal)
        {
            GoalWorkout = newGoal;
            GoalWorkoutUpdated?.Invoke();
        }
        public static void UpdateGoalSteps(int newGoal)
        {
            GoalSteps = newGoal;
            GoalStepsUpdated?.Invoke();
        }

        public static void UpdateGoalSleep(double newGoal)
        {
            GoalSleep = newGoal;     
            GoalSleepUpdated?.Invoke();
        }

        public static void UpdateGoalFood(int newGoal)
        {
            GoalFoodCalories = newGoal;
            GoalFoodUpdated?.Invoke();
        }
}


