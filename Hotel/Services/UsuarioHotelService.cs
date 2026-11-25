using Hotel.Data;
using Hotel.Data.Entidades;

namespace Hotel.Services

{
    public class UsuarioHotelService
    {
        private const string MarcaIdentity = "ASPNET_IDENTITY";

        private readonly HotelContext _contexto;
        private readonly FechaService _fechaService;

        public UsuarioHotelService(
            HotelContext contexto,
            FechaService fechaService)
        {
            _contexto = contexto;
            _fechaService = fechaService;
        }

        // Sincroniza el usuario de Identity con la tabla Usuario.
        // Retorna el IdUsuario de tipo int.
        public int AsegurarRegistro(string idIdentity)
        {
            // Buscar el usuario de ASP.NET Identity.
            var usuarioIdentity = _contexto.Users
                .FirstOrDefault(u => u.Id == idIdentity)
                ?? throw new InvalidOperationException(
                    "El usuario de la sesión no existe.");

            string correo = !string.IsNullOrWhiteSpace(usuarioIdentity.Email)
                ? usuarioIdentity.Email
                : !string.IsNullOrWhiteSpace(usuarioIdentity.UserName)
                    ? usuarioIdentity.UserName
                    : idIdentity;

            // Buscar si ya existe un registro relacionado con Identity.
            var usuario = _contexto.Usuarios
                .FirstOrDefault(u => u.IdentityId == idIdentity);

            if (usuario != null)
            {
                return usuario.IdUsuario;
            }

            // Si ya existe por correo, vincularlo con Identity.
            var porCorreo = _contexto.Usuarios
                .FirstOrDefault(u => u.Correo.ToLower() == correo.ToLower());

            if (porCorreo != null)
            {
                if (string.IsNullOrEmpty(porCorreo.IdentityId))
                {
                    porCorreo.IdentityId = idIdentity;
                    _contexto.SaveChanges();
                }
                else if (porCorreo.IdentityId != idIdentity)
                {
                    throw new InvalidOperationException(
                        "El correo ya está vinculado a otro usuario de Identity.");
                }

                return porCorreo.IdUsuario;
            }

            // Obtener el rol del usuario.
            string rol = (
                from ur in _contexto.UserRoles
                join r in _contexto.Roles
                    on ur.RoleId equals r.Id
                where ur.UserId == idIdentity
                select r.Name
            ).FirstOrDefault() ?? "Cliente";

            string idLimpio = idIdentity.Replace("-", "");

            // Crear el registro en nuestra tabla Usuario.
            var nuevoUsuario = new UsuarioEntidad
            {
                IdentityId = idIdentity,
                Nombre = Recortar(correo, 150),
                Cedula = "TMP-" + idLimpio[
                    ..Math.Min(16, idLimpio.Length)],
                Correo = Recortar(correo, 150),
                ContrasenaHash = MarcaIdentity,
                Rol = rol,
                Activo = true,
                FechaRegistro = _fechaService.ObtenerFechaActual()
            };

            _contexto.Usuarios.Add(nuevoUsuario);

            // IdUsuario se genera automáticamente en SQL Server.
            _contexto.SaveChanges();

            return nuevoUsuario.IdUsuario;
        }

        // Obtener los nombres usando los identificadores
        // de la tabla Usuario, que son de tipo int.
        public Dictionary<int, string> ObtenerNombres(
            IEnumerable<int> ids)
        {
            List<int> lista = ids
                .Where(i => i > 0)
                .Distinct()
                .ToList();

            Dictionary<int, string> nombres = _contexto.Usuarios
                .Where(u => lista.Contains(u.IdUsuario))
                .ToDictionary(
                    u => u.IdUsuario,
                    u => u.Nombre
                );

            foreach (int id in lista.Where(
                id => !nombres.ContainsKey(id)))
            {
                nombres[id] = "(usuario no encontrado)";
            }

            return nombres;
        }

        private static string Recortar(string texto, int maximo)
        {
            return texto.Length <= maximo
                ? texto
                : texto[..maximo];
        }
        public string? ObtenerIdentityId(int idUsuario)
        {
            return _contexto.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => u.IdentityId)
                .FirstOrDefault();
        }
    }
}