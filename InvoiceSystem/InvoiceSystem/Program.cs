using InvoiceSystem;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<InMemoryDataService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Services.GetRequiredService<InMemoryDataService>().SeedData();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<InvoicingSystem.Middleware.AuthMiddleware>(); 
app.UseAuthorization();
app.MapControllers();

app.Run();

// TODO: Error Handling, validation, and logging(?). Think about the storage (In memory, file, database)
// TODO: Docker
// TODO: Adjustments in README file
