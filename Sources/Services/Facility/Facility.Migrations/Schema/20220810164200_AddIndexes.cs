﻿namespace Pulsar.Services.Facility.Migrations.Schema;

[Migration(20220810164200, IsPersistent = true)]
public class AddIndexes : Migration
{
    public override async Task Up()
    {
        await this.UpIndexes(
            typeof(Estabelecimento).Assembly,
            // shadows
            typeof(UsuarioShadow).Assembly
            );
    }
}
