using System;
using System.IO;
using TemplateGenerator.Core;

namespace TemplateGenerator.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            // 获取当前 .exe 所在的文件夹路径
            // 这种方式无论在开发环境还是用户电脑上，都能精准定位到程序运行的目录
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 设置路径：Templates、project.json 和输出目录都在 .exe 的同级目录下
            string templateDir = Path.Combine(baseDir, "Templates");
            string outputDir = Path.Combine(baseDir, "publish_output");
            string configFile = Path.Combine(baseDir, "project.json");

            // 初始化所有引擎
            var engine = new VariableEngine();
            var docxProcessor = new DocxProcessor(engine);
            var pptxProcessor = new PptxProcessor(engine);

            // 传入处理器
            var pipeline = new DirectoryProcessor(engine, docxProcessor, pptxProcessor); 

            try
            {
                // 检查必要的文件和文件夹是否存在
                if (!Directory.Exists(templateDir))
                {
                    throw new Exception($"找不到模板目录！请确保 .exe 同级目录下存在 'Templates' 文件夹。当前搜索路径: {templateDir}");
                }
                if (!File.Exists(configFile))
                {
                    throw new Exception($"找不到配置文件！请确保 .exe 同级目录下存在 'project.json'。当前搜索路径: {configFile}");
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

            // 如果是双击运行，防止窗口直接关闭，方便用户查看结果
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}