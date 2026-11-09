
var builder = WebApplication.CreateBuilder(args);

// Register MVC services to enable Controllers and Views.
builder.Services.AddControllersWithViews();

// Allow controllers and views to access the current HTTP context.
builder.Services.AddHttpContextAccessor();

// Enable session management to store user information,
// such as UserRole, UserEmail and UserId, after login.
builder.Services.AddSession(options =>
{
    // Automatically expire the session after 30 minutes of inactivity.
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Prevent client-side scripts from accessing the session cookie.
    options.Cookie.HttpOnly = true;

    // Allow the session cookie to work even when consent is not required.
    options.Cookie.IsEssential = true;
});

// Read the API base address from appsettings.json.
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

// Register a named HttpClient that MVC controllers will use
// to communicate with the RaceDay API.
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    // Set the API address as the base URL for all API requests.
    client.BaseAddress = new Uri(apiBaseUrl!);
});

var app = builder.Build();

// Configure error handling and security for production.
if (!app.Environment.IsDevelopment())
{
    // Display the application's error page when an error occurs.
    app.UseExceptionHandler("/Home/Error");

    // Enable HTTP Strict Transport Security (HSTS).
    app.UseHsts();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Allow the application to serve static files such as CSS, JavaScript and images.
app.UseStaticFiles();

// Enable routing so requests can be directed to the correct controller and action.
app.UseRouting();

// Enable session access before authorization and controller execution.
app.UseSession();

// Enable authorization services for protected application areas.
app.UseAuthorization();

// Configure the default MVC route.
// If no controller or action is specified, Home/Index will be displayed.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the MVC application.
app.Run();
