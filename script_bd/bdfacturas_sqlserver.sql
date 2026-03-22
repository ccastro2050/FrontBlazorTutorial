-- ============================================================
-- Script de creación de base de datos: bdfacturas_sqlserver_local
-- Compatible con SQL Server 2016+
-- Incluye: tablas, restricciones, triggers y datos de ejemplo
-- ============================================================

USE bdfacturas_sqlserver_local;
GO

-- ============================================================
-- LIMPIEZA: Eliminar objetos existentes en orden correcto
-- ============================================================

-- Triggers
IF OBJECT_ID('trg_prodfact_insert', 'TR') IS NOT NULL DROP TRIGGER trg_prodfact_insert;
IF OBJECT_ID('trg_prodfact_update', 'TR') IS NOT NULL DROP TRIGGER trg_prodfact_update;
IF OBJECT_ID('trg_prodfact_delete', 'TR') IS NOT NULL DROP TRIGGER trg_prodfact_delete;
GO

-- Procedimientos almacenados
IF OBJECT_ID('sp_insertar_factura_y_productosporfactura', 'P') IS NOT NULL DROP PROCEDURE sp_insertar_factura_y_productosporfactura;
IF OBJECT_ID('sp_consultar_factura_y_productosporfactura', 'P') IS NOT NULL DROP PROCEDURE sp_consultar_factura_y_productosporfactura;
IF OBJECT_ID('sp_listar_facturas_y_productosporfactura', 'P') IS NOT NULL DROP PROCEDURE sp_listar_facturas_y_productosporfactura;
IF OBJECT_ID('sp_actualizar_factura_y_productosporfactura', 'P') IS NOT NULL DROP PROCEDURE sp_actualizar_factura_y_productosporfactura;
IF OBJECT_ID('sp_borrar_factura_y_productosporfactura', 'P') IS NOT NULL DROP PROCEDURE sp_borrar_factura_y_productosporfactura;
GO

-- Tablas dependientes primero
IF OBJECT_ID('rutarol', 'U') IS NOT NULL DROP TABLE rutarol;
IF OBJECT_ID('rol_usuario', 'U') IS NOT NULL DROP TABLE rol_usuario;
IF OBJECT_ID('productosporfactura', 'U') IS NOT NULL DROP TABLE productosporfactura;
IF OBJECT_ID('factura', 'U') IS NOT NULL DROP TABLE factura;
IF OBJECT_ID('cliente', 'U') IS NOT NULL DROP TABLE cliente;
IF OBJECT_ID('vendedor', 'U') IS NOT NULL DROP TABLE vendedor;
IF OBJECT_ID('empresa', 'U') IS NOT NULL DROP TABLE empresa;
IF OBJECT_ID('persona', 'U') IS NOT NULL DROP TABLE persona;
IF OBJECT_ID('producto', 'U') IS NOT NULL DROP TABLE producto;
IF OBJECT_ID('rol', 'U') IS NOT NULL DROP TABLE rol;
IF OBJECT_ID('ruta', 'U') IS NOT NULL DROP TABLE ruta;
IF OBJECT_ID('usuario', 'U') IS NOT NULL DROP TABLE usuario;
GO

-- ============================================================
-- TABLAS INDEPENDIENTES (sin foreign keys)
-- ============================================================

CREATE TABLE empresa (
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    CONSTRAINT pk_empresa PRIMARY KEY (codigo)
);

CREATE TABLE persona (
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL,
    telefono VARCHAR(20) NOT NULL,
    CONSTRAINT pk_persona PRIMARY KEY (codigo)
);

CREATE TABLE producto (
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    stock INT NOT NULL,
    valorunitario DECIMAL(18,2) NOT NULL,
    CONSTRAINT pk_producto PRIMARY KEY (codigo)
);

CREATE TABLE rol (
    id INT IDENTITY(1,1) NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    CONSTRAINT pk_rol PRIMARY KEY (id)
);

CREATE TABLE ruta (
    ruta VARCHAR(100) NOT NULL,
    descripcion VARCHAR(200) NOT NULL,
    CONSTRAINT pk_ruta PRIMARY KEY (ruta)
);

CREATE TABLE usuario (
    email VARCHAR(100) NOT NULL,
    contrasena VARCHAR(200) NOT NULL,
    CONSTRAINT pk_usuario PRIMARY KEY (email)
);
GO

-- ============================================================
-- TABLAS DEPENDIENTES (con foreign keys)
-- ============================================================

CREATE TABLE cliente (
    id INT IDENTITY(1,1) NOT NULL,
    credito DECIMAL(18,2) NOT NULL DEFAULT 0,
    fkcodpersona VARCHAR(10) NOT NULL,
    fkcodempresa VARCHAR(10),
    CONSTRAINT pk_cliente PRIMARY KEY (id),
    CONSTRAINT fk_cliente_persona FOREIGN KEY (fkcodpersona) REFERENCES persona(codigo),
    CONSTRAINT fk_cliente_empresa FOREIGN KEY (fkcodempresa) REFERENCES empresa(codigo)
);

