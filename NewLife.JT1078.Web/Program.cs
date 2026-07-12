using NewLife.JT1078.Server;

var builder = WebApplication.CreateBuilder(args);

// 使用 JT1078 网关构建器注册服务
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();

builder.Services.AddControllers();

var app = builder.Build();

app.UseWebSockets();
app.MapControllers();

app.Run();
