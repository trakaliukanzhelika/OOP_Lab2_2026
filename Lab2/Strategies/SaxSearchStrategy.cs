using System.Globalization;
using System.Xml;
using Lab2.Models;

namespace Lab2.Strategies
{
    public class SaxSearchStrategy : IXmlSearchStrategy
    {
        public List<Scientist> Search(string path, SearchCriteria criteria)
        {
            var results = new List<Scientist>();
            Scientist? current = null;

            using XmlReader reader = XmlReader.Create(path);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Scientist")
                {
                    current = new Scientist
                    {
                        FullName = reader.GetAttribute("FullName") ?? string.Empty,
                        Faculty = reader.GetAttribute("Faculty") ?? string.Empty,
                        Department = reader.GetAttribute("Department") ?? string.Empty,
                        Laboratory = reader.GetAttribute("Laboratory"),
                        Position = reader.GetAttribute("Position") ?? string.Empty
                    };
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Term" && current != null)
                {
                    string? start = reader.GetAttribute("Start");
                    string? end = reader.GetAttribute("End");

                    current.StartDate = start != null ? DateTime.Parse(start, CultureInfo.InvariantCulture) : DateTime.MinValue;
                    current.EndDate = end != null ? DateTime.Parse(end, CultureInfo.InvariantCulture) : null;
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Scientist" && current != null)
                {
                    if (Matches(current, criteria))
                        results.Add(current);

                    current = null;
                }
            }

            return results;
        }

        private static bool Matches(Scientist scientist, SearchCriteria criteria)
        {
            if (!string.IsNullOrWhiteSpace(criteria.Faculty) && scientist.Faculty != criteria.Faculty)
                return false;
            if (!string.IsNullOrWhiteSpace(criteria.Department) && scientist.Department != criteria.Department)
                return false;
            if (!string.IsNullOrWhiteSpace(criteria.Position) && scientist.Position != criteria.Position)
                return false;
            if (!string.IsNullOrWhiteSpace(criteria.Keyword) && !scientist.FullName.Contains(criteria.Keyword))
                return false;

            return true;
        }
    }
}
