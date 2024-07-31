using VideoPlatform.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

IConfiguration configuration = builder.Configuration;

builder.Services.ConfigureServices();
builder.Services.ConfigureAuthentication(configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();


if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
