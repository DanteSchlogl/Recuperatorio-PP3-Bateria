# Recuperatorio Prácticas Profesionalizantes III — Patrón Observer aplicado a la clase `Bateria`

| Dato | Valor |
|---|---|
| Instituto | Instituto Universitario Leonardo Da Vinci |
| Carrera | Analista en Sistemas Informáticos |
| Asignatura | Prácticas Profesionalizantes III |
| Instancia | Examen Recuperatorio — Turno Noche — 16/07/2026 |
| Alumno | Dante Schlögl |
| Lenguaje / IDE | C# / Visual Studio |
| Framework | .NET 8 (LTS) |
| Arquitectura | 4 capas: UI – BLL – DAL – Domain |

---

## 1. Consigna

Se dispone de una clase `Bateria` que modela el funcionamiento de la batería de una laptop:

| Atributo | Tipo | Descripción |
|---|---|---|
| `Conectado` | `bool` | ¿La batería está conectada a la tensión? |
| `Carga` | `int` | En tanto por ciento |
| `TiempoCarga` | `int` | Minutos restantes estimados de carga (0 si la carga está completa) |
| `TiempoUso` | `int` | Minutos restantes estimados de uso (si está desconectada de la tensión) |
| `Notificar` | método | Notifica el cambio de estado cada vez que `Conectado` se modifica |

Hay que probar el diseño con dos suscriptores:

1. **`SuscriptorVisual`** — muestra por consola si la batería está cargando o no, el porcentaje de carga, el tiempo restante de carga (si no está completa) y el tiempo estimado de uso (si está desconectada).
2. **`SuscriptorBitacora`** — escribe en una bitácora cada cambio de estado, con formato personalizado y fecha/hora, implementando **rotación de archivos por fecha**.

Además, toda la solución debe implementarse con **arquitectura de 4 capas**.

---

## 2. Cómo ejecutar

### Desde Visual Studio

1. Abrir `BateriaApp.sln`.
2. Verificar que `BateriaApp.UI` sea el proyecto de inicio.
3. `F5` (o `Ctrl+F5`).

El programa ejecuta primero una **demostración automática** de los 10 escenarios y luego habilita un **menú interactivo** para probar manualmente.

### Desde la línea de comandos

```bash
# Compilar toda la solución
dotnet build BateriaApp.sln -c Release

# Ejecutar (demostración + menú interactivo)
dotnet run --project BateriaApp.UI -c Release

# Ejecutar sólo la demostración, sin menú (modo no interactivo)
dotnet run --project BateriaApp.UI -c Release -- --demo
```

**Requisito:** .NET SDK 8.0 o superior.

La bitácora se genera en la carpeta `Bitacoras/` (relativa al directorio de ejecución).

---

## 3. Arquitectura en 4 capas

```
        +-------------------------------------------------------+
        |                     BateriaApp.UI                     |
        |   Presentación: Program, Consola, SuscriptorVisual    |
        +----------------------------+--------------------------+
                                     |  usa
                                     v
        +-------------------------------------------------------+
        |                    BateriaApp.BLL                     |
        |   Negocio: ServicioBateria, SuscriptorBitacora,       |
        |            FabricaBateria                             |
        +----------------------------+--------------------------+
                                     |  usa
                                     v
        +-------------------------------------------------------+
        |                    BateriaApp.DAL                     |
        |   Datos: IRepositorioBitacora,                        |
        |          RepositorioBitacoraArchivo, formateadores     |
        +----------------------------+--------------------------+
                                     |  usa
                                     v
        +-------------------------------------------------------+
        |                  BateriaApp.Domain                    |
        |   Dominio: Bateria, IObservadorBateria,               |
        |            EstadoBateria, RegistroBitacora,           |
        |            CargaFueraDeRangoException                  |
        +-------------------------------------------------------+
```

### Responsabilidades y reglas de dependencia

| Capa | Proyecto | Responsabilidad | Depende de |
|---|---|---|---|
| **Domain** | `BateriaApp.Domain` | Modelo del negocio y **patrón Observer**: `Bateria` como sujeto observable, `IObservadorBateria` como contrato, más las entidades y la excepción de dominio. | *(ninguna)* |
| **DAL** | `BateriaApp.DAL` | Persistencia de la bitácora: escritura en archivo, rotación por fecha y estrategias de formato. Expone sólo la interfaz `IRepositorioBitacora`. | Domain |
| **BLL** | `BateriaApp.BLL` | Reglas y orquestación: `SuscriptorBitacora` (persiste vía el repositorio) y `ServicioBateria` (punto de entrada único para la UI). La fábrica resuelve las implementaciones concretas. | Domain, DAL |
| **UI** | `BateriaApp.UI` | Presentación: `SuscriptorVisual`, formateo de la salida por consola, menú y programa principal. | BLL, Domain |

