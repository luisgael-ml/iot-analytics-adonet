using System.Data;
using System.Globalization;
using IotAnalytics.Data;
using IotAnalytics.Models;

// Punto de entrada del módulo IoT Analytics Dashboard (ADO.NET + PostgreSQL).
//   dotnet run            -> menú interactivo
//   dotnet run -- --demo  -> ejecuta todas las operaciones automáticamente (usado en Kubernetes)

Console.OutputEncoding = System.Text.Encoding.UTF8;
var repo = new SensorRepository();
var inv = CultureInfo.InvariantCulture;   // acepta "26.80" con punto decimal

Console.WriteLine("=== IOT ANALYTICS DASHBOARD · ADO.NET + PostgreSQL ===");
if (Conexion.EsperarDisponible())
{
    try { Console.WriteLine($"Conexión establecida. Cifrado SSL: {repo.EstadoSSL()}"); }
    catch (Exception e) { Console.WriteLine($"Error al verificar SSL: {e.Message}"); }
}

if (args.Contains("--demo"))
{
    Demo();
    return;
}

int opcion;
do
{
    Console.WriteLine();
    Console.WriteLine("1. Insertar sensor");
    Console.WriteLine("2. Listar sensores (modo conectado: DataReader)");
    Console.WriteLine("3. Actualizar lectura");
    Console.WriteLine("4. Eliminar sensor");
    Console.WriteLine("5. Insertar lote con transacción");
    Console.WriteLine("6. Listar sensores (modo desconectado: DataTable)");
    Console.WriteLine("0. Salir");
    Console.Write("Opción: ");
    if (!int.TryParse(Console.ReadLine(), out opcion)) { Console.WriteLine("Opción inválida."); opcion = -1; continue; }

    try
    {
        switch (opcion)
        {
            case 1:
                string nombre = Leer("Nombre: ");
                string tipo = Leer("Tipo: ");
                string ubicacion = Leer("Ubicación: ");
                decimal valor = decimal.Parse(Leer("Valor: "), inv);
                int id = repo.Insertar(new Sensor(nombre, tipo, ubicacion, valor));
                Console.WriteLine($"Sensor insertado con id {id}.");
                break;
            case 2:
                Imprimir(repo.Listar());
                break;
            case 3:
                int idAct = int.Parse(Leer("ID del sensor: "));
                decimal nuevo = decimal.Parse(Leer("Nuevo valor: "), inv);
                Console.WriteLine(repo.ActualizarValor(idAct, nuevo) ? "Lectura actualizada." : "ID no encontrado.");
                break;
            case 4:
                int idDel = int.Parse(Leer("ID del sensor: "));
                Console.WriteLine(repo.Eliminar(idDel) ? "Sensor eliminado." : "ID no encontrado.");
                break;
            case 5:
                int n = repo.InsertarLote(LoteEjemplo());
                Console.WriteLine($"Transacción confirmada: {n} sensores insertados.");
                break;
            case 6:
                ImprimirTabla(repo.ListarDesconectado());
                break;
            case 0:
                Console.WriteLine("Hasta luego.");
                break;
            default:
                Console.WriteLine("Opción inválida.");
                break;
        }
    }
    catch (FormatException)
    {
        Console.WriteLine("Dato con formato incorrecto.");
    }
    catch (Exception e)
    {
        // Errores de conexión o de SQL se muestran sin cerrar el programa
        Console.WriteLine($"Error: {e.Message}");
    }
} while (opcion != 0);

// ---------- Modo demostración (sin interacción) ----------
void Demo()
{
    Paso("P2 · READ con DataReader");
    Imprimir(repo.Listar());

    Paso("P3 · CREATE con ExecuteScalar (RETURNING id)");
    int nuevoId = repo.Insertar(new Sensor("SEN-E01", "Energía", "Línea 2", 12.75m));
    Console.WriteLine($"Sensor insertado con id {nuevoId}.");

    Paso("P4 · UPDATE con ExecuteNonQuery");
    Console.WriteLine(repo.ActualizarValor(nuevoId, 15.40m) ? $"Lectura del id {nuevoId} actualizada a 15.40." : "ID no encontrado.");

    Paso("P5 · Transacción (Commit de un lote)");
    Console.WriteLine($"Transacción confirmada: {repo.InsertarLote(LoteEjemplo())} sensores insertados.");

    Paso("P6 · DELETE con ExecuteNonQuery");
    Console.WriteLine(repo.Eliminar(nuevoId) ? $"Sensor {nuevoId} eliminado." : "ID no encontrado.");

    Paso("P7 · Id inexistente");
    Console.WriteLine(repo.ActualizarValor(9999, 1m) ? "Lectura actualizada." : "ID 9999 no encontrado.");

    Paso("P8 · READ desconectado con DataAdapter + DataTable");
    ImprimirTabla(repo.ListarDesconectado());

    Console.WriteLine();
    Console.WriteLine("Demostración terminada correctamente.");
}

// ---------- Utilidades ----------
static string Leer(string etiqueta)
{
    Console.Write(etiqueta);
    return Console.ReadLine() ?? "";
}

static void Paso(string titulo)
{
    Console.WriteLine();
    Console.WriteLine($"--- {titulo} ---");
}

static List<Sensor> LoteEjemplo() => new()
{
    new Sensor("SEN-M01", "Mantenimiento", "Caldera", 1m),
    new Sensor("SEN-V01", "Vibración", "Prensa 3", 4.20m)
};

static void Imprimir(List<Sensor> sensores)
{
    Console.WriteLine($"{"ID",-4} {"NOMBRE",-10} {"TIPO",-12} {"UBICACIÓN",-12} {"VALOR",8}  FECHA");
    sensores.ForEach(s => Console.WriteLine(s));
}

static void ImprimirTabla(DataTable tabla)
{
    Console.WriteLine($"DataTable '{tabla.TableName}': {tabla.Rows.Count} filas en memoria");
    Console.WriteLine($"{"ID",-4} {"NOMBRE",-10} {"TIPO",-14} {"UBICACIÓN",-12} {"VALOR",8}");
    foreach (DataRow fila in tabla.Rows)
    {
        Console.WriteLine($"{fila["id"],-4} {fila["nombre"],-10} {fila["tipo"],-14} {fila["ubicacion"],-12} {fila["valor"],8}");
    }
}
