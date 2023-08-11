using Grpc.Core;
using MediatR;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Queries;

namespace Route256.Week5.Homework.PriceCalculator.GrpcServer.Services
{
    public class CalcService : Calc.CalcBase
    {
        private readonly ILogger<CalcService> _logger;
        private readonly IMediator _mediator;

        public CalcService(
            ILogger<CalcService> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override async Task<CalcPriceReply> CalcPrice(CalcPriceRequest request, ServerCallContext context)
        {
            var goods = new GoodModel[request.Goods.Count];

            for (var i = 0; i < request.Goods.Count; i++)
            {
                goods[i] = new GoodModel(
                     request.Goods[i].Height,
                     request.Goods[i].Length,
                     request.Goods[i].Width,
                     request.Goods[i].Weight
                    );
            }

            var command = new CalculateDeliveryPriceCommand(
                 request.UserId, goods);

             var price = await _mediator.Send(command, context.CancellationToken);
            

            var reply = new CalcPriceReply
            {
                Price = DecimalValue.FromDecimal(price.Price)
            };

            return reply;
        }

        public override async Task<ClearHistoryReply> ClearHistory(ClearHistoryRequest request, ServerCallContext context)
        {
            var calculationIds = request.CalculationIds.ToArray();

            var command = new ClearHistoryCommand(
                 request.UserId, calculationIds);

            await _mediator.Send(command, context.CancellationToken);

            var reply = new ClearHistoryReply
            {
                Result = true
            };

            return reply;
        }

        public override async Task GetCalcHistory(GetCalcHistoryRequest request, IServerStreamWriter<GetCalcHistoryReply> response, ServerCallContext context)
        {
            var userId = request.UserId;

            var query = new GetCalculationHistoryQuery(
                 request.UserId, 
                 request.Take, 
                 request.Skip);

            var history = await _mediator.Send(query, context.CancellationToken);

            foreach ( var item in history.Items)
            {
                await response.WriteAsync(
                    new GetCalcHistoryReply 
                    {
                        Volume = item.Volume,
                        Weight = item.Weight,
                        Price = DecimalValue.FromDecimal(item.Price),
                        GoodIds = { item.GoodIds.ToArray() }
                    });
            }
        }

        public override async Task CalcPriceStream(
            IAsyncStreamReader<CalcPriceRequest> requestStream,
            IServerStreamWriter<CalcPriceReply> responseStream,
            ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                var goods = new GoodModel[request.Goods.Count];

                for (var i = 0; i < request.Goods.Count; i++)
                {
                    goods[i] = new GoodModel(
                         request.Goods[i].Height,
                         request.Goods[i].Length,
                         request.Goods[i].Width,
                         request.Goods[i].Weight
                        );
                }

                var command = new CalculateDeliveryPriceCommand(
                     request.UserId, goods);

                var price = await _mediator.Send(command, context.CancellationToken);


                var reply = new CalcPriceReply
                {
                    Price = DecimalValue.FromDecimal(price.Price)
                };

                await responseStream.WriteAsync(reply);
            }
        }
    }
}