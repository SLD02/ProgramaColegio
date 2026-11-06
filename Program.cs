using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Estudiante
{
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public double Nota1 { get; set; }
    public double Nota2 { get; set; }
    public double Nota3 { get; set; }
    public double Nota4 { get; set; }

    public double Promedio => (Nota1 + Nota2 + Nota3 + Nota4) / 4;

    public char Literal
    {
        get
        {
            double p = Promedio;
            if (p >= 91) return 'A';
            if (p >= 81) return 'B';
            if (p >= 70) return 'C';
            if (p >= 60) return 'D';
            return 'F';
        }
    }
}

class Curso
{
    public string Nombre { get; set; } = "";
    public List<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
}

class Program
{
    const string ARCHIVO = "estudiantes.txt";
    const char SEP = '|';

    static void Main()
    {
        List<Curso> cursos = CargarDatos();
        bool salir = false;

        while (!salir)
        {
            LimpiarPantalla();
            Encabezado("MENÚ PRINCIPAL");
            Console.WriteLine("1. Agregar un estudiante");
            Console.WriteLine("2. Eliminar un estudiante");
            Console.WriteLine("3. Reportes");
            Console.WriteLine("4. Salir");
            Console.WriteLine();
            Console.Write("Seleccione una opción: ");

            switch ((Console.ReadLine() ?? "").Trim())
            {
                case "1": AgregarEstudiante(cursos); break;
                case "2": EliminarEstudiante(cursos); break;
                case "3": MenuReportes(cursos); break;
                case "4": salir = true; break;
                default:
                    Console.WriteLine("Opción inválida.");
                    Pausa();
                    break;
            }
        }

        Console.WriteLine("Datos guardados. ¡Hasta luego!");
    }

    // ===================== AGREGAR =====================
    static void AgregarEstudiante(List<Curso> cursos)
    {
        LimpiarPantalla();
        Encabezado("AGREGAR ESTUDIANTE");

        if (cursos.Count > 0)
        {
            Console.WriteLine("Cursos existentes:");
            foreach (Curso c in cursos)
                Console.WriteLine($"  - {c.Nombre}");
            Console.WriteLine();
        }

        string nombreCurso = LeerTexto("Curso (si no existe se creará): ");
        Curso? curso = cursos.FirstOrDefault(c =>
            c.Nombre.Equals(nombreCurso, StringComparison.OrdinalIgnoreCase));

        if (curso == null)
        {
            curso = new Curso { Nombre = nombreCurso };
            cursos.Add(curso);
        }

        Estudiante est = new Estudiante();
        est.Nombre = LeerTexto("Nombre: ");
        est.Apellido = LeerTexto("Apellido: ");
        est.Nota1 = LeerNota("Nota 1: ");
        est.Nota2 = LeerNota("Nota 2: ");
        est.Nota3 = LeerNota("Nota 3: ");
        est.Nota4 = LeerNota("Nota 4: ");

        curso.Estudiantes.Add(est);
        GuardarDatos(cursos);

        Console.WriteLine();
        Console.WriteLine($"Estudiante agregado al curso \"{curso.Nombre}\" " +
                          $"(Promedio: {est.Promedio:F2}, Literal: {est.Literal}).");
        Pausa();
    }

