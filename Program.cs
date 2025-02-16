using Microsoft.AspNetCore.Authentication.Negotiate;
using RabbitTester.Domain.Configuration;
using RabbitTester.IOC;
using RabbitTester.Tester;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.Configure<RabbitMqConfigurations>(builder.Configuration.GetSection("RabbitMqConfigurations"));
builder.Services.AddTransient<DataHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRabbitMQ();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
