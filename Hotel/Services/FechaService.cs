namespace HotelLosPatitos.Services
{
    public class FechaService
    {
        private readonly int _zonaHoraria;

        public FechaService(IConfiguration configuration)
        {
            // Se lee "ZonaHoraria" de appsettings.json (en Costa Rica es -6)
            _zonaHoraria = configuration.GetValue<int>("ZonaHoraria");
        }

        public DateTime ObtenerFechaActual()
        {
            return DateTime.UtcNow.AddHours(_zonaHoraria);
        }
    }
}
