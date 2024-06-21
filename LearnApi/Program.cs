using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Word;
using LearnApi.Container;
using LearnApi.Helper;
using LearnApi.Model;
using LearnApi.Repos;
using LearnApi.Repos.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRefreshHandler, RefreshHandler>();
builder.Services.AddDbContext<ApplicationDbContext>(
    option => option.UseSqlServer(builder.Configuration.GetConnectionString("apiConfig"))
);
// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperHandler());
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

//Serilog logger configuration
var logPath = builder.Configuration.GetSection("Logging:LogPath").Value;
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("microsoft",Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logPath)
    .CreateLogger();
builder.Logging.AddSerilog(logger);

//Enable CORS
builder.Services.AddCors(x => x.AddPolicy("corPolicy", option =>
{
    option.WithOrigins("https://domain1.com/", "https://domain2.com/").AllowAnyMethod().AllowAnyHeader();
}));

//Rate Limiting
builder.Services.AddRateLimiter(x => x.AddFixedWindowLimiter(policyName: "fixedwindow", option =>
{
    option.Window = TimeSpan.FromSeconds(10);
    option.PermitLimit = 1;
    option.QueueLimit = 0;
    option.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode = 401);

//Register Basic Authentication
//builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

//Register appsetting key to class
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

//Configure JWT bearer token
var authKey = builder.Configuration.GetValue<string>("JwtSettings:securityKey");
builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(item =>
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
    };
});

var app = builder.Build();

//Minimal API
app.MapGet("/minimalApi/GetName", () => "SYED MUBEEN HUSSAIN");

app.MapGet("/minimalApi/GetAllCustomer", async (ApplicationDbContext _context) =>
{
    var customers = await _context.Students.ToListAsync();
    return customers;
});

app.MapGet("/minimalApi/GetCustomerById/{id}", async (ApplicationDbContext _context, int id) =>
{
    var customer = await _context.Students.FirstOrDefaultAsync(x=>x.Id == id);
    return customer;
});

app.MapPost("/minimalApi/CreateCustomer", async (ApplicationDbContext _context, Student customer) =>
{
    await _context.Students.AddAsync(new Student()
    {
        Name = customer.Name,
        Age = customer.Age,
        RollNumber = customer.RollNumber,
        CreatedAt = DateTime.Now,
        CreatedBy = "Admin",
    });
    await _context.SaveChangesAsync();
});

app.MapPost("/minimalApi/UpdateCustomer/{id}", async (ApplicationDbContext _context, Student customer, int id) =>
{
    var checkCustomer = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);
    if (checkCustomer != null)
    {
        checkCustomer.Name = customer.Name;
        checkCustomer.Age = customer.Age;
        checkCustomer.RollNumber = customer.RollNumber;
    }
    await _context.SaveChangesAsync();
});

app.MapPost("/minimalApi/DeleteCustomer/{id}", async (ApplicationDbContext _context, int id) =>
{
    var checkCustomer = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);
    if (checkCustomer != null)
    {
        _context.Students.Remove(checkCustomer);
        await _context.SaveChangesAsync();
    }
});


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// if we apply cors on global level then we will use this middleware
app.UseCors("corPolicy");

app.UseRateLimiter();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
