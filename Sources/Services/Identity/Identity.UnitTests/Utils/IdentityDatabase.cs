using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;
using Pulsar.BuildingBlocks.Utils;

namespace Identity.UnitTests.Utils;

public static class IdentityDatabase
{
    public static string AdminUserId => Usuario.SuperUsuarioId.ToString();

    public static void Seed(IMockedDatabase db)
    {
        var usuarioCollection = db.GetCollection<Usuario>("Usuarios");
        var sid = Usuario.SuperUsuarioId;
        var salt = GeneralExtensions.GetSalt();
        var superUser = new Usuario(
            sid,
            "Administrador",
            null,
            "administrador",
            salt,
            (salt + "administrador").SHA256Hash(),
            new AuditInfo(sid));
        superUser.IsAtivo = true;
        usuarioCollection.InsertManyAsync(new Usuario[] { superUser }).Wait(); // it should complete synchronously
        usuarioCollection.AddUniqueKey(u => u.Email ?? u.NomeUsuario);
        usuarioCollection.AddUniqueKey(u => u.NomeUsuario);
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> list)
    {
        var result = new List<T>();
        await foreach (var item in list)
        {
            result.Add(item);
        }
        return result;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> list)
    {
        return (await list.ToListAsync()).FirstOrDefault();
    }

    public static FindSpecificationWrapper<T> ToWrapper<T>(this FindSpecificationBuilder<T> spec)
    {
        return new FindSpecificationWrapper<T>(spec.Build());
    }
}
