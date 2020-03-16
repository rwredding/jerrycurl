using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Test.Integration.Database;

namespace Jerrycurl.Test.Integration.Views
{
    public class DatabaseView : Movie
    {
        public MovieDetails Details { get; set; }
        public IList<MovieCast> Cast { get; set; }
    }
}
