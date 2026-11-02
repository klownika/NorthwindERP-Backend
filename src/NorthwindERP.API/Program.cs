using NorthwindERP.API.Extensions;
using NorthwindERP.API.Middlewares;
using NorthwindERP.Infrastructure;
using NorthwindERP.Infrastructure.Data;

// Configuración para que las fechas (timestamp) de Supabase se lean como DateTime normal sin conflictos de zona horaria
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);




// Controladores de la API
builder.Services.AddControllers();

// Capa de Infraestructura (Base de datos, repositorios, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Lógica de negocio adicional
builder.Services.AddApplicationServices();

// Seguridad JWT (Validación de tokens para los usuarios)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Documentación interactiva con Swagger + soporte para tokens JWT
builder.Services.AddSwaggerWithJwt();

// Configuración de CORS (Permite que tu frontend en React/Vite en localhost:5173 se comunique con la API)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();




// 1. Manejador global de errores (Atrapa cualquier excepción no controlada en la app)
app.UseMiddleware<ErrorHandlingMiddleware>();

// 2. Documentación Swagger (Disponible en desarrollo y producción si lo deseas)
app.UseSwagger();
app.UseSwaggerUI(c => c.DocumentTitle = "NorthwindERP API - Documentación");

// 3. Redirección automática de HTTP a HTTPS
app.UseHttpsRedirection();

// 4. Aplicar la política de CORS definida arriba
app.UseCors("Frontend");

// 5. Autenticación (Identifica quién es el usuario leyendo el token JWT)
app.UseAuthentication();

// 6. Autorización (Verifica si el usuario tiene permisos para acceder al endpoint)
app.UseAuthorization();

// 7. Mapear los controladores de la API
app.MapControllers();





// Permite comprobar rápidamente si la API se comunica con Supabase: GET /api/health
app.MapGet("/api/health", async (NorthwindDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "ok", database = "conectada con éxito a Supabase uwu" })
        : Results.Problem("No se pudo conectar a Supabase. Revisa tu cadena de conexión.", statusCode: 503))
    .AllowAnonymous()
    .WithTags("Health");

app.Run();

// Necesario para pruebas de integración o WebApplicationFactory
public partial class Program { }