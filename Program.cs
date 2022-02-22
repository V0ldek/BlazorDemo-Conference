using System.Data;
using Conference.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("postgres");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<LectureService>();
builder.Services.AddScoped<PaperService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<IDbConnection>(_ => 
{
    var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    return connection;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
