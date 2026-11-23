using Hotel.Data;
using Hotel.Data.Entidades;

namespace Hotel.Services
{
    
    public class UsuarioHotelService
    {
        private const string MarcaIdentity = "ASPNET_IDENTITY";

        private readonly HotelContext _contexto;
        private readonly FechaService _fechaService;

        public UsuarioHotelService(HotelContext contexto, FechaService fechaService)
        {
            _contexto = contexto;
            _fechaService = fechaService;
        }

       
        public string AsegurarRegistro(string idIdentity)
        {
            if (_contexto.Usuarios.Any(u => u.IdUsuario == idIdentity))
            {
                return idIdentity;
            }

            var usuarioIdentity = _contexto.Users.FirstOrDefault(u => u.Id == idIdentity)
                ?? throw new InvalidOperationException("El usuario de la sesión no existe.");

            string correo = usuarioIdentity.Email ?? usuarioIdentity.UserName ?? idIdentity;

            // Si el módulo de Usuarios ya lo registró con otro Id, se usa ese registro
            UsuarioEntidad? porCorreo = _contexto.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (porCorreo != null)
            {
                return porCorreo.IdUsuario;
            }

            string rol = (from ur in _contexto.UserRoles
                          join r in _contexto.Roles on ur.RoleId equals r.Id
                          where ur.UserId == idIdentity
                          select r.Name).FirstOrDefault() ?? "Cliente";

            string idLimpio = idIdentity.Replace("-", "");

            _contexto.Usuarios.Add(new UsuarioEntidad
            {
                IdUsuario = idIdentity,
                Nombre = Recortar(correo, 150),
             
                Cedula = "TMP-" + idLimpio[..Math.Min(16, idLimpio.Length)],
                Correo = Recortar(correo, 150),
                ContrasenaHash = MarcaIdentity, 
                Rol = rol,
                Activo = true,
                FechaRegistro = _fechaService.ObtenerFechaActual()
            });
            _contexto.SaveChanges();

            return idIdentity;
        }

      
        public Dictionary<string, string> ObtenerNombres(IEnumerable<string> ids)
        {
            List<string> lista = ids.Where(i => !string.IsNullOrEmpty(i)).Distinct().ToList();

            Dictionary<string, string> nombres = _contexto.Users
                .Where(u => lista.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.Email ?? u.UserName ?? u.Id);

            foreach (var u in _contexto.Usuarios.Where(u => lista.Contains(u.IdUsuario)).ToList())
            {
                nombres[u.IdUsuario] = u.Nombre;
            }

            foreach (string id in lista.Where(id => !nombres.ContainsKey(id)))
            {
                nombres[id] = "(usuario no encontrado)";
            }

            return nombres;
        }

        private static string Recortar(string texto, int maximo)
        {
            return texto.Length <= maximo ? texto : texto[..maximo];
        }
    }
}
