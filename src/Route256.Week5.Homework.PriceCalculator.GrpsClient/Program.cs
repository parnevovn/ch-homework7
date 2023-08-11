
using Grpc.Core;
using Grpc.Net.Client;
using Route256.Week5.Homework.PriceCalculator.GrpcClient;
using System.Text.Json;

var channel = GrpcChannel.ForAddress("http://localhost:5251");
var client = new Calc.CalcClient(channel);
long userId;

do
{
    Console.WriteLine("������� ��� ������������:");
}
while (!long.TryParse(Console.ReadLine(), out userId));


Console.WriteLine("������� ������� � ��������� � ���� ������");
Console.WriteLine("������ ��������� ������: calc_price, clear_history, get_calc_history, calc_price_stream");
Console.WriteLine("������ �������: 'calc_price 2.5 1.0 0.5 3.0;4.5 2.0 1.5 2.5'");
Console.WriteLine("������ �������: 'clear_history 1 2'");
Console.WriteLine("������ �������: 'get_calc_history 2 0'");
Console.WriteLine("������ �������: 'calc_price_stream [���� � �����]'");

var input = Console.ReadLine();
var inputArray = input.Split(' ');
var command = inputArray[0];
var parameters = string.Join(' ', inputArray.Skip(1));

switch (command)
{
    case "calc_price":
        var goods = new List<Good>();

        if (string.IsNullOrEmpty(parameters))
        {
            Console.WriteLine("������: ������� 'calculate' ������� ��������� �������");
        }
        else
        {
            var goodsStrings = parameters.Split(';');

            foreach (var goodsString in goodsStrings)
            {
                var goodsValues = goodsString.Split(' ');

                if (goodsValues.Length != 4)
                {
                    Console.WriteLine("������: ������ ����� ������ ����� 4 ��������� (�����, ������, ������, �����)");
                    return;
                }

                var good = new Good
                {
                    Length = double.Parse(goodsValues[0]),
                    Width = double.Parse(goodsValues[1]),
                    Height = double.Parse(goodsValues[2]),
                    Weight = double.Parse(goodsValues[3])
                };

                goods.Add(good);
            }

            var request = new CalcPriceRequest
            {
                UserId = userId,
                Goods = { goods }
            };

            var response = await client.CalcPriceAsync(request);

            var decimalValue = new DecimalVal
            {
                Units = response.Price.Units,
                Nanos = response.Price.Nanos
            };

            Console.WriteLine($"��������� �������: {decimalValue.ToDecimal()}");
        }
        break;

    case "clear_history":
        if (string.IsNullOrEmpty(parameters))
        {
            Console.WriteLine("������: ������� 'delete' ������� ��������� ������ �������");
        }
        else
        {
            var calculationIds = parameters.Split(' ').Select(long.Parse).ToList();

            var clearHistoryRequest = new ClearHistoryRequest
            {
                UserId = userId,
                CalculationIds = { calculationIds }
            };

            var clearHistoryReply = await client.ClearHistoryAsync(clearHistoryRequest);

            Console.WriteLine("���������� �������� ������� �������");
        }
        break;

    case "get_calc_history":

        if (string.IsNullOrEmpty(parameters))
        {
            Console.WriteLine("������: ������� 'delete' ������� ��������� ������ �������");
        }
        else
        {
            var parm = parameters.Split(' ').Select(int.Parse).ToArray();

            var historyRequest = new GetCalcHistoryRequest
            {
                UserId = userId,
                Take = parm[0],
                Skip = parm[1]
            };

            using (var call = client.GetCalcHistory(historyRequest))
            {
                await foreach (var calculation in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(JsonSerializer.Serialize(calculation));
                }
            }
        }
        break;
        
    case "calc_price_stream":
        if (string.IsNullOrEmpty(parameters))
        {
            Console.WriteLine("������: ������� 'calculate' ������� ��������� �������");
        }
        else
        {
            var fileName = parameters;

            using (var call = client.CalcPriceStream())
            {
                var responseTask = Task.Run(async () =>
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        var decimalValue = new DecimalVal
                        {
                            Units = response.Price.Units,
                            Nanos = response.Price.Nanos
                        };

                        Console.WriteLine($"��������� �������: {decimalValue.ToDecimal()}");
                    }
                });

                try
                {
                    using StreamReader reader = File.OpenText(fileName);

                    var lineStr = reader.ReadLine();

                    while (lineStr != null)
                    {
                        var goodsValues = lineStr.Split(' ');//.Select(double.Parse);

                        if (goodsValues.Length != 4)
                        {
                            Console.WriteLine("������: ������ ����� ������ ����� 4 ��������� (�����, ������, ������, �����)");
                            return;
                        }

                        var good = new Good
                        {
                            Length = double.Parse(goodsValues[0]),
                            Width = double.Parse(goodsValues[1]),
                            Height = double.Parse(goodsValues[2]),
                            Weight = double.Parse(goodsValues[3])
                        };

                        var request = new CalcPriceRequest
                        {
                            UserId = userId,
                            Goods = { good }
                        };

                        await call.RequestStream.WriteAsync(request);

                        lineStr = reader.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"��������� ������: {ex.Message}");
                }
                finally
                {
                    await call.RequestStream.CompleteAsync();
                    await responseTask;
                }
            }
        }
        break;
    default:
        Console.WriteLine("������: �������� �������");
        break;
}

Console.WriteLine("������� ����� ������� ��� ����������...");
Console.ReadKey();