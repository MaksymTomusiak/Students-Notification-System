using Microsoft.AspNetCore.Mvc.Razor;

namespace Infrastructure.Services;

public class InfrastructureViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // No values to populate in this case
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // Add custom view locations for the Infrastructure project
        var customLocations = new[]
        {
            "/Infrastructure/Views/Emails/{1}/{0}.cshtml",
            "/Infrastructure/Views/Emails/{0}.cshtml",
            "wwwroot/Views/Emails/{0}.cshtml",
        };

        return viewLocations.Concat(customLocations);
    }
}