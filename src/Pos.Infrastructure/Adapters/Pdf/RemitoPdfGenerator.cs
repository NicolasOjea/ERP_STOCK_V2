using System.Globalization;
using System.Text;
using Pos.Application.Abstractions;
using Pos.Application.DTOs.Stock;

namespace Pos.Infrastructure.Adapters.Pdf;

public sealed class RemitoPdfGenerator : IRemitoPdfGenerator
{
    public byte[] Generate(StockRemitoPdfDataDto data)
    {
        var lines = new List<string>
        {
            "REMITO DE COMPRA",
            $"Fecha: {data.Fecha:yyyy-MM-dd HH:mm}",
            " "
        };

        foreach (var item in data.Items)
        {
            var cantidad = item.Cantidad.ToString("0.##", CultureInfo.InvariantCulture);
            lines.Add($"{item.Nombre} | {item.Codigo} | Cant: {cantidad}");
        }

        var content = string.Join("\n", lines);
        return SimplePdfBuilder.Build(content);
    }

    private static class SimplePdfBuilder
    {
        public static byte[] Build(string content)
        {
            var buffer = new StringBuilder();
            var offsets = new List<int>();

            void Write(string text) => buffer.Append(text);

            Write("%PDF-1.4\n");

            offsets.Add(buffer.Length);
            Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            offsets.Add(buffer.Length);
            Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

            offsets.Add(buffer.Length);
            Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");

            var contentStream = BuildContentStream(content);
            offsets.Add(buffer.Length);
            Write($"4 0 obj\n<< /Length {contentStream.Length} >>\nstream\n{contentStream}\nendstream\nendobj\n");

            offsets.Add(buffer.Length);
            Write("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

            var xrefPosition = buffer.Length;
            Write($"xref\n0 {offsets.Count + 1}\n");
            Write("0000000000 65535 f \n");
            foreach (var offset in offsets)
            {
                Write(offset.ToString("D10"));
                Write(" 00000 n \n");
            }

            Write($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");

            return Encoding.ASCII.GetBytes(buffer.ToString());
        }

        private static string BuildContentStream(string content)
        {
            var lines = content.Split('\n');
            var sb = new StringBuilder();
            sb.Append("BT\n/F1 12 Tf\n50 790 Td\n");
            foreach (var line in lines)
            {
                sb.AppendFormat("({0}) Tj\n", Escape(line));
                sb.Append("0 -16 Td\n");
            }
            sb.Append("ET");
            return sb.ToString();
        }

        private static string Escape(string text)
        {
            return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }
    }
}
