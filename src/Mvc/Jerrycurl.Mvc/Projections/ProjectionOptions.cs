using System;

namespace Jerrycurl.Mvc.Projections
{
    public class ProjectionOptions : IProjectionOptions
    {
        public string Separator { get; set; } = "," + Environment.NewLine;

        public ProjectionOptions()
        {

        }

        public ProjectionOptions(IProjectionOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.Separator = options.Separator;
        }
    }
}