using Npgsql;

namespace IotAnalytics.Data;

/// <summary>
/// Crea las conexiones ADO.NET (NpgsqlConnection) hacia PostgreSQL.
/// - Las credenciales se leen de variables de entorno (no están en el código).
/// - La conexión viaja cifrada con SSL/TLS.
/// - Npgsql administra un pool de conexiones automáticamente.
/// </summary>
public static class Conexion
{
    public static string CadenaConexion()
    {
        string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Falta la variable de entorno DB_PASSWORD");

        // El builder arma la cadena de conexión sin concatenar texto a mano
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"),
            Database = "iotanalytics",
            Username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
            Password = password,
            // Require: la conexión se rechaza si el servidor no acepta SSL
            SslMode = Enum.Parse<SslMode>(Environment.GetEnvironmentVariable("DB_SSLMODE") ?? "Require", true),
            // Pool de conexiones: se reutilizan en lugar de abrir una nueva cada vez
            Pooling = true,
            MinPoolSize = 1,
            MaxPoolSize = 20
        };
        return builder.ConnectionString;
    }

    /// <summary>Devuelve una conexión nueva (cerrada). Se abre con Open().</summary>
    public static NpgsqlConnection Crear() => new NpgsqlConnection(CadenaConexion());

    /// <summary>
    /// Espera a que PostgreSQL acepte conexiones (útil en Docker y Kubernetes,
    /// donde la aplicación puede arrancar antes que la base de datos).
    /// </summary>
    public static bool EsperarDisponible(int intentos = 30)
    {
        for (int i = 1; i <= intentos; i++)
        {
            try
            {
                using var con = Crear();
                con.Open();
                return true;
            }
            catch (Exception e) when (i < intentos)
            {
                Console.WriteLine($"Esperando a PostgreSQL ({i}/{intentos}): {e.Message}");
                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                Console.WriteLine($"No se pudo conectar: {e.Message}");
            }
        }
        return false;
    }
}
