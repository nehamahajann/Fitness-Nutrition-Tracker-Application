using System.Windows;

namespace FitnessNutritionTracker
{
    public partial class EditGoalWindow : Window
    {
        private Goal goal;

        public EditGoalWindow(Goal selectedGoal)
        {
            InitializeComponent();
            goal = selectedGoal;
            ValueTextBox.Text = goal.Current.ToString();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, out double newValue))
            {
                goal.Current = newValue;
                this.Close();
            }
            else
            {
                MessageBox.Show("Enter a valid number.");
            }
        }
    }
}

