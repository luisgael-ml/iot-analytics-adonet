-- Tabla de sensores del proyecto IoT Analytics Dashboard
CREATE TABLE IF NOT EXISTS sensores (
    id        SERIAL PRIMARY KEY,           -- identificador autoincremental
    nombre    VARCHAR(50)  NOT NULL,        -- nombre del sensor
    tipo      VARCHAR(30)  NOT NULL,        -- temperatura, humedad, presión...
    ubicacion VARCHAR(50),                  -- área de la planta
    valor     NUMERIC(10,2),                -- última lectura
    fecha     TIMESTAMP DEFAULT NOW()       -- fecha de registro
);

-- Datos iniciales
INSERT INTO sensores (nombre, tipo, ubicacion, valor) VALUES
('SEN-T01', 'Temperatura', 'Línea 1', 24.50),
('SEN-H01', 'Humedad', 'Almacén', 55.00),
('SEN-P01', 'Presión', 'Caldera', 101.30);
