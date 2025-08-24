using Microsoft.AspNetCore.OData;

namespace JustAnAiAgent.Api;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddJustAnAiAgentApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            })
            .AddOData(opt =>
             {
                 opt.Select()
                     .Filter()
                     .Expand()
                     .OrderBy()
                     .Count()
                     .SetMaxTop(1000);
             });

        return services;
    }
}