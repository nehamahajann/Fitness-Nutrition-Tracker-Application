using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;

class GeminiApiClient
{
    private readonly string apiKey = "AIzaSyBXuYQvtE9O8afYHjlH-obC59p_rUjfzwU";
    private const string requestUri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";


    public async Task<string> RequestGemini(string imagePath)
    {
        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
        string base64Image = Convert.ToBase64String(imageBytes);


        var promptObject = new JsonObject
        {
            ["contents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject { ["text"] = "Please analyze the food in the image and return a JSON object with its name, and estimated values for protein, carbohydrates, fat, dietary fiber, and calories. Please ensure the returned JSON is in the following format, No Markdown!:\"{\"meal_name\":\"\",\"protein\":0,\"carbs\": 0,\"fat\": 0,\"fiber\": 0,\"calories\": 0}" },
                            new JsonObject
                            {
                                ["inlineData"] = new JsonObject
                                {
                                    ["mimeType"] = "image/jpeg",
                                    ["data"] = base64Image
                                }
                            }
                        }
                    }
                }
        };

        string jsonContent = promptObject.ToJsonString();

        Console.WriteLine("Request JSON:" + jsonContent);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }


    /// <summary>
    /// Sends a food description as text to Gemini API and returns nutrition info.
    /// </summary>
    public async Task<string> RequestTextResp(string prompt)
    {
        var promptObject = new JsonObject
        {
            ["contents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject { ["text"] = prompt },
                        }
                    }
                }
        };

        string jsonContent = promptObject.ToJsonString();

        Console.WriteLine("Request JSON:" + jsonContent);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
