# Recuperatorio Prácticas Profesionalizantes III — Patrón Observer aplicado a la clase `Bateria`

| Dato | Valor |
|---|---|
| Instituto | Instituto Universitario Leonardo Da Vinci |
| Carrera | Analista en Sistemas Informáticos |
| Asignatura | Prácticas Profesionalizantes III |
| Instancia | Examen Recuperatorio — Turno Noche — 16/07/2026 |
| Alumno | Dante Schlögl |
| Lenguaje / IDE | C# / Visual Studio |
| Framework | .NET 8 |
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

El programa ejecuta primero una **demostración automática** y después habilita un **menú** para probar a mano.

### Desde la línea de comandos

```bash
# Compilar toda la solución
dotnet build BateriaApp.sln -c Release

# Ejecutar (demostración + menú)
dotnet run --project BateriaApp.UI -c Release

# Ejecutar sólo la demostración, sin menú
dotnet run --project BateriaApp.UI -c Release -- --demo
```

**Requisito:** .NET SDK 8.0 o superior.

La bitácora se genera en la carpeta `Bitacoras/`, relativa al directorio desde donde se ejecuta.

---

## 3. Arquitectura en 4 capas

```
        +-------------------------------------------+
        |              BateriaApp.UI                |
        |   Program, SuscriptorVisual               |
        +--------------------+----------------------+
                             |  usa
                             v
        +-------------------------------------------+
        |             BateriaApp.BLL                |
        |   ServicioBateria, SuscriptorBitacora,     |
        |   FabricaBateria                           |
        +--------------------+----------------------+
                             |  usa
                             v
        +-------------------------------------------+
        |             BateriaApp.DAL                |
        |   IRepositorioBitacora,                    |
        |   RepositorioBitacoraArchivo, formateadores |
        +--------------------+----------------------+
                             |  usa
                             v
        +-------------------------------------------+
        |            BateriaApp.Domain              |
        |   Bateria, IObservadorBateria,             |
        |   EstadoBateria, CargaFueraDeRangoException |
        +-------------------------------------------+
```

### Responsabilidades y reglas de dependencia

| Capa | Proyecto | Responsabilidad | Depende de |
|---|---|---|---|
| **Domain** | `BateriaApp.Domain` | El modelo del negocio y el **patrón Observer**: `Bateria` como sujeto observable, `IObservadorBateria` como contrato, más el estado y la excepción de dominio. | *(ninguna)* |
| **DAL** | `BateriaApp.DAL` | Persistencia de la bitácora: escritura en archivo, rotación por fecha y formatos. Expone sólo la interfaz `IRepositorioBitacora`. | Domain |
| **BLL** | `BateriaApp.BLL` | Reglas y armado: `SuscriptorBitacora` (guarda usando el repositorio) y `ServicioBateria` (única puerta de entrada para la UI). La fábrica resuelve las clases concretas. | Domain, DAL |
| **UI** | `BateriaApp.UI` | Presentación: `SuscriptorVisual`, el menú y el programa principal. | BLL, Domain |

Las dependencias van **siempre en un solo sentido** (UI → BLL → DAL → Domain):

- `Domain` no referencia a nadie, así que se podría reutilizar en otro proyecto.
- `DAL` implementa la interfaz del repositorio sin saber quién la va a usar.
- `BLL` depende de la **interfaz** `IRepositorioBitacora`, no del archivo: se podría cambiar por una base de datos sin tocar el negocio.
- `UI` nunca crea repositorios ni conoce el formato del archivo: le pide un servicio a `FabricaBateria`.

---

## 4. Patrones de diseño aplicados

### 4.1 Observer (el patrón principal)

**El problema:** la batería cambia de estado y hay varios interesados en enterarse (el visual y la bitácora). Si `Bateria` llamara directamente a cada uno, quedaría acoplada a ellos, habría que modificarla cada vez que se agrega un suscriptor y la relación 1-a-muchos no estaría representada.

**La solución:** el patrón **Observer**.

| Rol del patrón | Clase |
|---|---|
| **Sujeto** (Subject) | `Bateria` |
| **Observador** (Observer) | `IObservadorBateria` |
| **Observadores concretos** | `SuscriptorVisual` (UI) y `SuscriptorBitacora` (BLL) |
| **Dato de la notificación** | `EstadoBateria` |

Cómo se cumple cada requisito:

