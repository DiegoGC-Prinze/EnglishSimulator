-- EnglishSimulator
-- Actualización del esquema original de MySQL 8.4
-- Ejecutar una sola vez después del script inicial.

ALTER TABLE intentos_examen
    ADD COLUMN segundos_restantes
        SMALLINT UNSIGNED NOT NULL DEFAULT 60,
    ADD COLUMN ultima_sincronizacion
        DATETIME(6) NULL;

ALTER TABLE respuestas_intento
    ADD COLUMN orden_pregunta
        TINYINT UNSIGNED NOT NULL,
    ADD UNIQUE KEY uq_intento_orden
        (id_intento, orden_pregunta);