CREATE TABLE vendedor (
    id INT IDENTITY(1,1) NOT NULL,
    carnet INT NOT NULL,
    direccion VARCHAR(100) NOT NULL,
    fkcodpersona VARCHAR(10) NOT NULL,
    CONSTRAINT pk_vendedor PRIMARY KEY (id),
    CONSTRAINT fk_vendedor_persona FOREIGN KEY (fkcodpersona) REFERENCES persona(codigo)
);

CREATE TABLE factura (
    numero INT IDENTITY(1,1) NOT NULL,
    fecha DATETIME2 NOT NULL DEFAULT GETDATE(),
    total DECIMAL(18,2) NOT NULL DEFAULT 0,
    fkidcliente INT NOT NULL,
    fkidvendedor INT NOT NULL,
    CONSTRAINT pk_factura PRIMARY KEY (numero),
    CONSTRAINT fk_factura_cliente FOREIGN KEY (fkidcliente) REFERENCES cliente(id),
    CONSTRAINT fk_factura_vendedor FOREIGN KEY (fkidvendedor) REFERENCES vendedor(id)
);

CREATE TABLE productosporfactura (
    fknumfactura INT NOT NULL,
    fkcodproducto VARCHAR(10) NOT NULL,
    cantidad INT NOT NULL,
    subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT pk_productosporfactura PRIMARY KEY (fknumfactura, fkcodproducto),
    CONSTRAINT fk_prodfact_factura FOREIGN KEY (fknumfactura) REFERENCES factura(numero) ON DELETE CASCADE,
    CONSTRAINT fk_prodfact_producto FOREIGN KEY (fkcodproducto) REFERENCES producto(codigo)
);

CREATE TABLE rol_usuario (
    fkemail VARCHAR(100) NOT NULL,
    fkidrol INT NOT NULL,
    CONSTRAINT pk_rol_usuario PRIMARY KEY (fkemail, fkidrol),
    CONSTRAINT fk_rolusuario_usuario FOREIGN KEY (fkemail) REFERENCES usuario(email),
    CONSTRAINT fk_rolusuario_rol FOREIGN KEY (fkidrol) REFERENCES rol(id)
);

CREATE TABLE rutarol (
    ruta VARCHAR(100) NOT NULL,
    rol VARCHAR(50) NOT NULL,
    CONSTRAINT pk_rutarol PRIMARY KEY (ruta, rol)
);
GO

-- ============================================================
-- TRIGGERS: Actualizar totales de factura y stock de producto
-- SQL Server requiere triggers AFTER separados por operación.
-- Se usan las tablas virtuales INSERTED y DELETED.
-- ============================================================

-- TRIGGER INSERT
CREATE TRIGGER trg_prodfact_insert
ON productosporfactura
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar stock suficiente para cada producto insertado
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN producto p ON p.codigo = i.fkcodproducto
        WHERE p.stock < i.cantidad
    )
    BEGIN
        DECLARE @v_codigo_err VARCHAR(10), @v_stock_err INT, @v_cantidad_err INT;
        SELECT TOP 1 @v_codigo_err = i.fkcodproducto, @v_stock_err = p.stock, @v_cantidad_err = i.cantidad
        FROM inserted i
        JOIN producto p ON p.codigo = i.fkcodproducto
        WHERE p.stock < i.cantidad;

        DECLARE @v_msg_err NVARCHAR(500);
        SET @v_msg_err = CONCAT('Stock insuficiente para producto ', @v_codigo_err,
            '. Stock disponible: ', @v_stock_err, ', cantidad solicitada: ', @v_cantidad_err);
        THROW 50001, @v_msg_err, 1;
    END

    -- Calcular subtotal = cantidad * valorunitario y actualizar la fila insertada
    UPDATE pf
    SET pf.subtotal = i.cantidad * p.valorunitario
    FROM productosporfactura pf
    JOIN inserted i ON pf.fknumfactura = i.fknumfactura AND pf.fkcodproducto = i.fkcodproducto
    JOIN producto p ON p.codigo = i.fkcodproducto;

    -- Descontar stock del producto
    UPDATE p
    SET p.stock = p.stock - i.cantidad
    FROM producto p
    JOIN inserted i ON p.codigo = i.fkcodproducto;

    -- Recalcular total de la factura
    UPDATE f
    SET f.total = ISNULL(sub.suma, 0)
    FROM factura f
    JOIN (
        SELECT pf.fknumfactura, SUM(pf.subtotal) AS suma
        FROM productosporfactura pf
        WHERE pf.fknumfactura IN (SELECT DISTINCT fknumfactura FROM inserted)
        GROUP BY pf.fknumfactura
    ) sub ON f.numero = sub.fknumfactura;
END;
GO

