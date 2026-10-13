
using Microsoft.Maui.Storage;

namespace AIPhoneAssistant.Mobile
{
    public partial class MainPage : ContentPage
    {
        private bool _assistantActive = false;

        private const string BusyModeKey = "BusyModeEnabled";

        private bool IsBusyModeEnabled
        {
            get => Preferences.Default.Get(BusyModeKey, false);
            set => Preferences.Default.Set(BusyModeKey, value);
        }

        public MainPage()
        {
            InitializeComponent();

            BusyModeSwitch.IsToggled = IsBusyModeEnabled;
            UpdateBusyModeStatus(IsBusyModeEnabled);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            int count = Preferences.Default.Get(
                "CallScreeningCount", 0);

            RecentCallsCount.Text = count.ToString();

            RecentActivityLabel.Text = Preferences.Default.Get(
                "LastCallActivity",
                "No activity yet.");

            BusyModeSwitch.IsToggled = IsBusyModeEnabled;
            UpdateBusyModeStatus(IsBusyModeEnabled);
        }

        private void BusyModeSwitch_Toggled(
            object sender,
            ToggledEventArgs e)
        {
            IsBusyModeEnabled = e.Value;
            UpdateBusyModeStatus(e.Value);
        }

        private void UpdateBusyModeStatus(bool enabled)
        {
            BusyModeStatus.Text = enabled
                ? "Busy Mode ON - All callers eligible"
                : "Busy Mode OFF - Unknown callers only";
        }

        private async void OnAssistantClicked(
            object sender, EventArgs e)
        {
            _assistantActive = !_assistantActive;

            AssistantButton.Text = _assistantActive
                ? "Deactivate Assistant"
                : "Activate Assistant";

            AssistantStatus.Text = _assistantActive
                ? "Status: Assistant is active"
                : "Status: Assistant is inactive";

            await DisplayAlert(
                "AI Phone Assistant",
                _assistantActive
                    ? "Assistant activated successfully!"
                    : "Assistant deactivated.",
                "OK");
        }

        private async void OnEnableCallScreeningClicked(
            object sender, EventArgs e)
        {
#if ANDROID
            try
            {
                if (Android.OS.Build.VERSION.SdkInt <
                    Android.OS.BuildVersionCodes.Q)
                {
                    await DisplayAlert(
                        "Not Supported",
                        "Android 10 ya uske baad ka version chahiye.",
                        "OK");
                    return;
                }

                var contactsStatus =
                    await Permissions.CheckStatusAsync<
                        Permissions.ContactsRead>();

                if (contactsStatus != PermissionStatus.Granted)
                {
                    contactsStatus =
                        await Permissions.RequestAsync<
                            Permissions.ContactsRead>();
                }

                var roleManager =
                    (Android.App.Roles.RoleManager?)
                    Android.App.Application.Context
                        .GetSystemService(
                            Android.Content.Context.RoleService);

                if (roleManager == null)
                {
                    await DisplayAlert(
                        "Error",
                        "Role Manager nahi mila.",
                        "OK");
                    return;
                }

                string role =
                    Android.App.Roles.RoleManager.RoleCallScreening;

                if (!roleManager.IsRoleAvailable(role))
                {
                    await DisplayAlert(
                        "Unavailable",
                        "Is device par Call Screening role available nahi hai.",
                        "OK");
                    return;
                }

                if (roleManager.IsRoleHeld(role))
                {
                    await DisplayAlert(
                        "Enabled",
                        "Call Screening role pehle se enabled hai.",
                        "OK");
                    return;
                }

                var intent =
                    roleManager.CreateRequestRoleIntent(role);

                var activity = Platform.CurrentActivity;

                if (activity == null)
                {
                    await DisplayAlert(
                        "Error",
                        "Current Android activity nahi mili.",
                        "OK");
                    return;
                }

                activity.StartActivityForResult(intent, 1001);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
#else
            await DisplayAlert(
                "Android Only",
                "Call Screening Android app mein hi available hai.",
                "OK");
#endif
        }

        private async void OnTestNotificationClicked(
            object sender, EventArgs e)
        {
#if ANDROID
            var status =
                await Permissions.CheckStatusAsync<
                    Permissions.PostNotifications>();

            if (status != PermissionStatus.Granted)
            {
                status =
                    await Permissions.RequestAsync<
                        Permissions.PostNotifications>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert(
                    "Permission Required",
                    "Please allow notifications for AI Phone Assistant in phone settings.",
                    "OK");
                return;
            }

            AIPhoneAssistant.Mobile.Platforms.Android
                .NotificationHelper.ShowTestNotification();
#else
            await DisplayAlert(
                "Test",
                "Notifications are supported on Android.",
                "OK");
#endif
        }

        private void SaveSpeechPurpose(string purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose))
                return;

            Preferences.Default.Set(
                "LastCallActivity",
                $"Captured purpose: {purpose} at {DateTime.Now:dd MMM, hh:mm tt}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RecentActivityLabel.Text =
                    Preferences.Default.Get(
                        "LastCallActivity",
                        "No activity yet.");
            });
        }




        private async void OnTestSpeechClicked(object sender, EventArgs e)
        {
#if ANDROID
            try
            {
                var status = await Permissions.RequestAsync<
                    Permissions.Microphone>();

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert(
                        "Permission Required",
                        "Please allow microphone access in Settings.",
                        "OK");
                    return;
                }

                bool start = await DisplayAlert(
                    "Speech Test",
                    "Press Start, then speak clearly.",
                    "Start",
                    "Cancel");

                if (!start)
                    return;

                var speechService =
                    new AIPhoneAssistant.Mobile.Platforms.Android
                        .AndroidSpeechService();

                string result = await speechService.ListenOnceAsync();

                if (!string.IsNullOrWhiteSpace(result) &&
                    !result.StartsWith("Speech recognition failed:",
                        StringComparison.OrdinalIgnoreCase) &&
                    !result.StartsWith("No speech was recognized",
                        StringComparison.OrdinalIgnoreCase) &&
                    !result.StartsWith("Speech recognition is not available",
                        StringComparison.OrdinalIgnoreCase) &&
                    !result.StartsWith("Android activity is not available",
                        StringComparison.OrdinalIgnoreCase))
                {
                    SaveSpeechPurpose(result);
                }

                await DisplayAlert(
                    "Speech Recognition Result",
                    result,
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
#else
    await DisplayAlert(
        "Android Only",
        "Speech test is available on Android.",
        "OK");
#endif
        }




    }
}
