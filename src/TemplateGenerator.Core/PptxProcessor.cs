using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;

namespace TemplateGenerator.Core
{
    public class PptxProcessor
    {
        private readonly VariableEngine _engine;

        public PptxProcessor(VariableEngine engine) => _engine = engine;

        public void Process(string filePath, ProjectConfig config)
        {
            using (PresentationDocument doc = PresentationDocument.Open(filePath, true))
            {
                // 创建一个已访问列表，防止递归死循环
                var visitedParts = new HashSet<string>();

                // 从 PresentationPart 开始进行全量递归扫描
                if (doc.PresentationPart != null)
                {
                    RecursiveProcessParts(doc.PresentationPart, config, visitedParts);
                    
                    // 最后保存全局演示文稿设置
                    doc.PresentationPart.Presentation?.Save();
                }
            }
        }

        /// <summary>
        /// 全量递归扫描器：遍历 PPT 内所有的 XML 组件（Slide, Layout, Master, Notes, etc.）
        /// </summary>
        private void RecursiveProcessParts(OpenXmlPartContainer container, ProjectConfig config, HashSet<string> visitedParts)
        {
            foreach (var partId in container.Parts)
            {
                var part = partId.OpenXmlPart;
                
                // 过滤已处理过的 Part 和非 XML 资源（如图片、视频、主题等）
                if (visitedParts.Contains(part.Uri.ToString())) continue;
                visitedParts.Add(part.Uri.ToString());

                // 1. 处理该 Part 内的文字逻辑
                ProcessSinglePart(part, config);

                // 2. 递归处理该 Part 包含的子 Part
                RecursiveProcessParts(part, config, visitedParts);
            }
        }

        private void ProcessSinglePart(OpenXmlPart part, ProjectConfig config)
        {
            try
            {
                // 尝试获取根元素，如果不是 XML 格式则直接跳过
                var root = part.RootElement;
                if (root == null) return;

                // 第一步：合并被拆碎的文字块 (Run Merging)
                // 解决 "广州日报..." 在任何容器（包括表格、组合、占位符）中被拆碎的问题
                var paragraphs = root.Descendants<A.Paragraph>().ToList();
                foreach (var p in paragraphs)
                {
                    MergeDrawingRuns(p);
                }

                // 第二步：执行双阶段替换
                var textNodes = root.Descendants<A.Text>().ToList();
                foreach (var t in textNodes)
                {
                    if (t != null && !string.IsNullOrEmpty(t.Text))
                    {
                        t.Text = _engine.Render(t.Text, config);
                    }
                }
            }
            catch
            {
                // 忽略非 XML Part 导致的读取异常
            }
        }

        private void MergeDrawingRuns(A.Paragraph p)
        {
            var runs = p.Elements<A.Run>().ToList();
            if (runs.Count <= 1) return;

            for (int i = 0; i < runs.Count - 1; i++)
            {
                var currentRun = runs[i];
                var nextRun = runs[i + 1];

                var currentText = currentRun.GetFirstChild<A.Text>();
                var nextText = nextRun.GetFirstChild<A.Text>();

                if (currentText != null && nextText != null)
                {
                    // 只要有文字就合并，不考虑格式差异，以替换成功为第一目标
                    currentText.Text += nextText.Text;
                    nextRun.Remove();
                    runs.RemoveAt(i + 1);
                    i--;
                }
            }
        }
    }
}