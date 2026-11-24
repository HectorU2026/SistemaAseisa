



CREATE   PROCEDURE [dbo].[RegistrarFacturaCompraManual]
    @id_orden_compra          INT,
    @id_empresa               INT,
    @id_proveedor             INT,
    @id_usuario               INT,
    @fecha_factura            DATETIME2,
    @id_termino_pago          INT,
    @numero_factura_proveedor VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @fecha_factura = ISNULL(@fecha_factura, SYSDATETIME());

    DECLARE @id_estado_orden_compra INT,
            @id_estado_facturada    INT,
            @id_estado_pendiente    INT,
            @id_estado_activo       INT;

    SELECT @id_estado_orden_compra = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Orden de Compra';
    SELECT @id_estado_facturada    = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Facturada';
    SELECT @id_estado_pendiente    = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Pendiente';
    SELECT @id_estado_activo       = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Activo';

    IF @id_estado_orden_compra IS NULL OR @id_estado_facturada IS NULL
       OR @id_estado_pendiente IS NULL OR @id_estado_activo IS NULL
        THROW 50010, 'Faltan estados configurados en la tabla ESTADO.', 1;

    DECLARE @total             DECIMAL(18,2),
            @id_proveedor_real INT,
            @id_termino_oc     INT,
            @dias_credito_oc   INT,
            @estado_actual     INT,
            @llegada_confirmada BIT;

    SELECT
        @total              = oc.total,
        @id_proveedor_real  = oc.id_proveedor,
        @id_termino_oc      = oc.id_termino_pago,
        @dias_credito_oc    = oc.dias_credito,
        @estado_actual      = oc.id_estado,
        @llegada_confirmada = oc.confirmacion_llegada
    FROM dbo.ORDEN_COMPRA oc WITH (UPDLOCK, HOLDLOCK)
    WHERE oc.id_orden_compra = @id_orden_compra
      AND oc.id_empresa      = @id_empresa;

    IF @id_proveedor_real IS NULL
        THROW 50020, 'La orden de compra no existe o no pertenece a la empresa.', 1;

    IF @id_proveedor <> @id_proveedor_real
        THROW 50021, 'El proveedor enviado no coincide con el de la orden de compra.', 1;

    IF @total IS NULL OR @total <= 0
        THROW 50022, 'El total de la orden de compra debe ser mayor que cero.', 1;

    IF @llegada_confirmada IS NULL OR @llegada_confirmada = 0
        THROW 50027, 'No se puede facturar: el servicio/producto aún no ha sido recibido.', 1;

    IF @estado_actual <> @id_estado_orden_compra
        THROW 50023, 'La compra no está en estado "Orden de Compra", no se puede facturar.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.CUENTA_POR_PAGAR cpp WITH (NOLOCK)
        INNER JOIN dbo.ESTADO e WITH (NOLOCK) ON e.id_estado = cpp.id_estado
        WHERE cpp.id_orden_compra = @id_orden_compra
          AND e.nombre_estado <> 'Eliminado'
    )
        THROW 50024, 'Ya existe una cuenta por pagar registrada para esta orden de compra.', 1;

    DECLARE @dias_credito      INT = 0,
            @fecha_vencimiento DATETIME2,
            @id_termino_final  INT;

    SET @id_termino_final =
        CASE
            WHEN @id_termino_pago IS NOT NULL AND @id_termino_pago > 0 THEN @id_termino_pago
            ELSE @id_termino_oc
        END;

    IF @id_termino_final IS NOT NULL
    BEGIN
        SELECT @dias_credito = tp.dias_credito
        FROM dbo.TERMINO_PAGO tp WITH (NOLOCK)
        WHERE tp.id_termino_pago = @id_termino_final
          AND tp.id_empresa      = @id_empresa;

        IF @dias_credito IS NULL
            SET @dias_credito = ISNULL(@dias_credito_oc, 0);
    END
    ELSE
        SET @dias_credito = ISNULL(@dias_credito_oc, 0);

    IF @dias_credito < 0
        THROW 50025, 'Los días de crédito no pueden ser negativos.', 1;

    SET @fecha_vencimiento = DATEADD(DAY, @dias_credito, @fecha_factura);

    DECLARE @id_cuenta_pagar INT;
    DECLARE @numero_asiento  VARCHAR(50);
    DECLARE @id_asiento      INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Crear CUENTA_POR_PAGAR
        INSERT INTO dbo.CUENTA_POR_PAGAR
        (
            id_orden_compra, id_proveedor, fecha_emision, fecha_vencimiento,
            monto_original, saldo_pendiente, id_estado, id_empresa
        )
        VALUES
        (
            @id_orden_compra, @id_proveedor_real, @fecha_factura, @fecha_vencimiento,
            @total, @total, @id_estado_pendiente, @id_empresa
        );

        SET @id_cuenta_pagar = SCOPE_IDENTITY();

        -- 2. Actualizar estado de la OC
        UPDATE dbo.ORDEN_COMPRA
        SET id_estado          = @id_estado_facturada,
            fecha_modificacion = SYSDATETIME()
        WHERE id_orden_compra = @id_orden_compra
          AND id_empresa      = @id_empresa
          AND id_estado       = @id_estado_orden_compra;

        IF @@ROWCOUNT = 0
            THROW 50026, 'La orden de compra cambió de estado durante la operación. Intente nuevamente.', 1;

        -- 3. Generar Asiento Contable
        SET @numero_asiento = CONCAT('AST-CXP-', CONVERT(VARCHAR(11), @id_orden_compra));

        INSERT INTO dbo.ASIENTO_CONTABLE
            (id_empresa, numero_asiento, fecha_asiento, descripcion, tipo_origen,
             total_debe, total_haber, id_estado, id_usuario, fecha_creacion)
        VALUES
            (@id_empresa, @numero_asiento, CAST(@fecha_factura AS DATE),
             CONCAT('Factura de compra OC ', @id_orden_compra,
                    CASE WHEN @numero_factura_proveedor IS NOT NULL
                         THEN CONCAT(' | Factura Proveedor: ', @numero_factura_proveedor)
                         ELSE '' END),
             'Compra', @total, @total, @id_estado_activo, @id_usuario, SYSDATETIME());

        SET @id_asiento = SCOPE_IDENTITY();

        -- 3.1 DEBE: Gasto (una línea por cada cuenta de gasto distinta)
        IF EXISTS (
            SELECT 1 
            FROM DETALLE_ORDEN_COMPRA d
            INNER JOIN PRODUCTO p ON p.id_producto = d.id_producto
            WHERE d.id_orden_compra = @id_orden_compra AND p.id_cuenta_gasto IS NULL
        )
            THROW 50028, 'Uno o más productos no tienen configurada la cuenta de gasto.', 1;

        INSERT INTO dbo.DETALLE_ASIENTO (id_asiento, id_cuenta_contable, descripcion, debe, haber)
        SELECT 
            @id_asiento,
            p.id_cuenta_gasto,
            CONCAT('Gasto por compra OC ', @id_orden_compra),
            SUM(ISNULL(doc.total, doc.subtotal)),
            0
        FROM DETALLE_ORDEN_COMPRA doc
        INNER JOIN PRODUCTO p ON p.id_producto = doc.id_producto
        WHERE doc.id_orden_compra = @id_orden_compra
        GROUP BY p.id_cuenta_gasto
        HAVING SUM(ISNULL(doc.total, doc.subtotal)) > 0;

        -- 3.2 HABER: Cuentas por Pagar
        INSERT INTO dbo.DETALLE_ASIENTO (id_asiento, id_cuenta_contable, descripcion, debe, haber)
        SELECT 
            @id_asiento,
            id_cuenta_contable,
            CONCAT('Cuenta por pagar OC ', @id_orden_compra),
            0,
            @total
        FROM dbo.CUENTA_CONTABLE
        WHERE id_empresa = @id_empresa AND nombre LIKE '%Cuentas por Pagar%';

        -- 4. Bitácora
        INSERT INTO dbo.BITACORA
        (
            id_usuario, id_empresa, tabla_afectada, registro_id,
            accion, valor_anterior, valor_nuevo, fecha_accion
        )
        VALUES
        (
            @id_usuario, @id_empresa, 'CUENTA_POR_PAGAR',
            CAST(@id_cuenta_pagar AS VARCHAR(20)),
            'CREAR_FACTURA_COMPRA_MANUAL',
            NULL,
            (SELECT
                @id_orden_compra          AS id_orden_compra,
                @id_proveedor_real        AS id_proveedor,
                @id_cuenta_pagar          AS id_cuenta_pagar,
                @total                    AS monto_total,
                @fecha_factura            AS fecha_factura,
                @fecha_vencimiento        AS fecha_vencimiento,
                @id_termino_final         AS id_termino_pago,
                @id_estado_pendiente      AS id_estado,
                @numero_factura_proveedor AS numero_factura_proveedor,
                @numero_asiento           AS numero_asiento
             FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
            SYSDATETIME()
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    SELECT
        @id_cuenta_pagar          AS IdCuentaPagar,
        @id_orden_compra          AS IdOrdenCompra,
        @id_proveedor_real        AS IdProveedor,
        @total                    AS MontoOriginal,
        @total                    AS SaldoPendiente,
        @fecha_vencimiento        AS FechaVencimiento,
        @id_estado_pendiente      AS IdEstado,
        'Pendiente'               AS NombreEstado,
        @numero_asiento           AS NumeroAsiento;
END;