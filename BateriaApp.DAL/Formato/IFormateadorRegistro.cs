using BateriaApp.Domain;

namespace BateriaApp.DAL.Formato;

// Formas de escribir un evento en la bitacora. El repositorio usa una de estas.
// Si quiero otro formato, agrego otra clase aca y no toco el repositorio.
public interface IFormateadorRegistro
{
    string Nombre { get; }

    string Formatear(EstadoBateria estado);
}
