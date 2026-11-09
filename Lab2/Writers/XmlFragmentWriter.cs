using System.Globalization;
using System.Xml.Linq;
using Lab2.Models;

namespace Lab2.Writers
{
    public class XmlFragmentWriter : IFragmentWriter
    {
        public void Write(List<Scientist> data, Stream stream)
        {
            var doc = new XDocument(
                new XElement("Staff", data.Select(CreateElement)));

            doc.Save(stream);
        }

        private static XElement CreateElement(Scientist scientist)
        {
            var element = new XElement("Scientist",
                new XAttribute("FullName", scientist.FullName),
                new XAttribute("Faculty", scientist.Faculty),
                new XAttribute("Department", scientist.Department));

            if (scientist.Laboratory != null)
                element.Add(new XAttribute("Laboratory", scientist.Laboratory));

            element.Add(new XAttribute("Position", scientist.Position));

            var term = new XElement("Term",
                new XAttribute("Start", scientist.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

            if (scientist.EndDate != null)
                term.Add(new XAttribute("End", scientist.EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

            element.Add(term);
            return element;
        }
    }
}
