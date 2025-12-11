using Microsoft.EntityFrameworkCore;
using Sentara.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: allow your HTML, which is running on port 5500
builder.Services.AddCors(options =>
{
    options.AddPolicy("SentaraCors", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// TODO: keep your DbContext + email service registrations here, for example:
//
builder.Services.AddDbContext<SentaraDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("SentaraDb")));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// ---------- Middleware pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// We keep everything on HTTP:3001 so it matches your JS `BACKEND = 'http://127.0.0.1:3001'`
// app.UseHttpsRedirection();

// enable CORS before authorization/controllers
app.UseCors("SentaraCors");

app.UseAuthorization();

app.MapControllers();

// listen explicitly on http://127.0.0.1:3001
app.Run("http://127.0.0.1:3001");

