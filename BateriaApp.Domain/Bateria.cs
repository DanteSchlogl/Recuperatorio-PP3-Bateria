using BateriaApp.Domain.Exceptions;

namespace BateriaApp.Domain;

// Modela la bateria de una laptop. Cuando cambia su estado, avisa a todos los suscriptos.
public class Bateria
{
    // Supongo que carga 1% por minuto y se descarga medio punto por minuto.
    public const double PorcentajeCargaPorMinuto = 1.0;
    public const double PorcentajeDescargaPorMinuto = 0.5;

    private readonly List<IObservadorBateria> _observadores = new();

    private bool _conectado;
    private int _carga;

    public Bateria(int cargaInicial = 0, bool conectado = false)
    {
        ValidarCarga(cargaInicial);

        _carga = cargaInicial;
        _conectado = conectado;

        RecalcularTiempos();
    }

    // Si un suscriptor falla, aviso por aca y sigo con los demas.
    public event Action<string>? ErrorNotificacion;

    // Al cambiar Conectado se recalculan los tiempos y se avisa a todos.
    public bool Conectado
    {
        get { return _conectado; }
        set
        {
            if (_conectado == value) return;

            _conectado = value;
            RecalcularTiempos();
            Notificar();
        }
    }

    // Al cambiar la carga hago lo mismo, asi el visual se actualiza al toque.
    public int Carga
    {
        get { return _carga; }
        set
        {
            ValidarCarga(value);

            if (_carga == value) return;

            _carga = value;
            RecalcularTiempos();
            Notificar();
        }
    }

    // Minutos que faltan para el 100%. Es 0 si ya esta completa.
    public int TiempoCarga { get; private set; }

    // Minutos que quedan de uso. Es 0 si esta enchufada.
    public int TiempoUso { get; private set; }

    public int CantidadSuscriptores
    {
        get { return _observadores.Count; }
    }

    // Agrego un observador a la lista y le mando el estado actual para que muestre algo.
    public void Suscribir(IObservadorBateria observador)
    {
        if (observador == null) throw new ArgumentNullException(nameof(observador));
        if (_observadores.Contains(observador)) return;

        _observadores.Add(observador);
        NotificarA(observador, ObtenerEstado());
    }

    // Saco un observador de la lista. Devuelve true si estaba suscripto.
    public bool Desuscribir(IObservadorBateria observador)
    {
        if (observador == null) throw new ArgumentNullException(nameof(observador));

        return _observadores.Remove(observador);
    }

    // Aviso a todos. Recorro una copia por si alguno se desuscribe mientras aviso.
    public void Notificar()
    {
        EstadoBateria estado = ObtenerEstado();

        foreach (IObservadorBateria observador in _observadores.ToList())
        {
            NotificarA(observador, estado);
        }
    }

    // Devuelvo una foto del estado, asi los observadores no tocan la bateria de verdad.
    public EstadoBateria ObtenerEstado()
    {
        return new EstadoBateria
        {
            Conectado = _conectado,
            Carga = _carga,
            TiempoCarga = TiempoCarga,
            TiempoUso = TiempoUso,
            FechaHora = DateTime.Now
        };
    }

    // Aviso a uno solo. Si falla, no corto el aviso para los demas.
    private void NotificarA(IObservadorBateria observador, EstadoBateria estado)
    {
        try
        {
            observador.Actualizar(estado);
        }
        catch (Exception ex)
        {
            Action<string>? avisar = ErrorNotificacion;

            // Si nadie escucha el evento, dejo que la excepcion salga.
            if (avisar == null) throw;

            avisar("Fallo el suscriptor " + observador.Nombre + ": " + ex.Message);
        }
    }

    // La carga solo puede ir de 0 a 100.
    private static void ValidarCarga(int carga)
    {
        if (carga < 0 || carga > 100)
        {
            throw new CargaFueraDeRangoException(carga);
        }
    }

    // Calculo los minutos que faltan para cargar o para que se termine la bateria.
    private void RecalcularTiempos()
    {
        if (_conectado && _carga < 100)
        {
            TiempoCarga = (int)Math.Ceiling((100 - _carga) / PorcentajeCargaPorMinuto);
        }
        else
        {
            TiempoCarga = 0;
        }

        if (!_conectado)
        {
            TiempoUso = (int)Math.Ceiling(_carga / PorcentajeDescargaPorMinuto);
        }
        else
        {
            TiempoUso = 0;
        }
    }
}
