namespace Hotel.Models
{
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Recepcionista = "Recepcionista";
        public const string Cliente = "Cliente";

     
        public const string Personal = Administrador + "," + Recepcionista;

        public static readonly string[] Todos = { Administrador, Recepcionista, Cliente };
    }
}
