using BateriaApp.Domain.Exceptions;

namespace BateriaApp.Domain;

/// <summary>
/// Sujeto observable del patrón Observer.
/// Modela el funcionamiento de la batería de una laptop: mantiene la lista de
/// interesados (<see cref="IObservadorBateria"/>) y los notifica automáticamente
/// cada vez que cambia el estado, y en particular cada vez que se modifica el
/// atributo <see cref="Conectado"/>, tal como pide el enunciado.
/// </summary>
/// <remarks>
/// Cantidades asumidas: el enunciado no indica las velocidades de carga y descarga,
/// por lo que se adoptaron valores explícitos y documentados:
/// <list type="bullet">
///   <item><description>Carga: 1 % por minuto (de 0 a 100 % insume 100 minutos).</description></item>
///   <item><description>Descarga: 0,5 % por minuto (a plena carga la autonomía es de 200 minutos).</description></item>
/// </list>
/// Ambas constantes son públicas y pueden ajustarse sin modificar la lógica.
/// </remarks>
public class Bateria
{
    /// <summary>Velocidad de carga asumida, en puntos porcentuales por minuto.</summary>
    public const double PorcentajeCargaPorMinuto = 1.0;

    /// <summary>Velocidad de descarga asumida, en puntos porcentuales por minuto.</summary>
    public const double PorcentajeDescargaPorMinuto = 0.5;

    private readonly List<IObservadorBateria> _observadores = new();

    private bool _conectado;
    private int _carga;

    /// <summary>
    /// Crea una batería en el estado inicial indicado.
    /// No se notifica nada al construir el objeto porque todavía no existe ningún
    /// observador suscripto: la primera notificación ocurre al suscribirse o al
    /// producirse el primer cambio de estado.
    /// </summary>
    /// <param name="cargaInicial">Carga inicial en tanto por ciento (0 a 100).</param>
    /// <param name="conectado">Indica si arranca conectada a la tensión.</param>
    /// <exception cref="CargaFueraDeRangoException">Si la carga inicial está fuera de 0-100.</exception>
    public Bateria(int cargaInicial = 0, bool conectado = false)
    {
        ValidarCarga(cargaInicial);

        _carga = cargaInicial;
        _conectado = conectado;

        RecalcularTiempos();
    }

    /// <summary>
    /// Se dispara cuando un observador lanza una excepción al ser notificado.
    /// Si no hay ningún manejador suscripto, la excepción se propaga (no se oculta).
    /// </summary>
    public event EventHandler<ErrorNotificacionEventArgs>? ErrorNotificacion;

    /// <summary>
    /// ¿La batería está conectada a la tensión?
    /// Al modificarse este atributo se recalculan los tiempos y se notifica
    /// automáticamente a todos los observadores.
    /// </summary>
    public bool Conectado
    {
        get => _conectado;
        set
        {
            if (_conectado == value)
            {
                return;
            }

            _conectado = value;
            RecalcularTiempos();

            // Requisito central del enunciado: notificar cada vez que cambia Conectado.
            Notificar();
        }
    }

    /// <summary>
    /// Carga actual en tanto por ciento. Al modificarse se recalculan los tiempos y
    /// se notifica, de modo que el Suscriptor Visual se actualice en tiempo real.
    /// </summary>
    /// <exception cref="CargaFueraDeRangoException">Si el valor está fuera de 0-100.</exception>
    public int Carga
    {
        get => _carga;
        set
        {
            ValidarCarga(value);

            if (_carga == value)
            {
                return;
            }

            _carga = value;
            RecalcularTiempos();
            Notificar();
        }
    }

    /// <summary>Minutos restantes estimados de carga (0 si la carga está completa o si no está conectada).</summary>
    public int TiempoCarga { get; private set; }

    /// <summary>Minutos restantes estimados de uso (0 si está conectada a la tensión).</summary>
    public int TiempoUso { get; private set; }

    /// <summary>Cantidad de observadores actualmente suscriptos.</summary>
    public int CantidadSuscriptores => _observadores.Count;

    /// <summary>Copia de sólo lectura de la lista de observadores.</summary>
    public IReadOnlyList<IObservadorBateria> Observadores => _observadores.AsReadOnly();

