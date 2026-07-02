using System.Text.Json;
using System.Text.Json.Serialization;

namespace TemplateGenerator.Core
{
    public class ProjectConfig
    {
        [JsonPropertyName("Variables")]
        public Dictionary<string, object> Variables { get; set; } = new();

        [JsonPropertyName("Replace")]
        public Dictionary<string, string> Replace { get; set; } = new();

        /// <summary>
        /// 兼容老格式：如果根目录直接写了变量，自动适配到 Variables 中
        /// </summary>
        public static ProjectConfig LoadFromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var config = new ProjectConfig();

            // 如果包含明确的 "Variables" 节点，按新格式解析
            if (doc.RootElement.TryGetProperty("Variables", out _))
            {
                config = JsonSerializer.Deserialize<ProjectConfig>(json) ?? new ProjectConfig();
            }
            else
            {
                // 老项目直接扁平化写变量的兼容处理
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                var rawDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                if (rawDict != null) // 修复：将之前的 nonempty 改回了真实的 null 校验
                {
                    if (rawDict.TryGetValue("Replace", out var replaceObj) && replaceObj != null)
                    {
                        config.Replace = JsonSerializer.Deserialize<Dictionary<string, string>>(replaceObj.ToString()!) ?? new();
                        rawDict.Remove("Replace");
                    }
                    config.Variables = rawDict;
                }
            }
            return config;
        }
    }
}