using Lab2.Services;
using Lab2.Writers;

namespace Lab2.Exporters
{
    public class XmlFragmentExporter : FragmentExporter
    {
        public XmlFragmentExporter(GoogleDriveService drive) : base(drive)
        {
        }

        protected override IFragmentWriter CreateWriter()
        {
            return new XmlFragmentWriter();
        }
    }
}
