# 🎨 CAD AI 文字翻译插件

一款基于 OpenAI 大语言模型的智能 CAD 文字翻译工具，助您轻松实现图纸国际化！🚀


## ✨ 功能特性
- ✅ 智能识别「SOURCE_LAYER」图层中文内容
- 🌐 通过 OpenAI API 进行精准翻译
- 📍 自动生成「TARGET_LAYER」图层英文译文
- ⬇️ 译文智能定位在原文下方
- 💾 会话级翻译缓存减少 API 调用
- 🔒 支持 API 密钥安全配置
- ⚡ 快捷命令操作

## 🛠️ 系统要求
- AutoCAD 2016 或更高版本
- .NET Framework 4.7
- 有效的 OpenAI API 密钥

## 📥 安装指南

### 步骤 1 - 获取插件文件
1. 将插件安装文件下载至本地，以管理员身份安装

### 步骤 2 - 配置 API 参数
1. 用文本编辑器打开插件目录中的 `api.env` 文件
2. 修改配置参数：
   - `OPENAI_API_KEY` ➡️ 替换为您的有效 API 密钥
   - `OPENAI_BASE_URL` ➡️ 自定义接口地址（缺省:https://api.openai.com/v1/chat/completions）
   - `OPENAI_MODEL` ➡️ 指定 AI 模型（缺省:gpt-4）
   - `SYSTEMPROMPT` ➡️ 设置系统提示词，可在此设置翻译语言等（需加双引号""）
   - `SOURCE_LAYER` ➡️ 需要翻译的文字所在图层
   - `TARGET_LAYER` ➡️ 翻译后文字生成的图层

### 步骤 3 - 加载插件
1. 打开 AutoCAD 插件将自动加载

## 🚀 使用教程
1. **启动命令**  
   输入指令：`TRANSLATETEXT` 📥
2. **选择对象**  
   框选或点选「SOURCE_LAYER」图层的文字（支持多选）🖱️
3. **执行翻译**  
   按 Enter 键启动 AI 翻译 ▶️
4. **查看结果**  
   自动生成包含以下特性的英文文本：  
   📄 位于「TARGET_LAYER」图层 | ⬇️ 精准定位在原文下方 | 💾 当前会话缓存复用

## ⚠️ 注意事项
- 🔍 仅处理单行文字（TEXT 对象）
- 🌐 需确保网络通畅（访问 OpenAI API）
- 💡 缓存系统仅在 CAD 会话中有效
- ⚠️ 修改配置后需重新加载插件生效
- 📌 建议使用 GPT-4 等模型提升专业术语翻译质量

## 🌈 技术亮点
- ⚡ 多线程异步处理机制
- 🔐 本地会话缓存隔离
- 🎯 动态文本定位算法

## 🤝 参与贡献
欢迎提交 Issue 或 PR！