Las dependencias van **siempre en un solo sentido** (UI → BLL → DAL → Domain). Ninguna capa inferior conoce a una superior:

- `Domain` no referencia a nadie: podría reutilizarse en cualquier otro proyecto.
- `DAL` implementa la interfaz del repositorio sin saber quién la va a consumir.
- `BLL` depende de la **abstracción** `IRepositorioBitacora`, no del archivo concreto: se podría cambiar por una base de datos sin tocar una línea de negocio.
- `UI` nunca instancia repositorios ni conoce el formato de archivo: le pide un servicio a `FabricaBateria`.

---

## 4. Patrones de diseño aplicados

### 4.1 Observer (patrón principal)

**Problema detectado:** la batería cambia de estado y hay varios interesados en enterarse (el visual y la bitácora), pero la cantidad y el tipo de interesados puede variar. Si la clase `Bateria` tuviera que llamar directamente a `SuscriptorVisual` y a `SuscriptorBitacora`, quedaría acoplada a ellos, habría que modificarla cada vez que se agrega un suscriptor y la relación 1-a-muchos no estaría modelada.

**Solución:** patrón **Observer**.

| Rol del patrón | Clase |
|---|---|
| **Sujeto** (Subject) | `Bateria` |
| **Observador** (Observer) | `IObservadorBateria` |
| **Observadores concretos** | `SuscriptorVisual` (UI) y `SuscriptorBitacora` (BLL) |
| **Dato de la notificación** | `EstadoBateria` |

Cómo se cumple cada requisito:

- **Suscripción / desuscripción correcta** → `Bateria.Suscribir()` / `Bateria.Desuscribir()`, con validación de nulos y sin duplicar observadores. El método `Desuscribir` devuelve `bool` indicando si el observador realmente estaba dado de alta.
- **Notificación automática al cambiar `Conectado`** → el `set` de la propiedad `Conectado` llama a `Notificar()`. No hay que acordarse de notificar: es automático e inevitable.
- **Relación 1-a-muchos** → `Bateria` mantiene una `List<IObservadorBateria>` y notifica a todos, ignorando por completo sus tipos concretos.
- **Extensión razonable** → también se notifica al cambiar `Carga`, para que el suscriptor visual se actualice en tiempo real.
- **Aislamiento de fallos** → si un observador lanza una excepción, se informa por el evento `ErrorNotificacion` y se continúa notificando a los demás. Una bitácora que falla no impide ver el estado por consola.
- **Seguridad de la iteración** → `Notificar()` recorre una **copia** de la lista, para que un observador pueda desuscribirse durante la propia notificación sin romper la enumeración.

### 4.2 Strategy (formato de la bitácora)

**Problema detectado:** el enunciado pide un "formato personalizado" para la bitácora. Si el formato estuviera escrito adentro del repositorio, cambiarlo obligaría a modificar la clase de persistencia, y no se podrían ofrecer varios formatos.

**Solución:** patrón **Strategy** mediante `IFormateadorRegistro`, con dos implementaciones intercambiables:

- `FormateadorTextoPlano` → un renglón por evento (el usado por omisión).
- `FormateadorDetallado` → un bloque multilínea con todos los datos.

El repositorio sabe **dónde** guardar, pero no **con qué formato**: eso lo decide la estrategia inyectada.

### 4.3 Factory Method (armado de la solución)

**Problema detectado:** para construir el servicio hacen falta un repositorio y una estrategia de formato. Si la UI tuviera que instanciarlos, quedaría acoplada a la capa DAL y conocería clases que no le corresponden.

**Solución:** `FabricaBateria.Crear(...)` arma y cablea todo y devuelve un `ServicioBateria` listo para usar. La UI puede elegir el formato a través del enumerado `FormatoBitacora` sin nombrar ninguna clase de DAL.

### 4.4 ¿Por qué no se usaron otros patrones?

No se forzaron patrones que el problema no pedía. Por ejemplo, **Singleton** para la bitácora sería un antipatrón acá: agregaría estado global y dificultaría las pruebas, mientras que el repositorio ya se comparte por inyección. Se aplicaron patrones **donde el problema realmente lo requería**, que es exactamente lo que evalúa la consigna.

---

## 5. Reglas de negocio y cálculo de tiempos

### Supuestos adoptados

