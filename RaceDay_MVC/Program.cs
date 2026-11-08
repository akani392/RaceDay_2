using RaceDay_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

//add MVC controllers and views
builder.Services.AddControllersWithViews();

//add memory cache for session storage
builder.Services.AddDistributedMemoryCache();

//configure session management
builder.Services.AddSession(options =>
{
    //start of session configuration

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;

    //only send the MVC session cookie over HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    //session expires after 2 hours of inactivity
    options.IdleTimeout = TimeSpan.FromHours(2);

    //end of session configuration
});

//configure HTTP client to communicate with RaceDay API
builder.Services.AddHttpClient<RaceDayApi>(client =>
{
    //start of HTTP client configuration

    string api_url =
        builder.Configuration["RaceDayApi:BaseUrl"]
        ?? "https://localhost:7016/";

    client.BaseAddress = new Uri(api_url);

    //end of HTTP client configuration
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        UseCookies = false
    });

//build the application
var app = builder.Build();

//configure error handling for production
if (!app.Environment.IsDevelopment())
{
    //start of if statement

    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

    //end of if statement
}

//configure the HTTP request pipeline
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

//enable sessions before mapping controllers
app.UseSession();

//configure default MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

//start the MVC application
app.Run();


