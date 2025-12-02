using Deullam.Credit.Inquiry.Challenge.IoC.Containers; // Adicionar o using para o nosso projeto IoC

var builder = WebApplication.CreateBuilder(args);

// Método para registrar os serviços da Aplicação (AutoMapper, CreditoService)
builder.Services.AddApplicationServices();

// Chama o método para registrar os serviços da Infraestrutura (DbContext, CreditoRepository)
// Passamos o 'builder.Configuration' para que o método tenha acesso ao appsettings.json.
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
