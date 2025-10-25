using DotNetEnv;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Labverse.BLL.Interfaces;
using Labverse.BLL.Services;
using Labverse.BLL.Settings;
using Labverse.DAL.Data;
using Labverse.DAL.Repositories;
using Labverse.DAL.Repositories.Interfaces;
using Labverse.DAL.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Net.payOS;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Define the current environment (Development, Staging, Production)
var envName = builder.Environment.EnvironmentName.ToLower();

// Load base .env file
if (File.Exists(".env"))
{
    Env.Load(".env");
}

// Load environment-specific configuration
var envFile = envName switch
{
    "development" => ".env.development",
    "staging" => ".env.staging",
    "production" => ".env.production",
    _ => ".env",
};

// Load the environment-specific .env file if it exists (override variables)
if (File.Exists(envFile))
{
    Env.Load(envFile);
}

// JWT__Key => Jwt:Key
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Labverse API", Version = "v1" }
    );
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description =
                "Enter 'Bearer' [space] and then your valid JWT token. Example: Bearer eyJhbGci...",
        }
    );
    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AppCorsPolicy",
        policy =>
        {
            // Support multiple frontend origins via Frontend:BaseUrls (array) or fallback to Frontend:BaseUrl (single)
            var allowedOrigins =
                builder.Configuration.GetSection("Frontend:BaseUrls").Get<string[]>()
                ?? new[] { builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173" };

            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<LabverseDbContext>(options => options.UseSqlServer(connectionString));

// Register UnitOfWork and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IActivityQueryService, ActivityQueryService>();
builder.Services.AddScoped<ILabService, LabService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
builder.Services.AddScoped<IPayOSService, PayOSService>();
builder.Services.AddScoped<IUserProgressService, UserProgressService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IVectorService, VectorService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IKnowledgeImportService, KnowledgeImportService>();
builder.Services.AddScoped<ILabCommentService, LabCommentService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IEmailJsService, EmailJsService>();
builder.Services.AddScoped<ISupabaseService, SupabaseService>();
builder.Services.AddHttpClient("supabase");
builder.Services.AddHttpClient("gemini");

// Register Recaptcha Service with HttpClient
builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings =
    builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings not configured.");

var keyBytes = Encoding.UTF8.GetBytes(
    jwtSettings.Key ?? throw new InvalidOperationException("JWT key is missing")
);

// Configure Recaptcha settings
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("Recaptcha"));

var recaptchaSettings =
    builder.Configuration.GetSection("Recaptcha").Get<RecaptchaSettings>()
    ?? throw new InvalidOperationException("Recaptcha settings not configured.");

var recaptchaSecretKey =
    recaptchaSettings.SecretKey
    ?? throw new InvalidOperationException("Recaptcha secret key is missing.");

// Initialize Firebase (Admin SDK)
InitializeFirebase(builder.Configuration);

// Configure PayOS
PayOS payOS = new PayOS(
    builder.Configuration["PAYOS:CLIENT_ID"] ?? throw new Exception("Cannot find environment"),
    builder.Configuration["PAYOS:API_KEY"] ?? throw new Exception("Cannot find environment"),
    builder.Configuration["PAYOS:CHECKSUM_KEY"] ?? throw new Exception("Cannot find environment")
);

builder.Services.AddSingleton(payOS);

// Configure EmailJS settings
builder.Services.Configure<EmailJsSettings>(builder.Configuration.GetSection("EmailJs"));

// Configure Authentication and Authorization
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero,
            //RoleClaimType = "role",
            //NameClaimType = JwtRegisteredClaimNames.Sub,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AppCorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void InitializeFirebase(ConfigurationManager config)
{
    try
    {
        if (FirebaseApp.DefaultInstance != null)
            return;

        GoogleCredential credential;
        // Minimal secrets via env/.env only: client email + private key (+ optional project id / key id)
        var clientEmail =
            config["Firebase:ClientEmail"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_CLIENT_EMAIL");
        var privateKey =
            config["Firebase:PrivateKey"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY");
        var projectId =
            config["Firebase:ProjectId"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        var privateKeyId =
            config["Firebase:PrivateKeyId"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY_ID");

        if (string.IsNullOrWhiteSpace(clientEmail) || string.IsNullOrWhiteSpace(privateKey))
            throw new InvalidOperationException(
                "Firebase credentials missing: set ClientEmail and PrivateKey in env/config."
            );

        // Normalize private key newlines if provided as single-line with escaped \n
        var normalizedKey = privateKey.Replace("\\n", "\n");
        var initializer = new ServiceAccountCredential.Initializer(clientEmail)
        {
            ProjectId = string.IsNullOrWhiteSpace(projectId) ? null : projectId,
            KeyId = string.IsNullOrWhiteSpace(privateKeyId) ? null : privateKeyId,
        }.FromPrivateKey(normalizedKey);

        var svcCred = new ServiceAccountCredential(initializer);
        credential = GoogleCredential.FromServiceAccountCredential(svcCred);

        FirebaseApp.Create(new AppOptions { Credential = credential });
    }
    catch
    {
        // Do not fallback to default app without credentials to avoid unexpected behavior
        throw;
    }
}
