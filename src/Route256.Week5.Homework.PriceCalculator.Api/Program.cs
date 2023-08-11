using FluentValidation.AspNetCore;
using Route256.Week5.Homework.PriceCalculator.Api.Middleware;
using Route256.Week5.Homework.PriceCalculator.Api.NamingPolicies;
using Route256.Week5.Homework.PriceCalculator.Bll.Extensions;
using Route256.Week5.Homework.PriceCalculator.Dal.Extensions;
using Route256.Week5.Homework.PriceCalculator.GrpcServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
    });

services.AddEndpointsApiExplorer();

// add swagger
services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(x => x.FullName);
});

//add validation
services.AddFluentValidation(conf =>
{
    conf.RegisterValidatorsFromAssembly(typeof(Program).Assembly);
    conf.AutomaticValidationEnabled = true;
});


//add dependencies
services
    .AddBll()
    .AddDalInfrastructure(builder.Configuration)
    .AddDalRepositories()
    .AddGrpcServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();
app.MapGrpcServer();
app.MigrateUp();
app.Run();
