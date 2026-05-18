using Microsoft.AspNetCore.Mvc;
using PMS.Services;

namespace PMS.Services;

public static class AmsPermissionViewHelper
{
    public const string ModuleKey = "AccountsManagement";

    public static void SetViewBags(Controller controller, string? perm, IModulePermissionService modulePermission)
    {
        controller.ViewBag.CanCreate = modulePermission.CanEdit(perm);
        controller.ViewBag.CanEdit = modulePermission.CanEdit(perm);
        // CanDelete is set by AmsViewBagFilter from User.UserType = Admin.
    }
}
