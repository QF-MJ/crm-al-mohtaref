namespace CRM.AlMohtaref.Domain.Entities.Campaigns;

public class Track
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<Campaign> Campaigns { get; private set; }

    private Track()
    {
        Name = string.Empty;
        Campaigns = new List<Campaign>();
    }

    public Track(string name) : this()
    {
        Name = name;
    }
}