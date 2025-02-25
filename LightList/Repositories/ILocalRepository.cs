using LightList.Models;

namespace LightList.Repositories;

public interface ILocalRepository
{
    IEnumerable<Models.Task> GetAll();

    Models.Task Get(string filename);

    void Save(Models.Task task);
    
    void Delete(Models.Task task);
}