# 服务数据源策略总结

## 概述

为DroneService和TaskService添加了智能数据源切换功能，根据不同的操作类型自动选择最优的数据源策略，以提高系统性能和保证数据一致性。

## 数据源策略分类

### 1. 数据库优先策略 (DatabaseFirst)
**使用场景**：写操作、更新操作、删除操作
**目的**：确保数据一致性
**适用方法**：
- 添加无人机/任务
- 更新无人机/任务
- 删除无人机/任务
- 状态更新
- 位置更新
- 指标更新
- 任务分配/取消分配

### 2. 缓存优先策略 (CacheFirst)
**使用场景**：读操作、列表查询、统计查询
**目的**：提高查询性能
**适用方法**：
- 获取所有无人机/任务列表
- 根据名称查询
- 获取数量统计
- 获取在线/离线无人机
- 获取可用无人机
- 获取最新数据点
- 获取分配的子任务
- 获取未分配的子任务
- 根据状态查询子任务
- 任务统计查询

### 3. 混合策略 (Hybrid)
**使用场景**：单个查询、历史数据查询
**目的**：平衡性能和一致性
**适用方法**：
- 根据ID获取单个无人机/任务
- 获取历史数据点
- 分页查询

## DroneService 数据源策略

### 基础CRUD操作
```csharp
// 列表查询 - 缓存优先
GetDronesAsync() → CacheFirst
GetDroneByNameAsync() → CacheFirst

// 单个查询 - 混合策略
GetDroneAsync() → Hybrid

// 写操作 - 数据库优先
AddDroneAsync() → DatabaseFirst
UpdateDroneAsync() → DatabaseFirst
DeleteDroneAsync() → DatabaseFirst
```

### 状态管理
```csharp
// 状态更新 - 数据库优先
UpdateDroneStatusAsync() → DatabaseFirst
UpdateDronePositionAsync() → DatabaseFirst
UpdateDroneMetricsAsync() → DatabaseFirst

// 状态查询 - 缓存优先
GetOfflineDronesAsync() → CacheFirst
GetOnlineDronesAsync() → CacheFirst
GetAvailableDronesAsync() → CacheFirst
```

### 数据点管理
```csharp
// 数据点写入 - 数据库优先
AddDroneDataPointAsync() → DatabaseFirst

// 最新数据点查询 - 缓存优先
GetLatestDroneDataPointAsync() → CacheFirst
GetAllDronesLatestDataPointsAsync() → CacheFirst

// 历史数据查询 - 混合策略
GetDroneDataPointsAsync() → Hybrid
GetDroneDataPointsAsync(pageIndex, pageSize) → Hybrid

// 统计查询 - 缓存优先
GetDroneDataPointsCountAsync() → CacheFirst
```

## TaskService 数据源策略

### 主任务管理
```csharp
// 列表查询 - 缓存优先
GetMainTasksAsync() → CacheFirst

// 单个查询 - 混合策略
GetMainTaskAsync() → Hybrid

// 写操作 - 数据库优先
AddMainTaskAsync() → DatabaseFirst
UpdateMainTaskAsync() → DatabaseFirst
DeleteMainTaskAsync() → DatabaseFirst

// 统计查询 - 缓存优先
GetMainTaskCountAsync() → CacheFirst
```

### 子任务管理
```csharp
// 列表查询 - 缓存优先
GetSubTasksAsync() → CacheFirst

// 单个查询 - 混合策略
GetSubTaskAsync() → Hybrid

// 写操作 - 数据库优先
AddSubTaskAsync() → DatabaseFirst
UpdateSubTaskAsync() → DatabaseFirst
DeleteSubTaskAsync() → DatabaseFirst

// 统计查询 - 缓存优先
GetSubTaskCountAsync() → CacheFirst
```

### 任务分配管理
```csharp
// 分配操作 - 数据库优先
AssignSubTaskToDroneAsync() → DatabaseFirst
UnassignSubTaskFromDroneAsync() → DatabaseFirst

// 查询操作 - 缓存优先
GetAssignedSubTasksAsync() → CacheFirst
GetUnassignedSubTasksAsync() → CacheFirst
GetSubTasksByStatusAsync() → CacheFirst
```

### 任务统计
```csharp
// 统计查询 - 缓存优先
GetMainTaskStatusStatisticsAsync() → CacheFirst
GetSubTaskStatusStatisticsAsync() → CacheFirst
GetMainTaskCompletionRateAsync() → CacheFirst
GetMainTaskDurationAsync() → CacheFirst
```

## 性能优化效果

### 1. 查询性能提升
- **列表查询**：使用缓存优先策略，减少数据库访问
- **统计查询**：缓存结果，避免重复计算
- **最新数据**：优先从缓存获取，响应更快

### 2. 数据一致性保证
- **写操作**：使用数据库优先策略，确保数据持久化
- **更新操作**：直接写入数据库，避免缓存不一致
- **删除操作**：数据库优先，确保数据完整性

### 3. 智能平衡
- **单个查询**：使用混合策略，在性能和一致性间平衡
- **历史数据**：混合策略，根据数据特点选择最优方案

## 配置灵活性

### 1. 运行时调整
```csharp
// 可以根据业务需求动态调整策略
await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);
```

### 2. 监控和日志
- 每个操作都会记录使用的数据源策略
- 可以通过日志分析性能表现
- 支持健康状态监控

### 3. 故障处理
- 缓存失效时自动降级到数据库
- 数据库故障时可以使用缓存模式
- 支持手动刷新缓存

## 最佳实践

### 1. 操作分类
- **读操作**：优先使用缓存
- **写操作**：优先使用数据库
- **统计操作**：使用缓存提高性能
- **实时数据**：根据时效性选择策略

### 2. 监控指标
- 缓存命中率
- 数据库访问频率
- 响应时间
- 错误率

### 3. 调优建议
- 根据实际使用情况调整策略
- 定期分析性能数据
- 优化缓存配置
- 监控系统资源使用

## 总结

通过为DroneService和TaskService添加智能数据源切换功能，系统在保证数据一致性的同时，显著提升了查询性能。不同的操作类型使用不同的数据源策略，实现了性能和可靠性的最佳平衡。 