El enunciado no indica las velocidades de carga y descarga, por lo que se definieron como constantes explícitas y documentadas en la clase `Bateria`:

| Constante | Valor | Significado |
|---|---|---|
| `PorcentajeCargaPorMinuto` | `1,0` | De 0 % a 100 % insume 100 minutos |
| `PorcentajeDescargaPorMinuto` | `0,5` | A plena carga la autonomía es de 200 minutos |

Ambas son públicas y ajustables sin modificar la lógica.

### Reglas

| Situación | `TiempoCarga` | `TiempoUso` |
|---|---|---|
| Conectada y carga < 100 % | `⌈(100 − Carga) / 1,0⌉` | `0` |
| Conectada y carga = 100 % | `0` (requisito explícito del enunciado) | `0` |
| Desconectada | `0` | `⌈Carga / 0,5⌉` |

### Ejemplos verificados

| Carga | Estado | `TiempoCarga` | `TiempoUso` |
|---|---|---|---|
| 50 % | Desconectada | 0 | 100 min |
| 50 % | Conectada | 50 min | 0 |
| 80 % | Conectada | 20 min | 0 |
| 100 % | Conectada | 0 min | 0 |
| 100 % | Desconectada | 0 | 200 min |
| 25 % | Desconectada | 0 | 50 min |
| 60 % | Conectada | 40 min | 0 |

### Validación de rangos

La carga se valida en el `set` de la propiedad `Carga`: cualquier valor fuera del rango 0–100 lanza `CargaFueraDeRangoException` con un mensaje que indica el valor recibido. La validación está en el **dominio**, así que se aplica sin importar desde qué capa se intente modificar.

---

## 6. La bitácora: formato y rotación por fecha

### Ubicación y rotación

La rotación por fecha se resuelve con **un archivo por día**:

```
Bitacoras/
├── bitacora_2026-10-06.txt     <- archivo del día en curso
├── bitacora_2026-10-07.txt
└── historico/                  <- archivos apartados por antigüedad (opción 8 del menú)
```

Al cambiar el día, los eventos se derivan automáticamente al archivo nuevo: la rotación no depende de ningún proceso externo ni de un temporizador. Adicionalmente, `Rotar(dias)` mueve a `historico/` los archivos con más de la retención indicada (por omisión 30 días), **sin borrar nada**.

### Formato personalizado (por omisión, texto plano)

```
[2026-10-06 01:05:35]  50% | EN USO (quedan 100 min)    | origen: SuscriptorBitacora
[2026-10-06 01:05:35]  80% | CARGANDO (faltan 20 min)   | origen: SuscriptorBitacora
[2026-10-06 01:05:35] 100% | CARGADA COMPLETA           | origen: SuscriptorBitacora
```

### Formato alternativo (detallado, multilínea)

```
=== EVENTO DE BATERIA ===
  Fecha y hora    : 06/10/2026 01:05:35
  Origen          : SuscriptorBitacora
  Conectada       : SI
  Carga           : 80 %
  Tiempo de carga : 20 min
  Tiempo de uso   : 0 min
```

### Manejo de archivos

- Se crea la carpeta automáticamente si no existe.
- La escritura es en modo **append**: crea el archivo si falta y agrega al final si ya existe.
- Se asegura un único salto de línea final, sirva el formato que sirva.
- El acceso al archivo está protegido con un bloqueo, por si hay escrituras concurrentes.

---

## 7. Manejo de excepciones

| Excepción | Capa | Cuándo se lanza | Cómo se trata |
|---|---|---|---|
| `CargaFueraDeRangoException` | Domain | Se asigna una carga fuera de 0–100 | La UI la captura y muestra el mensaje; el programa **sigue funcionando** |
| `BitacoraException` | DAL | Falla de entrada/salida al escribir, leer o rotar (permisos, disco, ruta inválida). Envuelve la excepción original | La batería la aísla por observador, avisa por `ErrorNotificacion` y **continúa notificando a los demás** |
| `ArgumentNullException` | Domain / BLL / DAL | Se pasa `null` donde no corresponde (`Suscriptor`, `Registrar`, repositorio) | Falla temprano y con mensaje claro, mediante `ArgumentNullException.ThrowIfNull` |
| `ArgumentOutOfRangeException` | DAL | Se pide rotar con días negativos | Se valida antes de tocar el disco |

Las rutas que se informan en los errores se muestran **absolutas**, para que el problema sea fácil de ubicar.

---

## 8. Correspondencia con los criterios de evaluación

