using System.Text.Json;

namespace LudereAI.Shared;

public static class JsonSerializerExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Converts an object to a JSON string with indented formatting and returns it.
    /// </summary>
    /// <param name="obj">Object</param>
    /// <returns>JSON String</returns>
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, SerializerOptions);
    }

    /// <summary>
    /// Converts a JSON string to an object and returns it.
    /// </summary>
    /// <param name="json">JSON String</param>
    /// <typeparam name="T">Object Type</typeparam>
    /// <returns>Object</returns>
    public static T? FromJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}