# Sanalia & Asociados — Sistema de Asesoría de Seguros

Sistema full stack para gestionar solicitudes de asesoría, comparar productos de seguros, registrar asesorías y emitir cotizaciones. El backend es una API REST en .NET 10 con SQLite; el frontend web es un portal para asesores y administradores en React + Vite; la carpeta `mobile` contiene un proyecto Expo que todavía no está integrado con la API.

## 1. Estado actual

| Módulo | Estado | Descripción |
|---|---|---|
| `backend` | Funcional | API REST, JWT, EF Core, SQLite, migraciones, datos demo y pruebas |
| `web` | Funcional | Portal de asesores: login, resumen, clientes, solicitudes, comparador y cotizaciones |
| `mobile` | Plantilla | Template de Expo; no consume la API todavía |

> El flujo completo (cliente → solicitud → asesoría → cotización) se prueba por API y por el portal web. La aceptación o rechazo de cotizaciones por parte del cliente se hace por API, porque el portal web es solo para personal. La gestión de asistencias también está disponible por API; el portal solo muestra el contador en el resumen.

## 2. Tecnologías

- **Backend:** .NET 10, ASP.NET Core, EF Core 10, SQLite, JWT Bearer, xUnit.
- **Web:** React 19, Vite 8, React Router 7, JavaScript y CSS.
- **Mobile:** Expo SDK 57, React Native 0.86 y TypeScript (plantilla).
- **Base de datos:** SQLite en archivo local.

## 3. Requisitos

- .NET SDK 10.x.
- Node.js 22+ (recomendado; mínimo 20.19).
- pnpm 10+ (o npm).
- Git.
- Opcional: DB Browser for SQLite o `sqlite3` para inspeccionar la base de datos.
- Opcional: Expo Go, emulador Android o simulador iOS para la app móvil.

Verifica las versiones:

```bash
dotnet --version
node --version
pnpm --version
```

Si no tienes pnpm:

```bash
corepack enable
corepack prepare pnpm@latest --activate
```

## 4. Estructura del proyecto

```text
Sistema_Asesoria/
├── backend/
│   ├── Sanalia.slnx
│   ├── dotnet-tools.json
│   ├── data/
│   │   └── sanalia.db              # Base local generada; no versionada
│   ├── Sanalia.Api/                # Capa HTTP
│   │   ├── controllers/
│   │   ├── Contracts/
│   │   ├── Mappings/
│   │   ├── Middleware/
│   │   ├── Security/
│   │   ├── Properties/
│   │   ├── Program.cs
│   │   ├── Sanalia.Api.csproj
│   │   ├── Sanalia.Api.http
│   │   └── appsettings*.json
│   ├── Sanalia.Core/               # Dominio
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   │   └── IServices/
│   │   ├── Models/
│   │   └── Sanalia.Core.csproj
│   ├── Sanalia.Infrastructure/     # Persistencia y servicios
│   │   ├── Configuration/
│   │   ├── Data/
│   │   │   ├── Context/
│   │   │   ├── Security/
│   │   │   └── Seeding/
│   │   ├── Migrations/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── Sanalia.Infrastructure.csproj
│   └── Sanalia.Tests/              # Pruebas de integración
│       ├── ApiFactory.cs
│       ├── ApiIntegrationTests.cs
│       └── Sanalia.Tests.csproj
├── web/                            # Portal de asesores
│   ├── public/
│   ├── src/
│   │   ├── components/
│   │   ├── context/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── App.jsx
│   │   ├── main.jsx
│   │   ├── App.css
│   │   └── index.css
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   └── eslint.config.js
├── mobile/                         # Plantilla Expo
│   ├── src/
│   │   ├── app/
│   │   ├── components/
│   │   ├── constants/
│   │   └── hooks/
│   ├── assets/
│   ├── app.json
│   ├── package.json
│   └── README.md
└── .gitignore
```

### Capas del backend

