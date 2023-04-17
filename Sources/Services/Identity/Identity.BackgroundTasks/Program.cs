var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEventBus();
//builder.Services.AddRabbitMQ(); 
builder.Services.AddMongoDB(typeof(UsuarioMongoRepository).Assembly);
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddTransient(typeof(IdentityCommandHandlerContext<>));
builder.Services.AddTransient(typeof(IdentityDomainEventHandlerContext<>));
builder.Services.AddTransient(typeof(IdentityCommandHandlerContext<,>));

var app = builder.Build();

app.Run();