    // ===================== ELIMINAR =====================
    static void EliminarEstudiante(List<Curso> cursos)
    {
        LimpiarPantalla();
        Encabezado("ELIMINAR ESTUDIANTE");

        Curso? curso = SeleccionarCurso(cursos);
        if (curso == null) return;

        List<Estudiante> ordenados = curso.Estudiantes
            .OrderBy(e => e.Apellido, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"Estudiantes del curso \"{curso.Nombre}\":");
        for (int i = 0; i < ordenados.Count; i++)
            Console.WriteLine($"{i + 1}. {ordenados[i].Apellido}, {ordenados[i].Nombre}");

        Console.WriteLine();
        Console.Write("Número del estudiante a eliminar (0 para cancelar): ");
        if (!int.TryParse(Console.ReadLine(), out int num) || num < 1 || num > ordenados.Count)
        {
            Console.WriteLine("Operación cancelada.");
            Pausa();
            return;
        }

        Estudiante elegido = ordenados[num - 1];
        Console.Write($"¿Eliminar a {elegido.Nombre} {elegido.Apellido}? (S/N): ");
        if ((Console.ReadLine() ?? "").Trim().ToUpper() != "S")
        {
            Console.WriteLine("Operación cancelada.");
            Pausa();
            return;
        }

        curso.Estudiantes.Remove(elegido);
        if (curso.Estudiantes.Count == 0)
            cursos.Remove(curso); // un curso sin estudiantes no se guarda

        GuardarDatos(cursos);
        Console.WriteLine("Estudiante eliminado.");
        Pausa();
    }

    // ===================== REPORTES =====================
    static void MenuReportes(List<Curso> cursos)
    {
        LimpiarPantalla();
        Encabezado("REPORTES");

        if (cursos.Count == 0)
        {
            Console.WriteLine("No hay estudiantes registrados.");
            Pausa();
            return;
        }

        Console.WriteLine("1. Reporte de un curso");
        Console.WriteLine("2. Reporte de todos los cursos");
        Console.WriteLine("3. Volver");
        Console.WriteLine();
        Console.Write("Seleccione una opción: ");

        switch ((Console.ReadLine() ?? "").Trim())
        {
            case "1":
                Curso? curso = SeleccionarCurso(cursos);
                if (curso != null)
                {
                    LimpiarPantalla();
                    MostrarReporte(curso);
                    Pausa();
                }
                break;
            case "2":
                LimpiarPantalla();
                foreach (Curso c in cursos.OrderBy(c => c.Nombre, StringComparer.OrdinalIgnoreCase))
                {
                    MostrarReporte(c);
                    Console.WriteLine();
                }
                Pausa();
                break;
        }
    }

    static void MostrarReporte(Curso curso)
    {
        Console.WriteLine("================================================================");
        Console.WriteLine("                     COLEGIO DIOS ES BUENO");
        Console.WriteLine("                  CALIFICACIONES DEL CUATRIMESTRE");
        Console.WriteLine($"                  Curso: {curso.Nombre}");
        Console.WriteLine("================================================================");
        Console.WriteLine();

        Console.WriteLine(
            "{0,-15} {1,-15} {2,6} {3,6} {4,6} {5,6} {6,9} {7,8}",
            "Nombre", "Apellido", "Nota1", "Nota2", "Nota3", "Nota4", "Promedio", "Literal");
        Console.WriteLine("----------------------------------------------------------------");

        List<Estudiante> ordenados = curso.Estudiantes
            .OrderBy(e => e.Apellido, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (Estudiante e in ordenados)
        {
            Console.WriteLine(
                "{0,-15} {1,-15} {2,6:F0} {3,6:F0} {4,6:F0} {5,6:F0} {6,9:F2} {7,8}",
                e.Nombre, e.Apellido, e.Nota1, e.Nota2, e.Nota3, e.Nota4, e.Promedio, e.Literal);
        }

        int a = ordenados.Count(e => e.Literal == 'A');
        int b = ordenados.Count(e => e.Literal == 'B');
        int c = ordenados.Count(e => e.Literal == 'C');
        int reprobados = ordenados.Count(e => e.Literal == 'D' || e.Literal == 'F');

        Console.WriteLine();
        Console.WriteLine("================================================================");
        Console.WriteLine("                            TOTALES");
        Console.WriteLine("================================================================");
        Console.WriteLine($"Total de estudiantes:      {ordenados.Count}");
        Console.WriteLine($"Estudiantes en A:          {a}");
        Console.WriteLine($"Estudiantes en B:          {b}");
        Console.WriteLine($"Estudiantes en C:          {c}");
        Console.WriteLine($"Estudiantes Reprobados:    {reprobados}");
    }

    // ===================== ARCHIVO TXT =====================
    // Formato por línea: Curso|Nombre|Apellido|Nota1|Nota2|Nota3|Nota4
    static List<Curso> CargarDatos()
    {
        List<Curso> cursos = new List<Curso>();
        if (!File.Exists(ARCHIVO)) return cursos;

        foreach (string linea in File.ReadAllLines(ARCHIVO))
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            string[] p = linea.Split(SEP);
            if (p.Length != 7) continue;

            bool ok = double.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double n1)
                    & double.TryParse(p[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double n2)
                    & double.TryParse(p[5], NumberStyles.Float, CultureInfo.InvariantCulture, out double n3)
                    & double.TryParse(p[6], NumberStyles.Float, CultureInfo.InvariantCulture, out double n4);
            if (!ok) continue;

            Curso? curso = cursos.FirstOrDefault(c =>
                c.Nombre.Equals(p[0], StringComparison.OrdinalIgnoreCase));
            if (curso == null)
            {
                curso = new Curso { Nombre = p[0] };
                cursos.Add(curso);
            }

            curso.Estudiantes.Add(new Estudiante
            {
                Nombre = p[1],
                Apellido = p[2],
                Nota1 = n1,
                Nota2 = n2,
                Nota3 = n3,
                Nota4 = n4
            });
        }

        return cursos;
    }

