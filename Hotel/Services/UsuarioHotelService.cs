using Hotel.Data;
using Hotel.Data.Entidades;
using Hotel.Models;

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

        // Sincroniza usuarios existentes de Identity.
        // Retorna el IdUsuario de tipo int.
        public int AsegurarRegistro(string idIdentity)
        {
            var usuarioIdentity = _contexto.Users
                .FirstOrDefault(u => u.Id == idIdentity)
                ?? throw new InvalidOperationException(
                    "El usuario de la sesión no existe.");

            string correo = !string.IsNullOrWhiteSpace(usuarioIdentity.Email)
                ? usuarioIdentity.Email
                : !string.IsNullOrWhiteSpace(usuarioIdentity.UserName)
                    ? usuarioIdentity.UserName
                    : idIdentity;

            var usuario = _contexto.Usuarios
                .FirstOrDefault(u => u.IdentityId == idIdentity);

            if (usuario != null)
                return usuario.IdUsuario;

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

            string rol = (
                from ur in _contexto.UserRoles
                join r in _contexto.Roles
                    on ur.RoleId equals r.Id
                where ur.UserId == idIdentity
                select r.Name
            ).FirstOrDefault() ?? "Cliente";

            string idLimpio = idIdentity.Replace("-", "");

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
            _contexto.SaveChanges();

            return nuevoUsuario.IdUsuario;
        }

        // Registra o vincula un cliente con sus datos personales.
        public int AsegurarRegistroCliente(
            string idIdentity,
            string nombre,
            string cedula,
            string telefono,
            string? direccion)
        {
            var usuarioIdentity = _contexto.Users
                .FirstOrDefault(u => u.Id == idIdentity)
                ?? throw new InvalidOperationException(
                    "El usuario de Identity no existe.");

            string correo = usuarioIdentity.Email
                ?? throw new InvalidOperationException(
                    "El usuario no tiene un correo electrónico.");

            var usuario = _contexto.Usuarios
                .FirstOrDefault(u => u.IdentityId == idIdentity);

            if (usuario == null)
            {
                usuario = _contexto.Usuarios
                    .FirstOrDefault(u =>
                        u.Correo.ToLower() == correo.ToLower());

                if (usuario != null)
                {
                    if (!string.IsNullOrEmpty(usuario.IdentityId) &&
                        usuario.IdentityId != idIdentity)
                    {
                        throw new InvalidOperationException(
                            "El correo ya está vinculado a otro usuario.");
                    }

                    usuario.IdentityId = idIdentity;
                }
                else
                {
                    usuario = new UsuarioEntidad
                    {
                        IdentityId = idIdentity,
                        Correo = Recortar(correo, 150),
                        ContrasenaHash = MarcaIdentity,
                        Activo = true,
                        FechaRegistro = _fechaService.ObtenerFechaActual()
                    };

                    _contexto.Usuarios.Add(usuario);
                }
            }

            // Guardar los datos personales del cliente.
            usuario.Nombre = Recortar(nombre.Trim(), 150);
            usuario.Cedula = Recortar(cedula.Trim(), 20);
            usuario.Telefono = Recortar(telefono.Trim(), 20);
            usuario.Direccion = string.IsNullOrWhiteSpace(direccion)
                ? null
                : Recortar(direccion.Trim(), 250);

            usuario.Correo = Recortar(correo, 150);
            usuario.Rol = "Cliente";

            _contexto.SaveChanges();

            return usuario.IdUsuario;
        }

        // Obtener nombres usando los identificadores internos.
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

        public string ObtenerNombrePorIdentityId(string idIdentity)
        {
            var usuario = _contexto.Usuarios
                .FirstOrDefault(u => u.IdentityId == idIdentity);

            return usuario?.Nombre ?? "Usuario";
        }

        // Registra o vincula un usuario creado desde el panel administrativo.
        public int AsegurarRegistroAdministrativo(
            string idIdentity,
            string nombre,
            string cedula,
            string telefono,
            string? direccion,
            string rol)
        {
            if (!Roles.Todos.Contains(rol))
            {
                throw new InvalidOperationException(
                    "El rol seleccionado no es válido.");
            }

            var usuarioIdentity = _contexto.Users
                .FirstOrDefault(u => u.Id == idIdentity)
                ?? throw new InvalidOperationException(
                    "El usuario de Identity no existe.");

            string correo = usuarioIdentity.Email
                ?? throw new InvalidOperationException(
                    "El usuario no tiene un correo electrónico.");

            var usuario = _contexto.Usuarios
                .FirstOrDefault(u => u.IdentityId == idIdentity);

            if (usuario == null)
            {
                usuario = _contexto.Usuarios
                    .FirstOrDefault(u =>
                        u.Correo.ToLower() == correo.ToLower());

                if (usuario != null)
                {
                    if (!string.IsNullOrEmpty(usuario.IdentityId) &&
                        usuario.IdentityId != idIdentity)
                    {
                        throw new InvalidOperationException(
                            "El correo ya está vinculado a otro usuario.");
                    }

                    usuario.IdentityId = idIdentity;
                }
                else
                {
                    usuario = new UsuarioEntidad
                    {
                        IdentityId = idIdentity,
                        Correo = Recortar(correo, 150),
                        ContrasenaHash = MarcaIdentity,
                        Activo = true,
                        FechaRegistro = _fechaService.ObtenerFechaActual()
                    };

                    _contexto.Usuarios.Add(usuario);
                }
            }

            usuario.Nombre = Recortar(nombre.Trim(), 150);
            usuario.Cedula = Recortar(cedula.Trim(), 20);
            usuario.Telefono = Recortar(telefono.Trim(), 20);
            usuario.Direccion = string.IsNullOrWhiteSpace(direccion)
                ? null
                : Recortar(direccion.Trim(), 250);

            usuario.Correo = Recortar(correo, 150);
            usuario.Rol = rol;

            _contexto.SaveChanges();

            return usuario.IdUsuario;
        }
    }
}