using System.Data;
using IotAnalytics.Models;
using Npgsql;

namespace IotAnalytics.Data;

/// <summary>
/// Repositorio: concentra todas las operaciones SQL sobre la tabla sensores
/// usando los dos modelos de ADO.NET:
///   - Conectado:    NpgsqlConnection + NpgsqlCommand + NpgsqlDataReader
///   - Desconectado: NpgsqlDataAdapter + DataTable
/// </summary>
public class SensorRepository
{
    // ---------- MODELO CONECTADO ----------

    /// <summary>CREATE: inserta un sensor y devuelve el id generado.</summary>
    public int Insertar(Sensor s)
    {
        const string sql = @"INSERT INTO sensores (nombre, tipo, ubicacion, valor)
                             VALUES (@nombre, @tipo, @ubicacion, @valor)
                             RETURNING id";
        using var con = Conexion.Crear();
        con.Open();
        using var cmd = new NpgsqlCommand(sql, con);
        // Parámetros: evitan la inyección SQL
        cmd.Parameters.AddWithValue("nombre", s.Nombre);
        cmd.Parameters.AddWithValue("tipo", s.Tipo);
        cmd.Parameters.AddWithValue("ubicacion", (object?)s.Ubicacion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("valor", (object?)s.Valor ?? DBNull.Value);
        // ExecuteScalar devuelve la primera columna de la primera fila (el id)
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>READ: devuelve todos los sensores leyendo fila por fila con un DataReader.</summary>
    public List<Sensor> Listar()
    {
        const string sql = "SELECT id, nombre, tipo, ubicacion, valor, fecha FROM sensores ORDER BY id";
        var lista = new List<Sensor>();
        using var con = Conexion.Crear();
        con.Open();
        using var cmd = new NpgsqlCommand(sql, con);
        using NpgsqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())                       // avanza una fila (solo hacia adelante)
        {
            lista.Add(new Sensor
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Tipo = reader.GetString(2),
                Ubicacion = reader.IsDBNull(3) ? null : reader.GetString(3),
                Valor = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                Fecha = reader.GetDateTime(5)
            });
        }
        return lista;
    }

    /// <summary>UPDATE: actualiza la lectura de un sensor. Devuelve true si existía.</summary>
    public bool ActualizarValor(int id, decimal nuevoValor)
    {
        const string sql = "UPDATE sensores SET valor = @valor, fecha = NOW() WHERE id = @id";
        using var con = Conexion.Crear();
        con.Open();
        using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("valor", nuevoValor);
        cmd.Parameters.AddWithValue("id", id);
        return cmd.ExecuteNonQuery() > 0;           // filas afectadas
    }

    /// <summary>DELETE: elimina un sensor. Devuelve true si existía.</summary>
    public bool Eliminar(int id)
    {
        const string sql = "DELETE FROM sensores WHERE id = @id";
        using var con = Conexion.Crear();
        con.Open();
        using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    /// <summary>
    /// Inserta varios sensores dentro de una transacción:
    /// se guardan todos o ninguno (Commit / Rollback).
    /// </summary>
    public int InsertarLote(IEnumerable<Sensor> sensores)
    {
        const string sql = @"INSERT INTO sensores (nombre, tipo, ubicacion, valor)
                             VALUES (@nombre, @tipo, @ubicacion, @valor)";
        using var con = Conexion.Crear();
        con.Open();
        using NpgsqlTransaction tx = con.BeginTransaction();
        try
        {
            int total = 0;
            foreach (var s in sensores)
            {
                using var cmd = new NpgsqlCommand(sql, con, tx);
                cmd.Parameters.AddWithValue("nombre", s.Nombre);
                cmd.Parameters.AddWithValue("tipo", s.Tipo);
                cmd.Parameters.AddWithValue("ubicacion", (object?)s.Ubicacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("valor", (object?)s.Valor ?? DBNull.Value);
                total += cmd.ExecuteNonQuery();
            }
            tx.Commit();                            // confirma todo el lote
            return total;
        }
        catch
        {
            tx.Rollback();                          // deshace todo si algo falla
            throw;
        }
    }

    /// <summary>Consulta al servidor si la conexión actual va cifrada.</summary>
    public string EstadoSSL()
    {
        const string sql = "SELECT ssl, version FROM pg_stat_ssl WHERE pid = pg_backend_pid()";
        using var con = Conexion.Crear();
        con.Open();
        using var cmd = new NpgsqlCommand(sql, con);
        using var reader = cmd.ExecuteReader();
        return reader.Read() && reader.GetBoolean(0) ? $"sí ({reader.GetString(1)})" : "no";
    }

    // ---------- MODELO DESCONECTADO ----------

    /// <summary>
    /// READ desconectado: el DataAdapter abre la conexión, llena un DataTable
    /// en memoria y la cierra. Los datos se pueden recorrer sin conexión abierta.
    /// </summary>
    public DataTable ListarDesconectado()
    {
        const string sql = "SELECT id, nombre, tipo, ubicacion, valor FROM sensores ORDER BY id";
        using var con = Conexion.Crear();
        using var adapter = new NpgsqlDataAdapter(sql, con);
        var tabla = new DataTable("sensores");
        adapter.Fill(tabla);                        // abre, consulta y cierra la conexión
        return tabla;
    }
}