    /// <summary>
    /// Registra un observador (relación 1 a muchos: la batería acepta tantos como se quiera).
    /// </summary>
    /// <param name="observador">Observador que desea enterarse de los cambios.</param>
    /// <param name="notificarEstadoActual">
    /// Si es <c>true</c> (valor por omisión) el observador recibe de inmediato el estado
    /// actual, para que pueda mostrarlo sin esperar al próximo cambio.
    /// </param>
    /// <exception cref="ArgumentNullException">Si el observador es <c>null</c>.</exception>
    public void Suscribir(IObservadorBateria observador, bool notificarEstadoActual = true)
    {
        ArgumentNullException.ThrowIfNull(observador);

        if (_observadores.Contains(observador))
        {
            return;
        }

        _observadores.Add(observador);

        if (notificarEstadoActual)
        {
            NotificarA(observador, ObtenerEstado());
        }
    }

    /// <summary>Quita un observador de la lista de suscriptos.</summary>
    /// <param name="observador">Observador a dar de baja.</param>
    /// <returns><c>true</c> si estaba suscripto y se quitó; <c>false</c> en caso contrario.</returns>
    /// <exception cref="ArgumentNullException">Si el observador es <c>null</c>.</exception>
    public bool Desuscribir(IObservadorBateria observador)
    {
        ArgumentNullException.ThrowIfNull(observador);

        return _observadores.Remove(observador);
    }

    /// <summary>
    /// Notifica el estado actual a TODOS los observadores suscriptos.
    /// Es el método que el enunciado describe como encargado de avisar el cambio de estado.
    /// </summary>
    public void Notificar()
    {
        var estado = ObtenerEstado();

        // Se itera sobre una copia: así un observador puede desuscribirse (o suscribir
        // a otro) mientras se está notificando, sin romper la enumeración.
        foreach (var observador in _observadores.ToList())
        {
            NotificarA(observador, estado);
        }
    }

    /// <summary>Devuelve una fotografía inmutable del estado actual de la batería.</summary>
    /// <returns>El <see cref="EstadoBateria"/> correspondiente al instante actual.</returns>
    public EstadoBateria ObtenerEstado() => new()
    {
        Conectado = _conectado,
        Carga = _carga,
        TiempoCarga = TiempoCarga,
        TiempoUso = TiempoUso,
        FechaHora = DateTime.Now,
    };

    /// <summary>
    /// Notifica a un único observador aislando el fallo: si este observador lanza una
    /// excepción, se informa por <see cref="ErrorNotificacion"/> y se continúa con los
    /// demás, de modo que una bitácora caída no impida ver el estado por consola.
    /// </summary>
    private void NotificarA(IObservadorBateria observador, EstadoBateria estado)
    {
        try
        {
            observador.Actualizar(estado);
        }
        catch (Exception excepcion)
        {
            var manejador = ErrorNotificacion;

            if (manejador is null)
            {
                throw;
            }

            manejador(this, new ErrorNotificacionEventArgs(observador, excepcion));
        }
    }

    /// <summary>Valida que la carga se encuentre dentro del rango admitido (0 a 100).</summary>
    /// <param name="carga">Valor a validar.</param>
    /// <exception cref="CargaFueraDeRangoException">Si el valor está fuera de rango.</exception>
    private static void ValidarCarga(int carga)
    {
        if (carga < CargaFueraDeRangoException.CargaMinima || carga > CargaFueraDeRangoException.CargaMaxima)
        {
            throw new CargaFueraDeRangoException(carga);
        }
    }

    /// <summary>
    /// Recalcula <see cref="TiempoCarga"/> y <see cref="TiempoUso"/> a partir de la
    /// carga y del estado de conexión.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><c>TiempoCarga</c>: sólo tiene sentido mientras la batería está conectada y no está completa. Es 0 si la carga llegó al 100 % (requisito explícito del enunciado) y también si no hay tensión.</description></item>
    ///   <item><description><c>TiempoUso</c>: sólo tiene sentido sin tensión externa; es 0 mientras la batería está conectada.</description></item>
    /// </list>
    /// </remarks>
    private void RecalcularTiempos()
    {
        TiempoCarga = _conectado && _carga < CargaFueraDeRangoException.CargaMaxima
            ? (int)Math.Ceiling((CargaFueraDeRangoException.CargaMaxima - _carga) / PorcentajeCargaPorMinuto)
            : 0;

        TiempoUso = !_conectado
            ? (int)Math.Ceiling(_carga / PorcentajeDescargaPorMinuto)
            : 0;
    }
}
