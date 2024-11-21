using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;
using Pulsar.BuildingBlocks.Utils;
using Pulsar.Services.Facility.Contracts.Shadows;
using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using System.Linq.Expressions;

namespace Identity.UnitTests.Utils;

public static class IdentityDatabase
{
    public static string AdminUserId => Usuario.SuperUsuarioId.ToString();
    public static string SamanthaUserId => "6453180121fbe2cabb191b63";
    public static string AlexiaUserId => "6453182ab53bc7af4b76a479";
    public static string DominioPadraoId => "64531aa3aae47f456897b819";
    public static string EstabelecimentoPadraoId => "64531bb6d384e644752aa391";
    public static string RedeEstabelecimentosPadraoId => "64531bb94fed54407f4fac1a";
    public static string RedeEstabelecimentosNaoPadraoId => "64545f296e11d6e38efd291d";

    public static void Seed(IMockedDatabase db)
    {
        var usuarioCollection = db.GetCollection<Usuario>(Constants.CollectionNames.USUARIOS);
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


        var samanthaUser = new Usuario(
            ObjectId.Parse(SamanthaUserId),
            "Samantha",
            "samantha.test@gmail.com",
            "samantha",
            salt,
            (salt + "123456").SHA256Hash(),
            new AuditInfo(sid));


        var alexiaUser = new Usuario(
            ObjectId.Parse(AlexiaUserId),
            "Alexia",
            "alexia.test@gmail.com",
            "alexia",
            salt,
            (salt + "123456").SHA256Hash(),
            new AuditInfo(sid));

        superUser.IsAtivo = true;
        samanthaUser.IsAtivo = true;
        alexiaUser.IsAtivo = true;

        usuarioCollection.InsertManyAsync(new Usuario[] { superUser, samanthaUser, alexiaUser }).Wait(); // it should complete synchronously
        usuarioCollection.AddUniqueKey(u => u.Email ?? u.NomeUsuario);
        usuarioCollection.AddUniqueKey(u => u.NomeUsuario);

        var estabelecimentoCollection = db.GetCollection<EstabelecimentoShadow>(Shadow<EstabelecimentoShadow>.GetCollectionName());
        var estabelecimentoPadrao = new EstabelecimentoShadow(ObjectId.Parse(EstabelecimentoPadraoId), ObjectId.Parse(DominioPadraoId), "PADRÃO", "00112233",
            new List<ObjectId> { ObjectId.Parse(RedeEstabelecimentosPadraoId) }, true, new AuditInfo(sid).ToShadow());
        estabelecimentoCollection.InsertManyAsync(new EstabelecimentoShadow[] { estabelecimentoPadrao }).Wait();

        var redeEstabelecimentosCollection = db.GetCollection<RedeEstabelecimentosShadow>(Shadow<RedeEstabelecimentosShadow>.GetCollectionName());
        var redeEstabelecimentos01 = new RedeEstabelecimentosShadow(ObjectId.Parse(RedeEstabelecimentosPadraoId), ObjectId.Parse(DominioPadraoId), "PADRÃO", new AuditInfo(sid).ToShadow());
        var redeEstabelecimentos02 = new RedeEstabelecimentosShadow(ObjectId.Parse(RedeEstabelecimentosNaoPadraoId), ObjectId.Parse(DominioPadraoId), "NÃO-PADRÃO", new AuditInfo(sid).ToShadow());
        redeEstabelecimentosCollection.InsertManyAsync(new RedeEstabelecimentosShadow[] { redeEstabelecimentos01, redeEstabelecimentos02 }).Wait();

        var dominioCollection = db.GetCollection<Dominio>(Constants.CollectionNames.DOMINIOS);
        var dominioPadrao = new Dominio(ObjectId.Parse(DominioPadraoId), "PADRÃO", samanthaUser.Id, new AuditInfo(sid));
        dominioCollection.InsertManyAsync(new Dominio[] { dominioPadrao }).Wait();
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

    public static IAsyncEnumerable<T> FindAsync<T>(this IMockedCollection<T> col, Expression<Func<T, bool>> pred) where T : class
    {
        var spec = Find.Where<T>(pred).ToWrapper();
        return col.FindAsync(spec);
    }
}
