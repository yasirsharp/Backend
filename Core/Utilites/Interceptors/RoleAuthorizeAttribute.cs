
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RoleAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }


    public void OnAuthorization(AuthorizationFilterContext filterContext)
    {
        // Kullanıcı bilgilerini al
        var user = filterContext.HttpContext.User;

        if (user == null || !_roles.Any(role => user.IsInRole(role)))
        {
            // Yetkisiz erişim durumunda 403 döndür
            filterContext.Result = new ForbidResult();
        }
    }
}