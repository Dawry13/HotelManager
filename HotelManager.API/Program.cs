using HotelManager.API.Extensions;
using HotelManager.API.Middleware;
using HotelManager.Business;

var builder = WebApplication.CreateBuilder(args);

// Controllers, Swagger, CORS y ProblemDetails (ver Extensions/ServiceCollectionExtensions.cs)
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddHotelManager();

var app = builder.Build();

// Verifica/crea la base de datos al arrancar
try
{
    var acciones = DependencyInjection.InicializarBaseDatos();
    foreach (var accion in acciones)
        app.Logger.LogInformation("BD: {Accion}", accion);
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "No se pudo inicializar la base de datos");
    throw;
}

// Debe ir primero para capturar errores de todo lo que viene después
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");
app.UseAuthorization();
app.MapControllers();

app.Run();