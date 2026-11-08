using System.Globalization;
using System.Xml;
using Lab2.Models;

namespace Lab2.Strategies
{
    public class DomSearchStrategy : IXmlSearchStrategy
    {
        public List<Scientist> Search(string path, SearchCriteria criteria)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            string xpath = BuildXPath(criteria);
            XmlNodeList? nodes = doc.SelectNodes(xpath);

            var results = new List<Scientist>();
            if (nodes == null)
                return results;

            foreach (XmlNode node in nodes)
            {
                results.Add(ParseScientist((XmlElement)node));
            }
            return results;
        }

        private static string BuildXPath(SearchCriteria criteria)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(criteria.Faculty))
                conditions.Add($"@Faculty=\"{criteria.Faculty}\"");
            if (!string.IsNullOrWhiteSpace(criteria.Department))
                conditions.Add($"@Department=\"{criteria.Department}\"");
            if (!string.IsNullOrWhiteSpace(criteria.Position))
                conditions.Add($"@Position=\"{criteria.Position}\"");
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
                conditions.Add($"contains(@FullName, \"{criteria.Keyword}\")");

            if (conditions.Count == 0)
                return "/Staff/Scientist";

            return $"/Staff/Scientist[{string.Join(" and ", conditions)}]";
        }

        private static Scientist ParseScientist(XmlElement element)
        {
            XmlElement? term = element["Term"];

            return new Scientist
            {
                FullName = element.GetAttribute("FullName"),
                Faculty = element.GetAttribute("Faculty"),
                Department = element.GetAttribute("Department"),
                Laboratory = element.HasAttribute("Laboratory") ? element.GetAttribute("Laboratory") : null,
                Position = element.GetAttribute("Position"),
                StartDate = term != null
                    ? DateTime.Parse(term.GetAttribute("Start"), CultureInfo.InvariantCulture)
                    : DateTime.MinValue,
                EndDate = term != null && term.HasAttribute("End")
                    ? DateTime.Parse(term.GetAttribute("End"), CultureInfo.InvariantCulture)
                    : null
            };
        }
    }
}