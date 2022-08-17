var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEventBus();
builder.Services.AddRabbitMQ(); 
builder.Services.AddMongoDB(typeof(UsuarioMongoRepository).Assembly);
builder.Services.AddMediatR(typeof(Program).Assembly);

var app = builder.Build();

app.UseEventBus(typeof(Program).Assembly);

app.Run();
