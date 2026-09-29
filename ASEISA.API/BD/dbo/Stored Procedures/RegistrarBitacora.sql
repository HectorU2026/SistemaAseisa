
  CREATE   PROCEDURE RegistrarBitacora
    @id_usuario     INT,
    @id_empresa     INT,
    @tabla_afectada VARCHAR(150),
    @registro_id    VARCHAR(100) = NULL,
    @accion         VARCHAR(100),
    @valor_anterior VARCHAR(MAX) = NULL,
    @valor_nuevo    VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO BITACORA
    (
        id_usuario, id_empresa, tabla_afectada,
        registro_id, accion, valor_anterior, valor_nuevo
    )
    VALUES
    (
        @id_usuario, @id_empresa, @tabla_afectada,
        @registro_id, @accion, @valor_anterior, @valor_nuevo
    );
END;