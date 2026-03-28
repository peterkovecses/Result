var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCore();
var app = builder.Build();
app.MapControllers();
app.Run();
