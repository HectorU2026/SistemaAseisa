CREATE   PROCEDURE [dbo].[RegistrarFacturaCompraManual]
    @id_orden_compra INT,
    @id_empresa      INT,
    @id_proveedor    INT,
    @id_usuario      INT,
    @fecha_factura   DATETIME2,
    @id_termino_pago INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @fecha_factura = ISNULL(@fecha_factura, SYSUTCDATETIME());

    DECLARE @id_estado_orden_compra INT,
            @id_estado_facturada    INT,
            @id_estado_pendiente    INT;

    SELECT @id_estado_orden_compra = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Orden de Compra';
    SELECT @id_estado_facturada    = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Facturada';
    SELECT @id_estado_pendiente    = id_estado FROM dbo.ESTADO WHERE nombre_estado = 'Pendiente';

    IF @id_estado_orden_compra IS NULL
        THROW 50010, 'No existe el estado "Orden de Compra" en la tabla ESTADO.', 1;
    IF @id_estado_facturada IS NULL
        THROW 50011, 'No existe el estado "Facturada" en la tabla ESTADO.', 1;
    IF @id_estado_pendiente IS NULL
        THROW 50012, 'No existe el estado "Pendiente" en la tabla ESTADO.', 1;

    DECLARE @total             DECIMAL(18,2),
            @id_proveedor_real INT,
            @id_termino_oc     INT,
            @dias_credito_oc   INT,
            @estado_actual     INT;

    SELECT
        @total             = oc.total,
        @id_proveedor_real = oc.id_proveedor,
        @id_termino_oc     = oc.id_termino_pago,
        @dias_credito_oc   = oc.dias_credito,
        @estado_actual     = oc.id_estado
    FROM dbo.ORDEN_COMPRA oc WITH (UPDLOCK, HOLDLOCK)
    WHERE oc.id_orden_compra = @id_orden_compra
      AND oc.id_empresa      = @id_empresa;

    IF @id_proveedor_real IS NULL
        THROW 50020, 'La orden de compra no existe o no pertenece a la empresa.', 1;

    IF @id_proveedor <> @id_proveedor_real
        THROW 50021, 'El proveedor enviado no coincide con el de la orden de compra.', 1;

    IF @total IS NULL OR @total <= 0
        THROW 50022, 'El total de la orden de compra debe ser mayor que cero.', 1;

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

    BEGIN TRY
        BEGIN TRANSACTION;

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

        UPDATE dbo.ORDEN_COMPRA
        SET id_estado          = @id_estado_facturada,
            fecha_modificacion = SYSUTCDATETIME()
        WHERE id_orden_compra = @id_orden_compra
          AND id_empresa      = @id_empresa
          AND id_estado       = @id_estado_orden_compra;

        IF @@ROWCOUNT = 0
            THROW 50026, 'La orden de compra cambió de estado durante la operación. Intente nuevamente.', 1;

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
                @id_orden_compra      AS id_orden_compra,
                @id_proveedor_real    AS id_proveedor,
                @id_cuenta_pagar      AS id_cuenta_pagar,
                @total                AS monto_total,
                @fecha_factura        AS fecha_factura,
                @fecha_vencimiento    AS fecha_vencimiento,
                @id_termino_final     AS id_termino_pago,
                @id_estado_pendiente  AS id_estado
             FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
            SYSUTCDATETIME()
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
        'Pendiente'               AS NombreEstado;
END;