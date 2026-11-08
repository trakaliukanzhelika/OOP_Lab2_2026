using System.Globalization;
using System.Xml.Linq;
using Lab2.Models;

namespace Lab2.Strategies
{
    public class LinqSearchStrategy : IXmlSearchStrategy
    {
        public List<Scientist> Search(string path, SearchCriteria criteria)
        {
            XDocument doc = XDocument.Load(path);

            IEnumerable<XElement> query = doc.Root!.Elements("Scientist");

            string? faculty = criteria.Faculty;
            string? department = criteria.Department;
            string? position = criteria.Position;
            string? keyword = criteria.Keyword;

            if (!string.IsNullOrWhiteSpace(faculty))
                query = query.Where(e => (string?)e.Attribute("Faculty") == faculty);
            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(e => (string?)e.Attribute("Department") == department);
            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(e => (string?)e.Attribute("Position") == position);
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(e => ((string?)e.Attribute("FullName") ?? "").Contains(keyword));

            return query.Select(e => ParseScientist(e)).ToList();
        }

        private static Scientist ParseScientist(XElement element)
        {
            XElement? term = element.Element("Term");
            string? start = (string?)term?.Attribute("Start");
            string? end = (string?)term?.Attribute("End");

            return new Scientist
            {
                FullName = (string?)element.Attribute("FullName") ?? string.Empty,
                Faculty = (string?)element.Attribute("Faculty") ?? string.Empty,
                Department = (string?)element.Attribute("Department") ?? string.Empty,
                Laboratory = (string?)element.Attribute("Laboratory"),
                Position = (string?)element.Attribute("Position") ?? string.Empty,
                StartDate = start != null ? DateTime.Parse(start, CultureInfo.InvariantCulture) : DateTime.MinValue, // Default if StartDate is not provided
                EndDate = end != null ? DateTime.Parse(end, CultureInfo.InvariantCulture) : null
            };
        }
    }
}