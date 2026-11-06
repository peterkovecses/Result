var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddBookStoreCore();

var app = builder.Build();
app.MapBookStoreEndpoints();
app.Run();

public partial class Program { }
