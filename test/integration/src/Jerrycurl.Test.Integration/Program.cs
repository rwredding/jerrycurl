using System;
using Jerrycurl.Mvc;
using Jerrycurl.Test.Integration.Accessors;
using Jerrycurl.Test.Integration.Database;
using Jerrycurl.Data.Metadata.Annotations;
using System.IO;
using Jerrycurl.Test.Integration.Views;
using System.Collections.Generic;

namespace Jerrycurl.Test.Integration
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments.");
                
                return -1;
            }

            string connectionString = args[0];
            string resultsPath = args[1];

            if (File.Exists(resultsPath))
                File.Delete(resultsPath);
            
            Console.WriteLine();
            Console.WriteLine("| ConnectionString: " + connectionString);
            Console.WriteLine("| ResultsPath: " + resultsPath);    
            Console.WriteLine();
            Console.WriteLine("> Running integration code...");

            Settings.ConnectionString = connectionString;

            CrudAccessor accessor = new CrudAccessor();

            accessor.Clear();

            var newMovies = new[]
            {
                new Movie() { Id = 1, Title = "Coming to America", ReleaseYear = 1988 },
                new Movie() { Id = 2, Title = "Coming 2 America", ReleaseYear = 2020 },
                new Movie() { Id = 3, Title = "Coming 3 America", ReleaseYear = 2028 },
            };

            var newDetails = new MovieDetails() { MovieId = 1, Tagline = "The Four Funniest Men in America are Eddie Murphy" };

            var newCast = new[]
            {
                new MovieCast() { Id = 1, MovieId = 1, Actor = "Eddie Murphy", Plays = "Prince Akeem" },
                new MovieCast() { Id = 2, MovieId = 1, Actor = "Arsenio Hall", Plays = "Semmi" },
                new MovieCast() { Id = 3, MovieId = 2, Actor = "Eddie Murphy", Plays = "Randy Jackson" },
            };

            accessor.Create(newMovies);
            accessor.Create(newDetails);
            accessor.Create(newCast);

            var updateDetails = new MovieDetails() { MovieId = 1, Tagline = "This summer, Prince Akeem discovers America." };
            var deleteMovie = new Movie() { Id = 3 };

            accessor.Update(updateDetails);
            accessor.Delete(deleteMovie);

            var movies = accessor.GetDatabaseView();

            VerifyModel(movies);

            accessor.VerifyImport();
            
            Console.WriteLine($"> Ran to EOF. Writing results to '{resultsPath}'.");
            Console.WriteLine();

            File.WriteAllText(resultsPath, "OK");
            
            return 0;
        }

        private static void VerifyModel(IList<DatabaseView> model)
        {
            if (model.Count != 2)
                throw new InvalidOperationException();
            else if (model[0].Details == null || model[0].Cast == null || model[0].Cast.Count != 2)
                throw new InvalidOperationException();
            else if (!model[0].Details.Tagline.StartsWith("This summer"))
                throw new InvalidOperationException();
            else if (model[1].Details != null || model[1].Cast == null || model[1].Cast.Count != 1)
                throw new InvalidOperationException();
        }
    }
}