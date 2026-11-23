using System.Globalization;

namespace Hotel.Models
{
    public static class Formato
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("en-US");

       
        public static string Colones(this decimal monto)
        {
            return "₡" + monto.ToString("N2", Cultura);
        }
    }
}
