namespace Route256.Week5.Homework.PriceCalculator.GrpcClient
{
    public class DecimalVal
    {
        private const decimal NanoFactor = 1_000_000_000;

        public long Units { get; set; }
        public int Nanos { get; set; }

        public decimal ToDecimal()
        {
            return Units + Nanos / NanoFactor;
        }

        public static DecimalVal FromDecimal(decimal val)
        {
            var units = decimal.ToInt64(val);
            var nanos = decimal.ToInt32((val - units) * NanoFactor);

            return new DecimalVal
            {
                Units = units,
                Nanos = nanos
            };
        }
    }
}
