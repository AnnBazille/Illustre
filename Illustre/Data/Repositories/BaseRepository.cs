namespace Data.Repositories;

public class BaseRepository
{
    public BaseRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }

    public DatabaseContext DatabaseContext { get; set; }
}