- **`Sanalia.Core`:** entidades, enums, interfaces de repositorios y servicios, y modelos de identidad. No depende de EF Core.
- **`Sanalia.Infrastructure`:** implementación de persistencia (DbContext, configuraciones, migraciones y repositorios), servicios de negocio, hasher de contraseñas y datos demo.
- **`Sanalia.Api`:** controladores HTTP, contratos (DTOs), mappings, middleware de errores, JWT y configuración de arranque.
- **`Sanalia.Tests`:** pruebas de integración con `WebApplicationFactory` y SQLite en memoria.

Dependencias principales: `Api → Infrastructure → Core`; `Tests → Api`.

### Web

- `src/pages`: pantallas del portal (`LoginPage`, `DashboardPage`, `CustomersPage`, `EnquiriesPage`, `ComparatorPage`, `ContributionsPage`).
- `src/components`: layout y componentes de interfaz reutilizables.
- `src/context`: sesión y autenticación.
- `src/services/api.js`: cliente HTTP y servicios por dominio.
- `vite.config.js`: proxy de `/api` hacia el backend.

### Base de datos

Tablas principales, con nombres en `snake_case`:

`asesor`, `cliente`, `tipo_seguro`, `aseguradora`, `producto_seguro`, `cobertura`, `horario_disponible`, `solicitud_asesoria`, `solicitud_asistencia`, `asesoria`, `cotizacion`, `detalle_cotizacion`.

## 5. Flujo funcional

1. El cliente se registra o inicia sesión (`/api/auth/register`, `/api/auth/login/cliente`).
2. El cliente crea una solicitud de asesoría indicando tipo de seguro, horario disponible y presupuesto.
3. El asesor ve las solicitudes pendientes u operativas y confirma la cita.
4. El comparador calcula alternativas entre los productos vigentes del tipo solicitado. El puntaje (0–100) combina prima frente a presupuesto, nivel de cobertura, deducible, beneficios adicionales y límites de cobertura; la mejor alternativa se marca como recomendada.
5. Cuando la cita ya terminó, el asesor asignado registra la asesoría.
6. El asesor crea una cotización en borrador, la marca como presentada y el cliente la acepta o rechaza.
7. Las solicitudes de asistencia se crean por el cliente y se asignan o resuelven por el personal.

## 6. Configuración inicial del backend

### 6.1 Restaurar dependencias

```bash
cd backend
dotnet restore
```

### 6.2 Configurar User Secrets (obligatorio)

`appsettings.json` no contiene secretos. La API valida al arrancar que existan las claves `Jwt:*`, y el seed necesita `DemoUsers:*`. Configura estos valores con User Secrets:

```bash
cd backend

dotnet user-secrets set "Jwt:Issuer" "Sanalia.Api" --project Sanalia.Api
dotnet user-secrets set "Jwt:Audience" "Sanalia.Client" --project Sanalia.Api
dotnet user-secrets set "Jwt:SigningKey" "una-clave-local-de-al-menos-32-bytes-para-desarrollo" --project Sanalia.Api
dotnet user-secrets set "Jwt:LifetimeMinutes" "60" --project Sanalia.Api

dotnet user-secrets set "DemoUsers:AdminPassword" "AdminDemo123!" --project Sanalia.Api
dotnet user-secrets set "DemoUsers:AdvisorPassword" "AsesorDemo123!" --project Sanalia.Api
dotnet user-secrets set "DemoUsers:ClientPassword" "ClienteDemo123!" --project Sanalia.Api
```

Notas:

- `Jwt:SigningKey` debe tener al menos 32 bytes UTF-8. Cambia la clave en producción.
- `Jwt:LifetimeMinutes` debe estar entre 1 y 1440.
- No subas estos valores al repositorio. User Secrets los guarda fuera del proyecto.
- Si prefieres variables de entorno, usa `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SigningKey`, `Jwt__LifetimeMinutes`, `DemoUsers__AdminPassword`, etc.
- Puedes cambiar las contraseñas demo por las que quieras.