-- TRIGGER UPDATE
CREATE TRIGGER trg_prodfact_update
ON productosporfactura
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar stock suficiente (considerando la devolucion del stock anterior)
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.fknumfactura = d.fknumfactura AND i.fkcodproducto = d.fkcodproducto
        JOIN producto p ON p.codigo = i.fkcodproducto
        WHERE p.stock + d.cantidad < i.cantidad
    )
    BEGIN
        DECLARE @v_codigo_err VARCHAR(10), @v_stock_err INT, @v_cantidad_err INT;
        SELECT TOP 1 @v_codigo_err = i.fkcodproducto, @v_stock_err = p.stock + d.cantidad, @v_cantidad_err = i.cantidad
        FROM inserted i
        JOIN deleted d ON i.fknumfactura = d.fknumfactura AND i.fkcodproducto = d.fkcodproducto
        JOIN producto p ON p.codigo = i.fkcodproducto
        WHERE p.stock + d.cantidad < i.cantidad;

        DECLARE @v_msg_err NVARCHAR(500);
        SET @v_msg_err = CONCAT('Stock insuficiente para producto ', @v_codigo_err,
            '. Stock disponible: ', @v_stock_err, ', cantidad solicitada: ', @v_cantidad_err);
        THROW 50001, @v_msg_err, 1;
    END

    -- Recalcular subtotal
    UPDATE pf
    SET pf.subtotal = i.cantidad * p.valorunitario
    FROM productosporfactura pf
    JOIN inserted i ON pf.fknumfactura = i.fknumfactura AND pf.fkcodproducto = i.fkcodproducto
    JOIN producto p ON p.codigo = i.fkcodproducto;

    -- Ajustar stock: devolver old.cantidad y descontar new.cantidad
    UPDATE p
    SET p.stock = p.stock + d.cantidad - i.cantidad
    FROM producto p
    JOIN inserted i ON p.codigo = i.fkcodproducto
    JOIN deleted d ON d.fknumfactura = i.fknumfactura AND d.fkcodproducto = i.fkcodproducto;

    -- Recalcular total de la factura
    UPDATE f
    SET f.total = ISNULL(sub.suma, 0)
    FROM factura f
    JOIN (
        SELECT pf.fknumfactura, SUM(pf.subtotal) AS suma
        FROM productosporfactura pf
        WHERE pf.fknumfactura IN (SELECT DISTINCT fknumfactura FROM inserted)
        GROUP BY pf.fknumfactura
    ) sub ON f.numero = sub.fknumfactura;
END;
GO

-- TRIGGER DELETE
CREATE TRIGGER trg_prodfact_delete
ON productosporfactura
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Restaurar stock del producto
    UPDATE p
    SET p.stock = p.stock + d.cantidad
    FROM producto p
    JOIN deleted d ON p.codigo = d.fkcodproducto;

    -- Recalcular total de la factura
    UPDATE f
    SET f.total = ISNULL(sub.suma, 0)
    FROM factura f
    JOIN (
        SELECT d.fknumfactura,
               ISNULL((SELECT SUM(pf.subtotal) FROM productosporfactura pf WHERE pf.fknumfactura = d.fknumfactura), 0) AS suma
        FROM (SELECT DISTINCT fknumfactura FROM deleted) d
    ) sub ON f.numero = sub.fknumfactura;
END;
GO

-- ============================================================
-- PROCEDIMIENTOS ALMACENADOS - FACTURAS Y PRODUCTOS POR FACTURA
-- Los resultados se retornan via parámetro OUTPUT tipo NVARCHAR(MAX)
-- ============================================================

