namespace IotAnalytics.Models;

/// <summary>
/// Modelo de datos: cada objeto equivale a una fila de la tabla sensores.
/// </summary>
public class Sensor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string? Ubicacion { get; set; }
    public decimal? Valor { get; set; }
    public DateTime Fecha { get; set; }

    public Sensor() { }

    public Sensor(string nombre, string tipo, string? ubicacion, decimal valor)
    {
        Nombre = nombre;
        Tipo = tipo;
        Ubicacion = ubicacion;
        Valor = valor;
    }

    public override string ToString() =>
        $"{Id,-4} {Nombre,-10} {Tipo,-12} {Ubicacion,-12} {Valor,8:0.00}  {Fecha:yyyy-MM-dd HH:mm:ss}";
}