| # | Criterio (2 puntos cada uno) | Dónde se cumple |
|---|---|---|
| 1 | **Patrón en cada cambio de estado** — suscripción/desuscripción, notificación automática al cambiar `Conectado`, relación 1-a-muchos | `Bateria.Suscribir` / `Desuscribir` / `Notificar`; el `set` de `Conectado`. Demostrado en los pasos 1, 6, 7 y 8 |
| 2 | **Lógica de estado de batería** — cálculo de `TiempoCarga`, de `TiempoUso` y validación 0–100 % | `Bateria.RecalcularTiempos()` y `Bateria.ValidarCarga()`. Demostrado en los pasos 1 a 5, 9 y 10 |
| 3 | **Suscriptor Visual** — salida clara y formateada, actualización en tiempo real, visualización diferenciada carga vs. uso | `BateriaApp.UI/SuscriptorVisual.cs`: recuadro, barra de progreso y color diferenciado (verde cargando / amarillo en uso) |
| 4 | **Suscriptor Bitácora** — persistencia con formato personalizado, manejo de archivos, timestamps | `SuscriptorBitacora` (BLL) + `RepositorioBitacoraArchivo` (DAL) + estrategias de formato |
| 5 | **Calidad de código** — excepciones, arquitectura, comentarios y documentación | 4 capas con dependencias en un solo sentido, excepciones propias por capa, comentarios XML en todo el código y este documento |

---

## 9. Marco teórico

### Concepto de patrón de diseño

Un patrón de diseño es una **solución general y reutilizable** a un problema que aparece de forma recurrente en el diseño de software. No es código terminado ni una biblioteca: es una **descripción de una estructura de colaboración entre clases** que ya demostró ser correcta en muchos contextos. Cada patrón se compone de un nombre, el problema que resuelve, la solución propuesta, sus consecuencias (ventajas y costos) y las situaciones en que conviene aplicarlo.

### Relación entre los patrones de diseño y el desarrollo de software

Los patrones aportan un **vocabulario común** ("acá conviene un Observer") que permite comunicar decisiones de diseño sin reexplicarlas, y **capitalizan la experiencia** de otros desarrolladores en lugar de inventar una solución nueva para cada problema. Facilitan el mantenimiento y la evolución porque dejan el diseño preparado para el cambio, y mejoran la calidad al evitar errores ya conocidos. Su costo es que agregan abstracción: aplicar un patrón innecesario complica el código sin beneficio, por eso hay que reconocer primero el problema y recién después elegir el patrón.

### Patrones respecto al nivel de abstracción

Los patrones se clasifican según el nivel en que actúan. Los **patrones de diseño (GoF)** —como Observer, Strategy o Factory Method— trabajan a nivel de clases y objetos dentro de un mismo módulo o aplicación. Los de **arquitectura** —como capas, MVC o microservicios— actúan sobre la organización global del sistema. Los de **análisis** describen problemas del dominio del negocio. Esta solución usa las dos escalas: el patrón **arquitectural de capas** organiza el sistema completo (UI–BLL–DAL–Domain) y el patrón **Observer**, dentro de la capa de dominio, resuelve un problema puntual de comunicación entre objetos.

### Análisis y detección del problema

El paso previo a implementar un patrón es **detectar el problema en el diseño**, no partir del patrón. En este ejercicio la señal fue concreta: una clase que cambia de estado, varios objetos interesados en enterarse y la necesidad de no fijar de antemano ni la cantidad ni el tipo de esos objetos. Esa combinación —**un sujeto, muchos interesados, acoplamiento a evitar**— es precisamente la que resuelve Observer. Si el problema hubiera sido "elegir entre varios algoritmos", el patrón habría sido Strategy; si hubiera sido "crear objetos sin fijar la clase concreta", Factory Method.

### El producto y el proceso del desarrollo de software

El **producto** es el sistema resultante: el código, los datos y la documentación entregable. El **proceso** es el conjunto de actividades que lo producen —relevamiento, análisis, diseño, construcción, pruebas y mantenimiento—. Un buen proceso no garantiza por sí solo un buen producto, pero un producto sostenible en el tiempo es resultado de un proceso ordenado. Los patrones son una herramienta que actúa sobre **ambos**: mejoran el proceso porque dan un lenguaje y una guía de diseño comprobada, y mejoran el producto porque su estructura queda más clara, más fácil de modificar y menos propensa a errores.

### Gestión del riesgo

Gestionar el riesgo es **identificar** las amenazas que pueden hacer fracasar el trabajo, **evaluar** su probabilidad e impacto, y **definir acciones** para evitarlas o reducirlas. Los riesgos técnicos típicos de un desarrollo son el cambio frecuente de requisitos, el acoplamiento excesivo, la falta de pruebas y la dependencia de un único componente frágil. En esta solución se atacan varios de ellos:

