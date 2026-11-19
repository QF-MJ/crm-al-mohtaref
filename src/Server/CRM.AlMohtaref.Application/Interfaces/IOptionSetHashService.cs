namespace CRM.AlMohtaref.Application.Interfaces;

public interface IOptionSetHashService
{
    string ComputeHash(IEnumerable<Guid> optionIds);
}