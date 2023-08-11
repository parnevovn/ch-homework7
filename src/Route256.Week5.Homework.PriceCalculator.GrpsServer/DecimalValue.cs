namespace Route256.Week5.Homework.PriceCalculator.GrpcServer
{
    public partial class DecimalValue
    {
        private const decimal NanoFactor = 1_000_000_000;
        public decimal ToDecimal()
        {
            return units_ + nanos_ / NanoFactor;
        }

        public static DecimalValue FromDecimal(decimal val)
        {
            var units = decimal.ToInt64(val);
            var nanos = decimal.ToInt32((val - units) * NanoFactor);

            return new DecimalValue
            {
                Units = units,
                Nanos = nanos
            };
        }
    }
}