- **Riesgo de cambio de requisitos** (por ejemplo, que pidan otro formato de bitácora): se mitiga con Strategy, que permite agregar un formato sin modificar el repositorio.
- **Riesgo de acoplamiento**: se mitiga con la separación en capas y el principio de depender de abstracciones (`IRepositorioBitacora`, `IObservadorBateria`).
- **Riesgo de fallo en una funcionalidad accesoria** (que no se pueda escribir la bitácora): se mitiga aislando el fallo por observador, de modo que la aplicación siga operativa e informe el problema.
- **Riesgo de datos inválidos**: se mitiga validando en el dominio, en un solo punto, antes de que el dato circule.

---

## 10. Estructura del repositorio

```
Recuperatorio-PP3-Bateria/
├── BateriaApp.sln                       <- solución de Visual Studio
├── .gitignore
├── README.md
├── Bitacoras/                           <- salida en tiempo de ejecución
│   └── .gitkeep
│
├── BateriaApp.Domain/                   <- capa de dominio (sin dependencias)
│   ├── Bateria.cs                       <- sujeto observable del patrón Observer
│   ├── IObservadorBateria.cs            <- interfaz del observador
│   ├── EstadoBateria.cs                 <- dato inmutable de la notificación
│   ├── RegistroBitacora.cs              <- entidad de la bitácora
│   ├── ErrorNotificacionEventArgs.cs    <- error de un observador
│   └── Exceptions/
│       └── CargaFueraDeRangoException.cs
│
├── BateriaApp.DAL/                      <- capa de acceso a datos
│   ├── IRepositorioBitacora.cs
│   ├── RepositorioBitacoraArchivo.cs    <- archivo diario + rotación
│   ├── Excepciones/
│   │   └── BitacoraException.cs
│   └── Formato/
│       ├── IFormateadorRegistro.cs      <- estrategia de formato
│       ├── FormateadorTextoPlano.cs
│       └── FormateadorDetallado.cs
│
├── BateriaApp.BLL/                      <- capa de negocio
│   ├── ServicioBateria.cs               <- punto de entrada para la UI
│   ├── SuscriptorBitacora.cs            <- observador que persiste
│   └── FabricaBateria.cs                <- armado de la solución
│
└── BateriaApp.UI/                       <- capa de presentación (ejecutable)
    ├── Program.cs
    ├── Consola.cs
    └── SuscriptorVisual.cs              <- observador que muestra por consola
```

---

## 11. Salida esperada

Fragmento de la demostración automática (paso 3: la batería llega al 100 %):

```
  >> Paso 3: la batería llega al 100 % -> el tiempo de carga debe ser 0

  +--------------------------------------------------------+
  | SUSCRIPTOR VISUAL - CAMBIO DE ESTADO                   |
  +--------------------------------------------------------+
  | Hora:                   06/10/2026 01:05:35            |
  | Estado:                 CONECTADA - CARGA COMPLETA     |
  | Porcentaje de carga:    100 %                          |
  | [                       ######################## ]     |
  | Tiempo para el 100 %:   carga completa (0 min)         |
  +--------------------------------------------------------+
```

Y el resumen final de la ejecución:

```
  +========================================================+
  | RESUMEN                                                |
  +========================================================+
  Actualizaciones recibidas por el SuscriptorVisual : 7
  Eventos registrados por el SuscriptorBitacora      : 8
  Observadores suscriptos al finalizar                : 2
  Rotación de bitácoras (retención 30 días)     : 0 archivo(s) movido(s)
  Fin del programa.
```

Los contadores son la mejor prueba de que el patrón funciona: el `SuscriptorVisual` recibió **7** notificaciones y la bitácora registró **8** eventos. La diferencia es exactamente los **2 eventos del paso 7**, ocurridos mientras el observador visual estaba desuscripto (y el paso 6, la desuscripción, no genera notificación porque no cambia el estado).

---

## 12. Menú interactivo

Luego de la demostración, el programa ofrece un menú para probar manualmente:

| Opción | Acción |
|---|---|
| 1 | Conectar el cargador |
| 2 | Desconectar el cargador |
| 3 | Establecer el porcentaje de carga |
| 4 | Suscribir el `SuscriptorVisual` |
| 5 | Desuscribir el `SuscriptorVisual` |
| 6 | Forzar una notificación (`Notificar`) |
| 7 | Leer la bitácora del día |
| 8 | Rotar las bitácoras anteriores |
| 0 | Salir |
