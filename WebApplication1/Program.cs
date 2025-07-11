using WebApplication_Drone.Services;
using WebApplication1.Middleware;
using WebApplication1.Middleware.Interfaces;
using WebApplication1.Services;
using WebApplication1.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
#region 基础配置

// 路由配置
builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("string", typeof(string));
});

// 添加控制器
builder.Services.AddControllers();

// CORS配置 - 针对Aspire进行优化
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // 开发环境允许所有来源
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // 生产环境指定允许的来源
            policy.WithOrigins("https://blazorapp-web", "https://localhost:*")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });

    // 为图片API添加特殊CORS策略
    options.AddPolicy("ImagePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .WithMethods("GET")
              .WithHeaders("Accept", "Cache-Control", "If-None-Match", "If-Modified-Since");
    });
});

#endregion

// Add services to the container.
    
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
#region 数据服务注册
// 注册数据服务 - 添加接口到实现类的映射
builder.Services.AddSingleton<ISqlService, SqlService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddSingleton<IDroneService, DroneService>();
builder.Services.AddSingleton<ITaskService, TaskService>();
builder.Services.AddSingleton<IHistoryService, HistoryService>();

// 注册数据源中间件
builder.Services.Configure<DataSourceConfig>(builder.Configuration.GetSection("DataSource"));
builder.Services.AddSingleton<WebApplication1.Middleware.Interfaces.IDataSourceMiddleware, WebApplication1.Middleware.DataSourceMiddleware>();

// 注册实现类（用于直接注入）
builder.Services.AddSingleton<SqlService>();
builder.Services.AddSingleton<HistoryService>();
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<MissionSocketService>();
builder.Services.AddSingleton<SocketService>();
builder.Services.AddSingleton<DroneService>();
builder.Services.AddSingleton<TaskService>();

// 后台服务
builder.Services.AddHostedService<StartBackground>();
#endregion
#region HTTP客户端配置

// 配置默认HTTP客户端
builder.Services.AddHttpClient("DefaultApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler(); // 添加重试、熔断、超时等

#endregion
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
