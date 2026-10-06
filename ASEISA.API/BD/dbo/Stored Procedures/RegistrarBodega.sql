CREATE PROCEDURE RegistrarBodega
    @id_empresa INT,
    @codigo     VARCHAR(50),
    @nombre     VARCHAR(150),
    @ubicacion  VARCHAR(300),
    @id_estado  INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM BODEGA
        WHERE LTRIM(RTRIM(nombre)) = LTRIM(RTRIM(@nombre))
          AND LTRIM(RTRIM(ISNULL(ubicacion, ''))) = LTRIM(RTRIM(ISNULL(@ubicacion, '')))
    )
    BEGIN
        SELECT -1;
        RETURN;
    END;

    INSERT INTO BODEGA
    (
        id_empresa,
        codigo,
        nombre,
        ubicacion,
        id_estado
    )
    VALUES
    (
        @id_empresa,
        @codigo,
        @nombre,
        @ubicacion,
        @id_estado
    );

    SELECT 1;
END;
