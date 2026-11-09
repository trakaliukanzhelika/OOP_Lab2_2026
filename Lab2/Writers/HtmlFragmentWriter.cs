using System.Net;
using System.Text;
using Lab2.Models;

namespace Lab2.Writers
{
    public class HtmlFragmentWriter : IFragmentWriter
    {
        public void Write(List<Scientist> data, Stream stream)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\">");
            html.AppendLine("<title>Кадри науковців — відфільтровані дані</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: \"Segoe UI\", sans-serif; margin: 24px; }");
            html.AppendLine("table { border-collapse: collapse; }");
            html.AppendLine("th, td { border: 1px solid #999; padding: 6px 10px; text-align: left; }");
            html.AppendLine("th { background: #eeeeee; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<h1>Кадри науковців — відфільтровані дані</h1>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>П.І.П.</th><th>Факультет</th><th>Кафедра</th><th>Лабораторія</th><th>Посада</th><th>Початок</th><th>Кінець</th></tr>");

            foreach (Scientist s in data)
            {
                html.Append("<tr>");
                html.Append(Cell(s.FullName));
                html.Append(Cell(s.Faculty));
                html.Append(Cell(s.Department));
                html.Append(Cell(s.Laboratory ?? "—"));
                html.Append(Cell(s.Position));
                html.Append(Cell(s.StartDate.ToString("dd.MM.yyyy")));
                html.Append(Cell(s.EndDate?.ToString("dd.MM.yyyy") ?? "по теперішній час"));
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
            writer.Write(html.ToString());
        }

        private static string Cell(string value)
        {
            return $"<td>{WebUtility.HtmlEncode(value)}</td>";
        }
    }
}