- **Suscripción y desuscripción** → `Bateria.Suscribir()` y `Bateria.Desuscribir()`, sin duplicar observadores. `Desuscribir` devuelve `true` o `false` según si realmente estaba dado de alta.
- **Notificación automática al cambiar `Conectado`** → el `set` de la propiedad `Conectado` llama a `Notificar()`. No hay que acordarse de avisar: es automático.
- **Relación 1-a-muchos** → `Bateria` guarda una `List<IObservadorBateria>` y le avisa a todos, sin conocer sus tipos concretos.
- **Actualización en tiempo real** → también se avisa cuando cambia `Carga`, así el visual se actualiza al instante.
- **Si un suscriptor falla, los demás igual se enteran** → cada aviso va en un `try/catch` propio. El error se informa por el evento `ErrorNotificacion` y se sigue con el resto. Una bitácora que falla no impide ver el estado por consola.
- **Recorrido seguro** → `Notificar()` recorre una **copia** de la lista, así un observador puede desuscribirse mientras se está avisando sin romper la iteración.

### 4.2 Strategy (el formato de la bitácora)

**El problema:** el enunciado pide un "formato personalizado". Si el formato estuviera escrito dentro del repositorio, cambiarlo obligaría a modificar la clase que guarda.

**La solución:** el patrón **Strategy** con la interfaz `IFormateadorRegistro` y dos implementaciones intercambiables:

- `FormateadorTextoPlano` → un renglón por evento (el que se usa por omisión).
- `FormateadorDetallado` → un bloque de varios renglones con todo el detalle.

El repositorio sabe **dónde** guardar, pero no **con qué formato**: eso lo decide el formateador que se le pasa.

### 4.3 Factory Method (el armado de todo)

Para construir el servicio hacen falta un repositorio y un formato. Si la UI tuviera que crearlos, conocería la capa DAL. En cambio `FabricaBateria.Crear(...)` arma todo y devuelve un `ServicioBateria` listo, así la UI sólo elige el formato con el enumerado `FormatoBitacora`.

### 4.4 ¿Por qué no otros patrones?

No se agregaron patrones que el problema no pedía. Por ejemplo, un **Singleton** para la bitácora sería un antipatrón acá: agregaría estado global y complicaría las pruebas, cuando el repositorio ya se comparte pasándolo por parámetro. Se aplicaron patrones **donde el problema realmente lo necesitaba**, que es lo que evalúa la consigna.

---

## 5. Reglas de negocio y cálculo de tiempos

### Supuestos

El enunciado no indica las velocidades de carga y descarga, así que quedaron como constantes públicas en `Bateria`:

| Constante | Valor | Significado |
|---|---|---|
| `PorcentajeCargaPorMinuto` | `1,0` | De 0 % a 100 % tarda 100 minutos |
| `PorcentajeDescargaPorMinuto` | `0,5` | A plena carga la autonomía es de 200 minutos |

### Reglas

| Situación | `TiempoCarga` | `TiempoUso` |
|---|---|---|
| Conectada y carga menor a 100 % | `(100 − Carga) / 1,0` | `0` |
| Conectada y carga igual a 100 % | `0` (lo pide el enunciado) | `0` |
| Desconectada | `0` | `Carga / 0,5` |

### Ejemplos (los mismos que corren en la demostración)

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

La carga se valida en el `set` de la propiedad `Carga`, dentro del dominio: cualquier valor fuera de 0 a 100 lanza `CargaFueraDeRangoException`. Como está en el dominio, se aplica sin importar desde qué capa se intente cambiar.

---

## 6. La bitácora: formato y rotación por fecha

### Ubicación y rotación

La rotación se resuelve con **un archivo por día**:

```
Bitacoras/
├── bitacora_2026-10-06.txt     <- el archivo del día
├── bitacora_2026-10-07.txt
└── historico/                  <- archivos apartados por antigüedad (opción 8 del menú)
```

Como el nombre lleva la fecha, al cambiar el día los eventos van solos al archivo nuevo: la rotación no depende de ningún temporizador. Además, `Rotar(días)` mueve a `historico/` los archivos con más de la retención indicada (30 días por omisión), **sin borrar nada**.

### Formato por omisión (texto plano)

