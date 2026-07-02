using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;

namespace TemplateGenerator.Core
{
    public class DocxProcessor
    {
        private readonly VariableEngine _engine;

        public DocxProcessor(VariableEngine engine) => _engine = engine;

        public void Process(string filePath, ProjectConfig config)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true))
            {
                var mainPart = doc.MainDocumentPart;
                if (mainPart == null || mainPart.Document == null) return;

                // 1. 处理正文
                if (mainPart.Document.Body != null)
                    CleanAndReplace(mainPart.Document.Body, config);

                // 2. 处理页眉
                foreach (var headerPart in mainPart.HeaderParts)
                {
                    if (headerPart.Header != null)
                        CleanAndReplace(headerPart.Header, config);
                }

                // 3. 处理页脚
                foreach (var footerPart in mainPart.FooterParts)
                {
                    if (footerPart.Footer != null)
                        CleanAndReplace(footerPart.Footer, config);
                }

                // 4. 处理脚注
                if (mainPart.FootnotesPart != null && mainPart.FootnotesPart.Footnotes != null)
                    CleanAndReplace(mainPart.FootnotesPart.Footnotes, config);

                mainPart.Document.Save();
            }
        }

        private void CleanAndReplace(OpenXmlElement rootElement, ProjectConfig config)
        {
            if (rootElement == null) return;

            var paragraphs = rootElement.Descendants<Paragraph>().ToList();
            foreach (var p in paragraphs)
            {
                MergeParagraphRuns(p);
                
                var textNodes = p.Descendants<Text>().ToList();
                foreach (var textNode in textNodes)
                {
                    if (textNode.Text != null)
                    {
                        textNode.Text = _engine.Render(textNode.Text, config);
                    }
                }
            }
        }

        private void MergeParagraphRuns(Paragraph p)
        {
            var runs = p.Elements<Run>().ToList();
            if (runs.Count <= 1) return;

            for (int i = 0; i < runs.Count - 1; i++)
            {
                var currentRun = runs[i];
                var nextRun = runs[i + 1];

                var currentText = currentRun.GetFirstChild<Text>();
                var nextText = nextRun.GetFirstChild<Text>();

                if (currentText != null && nextText != null)
                {
                    currentText.Text += nextText.Text;
                    nextRun.Remove();
                    runs.RemoveAt(i + 1);
                    i--; 
                }
            }
        }
    }
}