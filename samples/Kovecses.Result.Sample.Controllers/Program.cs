var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddBookStoreCore();
var app = builder.Build();
app.MapControllers();
app.Run();

public partial class Program { }
