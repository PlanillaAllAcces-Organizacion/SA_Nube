using GRUPO_01_SubirArchivosNube.AppWebMVC.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//Configuración con google drive mediante las credenciales
builder.Services.AddScoped<GoogleDriveService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new GoogleDriveService(
        configuration["GoogleDrive:ClientId"],
        configuration["GoogleDrive:ClientSecret"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
