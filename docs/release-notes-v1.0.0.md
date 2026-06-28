# C盘无忧 v1.0.0

首个正式版本，提供 **Windows x64 免安装单文件可执行程序**。

## 下载

| 文件 | 说明 |
|------|------|
| `CCareFree-v1.0.0-win-x64.zip` | 解压后运行 `CCareFree.exe`（内含清理规则配置） |

> 自包含发布，**无需单独安装 .NET 运行时**。

## 亮点

- 启动即显示 C 盘空间概览
- 垃圾清理：白名单规则、风险分级、分类勾选清理
- 大文件搬家：多线程 Top-N 扫描、实时预览、智能搬家建议
- 原生 WPF 界面，浅色 Dashboard 设计

## 系统要求

- Windows 10 / 11（64 位）

## 使用方式

1. 下载并解压 `CCareFree-v1.0.0-win-x64.zip`
2. 双击 `CCareFree.exe` 运行
3. 左侧选择「垃圾清理」或「大文件搬家」，点击扫描

## 从源码构建

```powershell
git clone git@github.com:jy0529/c-care-free.git
cd c-care-free
.\publish.ps1
```

输出位于 `dist/CCareFree-v1.0.0-win-x64.zip`。
