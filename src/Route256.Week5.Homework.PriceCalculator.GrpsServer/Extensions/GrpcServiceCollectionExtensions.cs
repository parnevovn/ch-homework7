using MediatR;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Queries;

namespace Route256.Week5.Homework.PriceCalculator.GrpcServer.Extensions;

public static class GrpcServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcServer(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(GrpcServiceCollectionExtensions).Assembly));
        services.AddTransient<IRequestHandler<CalculateDeliveryPriceCommand, CalculateDeliveryPriceResult>, CalculateDeliveryPriceCommandHandler>();
        services.AddTransient<IRequestHandler<ClearHistoryCommand, Unit>, ClearHistoryCommandHandler>();
        services.AddTransient<IRequestHandler<GetCalculationHistoryQuery, GetHistoryQueryResult>, GetCalculationHistoryQueryHandler>();
        services.AddGrpc();
        return services;
    }
}