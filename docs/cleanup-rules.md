# 清理规则清单

## 默认勾选（低风险）

- 用户临时文件（%TEMP%）
- 系统临时文件（Windows\Temp）

## 系统安全清理（默认不勾选）

- Windows CBS / DISM 日志
- 崩溃转储（Minidump / MEMORY.DMP）
- 缩略图缓存
- 回收站（通过 Windows API 清空）

## 可选清理（默认不勾选）

- DirectX / NVIDIA / AMD Shader 缓存
- Chrome / Edge 浏览器缓存
- Windows 更新下载缓存（中风险）

## 开发环境（默认不勾选，防误删）

- npm / Yarn / pnpm
- Go 构建缓存
- Maven 本地仓库（中风险）
- Gradle caches
- Cargo 缓存
- Composer / pip / .NET NuGet（NuGet 为中风险）

## 仅展示

- 下载目录

## 大文件

- 全盘多线程扫描
- 可配置最小大小与显示数量上限
- 对 Docker/WSL/虚拟机/Android 等路径自动标注搬家建议
