using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlTableStorage
    {
        void CreateEventTable(string name,object id);
        void CreateStateTable(string name, object id);
      
    }
}
