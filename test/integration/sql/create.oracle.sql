BEGIN
    BEGIN
       EXECUTE IMMEDIATE 'DROP TABLE "MovieCast"';
    EXCEPTION
       WHEN OTHERS THEN
          NULL;
    END;
    
    BEGIN
       EXECUTE IMMEDIATE 'DROP TABLE "MovieDetails"';
    EXCEPTION
       WHEN OTHERS THEN
          NULL;
    END;
    
    BEGIN
       EXECUTE IMMEDIATE 'DROP TABLE "Movie"';
    EXCEPTION
       WHEN OTHERS THEN
          NULL;
    END;
    
    EXECUTE IMMEDIATE 'CREATE TABLE "Movie"
(
    "Id" number PRIMARY KEY,
    "Title" varchar2(100) NOT NULL,
    "ReleaseYear" number NOT NULL
)';

    EXECUTE IMMEDIATE 'CREATE TABLE "MovieDetails"
(
    "MovieId" number PRIMARY KEY,
    "Tagline" varchar2(100),
    "Budget" number
)';

    EXECUTE IMMEDIATE 'CREATE TABLE "MovieCast"
(
    "Id" number PRIMARY KEY,
    "MovieId" number NOT NULL,
    "Actor" varchar2(50) NOT NULL,
    "Plays" varchar2(50) NOT NULL,
    CONSTRAINT "FK_MovieCast_Movie" FOREIGN KEY ("MovieId") REFERENCES "Movie"("Id")
)';
END;