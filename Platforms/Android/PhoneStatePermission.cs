
using Microsoft.Maui.ApplicationModel;

namespace AIPhoneAssistant.Mobile;

public class PhoneStatePermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[]
        RequiredPermissions =>
        [
            (Android.Manifest.Permission.ReadPhoneState, true)
        ];
}