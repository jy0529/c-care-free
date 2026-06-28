<div align="center">

# C盘无忧

**让 C 盘重新呼吸的 Windows 清理工具**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white)](https://github.com/jy0529/c-care-free)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Release](https://img.shields.io/github/v/release/jy0529/c-care-free?label=Release)](https://github.com/jy0529/c-care-free/releases)

一键看清 C 盘空间 · 安全清理垃圾缓存 · 智能发现大文件并给出搬家建议

[快速开始](#-快速开始) · [下载安装](#-下载安装) · [功能亮点](#-功能亮点) · [文档](#-文档)

</div>

---

## 为什么选择 C盘无忧？

C 盘红了，往往不是缺空间，而是**不知道空间被谁占走了**。

C盘无忧 帮你做三件事：

| | 能力 | 你能得到什么 |
|---|------|-------------|
| 📊 | **即时磁盘概览** | 打开即显示 C 盘总容量、已用、可用，不用等扫描 |
| 🧹 | **白名单垃圾清理** | 系统临时文件、浏览器缓存、更新残留等分类展示，勾选即清 |
| 📦 | **大文件智能搬家** | 多线程扫描 Top 大文件，Docker/WSL/休眠文件等自动给出搬家建议 |

> 不扫全盘乱删，不碰未知路径。所有清理项来自可配置白名单，中高风险默认不勾选。

---

## ✨ 功能亮点

### 垃圾清理
- 启动即读 C 盘空间，顶栏实时展示
- 20+ 清理分类：临时文件、缩略图、Shader 缓存、浏览器缓存、Windows 更新缓存、回收站等
- 开发环境缓存（npm / Maven / NuGet 等）可选清理，默认关闭
- 按风险分级，清理前二次确认

### 大文件搬家
- 多线程全 C 盘扫描，内存友好的 **Top-N 堆** 算法
- 扫描过程中**实时预览**列表与搬家建议
- 识别 Docker `.vhdx`、WSL 磁盘、休眠文件、虚拟机等特殊大文件
- 支持打开文件位置、搬移到指定目录

### 工程化体验
- 原生 WPF 界面，浅色 Dashboard 设计
- 分层架构（Domain / Application / Infrastructure / App）
- 单元 + 集成 + UI 冒烟测试，GitHub Actions CI

---

## 🖥 界面预览

```
┌─────────────────────────────────────────────────────────┐
│  [Logo]  C盘无忧          C: 总共 200 GB · 可用 15 GB   │
├──────┬──────────────────────────────────────────────────┤
│ 垃圾 │  可释放垃圾  12.3 GB    [扫描垃圾项]             │
│ 清理 │  ┌────────────────────────────────────────────┐ │
│      │  │ ☑ 用户临时文件          2.1 GB              │ │
│ 大文 │  │ ☑ 浏览器缓存            1.8 GB              │ │
│ 件   │  │ ☐ Maven 本地仓库        4.2 GB  (中风险)    │ │
│ 搬家 │  └────────────────────────────────────────────┘ │
│      │  已选 3.9 GB              [立即清理]             │
└──────┴──────────────────────────────────────────────────┘
```

---

## 📥 下载安装

前往 [Releases](https://github.com/jy0529/c-care-free/releases) 下载最新版：

| 文件 | 说明 |
|------|------|
| `CCareFree-v1.0.0-win-x64.zip` | Windows 64 位，解压即用，**无需安装 .NET** |

使用步骤：
1. 下载并解压 zip
2. 双击 `CCareFree.exe`
3. 左侧选择功能，点击扫描

---

## 🚀 快速开始（开发者）

### 环境要求

- Windows 10 / 11（x64）
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 从源码运行

```powershell
git clone git@github.com:jy0529/c-care-free.git
cd c-care-free

# 一键运行
.\run.ps1

# 或手动
dotnet restore CDriveCleanupMaster.sln
dotnet run --project src/CDriveCleanupMaster.App/CDriveCleanupMaster.App.csproj
```

### 发布 exe

```powershell
.\publish.ps1
```

输出：`dist/CCareFree-v1.0.0-win-x64.zip`（含 `CCareFree.exe` 与清理规则配置）

### 构建与测试

```powershell
.\build.ps1

# 等价于
dotnet restore CDriveCleanupMaster.sln
dotnet build CDriveCleanupMaster.sln -c Release
dotnet test CDriveCleanupMaster.sln -c Release
```

---

## 📁 项目结构

```
src/
  CDriveCleanupMaster.App/            # WPF 界面层
  CDriveCleanupMaster.Application/    # 用例编排
  CDriveCleanupMaster.Domain/         # 领域模型与规则
  CDriveCleanupMaster.Infrastructure/ # 文件系统、日志、Shell
tests/                                # 单元 / 集成 / UI 测试
docs/                                 # 产品、架构、规则文档
scripts/                              # 图标生成等工具脚本
```

---

## ⚠️ 使用须知

- 清理系统目录（如 `C:\Windows\Temp`）可能需要**管理员权限**
- Docker / WSL 等大文件请优先按**搬家建议**在对应软件设置中迁移，而非直接删除
- Maven / NuGet 等开发缓存清理后需重新下载依赖

---

## 📚 文档

- [产品说明](docs/prd-v1.md)
- [架构设计](docs/architecture.md)
- [清理规则清单](docs/cleanup-rules.md)
- [测试策略](docs/testing-strategy.md)

---

## 🤝 参与贡献

欢迎 Issue 和 Pull Request。提交前请确保：

```powershell
dotnet test CDriveCleanupMaster.sln -c Release
```

---

## 📄 许可证

本项目基于 [MIT License](LICENSE) 开源。

---

<div align="center">

**C盘无忧** — 空间焦虑，一扫而空。

Made with ❤️ for Windows users

</div>
