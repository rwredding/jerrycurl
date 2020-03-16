CREATE TABLE "Movie"
(
    "Id" int PRIMARY KEY,
    "Title" varchar(100) NOT NULL,
    "ReleaseYear" int NOT NULL
);

CREATE TABLE "MovieDetails"
(
    "MovieId" int PRIMARY KEY,
    "Tagline" varchar(100),
    "Budget" int
);

CREATE TABLE "MovieCast"
(
    "Id" int PRIMARY KEY,
    "MovieId" int NOT NULL,
    "Actor" varchar(50) NOT NULL,
    "Plays" varchar(50) NOT NULL,
    CONSTRAINT "FK_MovieCast_Movie" FOREIGN KEY ("MovieId") REFERENCES "Movie"("Id")
);
