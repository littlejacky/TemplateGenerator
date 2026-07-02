using System;
using System.IO;

namespace TemplateGenerator.Core
{
    public class DirectoryProcessor
    {
        private readonly VariableEngine _engine;
        private readonly DocxProcessor _docxProcessor;
        private readonly PptxProcessor _pptxProcessor;

        // 显式定义接收 3 个参数的构造函数
        public DirectoryProcessor(VariableEngine engine, DocxProcessor docxProcessor, PptxProcessor pptxProcessor)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _docxProcessor = docxProcessor ?? throw new ArgumentNullException(nameof(docxProcessor));
            _pptxProcessor = pptxProcessor ?? throw new ArgumentNullException(nameof(pptxProcessor));
        }

        public void Process(string templateDir, string outputDir, string configFile)
        {
            if (!Directory.Exists(templateDir)) throw new DirectoryNotFoundException($"模板目录不存在: {templateDir}");
            if (!File.Exists(configFile)) throw new FileNotFoundException($"配置文件不存在: {configFile}");

            string json = File.ReadAllText(configFile);
            var config = ProjectConfig.LoadFromJson(json);

            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            Directory.CreateDirectory(outputDir);

            Console.WriteLine("======= 开始生成项目文档 =======");

            // 1. 递归创建文件夹结构
            foreach (string dirPath in Directory.GetDirectories(templateDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(templateDir, dirPath);
                string renderedRelativePath = _engine.Render(relativePath, config);
                string targetDir = Path.Combine(outputDir, renderedRelativePath);
                Directory.CreateDirectory(targetDir);
            }

            // 2. 复制并处理文件内容
            foreach (string filePath in Directory.GetFiles(templateDir, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(templateDir, filePath);
                string renderedRelativePath = _engine.Render(relativePath, config);
                string targetFilePath = Path.Combine(outputDir, renderedRelativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
                File.Copy(filePath, targetFilePath, true);
                Console.WriteLine($"[√ 文件名] {Path.GetFileName(targetFilePath)}");

                string extension = Path.GetExtension(targetFilePath).ToLower();
                if (extension == ".docx")
                {
                    _docxProcessor.Process(targetFilePath, config);
                    Console.WriteLine($"  └─ [√ docx 内容替换完成]");
                }
                else if (extension == ".pptx")
                {
                    _pptxProcessor.Process(targetFilePath, config);
                    Console.WriteLine($"  └─ [√ pptx 内容替换完成]");
                }
                else if (extension == ".xlsx")
                {
                    Console.WriteLine($"  └─ [提示] XLSX 处理器待接入...");
                }
            }

            Console.WriteLine("======= 文档全量一键生成成功！=======");
        }
    }
}