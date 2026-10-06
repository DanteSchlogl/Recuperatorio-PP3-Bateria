namespace BateriaApp.Domain;

/// <summary>
/// Fotografía inmutable del estado de la batería en un instante determinado.
/// Es el dato que el sujeto (<see cref="Bateria"/>) entrega a cada observador al
/// notificar un cambio, de modo que los suscriptores nunca manipulen directamente
/// el objeto observable: sólo lo leen.
/// </summary>
public sealed record EstadoBateria
{
    /// <summary>Indica si la batería está conectada a la tensión.</summary>
    public bool Conectado { get; init; }

    /// <summary>Carga actual expresada en tanto por ciento (0 a 100).</summary>
    public int Carga { get; init; }

    /// <summary>Minutos restantes estimados para completar la carga (0 si la carga está completa).</summary>
    public int TiempoCarga { get; init; }

    /// <summary>Minutos restantes estimados de uso (0 si la batería está conectada a la tensión).</summary>
    public int TiempoUso { get; init; }

    /// <summary>Momento exacto en que se generó el estado; se usa como marca de tiempo de la bitácora.</summary>
    public DateTime FechaHora { get; init; } = DateTime.Now;

    /// <summary>Verdadero cuando está conectada y todavía no alcanzó el 100 %.</summary>
    public bool EstaCargando => Conectado && Carga < 100;

    /// <summary>Verdadero cuando la carga alcanzó el 100 %.</summary>
    public bool EstaCompleta => Carga >= 100;

    /// <summary>Verdadero cuando la batería está trabajando sin tensión externa.</summary>
    public bool EstaEnUso => !Conectado;

    /// <summary>Resumen legible del estado, pensado para mostrarse al usuario.</summary>
    public string DescripcionEstado
    {
        get
        {
            if (Conectado)
            {
                return Carga >= 100 ? "CONECTADA - CARGA COMPLETA" : "CONECTADA - CARGANDO";
            }

            return "DESCONECTADA - EN USO";
        }
    }
}