Verificar los secretos configurados:

```bash
dotnet user-secrets list --project Sanalia.Api
```

> Esa salida muestra los secretos en texto plano; no la compartas.

### 6.3 Herramienta de EF Core (opcional)

Solo si vas a ejecutar comandos `dotnet ef`:

```bash
cd backend
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet ef --version
```

## 7. Cómo obtener la base de datos (`backend/data/sanalia.db`)

El archivo `backend/data/sanalia.db` **no está versionado**: `.gitignore` ignora `backend/data/` y `*.db`. Se genera localmente.

### Opción A (recomendada): arrancar la API en Development

```bash
cd backend
dotnet run --project Sanalia.Api --launch-profile http
```

Al iniciar en Development, `Program.cs` ejecuta `DbInitializer()`:

1. Aplica las migraciones pendientes.
2. Activa el modo WAL de SQLite.
3. Ejecuta el seed si las tablas están vacías.

Resultado: se crea `backend/data/sanalia.db` con los datos demo. La API queda en `http://localhost:5099`.

### Opción B: crear solo el esquema con EF Core

```bash
cd backend
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet ef database update --project Sanalia.Infrastructure --startup-project Sanalia.Api
```

Esto crea las tablas, pero **no** carga los datos demo. Para cargarlos, arranca la API en Development una vez usando la Opción A.

### Opción C: copiar una base existente

Puedes copiar `sanalia.db` de otro equipo, pero las contraseñas están hasheadas con los secretos de ese equipo. Si no conoces las contraseñas, borra la base y vuelve a generarla con tus propios `DemoUsers:*`.

### Ruta personalizada

En tiempo de ejecución:

```bash
ConnectionStrings__Default="Data Source=/ruta/a/sanalia.db" dotnet run --project Sanalia.Api
```

Para `dotnet ef`:

```bash
SANALIA_CONNECTION="Data Source=/ruta/a/sanalia.db" dotnet ef database update --project Sanalia.Infrastructure --startup-project Sanalia.Api
```

### Reiniciar la base de datos

Detén la API y elimina el archivo junto con sus auxiliares WAL:

```bash
rm -f backend/data/sanalia.db backend/data/sanalia.db-wal backend/data/sanalia.db-shm
```

Luego arranca la API de nuevo. El seed solo se ejecuta si las tablas están vacías; por eso, si cambias las contraseñas demo, debes borrar la base.

### Inspeccionar la base de datos

Con DB Browser for SQLite: abre `backend/data/sanalia.db`.

Con `sqlite3`:

```bash
sqlite3 backend/data/sanalia.db ".tables"
sqlite3 backend/data/sanalia.db "SELECT id, correo, rol FROM asesor;"
sqlite3 backend/data/sanalia.db "SELECT id, correo, nombre FROM cliente;"
```

### Datos demo que crea el seed

| Dato | Cantidad |
|---|---:|
| Asesores | 2 |
| Clientes | 16 |
| Tipos de seguro | 4 |
| Aseguradoras | 4 |
| Productos | 16 |
| Coberturas | 43 |
| Horarios | 104 (75 disponibles) |
| Solicitudes de asesoría | 15 (6 pendientes, 3 confirmadas, 1 cancelada, 5 atendidas) |
| Solicitudes de asistencia | 8 (4 pendientes, 2 en proceso, 2 resueltas) |
| Asesorías realizadas | 5 |
| Cotizaciones | 5 (1 borrador, 2 presentadas, 1 aceptada, 1 rechazada) |

Los horarios futuros se generan para los próximos 12 días laborables, con 7 franjas por día. También se crean 20 franjas pasadas para las asesorías ya realizadas. Las solicitudes demo pertenecen a 16 clientes e incluyen todos los estados del flujo, de modo que el portal tenga actividad desde el primer arranque.

