using Lab2.Models;

namespace Lab2.Writers
{
    public interface IFragmentWriter
    {
        void Write(List<Scientist> data, Stream stream);
    }
}
