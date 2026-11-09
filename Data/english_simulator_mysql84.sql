-- EnglishSimulator | Esquema inicial basado en el diagrama del equipo
CREATE DATABASE IF NOT EXISTS english_simulator
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE english_simulator;

CREATE TABLE IF NOT EXISTS usuarios (
    id_usuario INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(150) NOT NULL UNIQUE,
    contrasena VARCHAR(255) NOT NULL COMMENT 'Almacenar hash de contrasena, nunca texto plano',
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS niveles (
    id_nivel INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre_nivel VARCHAR(40) NOT NULL UNIQUE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS tipos_pruebas (
    id_tipo_prueba INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre_prueba VARCHAR(50) NOT NULL UNIQUE,
    max_intentos TINYINT UNSIGNED NOT NULL,
    cantidad_preguntas TINYINT UNSIGNED NOT NULL,
    CONSTRAINT chk_tipos_max_intentos CHECK (max_intentos > 0),
    CONSTRAINT chk_tipos_cantidad CHECK (cantidad_preguntas > 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS preguntas (
    id_pregunta INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_nivel INT UNSIGNED NOT NULL,
    texto_pregunta TEXT NOT NULL,
    segundos_tiempo SMALLINT UNSIGNED NOT NULL DEFAULT 60,
    CONSTRAINT fk_preguntas_niveles FOREIGN KEY (id_nivel)
        REFERENCES niveles(id_nivel),
    CONSTRAINT chk_preguntas_tiempo CHECK (segundos_tiempo > 0),
    INDEX idx_preguntas_nivel (id_nivel)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS opciones (
    id_opcion INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_pregunta INT UNSIGNED NOT NULL,
    texto_opcion VARCHAR(255) NOT NULL,
    es_correcta BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_opciones_preguntas FOREIGN KEY (id_pregunta)
        REFERENCES preguntas(id_pregunta),
    INDEX idx_opciones_pregunta (id_pregunta)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS intentos_examen (
    id_intento INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT UNSIGNED NOT NULL,
    id_tipo_prueba INT UNSIGNED NOT NULL,
    porcentaje_obtenido DECIMAL(5,2) NULL,
    puntaje_requerido DECIMAL(5,2) NOT NULL DEFAULT 70.00,
    resultado VARCHAR(20) NOT NULL DEFAULT 'EN_PROCESO',
    fecha_intento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_intentos_usuarios FOREIGN KEY (id_usuario)
        REFERENCES usuarios(id_usuario),
    CONSTRAINT fk_intentos_tipo FOREIGN KEY (id_tipo_prueba)
        REFERENCES tipos_pruebas(id_tipo_prueba),
    CONSTRAINT chk_intentos_porcentaje CHECK (porcentaje_obtenido IS NULL OR porcentaje_obtenido BETWEEN 0 AND 100),
    CONSTRAINT chk_intentos_requerido CHECK (puntaje_requerido BETWEEN 0 AND 100),
    CONSTRAINT chk_intentos_resultado CHECK (resultado IN ('EN_PROCESO','APROBADO','REPROBADO','ABANDONADO')),
    INDEX idx_intentos_usuario_tipo (id_usuario, id_tipo_prueba)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS respuestas_intento (
    id_respuesta INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_intento INT UNSIGNED NOT NULL,
    id_pregunta INT UNSIGNED NOT NULL,
    id_opcion INT UNSIGNED NULL COMMENT 'NULL = sin respuesta o tiempo agotado',
    es_correcta BOOLEAN NOT NULL DEFAULT FALSE,
    tiempo_respuesta SMALLINT UNSIGNED NULL COMMENT 'Segundos transcurridos para responder',
    CONSTRAINT fk_respuestas_intentos FOREIGN KEY (id_intento)
        REFERENCES intentos_examen(id_intento),
    CONSTRAINT fk_respuestas_preguntas FOREIGN KEY (id_pregunta)
        REFERENCES preguntas(id_pregunta),
    CONSTRAINT fk_respuestas_opciones FOREIGN KEY (id_opcion)
        REFERENCES opciones(id_opcion),
    CONSTRAINT uq_respuesta_por_pregunta UNIQUE (id_intento, id_pregunta),
    INDEX idx_respuestas_pregunta (id_pregunta),
    INDEX idx_respuestas_opcion (id_opcion)
) ENGINE=InnoDB;

INSERT INTO niveles (nombre_nivel) VALUES
('Básico'), ('Intermedio'), ('Avanzado')
ON DUPLICATE KEY UPDATE nombre_nivel = VALUES(nombre_nivel);

INSERT INTO tipos_pruebas (nombre_prueba, max_intentos, cantidad_preguntas) VALUES
('Práctica', 5, 20),
('Final', 2, 40)
ON DUPLICATE KEY UPDATE
 max_intentos = VALUES(max_intentos),
 cantidad_preguntas = VALUES(cantidad_preguntas);

-- Verificación
SHOW TABLES;
SELECT * FROM niveles;
SELECT * FROM tipos_pruebas;
