using MDT.Core.Data;
using MDT.Core.Interfaces;
using MDT.Core.Services;
using MDT.TaskSequence.Executors;
using MDT.TaskSequence.Parsers;
using MDT.Plugins.Steps;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Modern Deployment Toolkit API", Version = "v1" });
});

var useSqlite = builder.Configuration.GetValue<bool>("Database:UseSqlite", true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=mdt.db";

if (useSqlite)
{
    builder.Services.AddDbContext<MdtDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<MdtDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddSingleton<IVariableManager, VariableManager>();
builder.Services.AddSingleton<IConditionEvaluator, ConditionEvaluator>();

builder.Services.AddTransient<ITaskSequenceParser, XmlTaskSequenceParser>();
builder.Services.AddTransient<ITaskSequenceParser, JsonTaskSequenceParser>();
builder.Services.AddTransient<ITaskSequenceParser, YamlTaskSequenceParser>();

builder.Services.AddTransient<IStepExecutor, ApplyWindowsImageExecutor>();
builder.Services.AddTransient<IStepExecutor, ApplyFFUImageExecutor>();
builder.Services.AddTransient<IStepExecutor, InstallApplicationExecutor>();
builder.Services.AddTransient<IStepExecutor, InstallDriverExecutor>();
builder.Services.AddTransient<IStepExecutor, CaptureUserStateExecutor>();
builder.Services.AddTransient<IStepExecutor, RestoreUserStateExecutor>();
builder.Services.AddTransient<IStepExecutor, RunCommandLineExecutor>();
builder.Services.AddTransient<IStepExecutor, SetVariableExecutor>();
builder.Services.AddTransient<IStepExecutor, FormatAndPartitionExecutor>();
builder.Services.AddTransient<IStepExecutor, RestartComputerExecutor>();

builder.Services.AddTransient<TaskSequenceEngine>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MdtDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Modern Deployment Toolkit API v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
