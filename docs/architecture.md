# 架构说明

## 分层原则

- `App` 只负责界面、状态与交互绑定
- `Application` 负责用例编排与服务接口
- `Domain` 负责核心模型、规则和业务约束
- `Infrastructure` 负责文件系统、日志、Shell 与清理执行

## 数据流

1. UI 触发扫描命令
2. ViewModel 调用 Application 用例
3. Application 调用扫描接口与规则提供器
4. Infrastructure 返回扫描结果
5. ViewModel 生成卡片和列表展示
6. 用户确认后通过 Application 调用清理/搬家服务
7. Infrastructure 执行文件系统操作并返回结果

## 关键接口

- `ICleanupRuleProvider`
- `ICleanupScanner`
- `ICleanupExecutor`
- `ILargeFileScanner`
- `IFileMover`
- `IShellService`
- `IAppLogger`

## 关键领域组件

- `CleanupWorkspaceService` / `LargeFileWorkspaceService`：垃圾清理与大文件两个独立用例
- `LargeFileTopNCollector`：大文件扫描过程中的固定容量 Top-N 收集，避免无界内存增长
- `LargeFileRelocationMatcher`：大文件搬家建议匹配
- `RiskSelectionPolicy`：默认勾选策略

## 设计约束

- UI 不直接做文件系统调用
- 规则定义配置化
- 清理逻辑采用白名单
- 错误必须记录日志