## 8. Ejecutar el backend

```bash
cd backend
dotnet run --project Sanalia.Api --launch-profile http
```

- URL base: `http://localhost:5099`
- OpenAPI: `http://localhost:5099/openapi/v1.json`
- No hay Swagger UI; usa el JSON de OpenAPI o el archivo `Sanalia.Api/Sanalia.Api.http`.
- Para detener: `Ctrl+C`.

Si el puerto 5099 está ocupado:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Sanalia.Api --no-launch-profile --urls http://localhost:5100
```

Luego ajusta el proxy del web.

## 9. Ejecutar el frontend web

```bash
cd web
pnpm install
pnpm dev
```

Abre `http://localhost:5173`.

El proxy de Vite envía `/api` a `http://localhost:5099` por defecto. Si el backend usa otro puerto, crea `web/.env.local`:

```bash
VITE_API_PROXY_TARGET=http://localhost:5100
```

También puedes definir `VITE_API_BASE_URL` si quieres llamar a la API directamente, pero ten en cuenta que la API no configura CORS; para desarrollo local es preferible usar el proxy.

Otros comandos:

```bash
pnpm build      # compila a web/dist
pnpm preview    # sirve la build
pnpm lint       # ESLint
```

## 10. Probar el portal web

### 10.1 Credenciales demo

El seed crea estos usuarios:

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@sanalia.local` | `DemoUsers:AdminPassword` |
| Asesor | `asesor@sanalia.local` | `DemoUsers:AdvisorPassword` |
| Cliente | `cliente1@demo.local` | `DemoUsers:ClientPassword` |
| Cliente | `cliente2@demo.local` | `DemoUsers:ClientPassword` |

El portal web solo permite personal (`/api/auth/login/asesor`). Si intentas entrar con un cliente, verás “Esta cuenta no tiene acceso al espacio de asesores”. Los clientes se usan por API o en una futura app móvil.

Si ya configuraste los secretos y no recuerdas las contraseñas:

```bash
cd backend
dotnet user-secrets list --project Sanalia.Api
```

### 10.2 Recorrido recomendado

1. Arranca backend y web.
2. Entra a `http://localhost:5173/login`.
3. Inicia sesión con `asesor@sanalia.local` y tu `DemoUsers:AdvisorPassword`.
4. **Resumen** (`/app`): verás 16 clientes, 6 solicitudes pendientes y 6 asistencias abiertas.
5. **Clientes** (`/app/clientes`): aparecen 16 clientes con datos variados; prueba la búsqueda.
6. **Solicitudes** (`/app/solicitudes`): hay 9 solicitudes operativas (6 pendientes y 3 confirmadas). Abre una pendiente y confírmala.
7. **Comparador** (`/app/comparador`): selecciona una solicitud y verás 4 alternativas con su puntaje y la recomendada.
8. **Aportes** (`/app/aportes`): ya existen 5 asesorías y un historial de 5 cotizaciones en distintos estados; también puedes crear una nueva propuesta.

> Para ver la solicitud demo usa `asesor@sanalia.local`. El administrador también la ve, pero no puede registrar la asesoría de otro asesor: el servicio exige que el asesor autenticado sea el asignado al horario.

### 10.3 Completar el flujo completo (demo inmediata)

La solicitud demo tiene una cita en el futuro. El backend no permite registrar la asesoría hasta que la cita haya terminado. Para probar todo de inmediato, adelanta la fecha del horario reservado en la base:

1. Detén la API.
2. Ejecuta con `sqlite3`:

```bash
sqlite3 backend/data/sanalia.db "UPDATE horario_disponible SET fecha = date('now','-1 day') WHERE disponible = 0;"
```

Si no tienes `sqlite3`, usa DB Browser for SQLite o Python:

```bash
python3 - <<'PY'
import sqlite3
con = sqlite3.connect('backend/data/sanalia.db')
con.execute("UPDATE horario_disponible SET fecha = date('now','-1 day') WHERE disponible = 0")
con.commit()
con.close()
PY
```

