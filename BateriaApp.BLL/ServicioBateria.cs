using BateriaApp.DAL;
using BateriaApp.Domain;

namespace BateriaApp.BLL;

/// <summary>
/// Servicio de negocio que orquesta la batería y sus suscriptores.
/// Es el único punto de entrada que la capa de presentación necesita conocer: la UI no
/// instancia repositorios, no elige formatos de archivo y no conoce la capa DAL.
/// </summary>
public class ServicioBateria
{
    private readonly IRepositorioBitacora _repositorio;
    private readonly Bateria _bateria;

    /// <summary>Crea el servicio cableando el sujeto con su observador de bitácora.</summary>
    /// <param name="repositorio">Repositorio que usará el suscriptor de bitácora.</param>
    /// <param name="cargaInicial">Carga inicial en tanto por ciento (0 a 100).</param>
    /// <param name="conectado">Indica si la batería arranca conectada a la tensión.</param>
    /// <exception cref="ArgumentNullException">Si el repositorio es <c>null</c>.</exception>
    public ServicioBateria(IRepositorioBitacora repositorio, int cargaInicial = 50, bool conectado = false)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _bateria = new Bateria(cargaInicial, conectado);
        Bitacora = new SuscriptorBitacora(repositorio);
    }

    /// <summary>Suscriptor que persiste los cambios en la bitácora.</summary>
    public SuscriptorBitacora Bitacora { get; }

    /// <summary>Cantidad de observadores actualmente suscriptos a la batería.</summary>
    public int CantidadSuscriptores => _bateria.CantidadSuscriptores;

    /// <summary>Fotografía del estado actual de la batería.</summary>
    public EstadoBateria EstadoActual => _bateria.ObtenerEstado();

    /// <summary>Carpeta donde se guardan las bitácoras.</summary>
    public string CarpetaBitacoras => _repositorio.Carpeta;

    /// <summary>
    /// Se dispara cuando un observador falla al ser notificado. La UI lo aprovecha para
    /// informar el problema sin que se caiga la aplicación.
    /// </summary>
    public event EventHandler<ErrorNotificacionEventArgs>? ErrorNotificacion
    {
        add => _bateria.ErrorNotificacion += value;
        remove => _bateria.ErrorNotificacion -= value;
    }

    /// <summary>Suscriptor que se da de alta en la batería.</summary>
    /// <param name="observador">Observador a suscribir.</param>
    /// <param name="notificarEstadoActual">Si es <c>true</c>, recibe de inmediato el estado actual.</param>
    public void Suscribir(IObservadorBateria observador, bool notificarEstadoActual = true)
        => _bateria.Suscribir(observador, notificarEstadoActual);

    /// <summary>Suscriptor que se da de baja en la batería.</summary>
    /// <param name="observador">Observador a desuscribir.</param>
    /// <returns><c>true</c> si estaba suscripto y se dio de baja.</returns>
    public bool Desuscribir(IObservadorBateria observador)
        => _bateria.Desuscribir(observador);

    /// <summary>Conecta la batería a la tensión. Dispara la notificación automática.</summary>
    public void Conectar() => _bateria.Conectado = true;

    /// <summary>Desconecta la batería de la tensión. Dispara la notificación automática.</summary>
    public void Desconectar() => _bateria.Conectado = false;

    /// <summary>Establece la carga en tanto por ciento y notifica el cambio.</summary>
    /// <param name="carga">Nuevo valor de carga (0 a 100).</param>
    public void CambiarCarga(int carga) => _bateria.Carga = carga;

    /// <summary>Fuerza una notificación del estado actual a todos los suscriptores.</summary>
    public void Notificar() => _bateria.Notificar();

    /// <summary>Devuelve la ruta del archivo de bitácora correspondiente a una fecha.</summary>
    /// <param name="fecha">Fecha consultada.</param>
    /// <returns>Ruta del archivo diario.</returns>
    public string ObtenerRutaBitacora(DateTime fecha) => _repositorio.ObtenerRutaArchivo(fecha);

    /// <summary>Lee la bitácora de una fecha determinada.</summary>
    /// <param name="fecha">Fecha de la bitácora a leer.</param>
    /// <returns>Renglones leídos; colección vacía si todavía no hay registros.</returns>
    public IReadOnlyList<string> LeerBitacora(DateTime fecha) => _repositorio.LeerBitacora(fecha);

    /// <summary>Aplica la rotación por fecha de los archivos de bitácora.</summary>
    /// <param name="diasARetener">Cantidad de días que se conservan sin rotar.</param>
    /// <returns>Rutas de los archivos movidos a la subcarpeta <c>historico</c>.</returns>
    public IReadOnlyList<string> RotarBitacoras(int diasARetener) => _repositorio.Rotar(diasARetener);
}
