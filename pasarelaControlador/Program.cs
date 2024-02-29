using pasarelaControlador.Servicios;
using System;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ServicioControlProcesos>();
builder.Services.AddSingleton<ServicioCamara>();
var app = builder.Build();

//Thread HiloCamara = new Thread(MiHilo);
//HiloCamara.Start();

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


/*static bool MiHilo() 
{
    int StatusCamara = 0;
    StatusCamara = 1;
    while (StatusCamara < 1000000)
    {
        StatusCamara++;
        Console.WriteLine(StatusCamara);
        Thread.Sleep(50);
    }
    return true;
        
}*/

