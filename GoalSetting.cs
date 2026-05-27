using System.ComponentModel.DataAnnotations;

public class GoalSetting
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty; // Goal name (Weight, Calories, etc.)
    public double Target { get; set; }               // Target value
    public string Unit { get; set; } = string.Empty; // Unit (kg, kcal, hrs, steps, min)
    public double Start { get; set; }                // Starting value
    public double Current { get; set; }              // Current value

    public DateTime Timestamp { get; set; } = DateTime.Now;

}
