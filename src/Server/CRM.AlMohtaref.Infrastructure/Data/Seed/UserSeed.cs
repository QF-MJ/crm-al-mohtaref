using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;
using CRM.AlMohtaref.Infrastructure.ExternalServices;

namespace CRM.AlMohtaref.Infrastructure.Data.Seed;

internal static class UserSeed
{
    public const string AdminAccountName = "admin";

    // placeholder: replace with a real hash when IPasswordHasher / login is implemented
    public const string FakePasswordHash = "12345678";

    // persons[0] -> admin, persons[i] -> user01..user24 (same order as PersonSeed)
    public static List<User> Create(IReadOnlyList<Person> persons)
    {
        PasswordHasher password = new PasswordHasher();

        var users = new List<User>();

        for (var i = 0; i < persons.Count; i++)
        {
            var accountName = i == 0 ? AdminAccountName : "user" + i.ToString("00");

            users.Add(new User(accountName, password.HashPassword(FakePasswordHash), persons[i].Id, accountName + "@almohtaref.local"));
        }

        return users;
    }
}
