# TemplateGenerator - 企业级文档全量自动化引擎

![Platform](https://img.shields.io/badge/Platform-Windows-blue) ![.NET](https://img.shields.io/badge/.NET-10.0-blue) ![License](https://img.shields.io/badge/License-MIT-green)

这是一个专为企业交付设计的文档自动化生成工具。它能够一键处理成百上千份 Word 和 PPT 交付文档，支持复杂的逻辑渲染与碎片化文本修复。

## 核心优势

- **PPTX 碎片化文字修复 (Healing)**：彻底解决 PPT 中由于格式化导致的 XML 标签切断文字块的问题，确保长字符串 100% 替换成功。
- **两阶段渲染引擎**：集成 Handlebars 逻辑渲染与全局字符串强制映射，新老模板全兼容。
- **深度递归扫描**：地毯式扫描所有 XML 组件，覆盖母版、布局页、备注与页眉页脚。
- **单文件分发 (Portable)**：一键发布为独立的 .exe，用户无需安装 .NET 运行时即可使用。

## 目录结构说明

- `src/`: 核心源代码。
- `Templates/`: **[关键]** 存放您的 .docx 和 .pptx 模板。
- `project.json`: 配置文件，定义变量 (Variables) 和替换规则 (Replace)。

## 快速上手

1. **准备环境**: 确保安装了 .NET 10 SDK。
2. **配置数据**: 修改 `project.json` 中的变量。
3. **运行编译**: 
   ```powershell
   dotnet run --project src/TemplateGenerator.Cli/TemplateGenerator.Cli.csproj

```

4. **单文件发布**:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

```



## 技术架构

该引擎不只是简单的字符串替换，它通过递归遍历 `OpenXmlPartContainer`，对每一个 XML 组件执行 `MergeDrawingRuns` 算法，在内存中“缝合”被切碎的文字，从而实现工业级的替换精度。

## 许可证

MIT License
