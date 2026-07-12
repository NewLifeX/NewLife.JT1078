using NewLife.JT1078.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<SessionManager>();

var app = builder.Build();

app.UseWebSockets();
app.MapControllers();

app.Run();