3. Arranca la API de nuevo.
4. En el portal, entra como `asesor@sanalia.local`.
5. En **Solicitudes**, abre la solicitud, pulsa **Confirmar solicitud** y luego **Registrar asesoría realizada**. Si todo está bien, te llevará al comparador.
6. En el **Comparador**, selecciona alternativas y pulsa **Preparar cotización**.
7. En **Aportes**, ingresa las primas, guarda el borrador y pulsa **Marcar presentada**.
8. Para aceptar la cotización como cliente, usa la API:

```bash
# 1) Login del cliente
curl -s -X POST http://localhost:5099/api/auth/login/cliente \
  -H "Content-Type: application/json" \
  -d '{"correo":"cliente1@demo.local","contraseña":"ClienteDemo123!"}'

# 2) Copia el accessToken y consulta la cotización
curl -s http://localhost:5099/api/cotizaciones/1 \
  -H "Authorization: Bearer <TOKEN_CLIENTE>"

# 3) Acéptala
curl -s -X PATCH http://localhost:5099/api/cotizaciones/1/estado \
  -H "Authorization: Bearer <TOKEN_CLIENTE>" \
  -H "Content-Type: application/json" \
  -d '{"estado":"Aceptada"}'
```

Ajusta el id de la cotización si no es `1` y usa la contraseña que hayas configurado.

> Si no quieres modificar la base, la prueba automatizada ya recorre gran parte de este flujo, incluido el cambio de fecha del horario, y es la forma más rápida de verificar que todo funciona.

## 11. Pruebas automatizadas

El proyecto usa xUnit. La prueba está en `backend/Sanalia.Tests/ApiIntegrationTests.cs` y usa `ApiFactory` para levantar la API en memoria.

```bash
cd backend
dotnet test Sanalia.Tests/Sanalia.Tests.csproj
```

Resultado esperado:

```text
Passed! - Failed: 0, Passed: 1, Skipped: 0, Total: 1
```

Qué cubre la prueba:

- Registro de cliente y login.
- `/api/auth/me`.
- Restricciones por rol (cliente frente a asesor/administrador).
- Catálogo de tipos, aseguradoras y productos.
- Creación de horarios y solicitudes.
- Comparador de alternativas y producto recomendado.
- Confirmación de solicitud y registro de asesoría, incluidas las reglas de fecha y asignación.
- Creación y presentación de cotizaciones.
- Creación de una solicitud de asistencia.
- Validaciones y errores en formato `application/problem+json` (400, 401, 403, 404 y 409).

Detalles importantes:

- Usa SQLite en memoria y `EnsureCreated`; **no** toca `backend/data/sanalia.db`.
- Usa el entorno `Testing`, por lo que **no** necesita tus User Secrets.
- No requiere que el backend esté corriendo.
- Hay 1 prueba de integración que recorre un flujo completo.
- No cubre la aceptación o rechazo de cotizaciones por el cliente; eso se prueba manualmente por API.

Comandos útiles:

```bash
# Ejecutar solo esta clase
dotnet test Sanalia.Tests/Sanalia.Tests.csproj --filter FullyQualifiedName~ApiIntegrationTests

# Ver más detalle
dotnet test Sanalia.Tests/Sanalia.Tests.csproj --logger "console;verbosity=detailed"

# Ejecutar toda la solución
dotnet test
```

## 12. Probar la API manualmente

### Archivo `.http`

Abre `backend/Sanalia.Api/Sanalia.Api.http` con la extensión REST Client de VS Code, JetBrains HTTP Client o Visual Studio. Reemplaza los placeholders `@clientToken`, `@advisorToken`, etc.

### OpenAPI

`http://localhost:5099/openapi/v1.json`

Puedes importarlo en Postman o Insomnia, o visualizarlo con cualquier cliente OpenAPI.

