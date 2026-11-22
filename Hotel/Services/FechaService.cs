namespace Hotel.Services
{
    public class FechaService
    {
        private readonly int _zonaHoraria;

        public FechaService(IConfiguration configuration)
        {
           
            _zonaHoraria = configuration.GetValue<int>("ZonaHoraria");
        }

        public DateTime ObtenerFechaActual()
        {
            return DateTime.UtcNow.AddHours(_zonaHoraria);
        }
    }
}
