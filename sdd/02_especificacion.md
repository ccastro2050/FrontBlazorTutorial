# Etapa 2: Especificación

> Según [Spec-Kit de GitHub](https://github.com/github/spec-kit): la especificación
> (`/speckit.specify`) documenta QUÉ construir y POR QUÉ, sin enfocarse en tecnología.
> Se genera `spec.md` con historias de usuario y requisitos funcionales.
> "Se comienza con una idea, a menudo vaga, que evoluciona hacia un documento
> de requisitos comprensivo."
>
> Referencia: [spec-driven.md](https://github.com/github/spec-kit/blob/main/spec-driven.md)

---

## 1. Problema que resuelve

Los estudiantes de Diseño de Software necesitan aprender a construir un frontend web completo que consuma una API REST, con autenticación, control de acceso, CRUD de tablas, y buenas prácticas de desarrollo colaborativo. No existe un tutorial paso a paso que:

- Funcione con cualquier base de datos (no hardcodee nombres de columnas)
- Enseñe seguridad real (BCrypt + JWT + sesión encriptada con ProtectedSessionStorage)
- Permita trabajo colaborativo con Git (3 estudiantes, ramas, merges)
- Sea replicable en otros frameworks (Flask, Java, PHP, React)

## 2. Para quién va dirigido

| Audiencia | Nivel | Qué saben | Qué no saben |
|-----------|-------|-----------|--------------|
| Estudiantes universitarios | Intermedio | C# básico, HTML, BD relacional | Blazor Server, Razor, APIs REST, JWT, BCrypt |
| Profesor | Avanzado | Todo lo anterior + la API genérica | N/A (es quien evalúa) |

## 3. Qué se construye

Un **frontend web completo** en Blazor Server (.NET 9.0) que:

### 3.1 Funcionalidades principales

| # | Funcionalidad | Descripción | Tablas involucradas |
|---|---------------|-------------|---------------------|
| 1 | CRUD genérico | Listar, crear, editar, eliminar registros de cualquier tabla | producto, persona, empresa, cliente, vendedor, usuario, rol, ruta |
| 2 | Login | Autenticación con email + contraseña (BCrypt vía API) | usuario |
| 3 | Control de acceso | Roles y rutas permitidas por rol, verificación en cada navegación | rol, rol_usuario, ruta, rutarol |
| 4 | Cambiar contraseña | Con validación (6 chars, mayúscula, número) | usuario |
| 5 | Recuperar contraseña | Genera temporal, envía por SMTP, fuerza cambio | usuario |
| 6 | Navegación | Sidebar con NavMenu, layout base Bootstrap 5 | N/A |
| 7 | Factura maestro-detalle | Formulario con cabecera + líneas de detalle dinámicas | factura, productosporfactura |
| 8 | Descubrimiento dinámico | PKs y FKs se descubren de la API, no se hardcodean | Todas |

### 3.2 Modelo de datos (tablas de la BD)

```
┌──────────┐     ┌──────────────┐     ┌──────┐     ┌──────────┐     ┌──────┐
│ usuario  │──<──│ rol_usuario  │──>──│ rol  │──<──│ rutarol  │──>──│ ruta │
│ email PK │     │ fkemail      │     │ id   │     │ fkidrol  │     │ id   │
│ contrase │     │ fkidrol      │     │ nomb │     │ fkidruta │     │ ruta │
└──────────┘     └──────────────┘     └──────┘     └──────────┘     └──────┘

┌──────────┐     ┌──────────┐     ┌──────────┐
│ persona  │──<──│ cliente  │──>──│ empresa  │
│ codigo   │     │ fkcodper │     │ codigo   │
└──────────┘     │ fkcodemp │     └──────────┘
                 └──────────┘

┌──────────┐     ┌──────────┐     ┌──────────────────┐
│ vendedor │     │ factura  │──<──│productosporfactura│
│ codigo   │     │ numfact  │     │ fknumfact         │
└──────────┘     │ fkcodven │     │ fkcodprod         │
                 │ fkcodcli │     └──────────────────┘
                 └──────────┘

┌──────────┐
│ producto │
│ codigo   │
│ precio   │
└──────────┘
```

### 3.3 Modelo Entidad-Relación (ER) detallado

> El modelo ER define las entidades (tablas), sus atributos (columnas),
> las relaciones entre ellas (FKs) y las restricciones (PKs, NOT NULL, UNIQUE).
> Es la base para el diseño de la BD.

#### Normalización aplicada

| Forma Normal | Qué exige | Cumple? | Ejemplo |
|-------------|-----------|---------|---------|
| **1FN** | Valores atómicos, sin grupos repetidos | Sí | Cada columna tiene un solo valor, no hay arrays |
| **2FN** | Todo atributo depende de TODA la PK | Sí | En `productosporfactura`, cantidad depende de (fknumfact + fkcodprod), no solo de uno |
| **3FN** | No hay dependencias transitivas | Sí | En `cliente`, el nombre de la persona NO se duplica — se accede vía FK a `persona` |

#### Tabla de entidades y atributos

**Entidades de negocio:**

| Entidad | PK | Atributos | Tipo | NOT NULL | Descripción |
|---------|-----|-----------|------|----------|-------------|
| **producto** | codigo (varchar) | nombre | varchar | sí | Nombre del producto |
| | | precio | decimal | sí | Precio unitario |
| | | existencia | integer | no | Cantidad en stock |
| **persona** | codigo (varchar) | nombre | varchar | sí | Nombre completo |
| | | telefono | varchar | no | Teléfono de contacto |
| | | direccion | varchar | no | Dirección física |
| **empresa** | codigo (varchar) | nombre | varchar | sí | Razón social |
| | | nit | varchar | no | Número tributario |
| **cliente** | id (serial) | credito | decimal | sí | Límite de crédito |
| | | fkcodpersona | varchar FK | sí | -> persona.codigo |
| | | fkcodempresa | varchar FK | no | -> empresa.codigo |
| **vendedor** | codigo (varchar) | nombre | varchar | sí | Nombre del vendedor |
| | | comision | decimal | no | Porcentaje comisión |
| **factura** | numfactura (serial) | fecha | timestamp | sí | Fecha de emisión |
| | | total | decimal | sí | Total de la factura |
| | | fkcodvendedor | varchar FK | sí | -> vendedor.codigo |
| | | fkcodcliente | integer FK | sí | -> cliente.id |
| **productosporfactura** | id (serial) | cantidad | integer | sí | Unidades vendidas |
| | | precio | decimal | sí | Precio al momento de la venta |
| | | fknumfact | integer FK | sí | -> factura.numfactura |
| | | fkcodprod | varchar FK | sí | -> producto.codigo |

**Entidades de seguridad:**

| Entidad | PK | Atributos | Tipo | NOT NULL | Descripción |
|---------|-----|-----------|------|----------|-------------|
| **usuario** | email (varchar) | contrasena | varchar | sí | Hash BCrypt (irreversible) |
| | | nombre | varchar | no | Nombre para mostrar |
| | | debe_cambiar_contrasena | boolean | no | Forzar cambio en próximo login |
| **rol** | id (serial) | nombre | varchar | sí | Nombre del rol (Administrador, etc) |
| **rol_usuario** | id (serial) | fkemail | varchar FK | sí | -> usuario.email |
| | | fkidrol | integer FK | sí | -> rol.id |
| **ruta** | id (serial) | ruta | varchar | sí | Path de la página (/producto) |
| | | descripcion | text | no | Descripción de la página |
| **rutarol** | id (serial) | fkidrol | integer FK | sí | -> rol.id |
| | | fkidruta | integer FK | sí | -> ruta.id |

#### Cardinalidad de las relaciones

| Relación | Tipo | Lectura | Tabla intermedia |
|----------|------|---------|-----------------|
| persona <-> cliente | 1:N | Una persona puede ser 0 o N clientes | No (FK directo) |
| empresa <-> cliente | 1:N | Una empresa puede tener 0 o N clientes | No (FK directo) |
| vendedor <-> factura | 1:N | Un vendedor tiene 0 o N facturas | No (FK directo) |
| cliente <-> factura | 1:N | Un cliente tiene 0 o N facturas | No (FK directo) |
| factura <-> producto | N:M | Una factura tiene N productos, un producto aparece en M facturas | Sí: `productosporfactura` |
| usuario <-> rol | N:M | Un usuario tiene N roles, un rol tiene N usuarios | Sí: `rol_usuario` |
| rol <-> ruta | N:M | Un rol accede a N rutas, una ruta la acceden N roles | Sí: `rutarol` |

#### Integridad referencial

```
ON DELETE: Todas las FKs usan NO ACTION (no se puede borrar un registro
           que tenga hijos). La API devuelve error si se intenta.

ON UPDATE: NO ACTION. Si se cambia una PK, los FKs no se actualizan
           automáticamente (se debe actualizar manualmente).

Consecuencia práctica:
  - No se puede borrar un vendedor que tenga facturas
  - No se puede borrar un rol que tenga usuarios asignados
  - No se puede borrar una ruta que esté asignada a un rol
```

### 3.4 Flujos de usuario

#### Flujo 1: Login

```
Usuario abre app -> MainLayout.OnAfterRenderAsync -> _auth.Restaurar()
-> No hay sesión -> NavigateTo("/login")
-> Escribe email + contraseña -> _auth.Login(email, contraseña)
-> PrecargarEstructura() descubre PKs/FKs
-> PostJson("autenticacion/token") -> API verifica BCrypt -> Devuelve JWT
-> CargarDatosRolesYRutas() -> 1 SQL vía ConsultasController
-> Guardar en ProtectedSessionStorage -> NavigateTo("/")
```

#### Flujo 2: CRUD (listar, crear, editar, eliminar)

```
Usuario navega a /producto -> MainLayout.LocationChanged -> VerificarAcceso()
-> TieneAcceso("/producto")? Sí -> Página Producto.razor se renderiza
-> ApiService.ListarAsync("producto") -> AgregarTokenJwt() -> GET con Bearer
-> API devuelve datos -> Razor renderiza tabla HTML
-> Usuario llena formulario -> ApiService.CrearAsync("producto", datos)
-> POST con JWT -> API inserta en BD
-> Mensaje "Registro creado" -> Actualizar tabla
```

#### Flujo 3: Factura maestro-detalle

```
Usuario navega a /factura -> Lista facturas existentes
-> Clic "Nueva factura" -> Formulario con:
   - Cabecera: vendedor (select FK), cliente (select FK), fecha
   - Detalle: tabla dinámica con producto (select FK), cantidad, precio
   - Botones agregar/eliminar filas de detalle
-> Crear cabecera + detalles en la API
```

#### Flujo 4: Acceso denegado

```
Usuario navega a /factura -> MainLayout.LocationChanged
-> VerificarAcceso() -> TieneAcceso("/factura")? No
-> NavigateTo("/sin-acceso") -> Muestra página 403
```

## 4. Qué NO se construye (exclusiones explícitas)

| Excluido | Razón |
|----------|-------|
| API REST | Ya existe (ApiGenericaCsharp) |
| Base de datos | Ya existe (PostgreSQL) |
| Registro de usuarios | Los crea el admin vía CRUD de usuario |
| Panel de administración avanzado | Fuera del alcance del tutorial |
| Internacionalización (i18n) | Complejidad innecesaria para el tutorial |
| Tests automatizados | Se prueban manualmente (es un tutorial) |
| Deploy a producción | Se ejecuta localmente (`dotnet run`) |
| Notificaciones en tiempo real | Fuera del alcance |
| Blazor WebAssembly (WASM) | Se usa Blazor Server por simplicidad |
| Entity Framework | Se consume API REST, no acceso directo a BD |

## 5. Criterios de aceptación

### Para cada CRUD

- [ ] Listar registros en tabla HTML con todos los campos
- [ ] Crear registro con formulario (tipos HTML correctos por tipo de dato)
- [ ] Editar registro (formulario prellenado con datos actuales)
- [ ] Eliminar registro con confirmación
- [ ] Selects para campos FK (cargados desde la API)
- [ ] Mensajes de éxito/error después de cada operación

### Para login y seguridad

- [ ] Login con email + contraseña verificada con BCrypt
- [ ] JWT capturado y enviado en cada petición a la API (AgregarTokenJwt)
- [ ] Roles cargados desde la BD (no hardcodeados)
- [ ] Rutas permitidas verificadas en CADA navegación (LocationChanged + VerificarAcceso)
- [ ] Usuario sin roles -> rechazado con mensaje claro
- [ ] Ruta no permitida -> página 403 "Sin Acceso"
- [ ] Cambiar contraseña con validación (6 chars, mayúscula, número)
- [ ] Recuperar contraseña con email SMTP
- [ ] Sesión persiste al refrescar (F5) con ProtectedSessionStorage, se pierde al cerrar tab

### Para trabajo colaborativo

- [ ] Cada estudiante trabaja en su rama feature/
- [ ] Merge a main sin conflictos (o resueltos en la rama)
- [ ] Cada paso tiene su commit descriptivo
- [ ] El proyecto arranca con `dotnet run` después de cada merge

## 6. Métricas de éxito

| Métrica | Valor esperado |
|---------|----------------|
| Tablas CRUD funcionando | 10+ |
| Tiempo de login (con ConsultasController) | < 2 segundos |
| Estudiantes que completan el tutorial | 3/3 |
| El proyecto compila y corre después de merge | Sí |
| Cada paso tiene documentación (.md) | Sí (Paso0 a Paso12) |
