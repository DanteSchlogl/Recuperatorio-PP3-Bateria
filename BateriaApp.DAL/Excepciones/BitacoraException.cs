namespace BateriaApp.DAL.Excepciones;

// La uso cuando algo falla al leer o escribir la bitacora, asi las otras capas
// no tienen que saber de IOException ni de UnauthorizedAccessException.
public class BitacoraException : Exception
{
    public BitacoraException(string mensaje, string ruta, Exception interna)
        : base(mensaje, interna)
    {
        Ruta = ruta;
    }

    // La ruta del archivo donde fallo.
    public string Ruta { get; private set; }
}