```
[06/10/2026 01:20:15] 50% - EN USO (quedan 100 min)
[06/10/2026 01:20:15] 80% - CARGANDO (faltan 20 min)
[06/10/2026 01:20:15] 100% - CARGADA COMPLETA
```

### Formato alternativo (detallado)

```
--- evento ---
Fecha: 06/10/2026 01:20:15
Conectada: SI
Carga: 80 %
Tiempo de carga: 20 min
Tiempo de uso: 0 min
```

### Manejo de archivos

- La carpeta se crea automáticamente si no existe.
- La escritura es en modo **append**: crea el archivo si falta y agrega al final si ya existe.
- Un `lock` protege el archivo por si hay dos escrituras al mismo tiempo.

---

## 7. Manejo de excepciones

| Excepción | Capa | Cuándo se lanza | Cómo se trata |
|---|---|---|---|
| `CargaFueraDeRangoException` | Domain | Se intenta poner una carga fuera de 0 a 100 | La UI la captura, muestra el mensaje y el programa **sigue andando** |
| `BitacoraException` | DAL | Falla al escribir, leer o rotar (permisos, disco, ruta). Guarda la excepción original adentro | La batería la aísla por suscriptor, avisa por `ErrorNotificacion` y **sigue avisando a los demás** |
| `ArgumentNullException` | Domain / BLL / DAL | Se pasa `null` donde no corresponde | Se corta temprano y con un mensaje claro |

---

## 8. Correspondencia con los criterios de evaluación

| # | Criterio (2 puntos cada uno) | Dónde se cumple |
|---|---|---|
| 1 | **Patrón en cada cambio de estado** — suscripción/desuscripción, notificación automática al cambiar `Conectado`, relación 1-a-muchos | `Bateria.Suscribir`, `Desuscribir` y `Notificar`; el `set` de `Conectado`. Se ve en los pasos 1, 6, 7 y 8 |
| 2 | **Lógica de estado de batería** — cálculo de `TiempoCarga`, de `TiempoUso` y validación 0–100 % | `Bateria.RecalcularTiempos()` y `Bateria.ValidarCarga()`. Se ve en los pasos 1 a 5 y 9 |
| 3 | **Suscriptor Visual** — salida clara, actualización en tiempo real, visualización diferenciada carga vs. uso | `BateriaApp.UI/SuscriptorVisual.cs`: muestra el tiempo de carga cuando está enchufada y el de uso cuando no |
| 4 | **Suscriptor Bitácora** — formato personalizado, manejo de archivos, fecha y hora | `SuscriptorBitacora` (BLL) + `RepositorioBitacoraArchivo` (DAL) + los formateadores |
| 5 | **Calidad de código** — excepciones, arquitectura, comentarios y documentación | 4 capas con dependencias en un solo sentido, una excepción propia por capa, comentarios en todo el código y este documento |

---

## 9. Marco teórico

### Concepto de patrón de diseño

Un patrón de diseño es una **solución general y reutilizable** a un problema que aparece seguido al diseñar software. No es código terminado ni una biblioteca: describe cómo colaboran unas clases entre sí, y ya está probado en muchos proyectos. Cada patrón tiene un nombre, el problema que resuelve, la solución y sus consecuencias.

### Relación entre los patrones y el desarrollo de software

Los patrones dan un **vocabulario común** ("acá va un Observer") y aprovechan la experiencia de otros en vez de inventar algo nuevo en cada proyecto. Ayudan a que el código sea más fácil de mantener y de cambiar, y evitan errores ya conocidos. El costo es que agregan abstracción: si se aplica un patrón que no hace falta, el código se complica sin beneficio.

### Patrones respecto al nivel de abstracción

Se clasifican según el nivel en el que actúan. Los **patrones de diseño (GoF)** —como Observer, Strategy o Factory Method— trabajan entre clases y objetos dentro de una aplicación. Los **de arquitectura** —como capas o MVC— organizan el sistema completo. Los **de análisis** describen problemas del negocio. Esta solución usa los dos niveles: el patrón **arquitectural de capas** ordena todo el sistema (UI–BLL–DAL–Domain) y el patrón **Observer** resuelve, dentro del dominio, la comunicación entre la batería y sus suscriptores.

### Análisis y detección del problema

