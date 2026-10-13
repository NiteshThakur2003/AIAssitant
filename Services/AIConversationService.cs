
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Media;

namespace AIPhoneAssistant.Mobile.Services
{
    public class AIConversationService
    {
        public async Task<string> SpeakGreetingAsync()
        {
            const string greeting =
                "Hello! You have reached the AI Phone Assistant. " +
                "Please tell me the purpose of your call.";

            await TextToSpeech.Default.SpeakAsync(greeting);

            return greeting;
        }

        public async Task<string> CapturePurposeAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<
                    Permissions.Microphone>();

                if (status != PermissionStatus.Granted)
                {
                    return "Microphone permission was not granted.";
                }

                return await CapturePurposeOnAndroidAsync();
            }
            catch (Exception ex)
            {
                return $"Speech recognition error: {ex.Message}";
            }
        }

        private async Task<string> CapturePurposeOnAndroidAsync()
        {
#if ANDROID
            return await Task.FromResult(
                "Android native speech recognition integration is the next step.");
#else
            return await Task.FromResult(
                "Speech recognition is currently supported only on Android.");
#endif
        }
    }
}
