using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace HomeFlow.API.Infrastructure;

public class GlobalRoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefixRoute = new(new RouteAttribute(prefix));

    /// <summary>Prepends the configured route prefix to every controller's attribute route.</summary>
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
            foreach (var selector in controller.Selectors)
                selector.AttributeRouteModel = selector.AttributeRouteModel is not null
                    ? AttributeRouteModel.CombineAttributeRouteModel(_prefixRoute, selector.AttributeRouteModel)
                    : _prefixRoute;
    }
}
