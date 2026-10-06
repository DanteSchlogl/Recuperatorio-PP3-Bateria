using BateriaApp.DAL;
using BateriaApp.Domain;

namespace BateriaApp.BLL;

// Es la puerta de entrada para la pantalla. Arma la bateria y sus suscriptores.
// La interfaz solo habla con esta clase y no conoce el resto de las capas.
public class ServicioBateria
{
    private readonly Bateria _bateria;
    private readonly IRepositorioBitacora _repositorio;

    public ServicioBateria(IRepositorioBitacora repositorio, int cargaInicial, bool conectado)
    {
        if (repositorio == null) throw new ArgumentNullException(nameof(repositorio));

        _repositorio = repositorio;
        _bateria = new Bateria(cargaInicial, conectado);
        Bitacora = new SuscriptorBitacora(repositorio);
    }

    public SuscriptorBitacora Bitacora { get; private set; }

    public int CantidadSuscriptores
    {
        get { return _bateria.CantidadSuscriptores; }
    }

    public EstadoBateria EstadoActual
    {
        get { return _bateria.ObtenerEstado(); }
    }

    public string CarpetaBitacoras
    {
        get { return _repositorio.Carpeta; }
    }

    // Aviso a la pantalla si algun suscriptor fallo.
    public event Action<string>? ErrorNotificacion
    {
        add { _bateria.ErrorNotificacion += value; }
        remove { _bateria.ErrorNotificacion -= value; }
    }

    public void Suscribir(IObservadorBateria observador)
    {
        _bateria.Suscribir(observador);
    }

    public bool Desuscribir(IObservadorBateria observador)
    {
        return _bateria.Desuscribir(observador);
    }

    // Conecto la bateria a la corriente.
    public void Conectar()
    {
        _bateria.Conectado = true;
    }

    // La desconecto.
    public void Desconectar()
    {
        _bateria.Conectado = false;
    }

    // Cambio el porcentaje de carga.
    public void CambiarCarga(int carga)
    {
        _bateria.Carga = carga;
    }

    // Fuerzo el aviso a todos, aunque no haya cambiado nada.
    public void Notificar()
    {
        _bateria.Notificar();
    }

    public string ObtenerRutaBitacora(DateTime fecha)
    {
        return _repositorio.ObtenerRutaArchivo(fecha);
    }

    public IReadOnlyList<string> LeerBitacora(DateTime fecha)
    {
        return _repositorio.LeerBitacora(fecha);
    }

    public IReadOnlyList<string> RotarBitacoras(int diasARetener)
    {
        return _repositorio.Rotar(diasARetener);
    }
}