### Ejemplo rápido con curl

```bash
# Catálogo público
curl http://localhost:5099/api/catalog/tipos-seguro

# Login asesor
curl -X POST http://localhost:5099/api/auth/login/asesor \
  -H "Content-Type: application/json" \
  -d '{"correo":"asesor@sanalia.local","contraseña":"AsesorDemo123!"}'
```

### Convenciones de la API

- El JSON usa `camelCase`.
- Los enums se envían como texto: `Pendiente`, `Confirmada`, `Atendida`, `Cancelada`, `Borrador`, `Presentada`, `Aceptada`, `Rechazada`. No uses números.
- Los errores se devuelven como `application/problem+json`.
- Los endpoints protegidos esperan `Authorization: Bearer <token>`.

## 13. Endpoints principales

`Personal` significa Asesor o Administrador. `Autenticado` significa cualquier JWT válido, con validaciones adicionales de propiedad del recurso.

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/api/auth/register` | Público | Registra un cliente y devuelve JWT |
| POST | `/api/auth/login/cliente` | Público | Login de cliente |
| POST | `/api/auth/login/asesor` | Público | Login de asesor o administrador |
| GET | `/api/auth/me` | Autenticado | Identidad actual |
| GET | `/api/catalog/tipos-seguro` | Público | Lista tipos de seguro |
| GET | `/api/catalog/aseguradoras` | Público | Lista aseguradoras |
| GET | `/api/catalog/tipos-seguro/{tipoId}/productos` | Público | Productos vigentes por tipo |
| GET | `/api/catalog/productos/{id}` | Público | Producto con coberturas |
| GET | `/api/horarios/disponibles` | Público | Franjas disponibles (`desde`, `asesorId`) |
| POST | `/api/horarios` | Personal | Publica una franja de disponibilidad |
| POST | `/api/solicitudes-asesoria` | Cliente | Crea una solicitud de asesoría |
| GET | `/api/solicitudes-asesoria/mias` | Cliente | Solicitudes del cliente |
| GET | `/api/solicitudes-asesoria/{id}` | Autenticado | Detalle de una solicitud |
| GET | `/api/solicitudes-asesoria/pendientes` | Personal | Solicitudes pendientes |
| GET | `/api/solicitudes-asesoria/operativas` | Personal | Solicitudes pendientes o confirmadas |
| GET | `/api/solicitudes-asesoria/{id}/alternativas` | Cliente | Alternativas para el cliente |
| GET | `/api/solicitudes-asesoria/{id}/comparador` | Personal | Comparador para el asesor |
| PATCH | `/api/solicitudes-asesoria/{id}/estado` | Autenticado | Cambia estado; el cliente solo puede cancelar |
| POST | `/api/asesorias` | Personal | Registra una asesoría realizada |
| GET | `/api/asesorias/personal` | Personal | Asesorías del personal |
| GET | `/api/asesorias/solicitud/{solicitudId}` | Personal | Asesoría de una solicitud |
| GET | `/api/asesorias/mias` | Cliente | Historial del cliente con cotizaciones |
| GET | `/api/cotizaciones/mias` | Personal | Cotizaciones del personal |
| POST | `/api/asesorias/{asesoriaId}/cotizaciones` | Personal | Crea una cotización |
| GET | `/api/cotizaciones/{id}` | Autenticado | Detalle de una cotización |
| PATCH | `/api/cotizaciones/{id}/estado` | Autenticado | Presenta, acepta o rechaza una cotización |
| POST | `/api/solicitudes-asistencia` | Cliente | Crea una solicitud de asistencia |
| GET | `/api/solicitudes-asistencia/mias` | Cliente | Asistencias del cliente |
| GET | `/api/solicitudes-asistencia/abiertas` | Personal | Asistencias abiertas |
| POST | `/api/solicitudes-asistencia/{id}/asignacion` | Personal | Asigna la asistencia al asesor autenticado |
| PATCH | `/api/solicitudes-asistencia/{id}/estado` | Personal | Cambia el estado de la asistencia |
| GET | `/api/clientes?busqueda=...` | Personal | Directorio de clientes |

> Aunque `POST /api/asesorias` está disponible para el rol Personal, el servicio exige que el asesor autenticado sea el asignado al horario. Un administrador no puede registrar la asesoría de otro asesor.

## 14. App móvil (Expo)

```bash
cd mobile
pnpm install
pnpm start
```

Atajos:

```bash
pnpm web       # abre en el navegador
pnpm android   # emulador o dispositivo Android
pnpm ios       # simulador iOS (macOS)
pnpm lint      # lint
```

Estado actual: es el template de Expo (pantalla “Welcome to Expo”), no consume la API ni tiene login, solicitudes o comparador. Para integrarla habría que agregar el cliente HTTP y las pantallas. No la uses todavía para validar el flujo del sistema.

## 15. Solución de problemas

| Problema | Causa | Solución |
|---|---|---|
| `JWT configuration requires...` | Faltan las claves `Jwt:*` | Configura los 4 valores con User Secrets y reinicia |
| `DemoUsers:AdminPassword must be configured...` | Faltan las contraseñas demo | Configura `DemoUsers:*` |
| `No hay migraciones...` | No se restauró el proyecto o faltan migraciones | Ejecuta `dotnet restore` y verifica `Sanalia.Infrastructure/Migrations` |
| `dotnet ef` no se reconoce | Falta restaurar herramientas | `dotnet tool restore --tool-manifest dotnet-tools.json` |
| La web muestra “No fue posible conectar...” | Backend apagado o puerto incorrecto | Arranca la API en 5099 o ajusta `VITE_API_PROXY_TARGET` |
| El login web rechaza la cuenta | Es un cliente, no personal | Usa `admin@sanalia.local` o `asesor@sanalia.local` |
| “No se puede registrar una asesoría antes de finalizar la cita” | La cita demo es futura | Espera la fecha o adelanta el horario en la base (sección 10.3) |
| “Solo el asesor asignado...” | Iniciaste como administrador u otro asesor | Usa `asesor@sanalia.local` para la solicitud demo |
| Cambié las contraseñas demo y no funcionan | El seed no actualiza tablas existentes | Borra `backend/data/sanalia.db*` y reinicia |
| Error 401 en la web | Token vencido o sesión inválida | Vuelve a iniciar sesión |
| Error 403 en la web | Rol sin permiso | Usa una cuenta de personal |
| `database is locked` | Otra instancia de la API o un cliente SQLite | Cierra la otra instancia |
| El puerto 5099 está ocupado | Otro proceso | Usa `--urls http://localhost:5100` y ajusta el proxy |
| CORS al llamar directo a la API | La API no configura CORS | Usa el proxy de Vite o agrega CORS en `Program.cs` |

## 16. Comandos rápidos

```bash
# Backend
cd backend
dotnet restore
dotnet user-secrets list --project Sanalia.Api
dotnet run --project Sanalia.Api --launch-profile http
dotnet test Sanalia.Tests/Sanalia.Tests.csproj

# Web
cd web
pnpm install
pnpm dev
pnpm build
pnpm lint

# Mobile
cd mobile
pnpm install
pnpm start
```

## 17. Notas de seguridad

- No se versionan `appsettings.Production.json`, `.env` ni `backend/data/`.
- Las contraseñas se almacenan con PBKDF2-SHA256, 210 000 iteraciones y salt aleatorio.
- Los JWT usan HMAC-SHA256 y expiran según `Jwt:LifetimeMinutes`.
- La sesión del portal web se guarda en `sessionStorage`; al cerrar la pestaña tendrás que iniciar sesión de nuevo.
- Para producción: usa una clave de firma robusta, configura HTTPS, define una cadena de conexión y no ejecutes el seed de desarrollo.
