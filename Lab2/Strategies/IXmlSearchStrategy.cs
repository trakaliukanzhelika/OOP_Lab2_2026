using Lab2.Models;

namespace Lab2.Strategies
{
    public interface IXmlSearchStrategy
    {
        List<Scientist> Search(string path, SearchCriteria criteria);
    }
}