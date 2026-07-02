using HandlebarsDotNet;

namespace TemplateGenerator.Core
{
    public class VariableEngine
    {
        private readonly IHandlebars _handlebars;

        public VariableEngine()
        {
            // ================= 最新资讯官方解决方案 =================
            // 查阅最新文档，直接在初始化时传入底层配置，将没有编码器的策略注入，彻底关闭 HTML 转义
            var configuration = new HandlebarsConfiguration
            {
                TextEncoder = null // 官方规定：TextEncoder 赋予 null 即代表完全不进行任何 HTML 转义！
            };
            
            _handlebars = Handlebars.Create(configuration);
            // ========================================================
        }

        public string Render(string inputText, ProjectConfig config)
        {
            if (string.IsNullOrEmpty(inputText) || config == null) return inputText;

            // 第一遍：Handlebars 变量与逻辑（if/each）替换
            string stage1Result = inputText;
            try
            {
                var template = _handlebars.Compile(inputText);
                stage1Result = template(config.Variables);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[警告] Handlebars 渲染失败，跳过第一阶段。错误: {ex.Message}");
            }

            // 第二遍：老项目硬编码字符串强制 Replace 映射
            string stage2Result = stage1Result;
            if (config.Replace != null && config.Replace.Count > 0)
            {
                foreach (var kvp in config.Replace)
                {
                    if (!string.IsNullOrEmpty(kvp.Key))
                    {
                        stage2Result = stage2Result.Replace(kvp.Key, kvp.Value);
                    }
                }
            }

            return stage2Result;
        }
    }
}