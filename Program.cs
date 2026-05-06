using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace TriangleAnalyzer
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X}, {Y})";
    }

    public class TriangleResult
    {
        public string TriangleType { get; set; }
        public List<Point> Coordinates { get; set; }

        public TriangleResult()
        {
            Coordinates = new List<Point>();
        }
    }

    public static class TriangleProcessor
    {
        private const string LogFilePath = "triangle_log.txt";
        private const int FieldSize = 100;
        private const float Margin = 10f;
        private const float Epsilon = 0.0001f;

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static TriangleResult ProcessTriangle(string strA, string strB, string strC)
        {
            var result = new TriangleResult();

            if (!float.TryParse(strA, NumberStyles.Float, CultureInfo.InvariantCulture, out float a) ||
                !float.TryParse(strB, NumberStyles.Float, CultureInfo.InvariantCulture, out float b) ||
                !float.TryParse(strC, NumberStyles.Float, CultureInfo.InvariantCulture, out float c))
            {
                result.TriangleType = "";
                result.Coordinates = new List<Point> { new Point(-2, -2), new Point(-2, -2), new Point(-2, -2) };
                LogError($"Нечисловые данные: A='{strA}', B='{strB}', C='{strC}'", "Нечисловой ввод");
                return result;
            }

            if (a <= 0 || b <= 0 || c <= 0)
            {
                result.TriangleType = "не треугольник";
                result.Coordinates = new List<Point> { new Point(-1, -1), new Point(-1, -1), new Point(-1, -1) };
                LogError($"Неположительные стороны: A={a}, B={b}, C={c}", "Стороны должны быть положительными");
                return result;
            }

            if (a + b <= c || a + c <= b || b + c <= a)
            {
                result.TriangleType = "не треугольник";
                result.Coordinates = new List<Point> { new Point(-1, -1), new Point(-1, -1), new Point(-1, -1) };
                LogError($"Нарушено неравенство: A={a}, B={b}, C={c}", "Сумма двух сторон должна быть больше третьей");
                return result;
            }

            if (Math.Abs(a - b) < Epsilon && Math.Abs(b - c) < Epsilon)
                result.TriangleType = "равносторонний";
            else if (Math.Abs(a - b) < Epsilon || Math.Abs(a - c) < Epsilon || Math.Abs(b - c) < Epsilon)
                result.TriangleType = "равнобедренный";
            else
                result.TriangleType = "разносторонний";

            result.Coordinates = CalculateCoordinates(a, b, c);
            LogSuccess(a, b, c, result.TriangleType, result.Coordinates);

            return result;
        }

        private static List<Point> CalculateCoordinates(float a, float b, float c)
        {
            float xA = Margin;
            float yA = FieldSize - Margin;
            float xB = xA + c;
            float yB = yA;

            float dAB = c;

            if (dAB < Epsilon)
            {
                return new List<Point>
                {
                    new Point((int)Margin, (int)(FieldSize - Margin)),
                    new Point((int)(Margin + 1), (int)(FieldSize - Margin)),
                    new Point((int)(Margin + 2), (int)(FieldSize - Margin - 1))
                };
            }

            float xC = (b * b - a * a + dAB * dAB) / (2 * dAB);
            float yC = (float)Math.Sqrt(Math.Abs(b * b - xC * xC));
            xC += xA;

            float minX = Math.Min(xA, Math.Min(xB, xC));
            float maxX = Math.Max(xA, Math.Max(xB, xC));
            float minY = Math.Min(yA, Math.Min(yB, yC));
            float maxY = Math.Max(yA, Math.Max(yB, yC));

            float width = maxX - minX;
            float height = maxY - minY;

            if (width < Epsilon || height < Epsilon)
            {
                width = 1f;
                height = 1f;
            }

            float scaleX = (FieldSize - 2 * Margin) / width;
            float scaleY = (FieldSize - 2 * Margin) / height;
            float scale = Math.Min(scaleX, scaleY);

            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;
            float offsetX = FieldSize / 2f - centerX * scale;
            float offsetY = FieldSize / 2f - centerY * scale;

            int intXA = Clamp((int)Math.Round(xA * scale + offsetX), 0, FieldSize);
            int intYA = Clamp((int)Math.Round(yA * scale + offsetY), 0, FieldSize);
            int intXB = Clamp((int)Math.Round(xB * scale + offsetX), 0, FieldSize);
            int intYB = Clamp((int)Math.Round(yB * scale + offsetY), 0, FieldSize);
            int intXC = Clamp((int)Math.Round(xC * scale + offsetX), 0, FieldSize);
            int intYC = Clamp((int)Math.Round(yC * scale + offsetY), 0, FieldSize);

            return new List<Point>
            {
                new Point(intXA, intYA),
                new Point(intXB, intYB),
                new Point(intXC, intYC)
            };
        }

        private static void LogSuccess(float a, float b, float c, string triangleType, List<Point> coordinates)
        {
            string message = $"УСПЕШНО | Стороны: {a:F2}, {b:F2}, {c:F2} | Тип: {triangleType} | " +
                           $"A{coordinates[0]}, B{coordinates[1]}, C{coordinates[2]}";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            Console.ResetColor();

            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка записи лога: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void LogError(string inputData, string errorText)
        {
            string message = $"НЕУСПЕШНО | Данные: {inputData} | Ошибка: {errorText}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            Console.ResetColor();

            string fullLog = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n" +
                           $"Трассировка:\n{Environment.StackTrace}\n{new string('-', 80)}\n";

            try
            {
                File.AppendAllText(LogFilePath, fullLog);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка записи лога: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("АНАЛИЗАТОР ТРЕУГОЛЬНИКА");
            Console.WriteLine();

            Console.WriteLine("Введите длины трёх сторон:");
            Console.Write("Сторона A: ");
            string inputA = Console.ReadLine();
            Console.Write("Сторона B: ");
            string inputB = Console.ReadLine();
            Console.Write("Сторона C: ");
            string inputC = Console.ReadLine();

            var result = TriangleProcessor.ProcessTriangle(inputA, inputB, inputC);

            Console.WriteLine();
            Console.WriteLine("РЕЗУЛЬТАТ:");
            Console.WriteLine($"Тип треугольника: {result.TriangleType}");
            Console.WriteLine();
            Console.WriteLine("Координаты вершин (X, Y):");
            Console.WriteLine($"  Вершина A: ({result.Coordinates[0].X}, {result.Coordinates[0].Y})");
            Console.WriteLine($"  Вершина B: ({result.Coordinates[1].X}, {result.Coordinates[1].Y})");
            Console.WriteLine($"  Вершина C: ({result.Coordinates[2].X}, {result.Coordinates[2].Y})");

            Console.WriteLine();
            Console.WriteLine("Лог записан в файл triangle_log.txt");

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}