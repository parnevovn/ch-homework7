
using Grpc.Core;
using Grpc.Net.Client;
using Route256.Week5.Homework.PriceCalculator.GrpcClient;
using System.Text.Json;

var channel = GrpcChannel.ForAddress("http://localhost:5251");
var client = new Calc.CalcClient(channel);
long userId;

do
{
    Console.WriteLine("Введите код пользователя:");
}
while (!long.TryParse(Console.ReadLine(), out userId));


Console.WriteLine("Введите команду и параметры в одну строку");
Console.WriteLine("Список доступных команд: calc_price, clear_history, get_calc_history, calc_price_stream");
Console.WriteLine("Пример команды: 'calc_price 2.5 1.0 0.5 3.0;4.5 2.0 1.5 2.5'");
Console.WriteLine("Пример команды: 'clear_history 1 2'");
Console.WriteLine("Пример команды: 'get_calc_history 2 0'");
Console.WriteLine("Пример команды: 'calc_price_stream [путь к файлу]'");

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
            Console.WriteLine("Ошибка: команда 'calculate' требует параметры товаров");
        }
        else
        {
            var goodsStrings = parameters.Split(';');

            foreach (var goodsString in goodsStrings)
            {
                var goodsValues = goodsString.Split(' ');

                if (goodsValues.Length != 4)
                {
                    Console.WriteLine("Ошибка: каждый товар должен иметь 4 параметра (длина, ширина, высота, масса)");
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

            Console.WriteLine($"Результат расчета: {decimalValue.ToDecimal()}");
        }
        break;

    case "clear_history":
        if (string.IsNullOrEmpty(parameters))
        {
            Console.WriteLine("Ошибка: команда 'delete' требует параметры номера расчета");
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

            Console.WriteLine("Результаты расчетов успешно удалены");
        }
        break;

    case "get_calc_history":

        if (string.IsNullOrEmpty(parameters))
        {
            Console.WriteLine("Ошибка: команда 'delete' требует параметры номера расчета");
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
            Console.WriteLine("Ошибка: команда 'calculate' требует параметры товаров");
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

                        Console.WriteLine($"Результат расчета: {decimalValue.ToDecimal()}");
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
                            Console.WriteLine("Ошибка: каждый товар должен иметь 4 параметра (длина, ширина, высота, масса)");
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
                    Console.WriteLine($"Произошла ошибка: {ex.Message}");
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
        Console.WriteLine("Ошибка: неверная команда");
        break;
}

Console.WriteLine("Нажмите любую клавишу для завершения...");
Console.ReadKey();