-- ------------------------------------------------------------
-- 1. SP INSERTAR FACTURA Y PRODUCTOSPORFACTURA
-- Recibe: id cliente, id vendedor, y un JSON array de productos
-- Retorna: JSON con la factura creada y sus productos
-- Nota: El trigger se encarga de calcular subtotal, descontar
--       stock y actualizar total factura.
-- Ejemplo via API:
--   POST /api/procedimientos/ejecutarsp
--   { "nombreSP": "sp_insertar_factura_y_productosporfactura",
--     "p_fkidcliente": 1, "p_fkidvendedor": 1,
--     "p_productos": "[{\"codigo\":\"PR001\",\"cantidad\":2},{\"codigo\":\"PR003\",\"cantidad\":3}]",
--     "p_resultado": null }
-- ------------------------------------------------------------
CREATE PROCEDURE sp_insertar_factura_y_productosporfactura
    @p_fkidcliente INT,
    @p_fkidvendedor INT,
    @p_productos NVARCHAR(MAX),
    @p_minimo_detalle INT = 1,
    @p_resultado NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_numero INT;
    DECLARE @v_codigo VARCHAR(10);
    DECLARE @v_cantidad INT;
    DECLARE @v_minimo INT;
    DECLARE @v_count INT;
    DECLARE @v_msg NVARCHAR(500);

    -- Validar minimo de productos (antes de abrir transaccion)
    -- COALESCE(NULLIF(@p_minimo_detalle, 0), 1): la API envia 0 cuando no se pasa el parametro
    SET @v_minimo = COALESCE(NULLIF(@p_minimo_detalle, 0), 1);

    IF @p_productos IS NULL
    BEGIN
        SET @v_msg = CONCAT('La factura requiere minimo ', @v_minimo, ' producto(s).');
        THROW 50002, @v_msg, 1;
    END

    SELECT @v_count = COUNT(*) FROM OPENJSON(@p_productos);
    IF @v_count < @v_minimo
    BEGIN
        SET @v_msg = CONCAT('La factura requiere minimo ', @v_minimo, ' producto(s).');
        THROW 50002, @v_msg, 1;
    END

    -- ── TRANSACCION: todo o nada ──
    -- Si un trigger falla (ej: stock insuficiente en el 2do producto),
    -- se revierte la factura y todos los productos insertados previamente.
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Crear la factura con total 0 (el trigger actualiza el total)
        INSERT INTO factura (fkidcliente, fkidvendedor, total)
        VALUES (@p_fkidcliente, @p_fkidvendedor, 0);

        SET @v_numero = SCOPE_IDENTITY();

        -- Recorrer cada producto del JSON e insertar detalle
        -- El trigger calcula subtotal, descuenta stock y actualiza total
        DECLARE producto_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT
                JSON_VALUE(value, '$.codigo'),
                CAST(JSON_VALUE(value, '$.cantidad') AS INT)
            FROM OPENJSON(@p_productos);

        OPEN producto_cursor;
        FETCH NEXT FROM producto_cursor INTO @v_codigo, @v_cantidad;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO productosporfactura (fknumfactura, fkcodproducto, cantidad, subtotal)
            VALUES (@v_numero, @v_codigo, @v_cantidad, 0);

            FETCH NEXT FROM producto_cursor INTO @v_codigo, @v_cantidad;
        END

        CLOSE producto_cursor;
        DEALLOCATE producto_cursor;

        -- Retornar resultado como JSON
        DECLARE @v_factura_json NVARCHAR(MAX);
        DECLARE @v_productos_json NVARCHAR(MAX);

        SELECT @v_factura_json = (
            SELECT f.numero, f.fecha, f.total, f.fkidcliente, f.fkidvendedor
            FROM factura f WHERE f.numero = @v_numero
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        SELECT @v_productos_json = (
            SELECT pf.fkcodproducto AS codigo_producto, pr.nombre AS nombre_producto,
                   pf.cantidad, pr.valorunitario, pf.subtotal
            FROM productosporfactura pf
            JOIN producto pr ON pr.codigo = pf.fkcodproducto
            WHERE pf.fknumfactura = @v_numero
            FOR JSON PATH
        );

        SET @p_resultado = '{"factura":' + @v_factura_json + ',"productos":' + ISNULL(@v_productos_json, '[]') + '}';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Cerrar cursor si quedo abierto
        IF CURSOR_STATUS('local', 'producto_cursor') >= 0
        BEGIN
            CLOSE producto_cursor;
            DEALLOCATE producto_cursor;
        END

        -- Relanzar el error original (del trigger u otro)
        THROW;
    END CATCH
END;
GO

-- ------------------------------------------------------------
-- 2. SP CONSULTAR FACTURA Y PRODUCTOSPORFACTURA
-- Consulta una factura por número con detalle de productos,
-- nombre del cliente y nombre del vendedor
-- Ejemplo via API:
--   POST /api/procedimientos/ejecutarsp
--   { "nombreSP": "sp_consultar_factura_y_productosporfactura",
--     "p_numero": 1, "p_resultado": "" }
-- ------------------------------------------------------------
CREATE PROCEDURE sp_consultar_factura_y_productosporfactura
    @p_numero INT,
    @p_resultado NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM factura WHERE numero = @p_numero)
    BEGIN
        DECLARE @v_msg NVARCHAR(500);
        SET @v_msg = CONCAT('Factura ', @p_numero, ' no existe');
        THROW 50003, @v_msg, 1;
    END

    DECLARE @v_factura_json NVARCHAR(MAX);
    DECLARE @v_productos_json NVARCHAR(MAX);

    SELECT @v_factura_json = (
        SELECT f.numero, f.fecha, f.total, f.fkidcliente,
               pc.nombre AS nombre_cliente,
               f.fkidvendedor,
               pv.nombre AS nombre_vendedor
        FROM factura f
        JOIN cliente c ON c.id = f.fkidcliente
        JOIN persona pc ON pc.codigo = c.fkcodpersona
        JOIN vendedor v ON v.id = f.fkidvendedor
        JOIN persona pv ON pv.codigo = v.fkcodpersona
        WHERE f.numero = @p_numero
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    );

    SELECT @v_productos_json = (
        SELECT pr.codigo AS codigo_producto, pr.nombre AS nombre_producto,
               pf.cantidad, pr.valorunitario, pf.subtotal
        FROM productosporfactura pf
        JOIN producto pr ON pr.codigo = pf.fkcodproducto
        WHERE pf.fknumfactura = @p_numero
        FOR JSON PATH
    );

    SET @p_resultado = '{"factura":' + @v_factura_json + ',"productos":' + ISNULL(@v_productos_json, '[]') + '}';
