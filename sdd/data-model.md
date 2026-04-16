# Modelo de Datos - FrontBlazorTutorial

> Según [Spec-Kit de GitHub](https://github.com/github/spec-kit), cada feature puede
> tener un archivo `data-model.md` dedicado con el esquema detallado de entidades.
> Este archivo contiene el SQL completo para crear todas las tablas del proyecto.
>
> Referencia: [estructura .specify/specs/{feature}/data-model.md](https://github.com/github/spec-kit)

---

## 1. Diagrama Entidad-Relación (ER)

```
                           SEGURIDAD
    ┌──────────┐     ┌──────────────┐     ┌──────┐     ┌──────────┐     ┌──────┐
    │ usuario  │──<──│ rol_usuario  │──>──│ rol  │──<──│ rutarol  │──>──│ ruta │
    │ email PK │     │ fkemail  FK  │     │ id PK│     │ fkidrol  │     │ id PK│
    │ contrase │     │ fkidrol  FK  │     │ nomb │     │ fkidruta │     │ ruta │
    │ nombre   │     └──────────────┘     └──────┘     └──────────┘     │ desc │
    │ debe_cam │           N:M                              N:M         └──────┘
    └──────────┘

                            NEGOCIO
    ┌──────────┐     ┌──────────┐     ┌──────────┐
    │ persona  │──<──│ cliente  │──>──│ empresa  │
    │ codigo PK│ 1:N │ id PK    │ N:1 │ codigo PK│
    │ nombre   │     │ credito  │     │ nombre   │
    │ telefono │     │ fkcodper │     │ nit      │
    │ direccion│     │ fkcodemp │     │ direccion│
    └──────────┘     └──────────┘     └──────────┘

    ┌──────────┐     ┌──────────────┐     ┌──────────────────┐     ┌──────────┐
    │ vendedor │──<──│   factura    │──<──│productosporfactur│──>──│ producto │
    │ codigo PK│ 1:N │ numfact PK  │ 1:N │ id PK            │ N:1 │ codigo PK│
    │ nombre   │     │ fecha       │     │ cantidad         │     │ nombre   │
    │ comision │     │ total       │     │ precio           │     │ precio   │
    └──────────┘     │ fkcodven FK │     │ fknumfact FK     │     │ existenc │
                     │ fkcodcli FK │     │ fkcodprod FK     │     └──────────┘
                     └──────────────┘     └──────────────────┘
```

## 2. SQL completo para PostgreSQL

```sql
-- ═══════════════════════════════════════════════════════
-- TABLAS DE NEGOCIO
-- ═══════════════════════════════════════════════════════

-- Producto: artículos que se venden
CREATE TABLE producto (
    codigo VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    precio DECIMAL(18,2) NOT NULL DEFAULT 0,
    existencia INTEGER DEFAULT 0
);

-- Persona: datos de personas naturales
CREATE TABLE persona (
    codigo VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    telefono VARCHAR(50) DEFAULT '',
    direccion VARCHAR(300) DEFAULT ''
);

-- Empresa: datos de personas jurídicas
CREATE TABLE empresa (
    codigo VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    nit VARCHAR(50) DEFAULT '',
    direccion VARCHAR(300) DEFAULT ''
);

-- Cliente: puede ser persona natural (fkcodpersona) o jurídica (fkcodempresa)
-- fkcodempresa es nullable: si es persona natural, no tiene empresa
CREATE TABLE cliente (
    id SERIAL PRIMARY KEY,
    credito DECIMAL(18,2) NOT NULL DEFAULT 0,
    fkcodpersona VARCHAR(20) NOT NULL REFERENCES persona(codigo),
    fkcodempresa VARCHAR(20) REFERENCES empresa(codigo)  -- nullable
);

-- Vendedor: personas que venden
CREATE TABLE vendedor (
    codigo VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    comision DECIMAL(5,2) DEFAULT 0
);

-- Factura: cabecera de la factura (maestro)
CREATE TABLE factura (
    numfactura SERIAL PRIMARY KEY,
    fecha TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    total DECIMAL(18,2) NOT NULL DEFAULT 0,
    fkcodvendedor VARCHAR(20) NOT NULL REFERENCES vendedor(codigo),
    fkcodcliente INTEGER NOT NULL REFERENCES cliente(id)
);

-- ProductosPorFactura: detalle de la factura (detalle)
-- Guarda el precio al momento de la venta (puede cambiar en el futuro)
CREATE TABLE productosporfactura (
    id SERIAL PRIMARY KEY,
    cantidad INTEGER NOT NULL DEFAULT 1,
    precio DECIMAL(18,2) NOT NULL,  -- precio al momento de la venta
    fknumfact INTEGER NOT NULL REFERENCES factura(numfactura),
    fkcodprod VARCHAR(20) NOT NULL REFERENCES producto(codigo)
);

-- ═══════════════════════════════════════════════════════
-- TABLAS DE SEGURIDAD (autenticación y autorización)
-- ═══════════════════════════════════════════════════════

-- Usuario: credenciales de acceso
-- contrasena se guarda como hash BCrypt (irreversible)
CREATE TABLE usuario (
    email VARCHAR(200) PRIMARY KEY,
    contrasena VARCHAR(200) NOT NULL,  -- hash BCrypt, NUNCA texto plano
    nombre VARCHAR(200) DEFAULT '',
    debe_cambiar_contrasena BOOLEAN DEFAULT false
);

-- Rol: tipos de usuario del sistema
CREATE TABLE rol (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL  -- ej: "Administrador", "Vendedor", "Contador"
);

-- Rol_usuario: asigna roles a usuarios (relación N:M)
-- Un usuario puede tener varios roles
CREATE TABLE rol_usuario (
    id SERIAL PRIMARY KEY,
    fkemail VARCHAR(200) NOT NULL REFERENCES usuario(email),
    fkidrol INTEGER NOT NULL REFERENCES rol(id)
);

-- Ruta: páginas/endpoints del sistema
CREATE TABLE ruta (
    id SERIAL PRIMARY KEY,
    ruta VARCHAR(200) NOT NULL,  -- ej: "/producto", "/factura"
    descripcion TEXT DEFAULT ''
);

-- Rutarol: define qué páginas puede acceder cada rol (relación N:M)
CREATE TABLE rutarol (
    id SERIAL PRIMARY KEY,
    fkidrol INTEGER NOT NULL REFERENCES rol(id),
    fkidruta INTEGER NOT NULL REFERENCES ruta(id)
);
```

## 3. SQL equivalente para SqlServer

```sql
-- Las diferencias con PostgreSQL:
--   SERIAL            -> INT IDENTITY(1,1)
--   BOOLEAN           -> BIT
--   TEXT               -> NVARCHAR(MAX)
--   VARCHAR            -> NVARCHAR
--   DECIMAL            -> DECIMAL (igual)
--   DEFAULT CURRENT_TIMESTAMP -> DEFAULT GETDATE()
--   REFERENCES         -> FOREIGN KEY ... REFERENCES (igual)

-- Ejemplo completo de una tabla con todas las diferencias:
CREATE TABLE producto (
    codigo NVARCHAR(20) PRIMARY KEY,
    nombre NVARCHAR(200) NOT NULL,
    precio DECIMAL(18,2) NOT NULL DEFAULT 0,
    existencia INT DEFAULT 0
);

CREATE TABLE persona (
    codigo NVARCHAR(20) PRIMARY KEY,
    nombre NVARCHAR(200) NOT NULL,
    telefono NVARCHAR(50) DEFAULT '',
    direccion NVARCHAR(300) DEFAULT ''
);

CREATE TABLE empresa (
    codigo NVARCHAR(20) PRIMARY KEY,
    nombre NVARCHAR(200) NOT NULL,
    nit NVARCHAR(50) DEFAULT '',
    direccion NVARCHAR(300) DEFAULT ''
);

CREATE TABLE cliente (
    id INT IDENTITY(1,1) PRIMARY KEY,
    credito DECIMAL(18,2) NOT NULL DEFAULT 0,
    fkcodpersona NVARCHAR(20) NOT NULL REFERENCES persona(codigo),
    fkcodempresa NVARCHAR(20) REFERENCES empresa(codigo)
);

CREATE TABLE vendedor (
    codigo NVARCHAR(20) PRIMARY KEY,
    nombre NVARCHAR(200) NOT NULL,
    comision DECIMAL(5,2) DEFAULT 0
);

CREATE TABLE factura (
    numfactura INT IDENTITY(1,1) PRIMARY KEY,
    fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    total DECIMAL(18,2) NOT NULL DEFAULT 0,
    fkcodvendedor NVARCHAR(20) NOT NULL REFERENCES vendedor(codigo),
    fkcodcliente INT NOT NULL REFERENCES cliente(id)
);

CREATE TABLE productosporfactura (
    id INT IDENTITY(1,1) PRIMARY KEY,
    cantidad INT NOT NULL DEFAULT 1,
    precio DECIMAL(18,2) NOT NULL,
    fknumfact INT NOT NULL REFERENCES factura(numfactura),
    fkcodprod NVARCHAR(20) NOT NULL REFERENCES producto(codigo)
);

CREATE TABLE usuario (
    email NVARCHAR(200) PRIMARY KEY,
    contrasena NVARCHAR(200) NOT NULL,
    nombre NVARCHAR(200) DEFAULT '',
    debe_cambiar_contrasena BIT DEFAULT 0
);

CREATE TABLE rol (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL
);

CREATE TABLE rol_usuario (
    id INT IDENTITY(1,1) PRIMARY KEY,
    fkemail NVARCHAR(200) NOT NULL REFERENCES usuario(email),
    fkidrol INT NOT NULL REFERENCES rol(id)
);

CREATE TABLE ruta (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ruta NVARCHAR(200) NOT NULL,
    descripcion NVARCHAR(MAX) DEFAULT ''
);

CREATE TABLE rutarol (
    id INT IDENTITY(1,1) PRIMARY KEY,
    fkidrol INT NOT NULL REFERENCES rol(id),
    fkidruta INT NOT NULL REFERENCES ruta(id)
);
```

## 4. Datos iniciales de ejemplo

```sql
-- Roles del sistema
INSERT INTO rol (nombre) VALUES ('Administrador');
INSERT INTO rol (nombre) VALUES ('Vendedor');
INSERT INTO rol (nombre) VALUES ('Cajero');
INSERT INTO rol (nombre) VALUES ('Contador');
INSERT INTO rol (nombre) VALUES ('Cliente');

-- Usuario administrador (contraseña se crea vía API con BCrypt):
-- POST http://localhost:5035/api/usuario?camposEncriptar=contrasena
-- Body: {"email": "admin@test.com", "contrasena": "Admin123"}

-- Asignar rol administrador
INSERT INTO rol_usuario (fkemail, fkidrol) VALUES ('admin@test.com', 1);

-- Rutas del sistema
INSERT INTO ruta (ruta, descripcion) VALUES ('/home', 'Página inicio');
INSERT INTO ruta (ruta, descripcion) VALUES ('/producto', 'Gestión de productos');
INSERT INTO ruta (ruta, descripcion) VALUES ('/factura', 'Gestión de facturas');
INSERT INTO ruta (ruta, descripcion) VALUES ('/cliente', 'Gestión de clientes');
INSERT INTO ruta (ruta, descripcion) VALUES ('/usuario', 'Gestión de usuarios');

-- Asignar todas las rutas al rol Administrador
INSERT INTO rutarol (fkidrol, fkidruta) VALUES (1, 1);  -- admin -> /home
INSERT INTO rutarol (fkidrol, fkidruta) VALUES (1, 2);  -- admin -> /producto
INSERT INTO rutarol (fkidrol, fkidruta) VALUES (1, 3);  -- admin -> /factura
INSERT INTO rutarol (fkidrol, fkidruta) VALUES (1, 4);  -- admin -> /cliente
INSERT INTO rutarol (fkidrol, fkidruta) VALUES (1, 5);  -- admin -> /usuario

-- Productos de ejemplo
INSERT INTO producto (codigo, nombre, precio, existencia) VALUES ('P001', 'Laptop HP', 2500000, 10);
INSERT INTO producto (codigo, nombre, precio, existencia) VALUES ('P002', 'Mouse Logitech', 85000, 50);
INSERT INTO producto (codigo, nombre, precio, existencia) VALUES ('P003', 'Teclado Mecánico', 320000, 25);

-- Personas de ejemplo
INSERT INTO persona (codigo, nombre, telefono, direccion) VALUES ('PER01', 'Juan Pérez', '3001234567', 'Calle 10 #20-30');
INSERT INTO persona (codigo, nombre, telefono, direccion) VALUES ('PER02', 'María López', '3109876543', 'Carrera 5 #15-20');

-- Empresa de ejemplo
INSERT INTO empresa (codigo, nombre, nit, direccion) VALUES ('EMP01', 'Tech Solutions SAS', '900123456-1', 'Av. El Poblado #80-50');

-- Vendedor de ejemplo
INSERT INTO vendedor (codigo, nombre, comision) VALUES ('V001', 'Carlos Gómez', 5.50);
```

## 5. Diccionario de datos

| Tipo de dato | PostgreSQL | SqlServer | C# | HTML input |
|-------------|-----------|-----------|-----|------------|
| Texto corto | VARCHAR(N) | NVARCHAR(N) | string | type="text" |
| Texto largo | TEXT | NVARCHAR(MAX) | string | textarea |
| Entero | INTEGER | INT | int | type="number" |
| Decimal | DECIMAL(18,2) | DECIMAL(18,2) | decimal | type="number" step="0.01" |
| Booleano | BOOLEAN | BIT | bool | type="checkbox" |
| Fecha/hora | TIMESTAMP | DATETIME2 | DateTime | type="datetime-local" |
| Auto-incremento | SERIAL | IDENTITY(1,1) | (generado por BD) | (oculto) |
| Email | VARCHAR(200) | NVARCHAR(200) | string | type="email" |
| Contraseña | VARCHAR(200) | NVARCHAR(200) | string (hash) | type="password" |

### Mapeo completo PostgreSQL -> SqlServer -> C# -> Blazor

| Columna ejemplo | PostgreSQL | SqlServer | Tipo C# | Componente Razor |
|----------------|-----------|-----------|---------|-----------------|
| producto.codigo | VARCHAR(20) PK | NVARCHAR(20) PK | string | `<input type="text">` |
| producto.precio | DECIMAL(18,2) | DECIMAL(18,2) | decimal | `<input type="number" step="0.01">` |
| producto.existencia | INTEGER | INT | int | `<input type="number">` |
| factura.fecha | TIMESTAMP | DATETIME2 | DateTime | `<input type="datetime-local">` |
| usuario.email | VARCHAR(200) PK | NVARCHAR(200) PK | string | `<input type="email">` |
| usuario.contrasena | VARCHAR(200) | NVARCHAR(200) | string | `<input type="password">` |
| usuario.debe_cambiar | BOOLEAN | BIT | bool | `<input type="checkbox">` |
| ruta.descripcion | TEXT | NVARCHAR(MAX) | string | `<textarea>` |
| cliente.id | SERIAL PK | INT IDENTITY PK | int | (oculto, auto-generado) |
| cliente.fkcodpersona | VARCHAR(20) FK | NVARCHAR(20) FK | string | `<select>` (cargado con ListarAsync) |

---

## Referencias

- Formato data-model: [Spec-Kit estructura de specs](https://github.com/github/spec-kit)
- Normalización: 02_especificacion.md, sección 3.3
- ACID: 01_constitucion.md
- Compatibilidad Postgres/SqlServer: 03_clarificacion.md
