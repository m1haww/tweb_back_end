//bildul app-ului 
var builder = WebApplication.CreateBuilder(args);
//inregistreaza serviicele pe care api le va folosi 
builder.Services.AddControllers();
//consturiecte aplicatia
var app = builder.Build();
//permisiunele
app.UseAuthorization();
//Mapezi rutele: request-uri de tip /api/NumeController vor merge la controllerele 
app.MapControllers();
app.Run();