    static void GuardarDatos(List<Curso> cursos)
    {
        List<string> lineas = new List<string>();

        foreach (Curso c in cursos)
        {
            foreach (Estudiante e in c.Estudiantes)
            {
                lineas.Add(string.Join(SEP.ToString(),
                    c.Nombre, e.Nombre, e.Apellido,
                    e.Nota1.ToString(CultureInfo.InvariantCulture),
                    e.Nota2.ToString(CultureInfo.InvariantCulture),
                    e.Nota3.ToString(CultureInfo.InvariantCulture),
                    e.Nota4.ToString(CultureInfo.InvariantCulture)));
            }
        }

        File.WriteAllLines(ARCHIVO, lineas);
    }

    // ===================== UTILIDADES =====================
    static Curso? SeleccionarCurso(List<Curso> cursos)
    {
        if (cursos.Count == 0)
        {
            Console.WriteLine("No hay cursos registrados.");
            Pausa();
            return null;
        }

        Console.WriteLine("Cursos disponibles:");
        for (int i = 0; i < cursos.Count; i++)
            Console.WriteLine($"{i + 1}. {cursos[i].Nombre} ({cursos[i].Estudiantes.Count} estudiantes)");

        Console.WriteLine();
        Console.Write("Número del curso (0 para cancelar): ");
        if (int.TryParse(Console.ReadLine(), out int n) && n >= 1 && n <= cursos.Count)
            return cursos[n - 1];

        Console.WriteLine("Operación cancelada.");
        Pausa();
        return null;
    }

    static string LeerTexto(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            string texto = (Console.ReadLine() ?? "").Trim().Replace(SEP.ToString(), "");
            if (texto.Length > 0) return texto;
            Console.WriteLine("Este campo no puede estar vacío.");
        }
    }

    static double LeerNota(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            string? entrada = Console.ReadLine();
            if (double.TryParse(entrada, NumberStyles.Float, CultureInfo.CurrentCulture, out double nota)
                && nota >= 0 && nota <= 100)
                return nota;
            Console.WriteLine("Nota inválida. Ingrese un número entre 0 y 100.");
        }
    }

    static void Encabezado(string titulo)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("            COLEGIO DIOS ES BUENO");
        Console.WriteLine($"  {titulo}");
        Console.WriteLine("==============================================");
        Console.WriteLine();
    }

    static void LimpiarPantalla()
    {
        try { Console.Clear(); } catch (IOException) { }
    }

    static void Pausa()
    {
        Console.WriteLine();
        Console.Write("Presione ENTER para continuar...");
        Console.ReadLine();
    }
}
