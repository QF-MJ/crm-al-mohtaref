using System.Security.Cryptography;
using System.Text;
using CRM.AlMohtaref.Application.Interfaces;

namespace CRM.AlMohtaref.Infrastructure.ExternalServices;

public class OptionSetHashService : IOptionSetHashService
{
    public string ComputeHash(IEnumerable<Guid> optionIds)
    {
        var canonical = string.Join("|", optionIds
            .Distinct()
            .Select(id => id.ToString("N"))
            .OrderBy(id => id, StringComparer.Ordinal));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));

        return Convert.ToHexString(bytes);
    }
}