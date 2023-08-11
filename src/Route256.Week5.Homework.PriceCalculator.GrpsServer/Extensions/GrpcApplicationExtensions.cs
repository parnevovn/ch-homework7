using Route256.Week5.Homework.PriceCalculator.GrpcServer.Services;

namespace Route256.Week5.Homework.PriceCalculator.GrpcServer.Extensions;

public static class GrpcApplicationExtensions
{
    public static WebApplication MapGrpcServer(this WebApplication app)
    {
        app.MapGrpcService<CalcService>();
       // app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

        return app;
    }
}