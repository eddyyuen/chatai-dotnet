using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Yitter.IdGenerator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//初始化 id
var options = new IdGeneratorOptions(1);
YitIdHelper.SetIdGenerator(options);
//YitIdHelper.NextId();

//设置并发的速率控制
//var concurrencyPolicy = "Concurrency";
////var myOptions = new MyRateLimitOptions();
////builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions);

var RateLimiter = builder.Configuration.GetSection("RateLimiter");

//builder.Services.AddRateLimiter(_ => _.AddConcurrencyLimiter(policyName: concurrencyPolicy, options =>
//{
//    options.PermitLimit = Convert.ToInt16(RateLimiter["PermitLimit"]);// myOptions.PermitLimit;
//    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//    options.QueueLimit = Convert.ToInt16(RateLimiter["QueueLimit"]);// myOptions.QueueLimit;
//}));

var tokenPolicy = "token";

builder.Services.AddRateLimiter(_ => _
    .AddTokenBucketLimiter(policyName: tokenPolicy, options =>
    {
        options.TokenLimit = Convert.ToInt16(RateLimiter["TokenLimit"]);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = Convert.ToInt16(RateLimiter["QueueLimit"]);
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(Convert.ToInt16(RateLimiter["ReplenishmentPeriod"]));
        options.TokensPerPeriod = Convert.ToInt16(RateLimiter["TokensPerPeriod"]);
        options.AutoReplenishment = Convert.ToBoolean(RateLimiter["AutoReplenishment"]);
    }));


var api = new OpenAI_API.OpenAIAPI(new OpenAI_API.APIAuthentication(builder.Configuration.GetSection("ChaiAI")));
builder.Services.AddSingleton(api);
//builder.Services.AddSingleton<OpenAI_API.IOpenAIAPI,OpenAI_API.OpenAIAPI>();
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers().RequireRateLimiting(tokenPolicy);

app.Run();
