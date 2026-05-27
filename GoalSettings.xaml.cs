using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FitnessNutritionTracker.ViewModel;
using FitnessNutritionTracker.Repository;

namespace FitnessNutritionTracker
{
    public partial class GoalSettings : UserControl
    {
        public GoalSettings()
        {
            InitializeComponent();
            this.DataContext = GoalSettingsViewModel.Instance;
            this.Loaded += GoalSettings_Loaded;
        }


        private void GoalSettings_Loaded(object sender, RoutedEventArgs e)
        {
            GoalSettingsViewModel.Instance.RefreshGoals();
        }

        private void GoalEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Goal goal)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Enter new target for '{goal.Name}' (current target: {goal.Target} {goal.Unit})",
                    "Edit Target",
                    goal.Target.ToString());

                if (double.TryParse(input, out double newTarget))
                {
                    goal.Target = newTarget;

                    MessageBox.Show($"Saving Goal for '{goal.Name}'...");

                    // Update DB
                    GoalSettingsViewModel.Instance.SaveGoalsToDatabase();                 

                    if (goal.Name.Equals("Weight", StringComparison.OrdinalIgnoreCase))
                    { 
                        goal.Target = newTarget;
                        GoalSettingsViewModel.Instance.SaveGoalsToDatabase();
                        DashboardSharedData.UpdateGoalWeight(newTarget);                      
                    }

                    if (goal.Name.Equals("Steps", StringComparison.OrdinalIgnoreCase))
                    {
                        goal.Target = newTarget;
                        GoalSettingsViewModel.Instance.SaveGoalsToDatabase();
                        DashboardSharedData.UpdateGoalSteps((int)newTarget);  
                    }

                    if (goal.Name.Equals("Sleep", StringComparison.OrdinalIgnoreCase))
                    {
                        goal.Target = newTarget;
                        GoalSettingsViewModel.Instance.SaveGoalsToDatabase();
                        DashboardSharedData.UpdateGoalSleep(newTarget);
                    }

                    if (goal.Name.Equals("Calories", StringComparison.OrdinalIgnoreCase))
                    {
                        goal.Target = newTarget;
                        GoalSettingsViewModel.Instance.SaveGoalsToDatabase();
                        DashboardSharedData.UpdateGoalFood((int)newTarget);  // added int here, Omkar
                    }

                    if (goal.Name.Equals("Workout", StringComparison.OrdinalIgnoreCase))
                    {
                        goal.Target = newTarget;
                        GoalSettingsViewModel.Instance.SaveGoalsToDatabase();
                        DashboardSharedData.UpdateGoalWorkout(newTarget);
                    }

                    MessageBox.Show($"Goal for '{goal.Name}' updated successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);    
                }
                else if (!string.IsNullOrWhiteSpace(input))
                {
                    MessageBox.Show("Invalid input. Please enter a numeric value.", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    public class Goal : INotifyPropertyChanged
    {
        private string _name;
        private double _start;
        private double _current;
        private double _target;
        private string _unit;
        private GoalType? _type;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(ShowWeightFields)); // notify UI if Name changes
            }
        }

        public double Start
        {
            get => _start;
            set { _start = value; OnPropertyChanged(nameof(Start)); }
        }

        public double Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(nameof(Current)); }
        }

        public double Target
        {
            get => _target;
            set { _target = value; OnPropertyChanged(nameof(Target)); }
        }

        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(nameof(Unit)); }
        }

        public GoalType? Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public bool ShowWeightFields => Name?.Equals("Weight", StringComparison.OrdinalIgnoreCase) == true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum GoalType
    {
        Gain,
        Lose
    }

}
