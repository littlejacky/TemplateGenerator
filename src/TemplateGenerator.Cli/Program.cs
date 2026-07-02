using System;
using System.IO;
using TemplateGenerator.Core;

namespace TemplateGenerator.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 默认路径：.exe 同级目录
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templateDir = Path.Combine(baseDir, "Templates");
            string configFile = Path.Combine(baseDir, "project.json");
            string outputDir = Path.Combine(baseDir, "publish_output");

            // 2. 自适应自愈逻辑：如果在当前运行目录找不到，尝试向上逐级查找（最多5层）
            // 完美兼容开发阶段的 dotnet run (此时文件在项目根目录，而 BaseDirectory 在 bin/Debug 下)
            if (!Directory.Exists(templateDir) || !File.Exists(configFile))
            {
                string currentCheckDir = baseDir;
                for (int i = 0; i < 5; i++)
                {
                    currentCheckDir = Path.GetDirectoryName(currentCheckDir);
                    if (string.IsNullOrEmpty(currentCheckDir)) break;

                    string candidateTemplateDir = Path.Combine(currentCheckDir, "Templates");
                    string candidateConfigFile = Path.Combine(currentCheckDir, "project.json");

                    if (Directory.Exists(candidateTemplateDir) && File.Exists(candidateConfigFile))
                    {
                        templateDir = candidateTemplateDir;
                        configFile = candidateConfigFile;
                        outputDir = Path.Combine(currentCheckDir, "publish_output");
                        break; // 找到了开发环境的项目根目录，直接退出循环
                    }
                }
            }

            // 初始化所有引擎
            var engine = new VariableEngine();
            var docxProcessor = new DocxProcessor(engine);
            var pptxProcessor = new PptxProcessor(engine);
            var pipeline = new DirectoryProcessor(engine, docxProcessor, pptxProcessor); 

            try
            {
                // 检查必要的文件和文件夹是否存在
                if (!Directory.Exists(templateDir))
                {
                    throw new Exception($"找不到模板目录！当前搜索路径: {templateDir}");
                }
                if (!File.Exists(configFile))
                {
                    throw new Exception($"找不到配置文件！当前搜索路径: {configFile}");
                }

                pipeline.Process(templateDir, outputDir, configFile);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[成功] 运行完毕！文档已生成至: {outputDir}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"运行失败: {ex.Message}");
                Console.ResetColor();
            }

            // 3. 修复 Git Bash / MINGW64 环境下 dotnet run 报 ReadKey 异常的问题
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("\n按任意键退出...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("\n运行结束。");
            }
        }
    }
}