END;
GO

-- ------------------------------------------------------------
-- 3. SP LISTAR FACTURAS Y PRODUCTOSPORFACTURA
-- Lista todas las facturas con sus productos, cliente y vendedor
-- Ejemplo via API:
--   POST /api/procedimientos/ejecutarsp
--   { "nombreSP": "sp_listar_facturas_y_productosporfactura",
--     "p_resultado": "" }
-- ------------------------------------------------------------
CREATE PROCEDURE sp_listar_facturas_y_productosporfactura
    @p_resultado NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_result NVARCHAR(MAX) = '[';
    DECLARE @v_numero INT;
    DECLARE @v_factura_json NVARCHAR(MAX);
    DECLARE @v_productos_json NVARCHAR(MAX);
    DECLARE @v_first BIT = 1;

    DECLARE factura_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT f.numero
        FROM factura f
        ORDER BY f.numero;

    OPEN factura_cursor;
    FETCH NEXT FROM factura_cursor INTO @v_numero;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @v_first = 0
            SET @v_result = @v_result + ',';
        SET @v_first = 0;

        SELECT @v_factura_json = (
            SELECT f.numero, f.fecha, f.total, f.fkidcliente,
                   pc.nombre AS nombre_cliente,
                   f.fkidvendedor,
                   pv.nombre AS nombre_vendedor
            FROM factura f
            JOIN cliente c ON c.id = f.fkidcliente
            JOIN persona pc ON pc.codigo = c.fkcodpersona
            JOIN vendedor v ON v.id = f.fkidvendedor
            JOIN persona pv ON pv.codigo = v.fkcodpersona
            WHERE f.numero = @v_numero
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        SELECT @v_productos_json = (
            SELECT pr.codigo AS codigo_producto, pr.nombre AS nombre_producto,
                   pf.cantidad, pr.valorunitario, pf.subtotal
            FROM productosporfactura pf
            JOIN producto pr ON pr.codigo = pf.fkcodproducto
            WHERE pf.fknumfactura = @v_numero
            FOR JSON PATH
        );

        SET @v_result = @v_result + '{' +
            '"numero":' + CAST(@v_numero AS NVARCHAR) + ',' +
            '"fecha":"' + CONVERT(NVARCHAR(30), (SELECT fecha FROM factura WHERE numero = @v_numero), 126) + '",' +
            '"total":' + CAST((SELECT total FROM factura WHERE numero = @v_numero) AS NVARCHAR) + ',' +
            '"fkidcliente":' + CAST((SELECT fkidcliente FROM factura WHERE numero = @v_numero) AS NVARCHAR) + ',' +
            '"nombre_cliente":"' + (SELECT pc.nombre FROM factura f JOIN cliente c ON c.id = f.fkidcliente JOIN persona pc ON pc.codigo = c.fkcodpersona WHERE f.numero = @v_numero) + '",' +
            '"fkidvendedor":' + CAST((SELECT fkidvendedor FROM factura WHERE numero = @v_numero) AS NVARCHAR) + ',' +
            '"nombre_vendedor":"' + (SELECT pv.nombre FROM factura f JOIN vendedor v ON v.id = f.fkidvendedor JOIN persona pv ON pv.codigo = v.fkcodpersona WHERE f.numero = @v_numero) + '",' +
            '"productos":' + ISNULL(@v_productos_json, '[]') +
            '}';

        FETCH NEXT FROM factura_cursor INTO @v_numero;
    END

    CLOSE factura_cursor;
    DEALLOCATE factura_cursor;

    SET @v_result = @v_result + ']';

    -- Si no hay facturas, retornar array vacio
    IF @v_first = 1
        SET @v_result = '[]';

    SET @p_resultado = @v_result;
END;
GO

-- ------------------------------------------------------------
-- 4. SP ACTUALIZAR FACTURA Y PRODUCTOSPORFACTURA
-- Reemplaza los productos de una factura existente.
-- Nota: El trigger se encarga de restaurar stock (DELETE),
--       descontar stock (INSERT) y recalcular subtotales/total.
-- Ejemplo via API:
--   POST /api/procedimientos/ejecutarsp
--   { "nombreSP": "sp_actualizar_factura_y_productosporfactura",
--     "p_numero": 1, "p_fkidcliente": 2, "p_fkidvendedor": 1,
--     "p_productos": "[{\"codigo\":\"PR002\",\"cantidad\":1},{\"codigo\":\"PR004\",\"cantidad\":5}]",
--     "p_resultado": null }
-- ------------------------------------------------------------
CREATE PROCEDURE sp_actualizar_factura_y_productosporfactura
    @p_numero INT,
    @p_fkidcliente INT,
    @p_fkidvendedor INT,
    @p_productos NVARCHAR(MAX),
    @p_minimo_detalle INT = 1,
    @p_resultado NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_codigo VARCHAR(10);
    DECLARE @v_cantidad INT;
    DECLARE @v_minimo INT;
    DECLARE @v_count INT;
    DECLARE @v_msg NVARCHAR(500);

    -- Validaciones antes de abrir transaccion
    IF NOT EXISTS (SELECT 1 FROM factura WHERE numero = @p_numero)
    BEGIN
        SET @v_msg = CONCAT('Factura ', @p_numero, ' no existe');
        THROW 50004, @v_msg, 1;
    END

    SET @v_minimo = COALESCE(NULLIF(@p_minimo_detalle, 0), 1);

    IF @p_productos IS NULL
    BEGIN
        SET @v_msg = CONCAT('La factura requiere minimo ', @v_minimo, ' producto(s).');
        THROW 50004, @v_msg, 1;
    END

    SELECT @v_count = COUNT(*) FROM OPENJSON(@p_productos);
    IF @v_count < @v_minimo
    BEGIN
        SET @v_msg = CONCAT('La factura requiere minimo ', @v_minimo, ' producto(s).');
        THROW 50004, @v_msg, 1;
    END

    -- ── TRANSACCION: todo o nada ──
    -- Si un trigger falla al insertar un nuevo producto (ej: stock insuficiente),
    -- se revierten todos los cambios: el DELETE previo, los INSERTs parciales y el UPDATE.
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Eliminar detalle anterior (el trigger restaura stock y recalcula total)
        DELETE FROM productosporfactura WHERE fknumfactura = @p_numero;

        -- Insertar nuevos productos (el trigger calcula subtotal, descuenta stock, actualiza total)
        DECLARE producto_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT
                JSON_VALUE(value, '$.codigo'),
                CAST(JSON_VALUE(value, '$.cantidad') AS INT)
            FROM OPENJSON(@p_productos);

        OPEN producto_cursor;
        FETCH NEXT FROM producto_cursor INTO @v_codigo, @v_cantidad;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO productosporfactura (fknumfactura, fkcodproducto, cantidad, subtotal)
            VALUES (@p_numero, @v_codigo, @v_cantidad, 0);

            FETCH NEXT FROM producto_cursor INTO @v_codigo, @v_cantidad;
        END

        CLOSE producto_cursor;
        DEALLOCATE producto_cursor;

        -- Actualizar cliente y vendedor de la factura
        UPDATE factura
        SET fkidcliente = @p_fkidcliente,
            fkidvendedor = @p_fkidvendedor
        WHERE numero = @p_numero;

        -- Retornar resultado como JSON
        DECLARE @v_factura_json NVARCHAR(MAX);
        DECLARE @v_productos_json NVARCHAR(MAX);

        SELECT @v_factura_json = (
            SELECT f.numero, f.fecha, f.total, f.fkidcliente, f.fkidvendedor
            FROM factura f WHERE f.numero = @p_numero
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        SELECT @v_productos_json = (
            SELECT pf.fkcodproducto AS codigo_producto, pr.nombre AS nombre_producto,
                   pf.cantidad, pr.valorunitario, pf.subtotal
            FROM productosporfactura pf
            JOIN producto pr ON pr.codigo = pf.fkcodproducto
            WHERE pf.fknumfactura = @p_numero
            FOR JSON PATH
        );

        SET @p_resultado = '{"factura":' + @v_factura_json + ',"productos":' + ISNULL(@v_productos_json, '[]') + '}';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        IF CURSOR_STATUS('local', 'producto_cursor') >= 0
        BEGIN
            CLOSE producto_cursor;
            DEALLOCATE producto_cursor;
        END

        THROW;
    END CATCH
END;
GO

-- ------------------------------------------------------------
-- 5. SP BORRAR FACTURA Y PRODUCTOSPORFACTURA
-- ON DELETE CASCADE elimina productosporfactura automáticamente.
-- El trigger restaura stock al borrar cada producto de la factura.
-- Ejemplo via API:
--   POST /api/procedimientos/ejecutarsp
--   { "nombreSP": "sp_borrar_factura_y_productosporfactura",
--     "p_numero": 1, "p_resultado": null }
-- ------------------------------------------------------------
CREATE PROCEDURE sp_borrar_factura_y_productosporfactura
    @p_numero INT,
    @p_resultado NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @v_total DECIMAL(18,2);
    DECLARE @v_cantidad_productos INT;
    DECLARE @v_msg NVARCHAR(500);

    -- Validacion antes de abrir transaccion
    IF NOT EXISTS (SELECT 1 FROM factura WHERE numero = @p_numero)
    BEGIN
        SET @v_msg = CONCAT('Factura ', @p_numero, ' no existe');
        THROW 50005, @v_msg, 1;
    END

    -- ── TRANSACCION: todo o nada ──
    -- El DELETE CASCADE dispara el trigger de delete para cada producto,
    -- restaurando stock. Si algo falla, se revierte todo.
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Guardar info antes de borrar para el JSON de respuesta
        SELECT @v_cantidad_productos = COUNT(*)
        FROM productosporfactura WHERE fknumfactura = @p_numero;

        SELECT @v_total = f.total FROM factura f WHERE f.numero = @p_numero;

        -- Borrar factura (ON DELETE CASCADE borra productosporfactura,
        -- y el trigger restaura stock por cada producto eliminado)
        DELETE FROM factura WHERE numero = @p_numero;

        -- Retornar resultado como JSON
        SET @p_resultado = '{"mensaje":"Factura eliminada exitosamente",' +
            '"numero_eliminado":' + CAST(@p_numero AS NVARCHAR) + ',' +
            '"total_eliminado":' + CAST(@v_total AS NVARCHAR) + ',' +
            '"productos_eliminados":' + CAST(@v_cantidad_productos AS NVARCHAR) + '}';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

-- ============================================================
-- DATOS
-- ============================================================

-- Empresas
INSERT INTO empresa (codigo, nombre) VALUES
('E001', 'Comercial Los Andes S.A.'),
('E002', 'Distribuciones El Centro S.A.'),
('E999', 'Empresa Test');

-- Personas
INSERT INTO persona (codigo, nombre, email, telefono) VALUES
('P001', 'Ana Torres', 'ana.torres@correo.com', '3011111111'),
('P002', 'Carlos Pérez', 'carlos.perez@correo.com', '3022222222'),
('P003', 'María Gómez', 'maria.gomez@correo.com', '3033333333'),
('P004', 'Juan Díaz', 'juan.diaz@correo.com', '3044444444'),
('P005', 'Laura Rojas', 'laura.rojas@correo.com', '3055555555'),
('P006', 'Pedro Castillo', 'pedro.castillo@correo.com', '3066666666');

-- Productos
INSERT INTO producto (codigo, nombre, stock, valorunitario) VALUES
('PR001', 'Laptop Lenovo IdeaPad', 17, 2500000),
('PR002', 'Monitor Samsung 24"', 27, 800000),
('PR003', 'Teclado Logitech K380', 42, 150000),
('PR004', 'Mouse HP', 55, 90000),
('PR005', 'Impresora Epson EcoTank1', 14, 1100000),
('PR006', 'Auriculares Sony WH-CH510', 23, 240000),
('PR007', 'Tablet Samsung Tab A9', 15, 950000),
('PR008', 'Disco Duro Seagate 1TB', 32, 280000);

-- Roles (con IDENTITY_INSERT para IDs explícitos)
SET IDENTITY_INSERT rol ON;
INSERT INTO rol (id, nombre) VALUES
(1, 'Administrador'),
(2, 'Vendedor'),
(3, 'Cajero'),
(4, 'Contador'),
(5, 'Cliente');
SET IDENTITY_INSERT rol OFF;

-- Rutas
INSERT INTO ruta (ruta, descripcion) VALUES
('/home', 'Página principal - Dashboard'),
('/usuarios', 'Gestión de usuarios'),
('/facturas', 'Gestión de facturas'),
('/clientes', 'Gestión de clientes'),
('/vendedores', 'Gestión de vendedores'),
('/personas', 'Gestión de personas'),
('/empresas', 'Gestión de empresas'),
('/productos', 'Gestión de productos'),
('/roles', 'Gestión de roles'),
('/permisos', 'Gestión de permisos (asignación rol-ruta)'),
('/permisos/crear', 'Crear permiso (POST)'),
('/permisos/eliminar', 'Eliminar permiso (POST)'),
('/rutas', 'Gestión de rutas del sistema'),
('/rutas/crear', 'Crear ruta (POST)'),
('/rutas/eliminar', 'Eliminar ruta (POST)');

-- Usuarios
INSERT INTO usuario (email, contrasena) VALUES
('admin@correo.com', '$2a$12$3UgI.Eof.FhzsYUWESI9n.qFaqkV2JPhvW3L/1GTKowNJnGaD8F.G'),
('vendedor1@correo.com', '$2a$12$Dgog4VaHqMzhliPVJy1BcOMd6.izEGNeRDtZ.O7SPmBLc6UVthVTG'),
('jefe@correo.com', 'jefe123'),
('cliente1@correo.com', 'cli123'),
('test_encript@correo.com', '$2a$11$Ci0J2yBltDgQHfjadgkl0OtbcF5pUf97vTq/4Xr0KEU/86l8ybjBe'),
('nuevo@correo.com', '$2a$11$cmtGBxllwc7MCzpnKVSWuumiOgCaG6PaKWcN1z9N0bjjnkobbFDzO');

-- Clientes (con IDENTITY_INSERT para IDs explícitos)
SET IDENTITY_INSERT cliente ON;
INSERT INTO cliente (id, credito, fkcodpersona, fkcodempresa) VALUES
(1, 520000, 'P001', 'E001'),
(2, 250000, 'P003', 'E002'),
(3, 400000, 'P005', 'E001'),
(5, 700000, 'P006', 'E001');
SET IDENTITY_INSERT cliente OFF;

-- Vendedores (con IDENTITY_INSERT para IDs explícitos)
SET IDENTITY_INSERT vendedor ON;
INSERT INTO vendedor (id, carnet, direccion, fkcodpersona) VALUES
(1, 1001, 'Calle 10 #5-33', 'P002'),
(2, 1002, 'Carrera 15 #7-20', 'P004'),
(3, 1003, 'Avenida 30 #18-09', 'P006');
SET IDENTITY_INSERT vendedor OFF;

-- Facturas (con IDENTITY_INSERT para IDs explícitos)
-- Nota: los totales se insertan como 0, pero como los triggers están
-- deshabilitados para la carga de datos semilla, insertamos los totales directamente.
-- Primero deshabilitamos los triggers para la carga de datos semilla.
DISABLE TRIGGER trg_prodfact_insert ON productosporfactura;
DISABLE TRIGGER trg_prodfact_update ON productosporfactura;
DISABLE TRIGGER trg_prodfact_delete ON productosporfactura;
GO

SET IDENTITY_INSERT factura ON;
INSERT INTO factura (numero, fecha, total, fkidcliente, fkidvendedor) VALUES
(1, '2025-12-03 12:57:19.2759200', 5000000, 1, 1),
(2, '2025-12-03 12:57:19.2759200', 1250000, 2, 2),
(3, '2025-12-03 12:57:19.2759200', 2030000, 3, 3),
(4, '2025-12-03 13:04:59.0286130', 950000, 1, 1),
(5, '2025-12-03 13:05:17.8743850', 2740000, 2, 2),
(6, '2025-12-03 13:05:35.0284600', 4850000, 3, 3);
SET IDENTITY_INSERT factura OFF;

-- Productos por factura (triggers deshabilitados, insertamos subtotales directamente)
INSERT INTO productosporfactura (fknumfactura, fkcodproducto, cantidad, subtotal) VALUES
(1, 'PR001', 2, 5000000),
(2, 'PR002', 1, 800000),
(2, 'PR003', 3, 450000),
(3, 'PR004', 5, 450000),
(3, 'PR005', 1, 1100000),
(3, 'PR006', 2, 480000),
(4, 'PR007', 1, 950000),
(5, 'PR007', 2, 1900000),
(5, 'PR008', 3, 840000),
(6, 'PR001', 1, 2500000),
(6, 'PR002', 2, 1600000),
(6, 'PR003', 5, 750000);

-- Rehabilitar triggers
ENABLE TRIGGER trg_prodfact_insert ON productosporfactura;
ENABLE TRIGGER trg_prodfact_update ON productosporfactura;
ENABLE TRIGGER trg_prodfact_delete ON productosporfactura;
GO

-- Roles por usuario
INSERT INTO rol_usuario (fkemail, fkidrol) VALUES
('admin@correo.com', 1),
('vendedor1@correo.com', 2),
('vendedor1@correo.com', 3),
('jefe@correo.com', 1),
('jefe@correo.com', 3),
('jefe@correo.com', 4),
('cliente1@correo.com', 5),
('test_encript@correo.com', 1),
('nuevo@correo.com', 1),
('nuevo@correo.com', 2),
('nuevo@correo.com', 3);

-- Rutas por rol
INSERT INTO rutarol (ruta, rol) VALUES
('/home', 'Administrador'),
('/usuarios', 'Administrador'),
('/facturas', 'Administrador'),
('/clientes', 'Administrador'),
('/vendedores', 'Administrador'),
('/personas', 'Administrador'),
('/empresas', 'Administrador'),
('/productos', 'Administrador'),
('/roles', 'Administrador'),
('/permisos', 'Administrador'),
('/permisos/crear', 'Administrador'),
('/permisos/eliminar', 'Administrador'),
('/rutas', 'Administrador'),
('/rutas/crear', 'Administrador'),
('/rutas/eliminar', 'Administrador'),
('/home', 'Vendedor'),
('/facturas', 'Vendedor'),
('/clientes', 'Vendedor'),
('/home', 'Cajero'),
('/facturas', 'Cajero'),
('/home', 'Contador'),
('/clientes', 'Contador'),
('/productos', 'Contador'),
('/home', 'Cliente'),
('/productos', 'Cliente');
GO
