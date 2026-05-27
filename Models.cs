using FitnessNutritionTracker;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;


public class IntOrStringConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (stringValue != null && int.TryParse(stringValue, out int result))
            {
                return result;
            }
            throw new JsonException($"Unable conver string '{stringValue}' to integer");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }

        throw new JsonException($"unexpet token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

public class GeminiObject
{
    [JsonPropertyName("candidates")]
    public required List<Candidate> Candidates { get; set; }

    [JsonPropertyName("modelVersion")]
    public string ModelVersion { get; set; }

    [JsonPropertyName("responseId")]
    public string ResponseId { get; set; }
}


public class Candidate
{
    [JsonPropertyName("content")]
    public required Content Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public required List<Part> Parts { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; }
}


public class Part
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}


public class MealNutrition
{
    [JsonPropertyName("meal_name")]
    public required string MealName { get; init; }

    [JsonPropertyName("protein")]
    public required int Protein { get; init; }


    [JsonPropertyName("carbs")]
    public required int Carbs { get; init; }


    [JsonPropertyName("fat")]
    public required int Fat { get; init; }


    [JsonPropertyName("fiber")]
    public required int Fiber { get; init; }


    [JsonPropertyName("calories")]
    public required int Calories { get; init; }
}


public enum ActivityType
{
    Food,
    Exercise,
    Sleep,
    Weight,
    WaterIntake
}


public enum MealType
{
    [Description("Breakfast")]
    Breakfast,

    [Description("Lunch")]
    Lunch,

    [Description("Dinner")]
    Dinner,

    [Description("Snack")]
    Snack
}


public enum LogType
{
    Food,
    Weight,
    Workout,
    Steps,
    Sleep,
}



public class ActivityLog
{
    public int Id { get; set; }
    public required LogType LogType { get; set; }
    public required string Detail { get; set; }

    [JsonIgnore]
    [NotMapped]
    public string? DetailDisplay { get; set; }
    public long Timestamp { get; set; }
}


public class FoodActivity
{
    [JsonPropertyName("MealType")]
    public required MealType MealType { get; set; }

    [JsonPropertyName("FoodName")]
    public required string FoodName { get; set; }

    [JsonPropertyName("Calories")]
    [JsonConverter(typeof(IntOrStringConverter))]
    public required int Calories { get; set; }

    [JsonPropertyName("Protein")]
    [JsonConverter(typeof(IntOrStringConverter))]
    public required int Protein { get; set; }

    [JsonPropertyName("Carbs")]
    [JsonConverter(typeof(IntOrStringConverter))]
    public required int Carbs { get; set; }

    [JsonPropertyName("Fat")]
    [JsonConverter(typeof(IntOrStringConverter))]
    public required int Fat { get; set; }

    [JsonPropertyName("Fiber")]
    [JsonConverter(typeof(IntOrStringConverter))]
    public required int Fiber { get; set; }
}

public class WeightActivity
{
    [JsonPropertyName("Weight")]
    public required decimal Weight { get; set; }

    [JsonPropertyName("Change")]
    public required decimal Change { get; set; }
}


public enum WorkoutType
{
    Running,
    Bicycling,
    Swimming
}

public class WorkoutActivity
{
    [JsonPropertyName("WorkoutType")]
    public required WorkoutType WorkoutType { get; set; }
    [JsonPropertyName("Calories")]
    public required int Calories { get; set; }
}



public class StepsActivity
{
    [JsonPropertyName("Steps")]
    public required int Steps { get; set; }
}


public class SleepActivity
{
    [JsonPropertyName("Hours")]
    public required int Hours { get; set; }
}