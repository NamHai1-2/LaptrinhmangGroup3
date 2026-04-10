using System;
using System.Text.Json;

namespace _1_SharedLibrary.Utils
{
    public static class JsonParser
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, 
            WriteIndented = false 
        };


        public static string Serialize<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI JSON] Không thể đóng gói: {ex.Message}");
                return string.Empty;
            }
        }


        public static T Deserialize<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return default;
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI JSON] Không thể giải mã: {ex.Message}");
                return default;
            }
        }
    }
}