Primero hay que **detectar el problema**, no partir del patrón. En este ejercicio la señal fue clara: una clase que cambia de estado, varios objetos que quieren enterarse y la necesidad de no fijar de antemano cuántos ni cuáles son. Esa combinación —un sujeto, muchos interesados, acoplamiento a evitar— es justo la que resuelve Observer. Si el problema hubiera sido "elegir entre varios algoritmos", el patrón era Strategy; si hubiera sido "crear objetos sin fijar la clase", Factory Method.

### El producto y el proceso del desarrollo de software

El **producto** es lo que se entrega: el código, los datos y la documentación. El **proceso** son las actividades para producirlo: relevamiento, análisis, diseño, construcción, pruebas y mantenimiento. Un buen proceso no garantiza solo un buen producto, pero un producto que dura en el tiempo sale de un proceso ordenado. Los patrones ayudan a los dos: ordenan el proceso porque dan una guía de diseño ya probada, y mejoran el producto porque dejan una estructura más clara y más fácil de modificar.

### Gestión del riesgo

Gestionar el riesgo es **identificar** qué puede salir mal, **evaluar** qué tan probable e importante es, y **decidir** qué hacer para evitarlo. En esta solución se cubren varios riesgos:

- **Que cambien los requisitos** (por ejemplo, otro formato de bitácora): se resuelve con Strategy, que permite agregar un formato sin tocar el repositorio.
- **Acoplamiento excesivo**: se evita con la separación en capas y dependiendo de interfaces (`IRepositorioBitacora`, `IObservadorBateria`).
- **Que falle algo secundario** (no poder escribir la bitácora): se aísla el error por suscriptor, así el programa sigue funcionando y avisa del problema.
- **Datos inválidos**: se validan en el dominio, en un solo lugar, antes de que el dato circule.

---

## 10. Estructura del repositorio

```
Recuperatorio-PP3-Bateria/
├── BateriaApp.sln                       <- solución de Visual Studio
├── .gitignore
├── .gitattributes
├── README.md
├── Bitacoras/                           <- se genera al ejecutar
│   └── .gitkeep
│
├── BateriaApp.Domain/                   <- capa de dominio (sin dependencias)
│   ├── Bateria.cs                       <- el sujeto observable
│   ├── IObservadorBateria.cs            <- la interfaz del observador
│   ├── EstadoBateria.cs                 <- la foto del estado
│   └── Exceptions/
│       └── CargaFueraDeRangoException.cs
│
├── BateriaApp.DAL/                      <- capa de datos
│   ├── IRepositorioBitacora.cs
│   ├── RepositorioBitacoraArchivo.cs    <- archivo por día + rotación
│   ├── Excepciones/
│   │   └── BitacoraException.cs
│   └── Formato/
│       ├── IFormateadorRegistro.cs
│       ├── FormateadorTextoPlano.cs
│       └── FormateadorDetallado.cs
│
├── BateriaApp.BLL/                      <- capa de negocio
│   ├── ServicioBateria.cs
│   ├── SuscriptorBitacora.cs
│   └── FabricaBateria.cs
│
└── BateriaApp.UI/                       <- capa de presentación (el ejecutable)
    ├── Program.cs
    └── SuscriptorVisual.cs
```

---

## 11. Salida esperada

Fragmento de la demostración automática:

```
Paso 2: subo la carga al 80 %.

--- cambio de estado ---
Hora: 06/10/2026 01:20:15
Estado: CONECTADA - CARGANDO
Carga: 80 %
Tiempo de carga: 20 min
```

Y el resumen final:

```
=== Resumen ===
Avisos que recibio el visual: 7
Eventos guardados en la bitacora: 8
Suscriptos al final: 2
Bitacoras rotadas (mas de 30 dias): 0
```

Esos dos contadores son la mejor prueba de que el patrón funciona: el visual recibió **7** avisos y la bitácora guardó **8** eventos. La diferencia son los **2 eventos del paso 7**, que pasaron mientras el visual estaba desuscripto (el paso 6, la desuscripción, no genera evento porque no cambia el estado).

---

## 12. Menú

Después de la demostración, el programa muestra este menú para probar a mano:

| Opción | Acción |
|---|---|
| 1 | Conectar el cargador |
| 2 | Desconectar el cargador |
| 3 | Cambiar la carga |
| 4 | Suscribir el visual |
| 5 | Desuscribir el visual |
| 6 | Forzar el aviso a todos |
| 7 | Ver la bitácora de hoy |
| 8 | Rotar bitácoras viejas |
| 0 | Salir |
