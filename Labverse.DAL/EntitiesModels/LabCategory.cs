namespace Labverse.DAL.EntitiesModels;

public class LabCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Lab> Labs { get; set; } = new List<Lab>();
}
