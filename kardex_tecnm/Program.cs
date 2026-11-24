using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PracticaKardex
{
    class Program
    {
        static string rutaXsd = "Kardex.xsd";
        static string archivoValido = "kardex_valido.xml";
        static string archivoInvalido = "kardex_invalido.xml";

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE CAPTURA DE CALIFICACIONES ===");
                Console.WriteLine("1. Registrar Calificaciones");
                Console.WriteLine("2. Validacion de kardex");
                Console.WriteLine("3. Generar archivo inválido de prueba y validarlo");
                Console.WriteLine("4. promedio y materias aprobadas");
                Console.WriteLine("5. Salir");
                Console.Write("\nSeleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": CapturarYSerializar(); break;
                    case "2": ValidarArchivoInteractivo(); break;
                    case "3": GenerarInvalidoYValidar(); break;
                    case "4": ParsearYConsultar(); break;
                    case "5": salir = true; break;
                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ---------- OPCIÓN 1: Captura por teclado y serialización ----------
        static void CapturarYSerializar()
        {
            Console.Clear();
            Console.WriteLine("--- CAPTURA DE DATOS DEL ESTUDIANTE ---");

            var expediente = new ExpedienteAcademico();

            Console.Write("Semestre (ej. AGO-DIC-2026): ");
            expediente.Periodo = Console.ReadLine();

            Console.Write("Número de control: ");
            expediente.NumeroControl = Console.ReadLine();

            Console.Write("Nombre completo: ");
            expediente.NombreCompleto = Console.ReadLine();

            Console.Write("Carrera (Ingeniería en Informática / Ingeniería en Sistemas Computacionales / Ingeniería Industrial): ");
            expediente.Carrera = Console.ReadLine();

            Console.Write("Semestre (1-12): ");
            expediente.Semestre = int.Parse(Console.ReadLine());

            Console.Write("\n¿Cuántas materias desea registrar?: ");
            int cantidad = int.Parse(Console.ReadLine());

            for (int i = 0; i < cantidad; i++)
            {
                Console.WriteLine($"\n--- Materia {i + 1} ---");
                var materia = new Materia();

                Console.Write("Clave (ej. IFD-1010): ");
                materia.Clave = Console.ReadLine();

                Console.Write("Nombre de la materia: ");
                materia.NombreMateria = Console.ReadLine();

                Console.Write("Créditos (4-6): ");
                materia.Creditos = int.Parse(Console.ReadLine());

                Console.Write("Calificación (0.0 - 100.0): ");
                materia.Calificacion = decimal.Parse(Console.ReadLine());

                Console.Write("¿Aprobada? (Si/No): ");
                materia.Aprobada = bool.Parse(Console.ReadLine());

                expediente.Materias.Add(materia);
            }

            // Serialización automática
            XmlSerializer serializer = new XmlSerializer(typeof(ExpedienteAcademico));
            using (TextWriter writer = new StreamWriter(archivoValido))
            {
                serializer.Serialize(writer, expediente);
            }

            Console.WriteLine($"\n✅ Archivo '{archivoValido}' generado correctamente.");
            Console.WriteLine("Presione una tecla para continuar...");
            Console.ReadKey();
        }

        // ---------- OPCIÓN 2: Validación interactiva ----------
        static void ValidarArchivoInteractivo()
        {
            Console.Clear();
            Console.Write("Ingrese la ruta del archivo XML a validar (Enter para usar kardex_valido.xml): ");
            string ruta = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ruta)) ruta = archivoValido;

            ValidarXml(ruta, rutaXsd);

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // ---------- OPCIÓN 3: Generar inválido y validar ----------
        static void GenerarInvalidoYValidar()
        {
            Console.Clear();
            Console.WriteLine("--- GENERANDO ARCHIVO INVÁLIDO DE PRUEBA ---");

            // Creamos un XML que viola 3 reglas: calificación 150, número de control con letras, carrera inexistente
            string xmlInvalido = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ExpedienteAcademico periodo=""AGO-DIC-2026"">
  <NumeroControl>ABC123</NumeroControl>
  <NombreCompleto>Juan Pérez</NombreCompleto>
  <Carrera>Medicina</Carrera>
  <Semestre>5</Semestre>
  <Materias>
    <Materia clave=""IFD-1010"">
      <NombreMateria>Programación</NombreMateria>
      <Creditos>4</Creditos>
      <Calificacion>150</Calificacion>
      <Aprobada>true</Aprobada>
    </Materia>
  </Materias>
</ExpedienteAcademico>";

            File.WriteAllText(archivoInvalido, xmlInvalido);
            Console.WriteLine($"Archivo '{archivoInvalido}' creado con 3 violaciones intencionales.");

            ValidarXml(archivoInvalido, rutaXsd);

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // ---------- OPCIÓN 4: Parsing con LINQ to XML ----------
        static void ParsearYConsultar()
        {
            Console.Clear();
            Console.WriteLine("--- CONSULTA DE DATOS DEL KARDEX VÁLIDO ---");

            if (!File.Exists(archivoValido))
            {
                Console.WriteLine($"❌ No existe el archivo '{archivoValido}'. Genérelo primero con la opción 1.");
                Console.ReadKey();
                return;
            }

            XDocument doc = XDocument.Load(archivoValido);

            // Extraer datos del estudiante
            string nombre = doc.Root.Element("NombreCompleto")?.Value;
            string control = doc.Root.Element("NumeroControl")?.Value;

            // Obtener todas las calificaciones y calcular el promedio
            var calificaciones = doc.Descendants("Calificacion")
                                    .Select(x => (double)x)
                                    .ToList();

            double promedio = calificaciones.Any() ? calificaciones.Average() : 0;

            Console.WriteLine($"Estudiante: {nombre} ({control})");
            Console.WriteLine($"Promedio general: {promedio:F2}");

            Console.WriteLine("\n--- MATERIAS APROBADAS ---");
            var aprobadas = doc.Descendants("Materia")
                               .Where(m => (bool)m.Element("Aprobada"))
                               .Select(m => new
                               {
                                   Clave = m.Attribute("clave")?.Value,
                                   Nombre = m.Element("NombreMateria")?.Value,
                                   Calificacion = (double)m.Element("Calificacion")
                               });

            foreach (var mat in aprobadas)
            {
                Console.WriteLine($"- {mat.Clave}: {mat.Nombre} | Calificación: {mat.Calificacion:F2}");
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // ---------- MÉTODO DE VALIDACIÓN CON REPORTE DE LÍNEA ----------
        static void ValidarXml(string rutaXml, string rutaXsd)
        {
            Console.WriteLine($"\nValidando '{rutaXml}' contra '{rutaXsd}'...");

            try
            {
                var schemas = new XmlSchemaSet();
                schemas.Add("", rutaXsd);

                var settings = new XmlReaderSettings();
                settings.Schemas = schemas;
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += (sender, e) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Error en Línea {e.Exception.LineNumber}, Posición {e.Exception.LinePosition}: {e.Message}");
                    Console.ResetColor();
                };

                using (var reader = XmlReader.Create(rutaXml, settings))
                {
                    while (reader.Read()) { }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ El archivo es VÁLIDO según el esquema XSD.");
                Console.ResetColor();
            }
            catch (XmlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error de formato XML (no está bien formado): Línea {ex.LineNumber}, Posición {ex.LinePosition}");
                Console.WriteLine($"   {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}