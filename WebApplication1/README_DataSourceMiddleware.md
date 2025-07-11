# 数据源中间件使用说明

## 概述

数据源中间件（DataSourceMiddleware）是一个可配置的数据访问层，允许您在运行时动态调整数据源策略，包括缓存和数据库的使用方式。

## 功能特性

### 1. 数据源类型

- **DatabaseFirst**: 数据库优先，缓存作为加速层
- **CacheFirst**: 缓存优先，数据库作为备份
- **DatabaseOnly**: 仅使用数据库，禁用缓存
- **CacheOnly**: 仅使用缓存，禁用数据库
- **Hybrid**: 混合模式，智能选择数据源

### 2. 配置选项

- **CacheExpiryMinutes**: 缓存过期时间（分钟）
- **EnableCache**: 是否启用缓存
- **EnableDatabase**: 是否启用数据库
- **WarmupStrategy**: 缓存预热策略
- **DatabaseTimeoutSeconds**: 数据库连接超时时间
- **EnableDetailedLogging**: 是否启用详细日志

### 3. 缓存预热策略

- **OnDemand**: 按需预热
- **OnStartup**: 启动时预热
- **Scheduled**: 定时预热
- **None**: 不预热

## API 接口

### 获取数据源状态
```
GET /api/datasource/status
```

### 获取数据源配置
```
GET /api/datasource/config
```

### 更新数据源配置
```
PUT /api/datasource/config
Content-Type: application/json

{
  "dataSourceType": "DatabaseFirst",
  "cacheExpiryMinutes": 10,
  "enableCache": true,
  "enableDatabase": true,
  "warmupStrategy": "OnDemand",
  "databaseTimeoutSeconds": 60,
  "enableDetailedLogging": true
}
```

### 切换数据源类型
```
POST /api/datasource/switch
Content-Type: application/json

{
  "dataSourceType": "CacheFirst"
}
```

### 缓存管理

#### 刷新缓存
```
POST /api/datasource/cache/refresh
```

#### 清空缓存
```
POST /api/datasource/cache/clear
```

#### 预热缓存
```
POST /api/datasource/cache/warmup
```

### 健康检查
```
GET /api/datasource/health
```

## 使用示例

### 1. 基本配置

在 `appsettings.json` 中配置数据源：

```json
{
  "DataSource": {
    "DataSourceType": "DatabaseFirst",
    "CacheExpiryMinutes": 5,
    "EnableCache": true,
    "EnableDatabase": true,
    "WarmupStrategy": "OnDemand",
    "DatabaseTimeoutSeconds": 60,
    "EnableDetailedLogging": false
  }
}
```

### 2. 运行时切换数据源

```csharp
// 注入数据源中间件
private readonly IDataSourceMiddleware _dataSourceMiddleware;

// 切换到缓存优先模式
await _dataSourceMiddleware.SwitchDataSourceAsync(DataSourceType.CacheFirst);

// 更新配置
var config = new DataSourceConfig
{
    DataSourceType = DataSourceType.Hybrid,
    CacheExpiryMinutes = 10,
    EnableCache = true,
    EnableDatabase = true
};
await _dataSourceMiddleware.UpdateDataSourceConfigAsync(config);
```

### 3. 监控数据源状态

```csharp
// 获取状态
var status = _dataSourceMiddleware.GetDataSourceStatus();

// 健康检查
var health = await _dataSourceMiddleware.CheckHealthAsync();

// 获取缓存统计
var cacheStats = await _dataSourceMiddleware.GetCacheStatisticsAsync();
```

## 最佳实践

### 1. 性能优化

- 使用 `CacheFirst` 模式提高读取性能
- 设置合适的缓存过期时间
- 定期预热热点数据

### 2. 故障处理

- 使用 `Hybrid` 模式提供容错能力
- 监控数据源健康状态
- 实现降级策略

### 3. 监控和日志

- 启用详细日志进行调试
- 定期检查缓存命中率
- 监控数据库连接状态

## 测试

使用提供的 `datasource_test.http` 文件进行API测试：

1. 启动应用程序
2. 在VS Code中打开 `datasource_test.http`
3. 设置 `baseUrl` 变量（如：`https://localhost:7001`）
4. 逐个执行测试请求

## 故障排除

### 常见问题

1. **缓存未命中**
   - 检查缓存是否启用
   - 验证缓存键是否正确
   - 查看缓存过期时间

2. **数据库连接失败**
   - 检查连接字符串
   - 验证数据库服务状态
   - 查看超时配置

3. **性能问题**
   - 调整缓存策略
   - 优化数据库查询
   - 增加缓存预热

### 日志分析

启用详细日志后，可以查看以下信息：

- 数据源切换日志
- 缓存操作日志
- 数据库查询日志
- 性能指标日志

## 扩展功能

### 自定义数据源

可以通过实现 `IDataSourceMiddleware` 接口来创建自定义数据源：

```csharp
public class CustomDataSourceMiddleware : IDataSourceMiddleware
{
    // 实现接口方法
}
```

### 插件化架构

数据源中间件支持插件化扩展，可以轻松添加新的数据源类型和策略。 