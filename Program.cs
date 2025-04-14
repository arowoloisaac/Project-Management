
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.ExternalServices.EmailService;
using Task_Management_System.Services.AvatarService;
using Task_Management_System.Services.Configurations.TokenGenerator;
using Task_Management_System.Services.GroupUserService;
using Task_Management_System.Services.OrganizationGroupService;
using Task_Management_System.Services.OrganizationProjectService;
using Task_Management_System.Services.OrganizationService;
using Task_Management_System.Services.OrganizationUserService;
using Task_Management_System.Services.ProjectService;
using Task_Management_System.Services.UserConfiguration.UserRoleConfiguration;
using Task_Management_System.Services.UserConfiguration;
using Task_Management_System.Services.UserService;
using Task_Management_System.Services.WikiService;
using Task_Management_System.Services.TaskService;
using Task_Management_System.Services.GroupIssueService;
using Task_Management_System.Services.TaskAnalyserService;
using Quartz;
using Task_Management_System.Services.BackgroundJobs;
using Task_Management_System.Configurations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Task_Management_System.Services.Configurations;
using Amazon.Runtime;
using Amazon.S3;
using Task_Management_System.ExternalServices.CloudSetting;
using Task_Management_System.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Task_Management_System
{
    public class Program
     {
         public static async Task Main(string[] args)
         {
             var builder = WebApplication.CreateBuilder(args);

             // Add services to the container.

             builder.Services.AddAutoMapper(typeof(AutoMapperConfiguration));

             //database configuration
             builder.Services.AddDbContext<ApplicationDbContext>(options =>
             {
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
             });

             //adding the dataonly configuration
             builder.Services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add( new DateOnlyJsonConverter()));


             // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
             builder.Services.AddEndpointsApiExplorer();
             builder.Services.AddSwaggerGen(opt => {
                 opt.MapType<DateOnly>(() => new OpenApiSchema
                 {
                     Type = "string",
                     Format = "date",
                     Example = new OpenApiString(DateTime.Today.ToString("yyyy-MM-dd"))
                 });

                 opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Project Manager", Version = "v1" });
                 opt.EnableAnnotations();
                 opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                 {
                     In = ParameterLocation.Header,
                     Description = "Input valid token",
                     Name = "Authorization",
                     Type = SecuritySchemeType.Http,
                     BearerFormat = "JWT",
                     Scheme = "Bearer"
                 });
                 opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id ="Bearer"
                             }
                         },
                         new string[]{}
                     }
                 });
             });

             //configuring identity
             builder.Services.AddIdentity<User, Role>(
                options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 8;
                }
            ).AddEntityFrameworkStores<ApplicationDbContext>();

             var jwtSection = builder.Configuration.GetSection("JwtBearerToken");
             builder.Services.Configure<JwtBearerSetting>(jwtSection);

             var jwtConfig = jwtSection.Get<JwtBearerSetting>();
             var key = Encoding.ASCII.GetBytes(jwtConfig.SecretKey);

             builder.Services.AddAuthentication(opt =>
             {
                 opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             }).AddJwtBearer(opt =>
             {
                 opt.SaveToken = true;
                 opt.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = jwtConfig.Audience,
                     ValidIssuer = jwtConfig.Issuer,
                     IssuerSigningKey = new SymmetricSecurityKey(key)
                 };
             });

             //for quartz configration
             builder.Services.AddQuartz(qrt =>
             {
                 var jobs = new JobKey("SendReminderJob");
                 qrt.AddJob<SendReminderJobs>(opt => opt.WithIdentity(jobs));

                 qrt.AddTrigger(opts =>
                 {
                     opts
                     .ForJob(jobs)
                     .WithIdentity("SendReminderJobTrigger")
                     .WithSimpleSchedule(x => x
                         .WithInterval(TimeSpan.FromDays(1))
                         .RepeatForever());
                 });
             });

             //host the quartz
             builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

             // bind the cloud setting with the json
             var cloudSection = builder.Configuration.GetSection("AWS");
             builder.Services.Configure<YandexCloudSetting>(cloudSection);

             // get the binded section here
             var cloudConfig = cloudSection.Get<YandexCloudSetting>();

             //aws for yandex cloud configuration
             AmazonS3Config configS3 = new AmazonS3Config
             {
                 ServiceURL = cloudConfig.ServiceURL,
                 ForcePathStyle = true,
             };

             // set the credentials
             var credentials = new BasicAWSCredentials(cloudConfig.AccessKey, cloudConfig.SecretKey);
             AmazonS3Client s3Client = new AmazonS3Client(credentials, configS3);

             // this is to register the aws using this package "dotnet add package AWSSDK.Extensions.NETCore.Setup"
             builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
             builder.Services.AddAWSService<IAmazonS3>();

             builder.Services.AddScoped<IUserService, UserService>();
             builder.Services.AddScoped<IUserConfig, UserConfig>();
             builder.Services.AddScoped<ITaskService, TaskService>();
             builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
             builder.Services.AddScoped<IProjectService, ProjectService>();
             builder.Services.AddScoped<IAvatarService, AvatarService>();
             builder.Services.AddScoped<IUserRoleConfiguration, UserRoleConfiguration>();
             builder.Services.AddScoped<IOrganizationService, OrganizationService>();
             builder.Services.AddScoped<IOrganizationUserService, OrganizationUserService>();
             builder.Services.AddScoped<IOrganizationGroupService, OrganizationGroupService>();
             builder.Services.AddScoped<IOrganizationProjectService, OrganizationProjectService>();
             builder.Services.AddSingleton(s3Client);
             builder.Services.AddScoped<ICloudService, CloudService>();
             builder.Services.AddScoped<IEmailService, EmailService>();
             builder.Services.AddScoped<ITaskAnalyserService, TaskAnalyserService>();
             builder.Services.AddScoped<IWikiService, WikiService>();
             builder.Services.AddScoped<IGroupUserService, GroupUserService>();
             builder.Services.AddScoped<IGroupTaskService, GroupTaskService>();

             builder.Services.AddCors();

             var app = builder.Build();

             using var serviceScope = app.Services.CreateScope();
             var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
             context.Database.Migrate();

             // Configure the HTTP request pipeline.
             if (app.Environment.IsDevelopment())
             {
                 app.UseSwagger();
                 app.UseSwaggerUI();
             }

             app.UseHttpsRedirection();

             app.UseCors(builder => builder
               .AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod());


             app.UseAuthorization();
             app.UseAuthorization();

             await app.ConfigureIdentityAsync();
             await app.ConfigureDefaultAvatar();

             app.MapControllers();

             app.Run();
         }
     }
}
