namespace BateriaApp.Domain;

/// <summary>
/// Entidad que representa una línea de la bitácora: un evento de cambio de estado
/// con su marca de tiempo.
/// La capa de negocio arma este objeto a partir de cada notificación recibida y la
/// capa DAL se encarga de persistirlo; así ninguna de las dos conoce el formato del otro.
/// </summary>
public sealed record RegistroBitacora
{
    /// <summary>Construye el registro a partir del estado notificado.</summary>
    /// <param name="estado">Estado de la batería en el momento del cambio.</param>
    /// <param name="origen">Nombre del observador que generó el registro.</param>
    public RegistroBitacora(EstadoBateria estado, string origen)
    {
        ArgumentNullException.ThrowIfNull(estado);

        FechaHora = estado.FechaHora;
        Origen = origen;
        Conectado = estado.Conectado;
        Carga = estado.Carga;
        TiempoCarga = estado.TiempoCarga;
        TiempoUso = estado.TiempoUso;
    }

    /// <summary>Fecha y hora del evento (marca de tiempo).</summary>
    public DateTime FechaHora { get; init; }

    /// <summary>Observador que originó el registro.</summary>
    public string Origen { get; init; }

    /// <summary>Estado de conexión en el momento del evento.</summary>
    public bool Conectado { get; init; }

    /// <summary>Carga en tanto por ciento en el momento del evento.</summary>
    public int Carga { get; init; }

    /// <summary>Minutos restantes de carga informados en el evento.</summary>
    public int TiempoCarga { get; init; }

    /// <summary>Minutos restantes de uso informados en el evento.</summary>
    public int TiempoUso { get; init; }
}
