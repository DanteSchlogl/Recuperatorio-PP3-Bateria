using BateriaApp.DAL;
using BateriaApp.Domain;

namespace BateriaApp.BLL;

/// <summary>
/// Observador concreto (patrón Observer) que registra cada cambio de estado en la bitácora.
/// Vive en la capa de negocio porque decide <b>qué</b> se registra; la capa DAL decide
/// <b>cómo</b> y <b>dónde</b> se guarda, a través de <see cref="IRepositorioBitacora"/>.
/// </summary>
public class SuscriptorBitacora : IObservadorBateria
{
    private readonly IRepositorioBitacora _repositorio;

    /// <summary>Crea el suscriptor sobre el repositorio indicado.</summary>
    /// <param name="repositorio">Repositorio donde se persistirán los eventos.</param>
    /// <param name="nombre">Nombre simbólico del observador.</param>
    /// <exception cref="ArgumentNullException">Si el repositorio es <c>null</c>.</exception>
    public SuscriptorBitacora(IRepositorioBitacora repositorio, string nombre = "SuscriptorBitacora")
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        Nombre = nombre;
    }

    /// <inheritdoc />
    public string Nombre { get; }

    /// <summary>Cantidad de eventos que este observador registró.</summary>
    public int EventosRegistrados => _repositorio.EventosRegistrados;

    /// <summary>Carpeta donde se están guardando las bitácoras.</summary>
    public string Carpeta => _repositorio.Carpeta;

    /// <summary>
    /// Arma el registro a partir del estado notificado y lo persiste.
    /// Si la escritura falla, la excepción se propaga: la batería la captura, avisa por
    /// <c>ErrorNotificacion</c> y continúa notificando al resto de los observadores.
    /// </summary>
    /// <param name="estado">Estado notificado por el sujeto.</param>
    public void Actualizar(EstadoBateria estado)
    {
        var registro = new RegistroBitacora(estado, Nombre);

        _repositorio.Registrar(registro);
    }
}
