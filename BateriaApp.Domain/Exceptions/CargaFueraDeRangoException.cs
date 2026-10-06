namespace BateriaApp.Domain.Exceptions;

// Se lanza cuando alguien intenta poner una carga que no esta entre 0 y 100.
public class CargaFueraDeRangoException : Exception
{
    public CargaFueraDeRangoException(int valorInvalido)
        : base("La carga tiene que estar entre 0 y 100. Me llego: " + valorInvalido)
    {
        ValorInvalido = valorInvalido;
    }

    // Guardo el valor que vino mal, por si lo necesito despues.
    public int ValorInvalido { get; private set; }
}
