#!/bin/bash

echo "------------------------------------------------"
echo "🚀 准备提交 TemplateGenerator 到 GitHub"
echo "------------------------------------------------"

# 1. 确保 Templates 文件夹下有一个隐藏的 .gitkeep 文件
# 这样即便文件夹里没文件，GitHub 也会保留这个目录
mkdir -p Templates
touch Templates/.gitkeep

# 2. 初始化与添加
if [ ! -d ".git" ]; then
    git init
    git branch -M main
fi

# 3. 添加文件 (此时会根据 .gitignore 自动过滤掉 Templates 里的 docx/pptx)
git add .

# 4. 提交
echo "📝 正在创建提交..."
git commit -m "feat: 企业级 PPT 碎片缝合技术实现，支持自包含 EXE 发布与同级路径适配"

echo ""
echo "✅ 本地提交已完成！"
echo "提示：如果是第一次上传，请手动运行以下命令："
echo "git remote add origin <您的GitHub仓库地址>"
echo "git push -u origin main"