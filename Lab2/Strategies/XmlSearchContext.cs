using Lab2.Models;

namespace Lab2.Strategies
{
    public class XmlSearchContext
    {
        private IXmlSearchStrategy _strategy;

        public XmlSearchContext(IXmlSearchStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IXmlSearchStrategy strategy)
        {
            _strategy = strategy;
        }

        public List<Scientist> ExecuteSearch(string path, SearchCriteria criteria)
        {
            return _strategy.Search(path, criteria);
        }
    }
}