
using Android.Telephony;

namespace AIPhoneAssistant.Mobile;

public class CallStateListener : PhoneStateListener
{
    public event Action<string>? CallStateChanged;

    public override void OnCallStateChanged(
        CallState state,
        string? phoneNumber)
    {
        string message = state switch
        {
            CallState.Ringing => "Incoming call detected",
            CallState.Offhook => "Call active or answered",
            CallState.Idle => "Phone is idle",
            _ => "Unknown call state"
        };

        CallStateChanged?.Invoke(message);
    }
}