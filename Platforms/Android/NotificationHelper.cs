
using Android.App;
using Android.Content;
using Android.OS;

namespace AIPhoneAssistant.Mobile.Platforms.Android;

public static class NotificationHelper
{
    private const string ChannelId = "ai_phone_assistant_calls";
    private const int NotificationId = 1001;

    public static void ShowTestNotification()
    {
        var context = global::Android.App.Application.Context;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ChannelId,
                "AI Phone Assistant",
                NotificationImportance.High)
            {
                Description = "Incoming call alerts from AI Phone Assistant"
            };

            var manager = (NotificationManager)context.GetSystemService(
                Context.NotificationService)!;

            manager.CreateNotificationChannel(channel);
        }

        var builder = new Notification.Builder(context)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentTitle("AI Phone Assistant")
            .SetContentText("Test notification is working!")
            .SetAutoCancel(true);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            builder.SetChannelId(ChannelId);
        }

        var notificationManager =
            (NotificationManager)context.GetSystemService(
                Context.NotificationService)!;

        notificationManager.Notify(NotificationId, builder.Build());
    }

    public static void ShowIncomingCallNotification(string callerInfo)
    {
        var context = global::Android.App.Application.Context;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ChannelId,
                "AI Phone Assistant",
                NotificationImportance.High)
            {
                Description = "Incoming call alerts"
            };

            var manager = (NotificationManager)context.GetSystemService(
                Context.NotificationService)!;

            manager.CreateNotificationChannel(channel);
        }

        var builder = new Notification.Builder(context)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentTitle("Incoming Call Detected")
            .SetContentText($"Caller: {callerInfo}")
            .SetAutoCancel(true);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            builder.SetChannelId(ChannelId);
        }

        var notificationManager =
            (NotificationManager)context.GetSystemService(
                Context.NotificationService)!;

        notificationManager.Notify(1002, builder.Build());
    }
}