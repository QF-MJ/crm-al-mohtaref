using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

internal static class UserSeed
{
    public const string AdminAccountName = "admin";

    // placeholder: replace with a real hash when IPasswordHasher / login is implemented
    public const string FakePasswordHash = "FAKE-HASH-REPLACE-WHEN-LOGIN-IS-IMPLEMENTED";

    // persons[0] -> admin, persons[i] -> user01..user24 (same order as PersonSeed)
    public static List<User> Create(IReadOnlyList<Person> persons)
    {
        var users = new List<User>();

        for (var i = 0; i < persons.Count; i++)
        {
            var accountName = i == 0 ? AdminAccountName : "user" + i.ToString("00");

            users.Add(new User(accountName, FakePasswordHash, persons[i].Id, accountName + "@almohtaref.local"));
        }

        return users;
    }
}
