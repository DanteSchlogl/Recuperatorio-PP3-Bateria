namespace BateriaApp.Domain;

/// <summary>
/// Datos del error producido mientras un observador atendía una notificación.
/// Permite informar el problema sin interrumpir la notificación de los demás
/// suscriptores (principio de aislamiento de fallos).
/// </summary>
public sealed class ErrorNotificacionEventArgs : EventArgs
{
    /// <summary>Crea los datos del error.</summary>
    /// <param name="observador">Observador que falló.</param>
    /// <param name="excepcion">Excepción lanzada por ese observador.</param>
    public ErrorNotificacionEventArgs(IObservadorBateria observador, Exception excepcion)
    {
        Observador = observador;
        Excepcion = excepcion;
    }

    /// <summary>Observador que produjo el error.</summary>
    public IObservadorBateria Observador { get; }

    /// <summary>Excepción concreta que lanzó el observador.</summary>
    public Exception Excepcion { get; }
}
