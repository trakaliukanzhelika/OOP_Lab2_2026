using Lab2.Services;
using Lab2.Writers;

namespace Lab2.Exporters
{
    public class HtmlFragmentExporter : FragmentExporter
    {
        public HtmlFragmentExporter(GoogleDriveService drive) : base(drive)
        {
        }

        protected override IFragmentWriter CreateWriter()
        {
            return new HtmlFragmentWriter();
        }
    }
}
