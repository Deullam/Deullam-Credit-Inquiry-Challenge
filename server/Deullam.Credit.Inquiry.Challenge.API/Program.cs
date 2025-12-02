using Deullam.Credit.Inquiry.Challenge.IoC.Containers;
using Deullam.Credit.Inquiry.Challenge.API.Middlewares;

namespace Deullam.Credit.Inquiry.Challenge.API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {

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
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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


        }
    }
}
