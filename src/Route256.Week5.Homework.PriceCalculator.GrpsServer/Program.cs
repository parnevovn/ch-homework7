using Route256.Week5.Homework.PriceCalculator.Bll.Extensions;
using Route256.Week5.Homework.PriceCalculator.Dal.Extensions;
using Route256.Week5.Homework.PriceCalculator.GrpcServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

//add dependencies
services
    .AddBll()
    .AddDalInfrastructure(builder.Configuration)
    .AddDalRepositories()
    .AddGrpcServer();

var app = builder.Build();

app.MapGrpcServer();

app.Run();
