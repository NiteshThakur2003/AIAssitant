
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Android.App;
using Android.Telecom;
using Android.Provider;

namespace AIPhoneAssistant.Mobile;

[Service(
    Name = "com.aiphoneassistant.mobile.AICallScreeningService",
    Permission = "android.permission.BIND_SCREENING_SERVICE",
    Exported = true)]
[IntentFilter(new[] { "android.telecom.CallScreeningService" })]
public class AICallScreeningService : CallScreeningService
{
    private const string HistoryKey = "RecentCallHistory";
    private const string CountKey = "CallScreeningCount";
    private const string LastActivityKey = "LastCallActivity";
    private const string BusyModeKey = "BusyModeEnabled";

    private sealed class CallHistoryItem
    {
        public string Caller { get; set; } = "";
        public string Time { get; set; } = "";
    }

    public override void OnScreenCall(Call.Details callDetails)
    {
        Android.Util.Log.Info(
            "AIPhoneAssistant",
            "Incoming call screening service triggered.");

        string number = GetCallerNumber(callDetails);
        string callerInfo = number;
        bool isKnownContact = false;

        Android.Util.Log.Info(
            "AIPhoneAssistant",
            $"Extracted caller number: {number}");

        try
        {
            string contactName = FindContactName(number);

            if (!string.IsNullOrWhiteSpace(contactName))
            {
                callerInfo = contactName;
                isKnownContact = true;
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn(
                "AIPhoneAssistant",
                $"Contact lookup failed: {ex.Message}");
        }

        bool busyMode = Preferences.Default.Get(
            BusyModeKey, false);

        // Busy ON: all callers eligible.
        // Busy OFF: unknown callers eligible only.
        bool shouldHandleWithAI =
            busyMode || !isKnownContact;

        Android.Util.Log.Info(
            "AIPhoneAssistant",
            $"Busy Mode: {busyMode}; " +
            $"Known Contact: {isKnownContact}; " +
            $"AI Eligible: {shouldHandleWithAI}");

        try
        {
            SaveCallHistory(callerInfo);

            AIPhoneAssistant.Mobile.Platforms.Android
                .NotificationHelper
                .ShowIncomingCallNotification(callerInfo);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "AIPhoneAssistant",
                $"Call history/notification error: {ex.Message}");
        }

        // This version preserves normal call behaviour.
        // Eligibility is logged; it does not answer the call
        // or start a two-way AI conversation.
        var response = new CallResponse.Builder()
            .SetDisallowCall(false)
            .SetRejectCall(false)
            .SetSilenceCall(false)
            .SetSkipCallLog(false)
            .SetSkipNotification(false)
            .Build();

        RespondToCall(callDetails, response);
    }

    private string GetCallerNumber(Call.Details details)
    {
        try
        {
            var uri = details.GetHandle();

            if (uri == null)
                return "Unknown number";

            string number = uri.SchemeSpecificPart ?? "";

            if (string.IsNullOrWhiteSpace(number))
                return "Unknown number";

            number = Android.Net.Uri.Decode(number) ?? number;

            return string.IsNullOrWhiteSpace(number)
                ? "Unknown number"
                : number;
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn(
                "AIPhoneAssistant",
                $"Caller number extraction failed: {ex.Message}");

            return "Unknown number";
        }
    }

    private string FindContactName(string number)
    {
        if (number == "Unknown number" ||
            CheckSelfPermission(
                Android.Manifest.Permission.ReadContacts)
                != Android.Content.PM.Permission.Granted)
        {
            return "";
        }

        var uri = Android.Net.Uri.WithAppendedPath(
            ContactsContract.PhoneLookup.ContentFilterUri,
            Android.Net.Uri.Encode(number));

        string[] projection =
        {
            ContactsContract.PhoneLookup.InterfaceConsts.DisplayName
        };

        using var cursor = ContentResolver?.Query(
            uri,
            projection,
            null,
            null,
            null);

        if (cursor != null && cursor.MoveToFirst())
        {
            int index = cursor.GetColumnIndex(
                ContactsContract.PhoneLookup.InterfaceConsts.DisplayName);

            if (index >= 0)
                return cursor.GetString(index) ?? "";
        }

        return "";
    }

    private void SaveCallHistory(string callerInfo)
    {
        var history = new List<CallHistoryItem>();

        string savedJson = Preferences.Default.Get(
            HistoryKey, "");

        if (!string.IsNullOrWhiteSpace(savedJson))
        {
            try
            {
                history =
                    JsonSerializer.Deserialize<List<CallHistoryItem>>(
                        savedJson) ?? new List<CallHistoryItem>();
            }
            catch
            {
                history = new List<CallHistoryItem>();
            }
        }

        history.Insert(0, new CallHistoryItem
        {
            Caller = callerInfo,
            Time = DateTime.Now.ToString(
                "dd MMM yyyy, hh:mm tt")
        });

        history = history.Take(10).ToList();

        Preferences.Default.Set(
            HistoryKey,
            JsonSerializer.Serialize(history));

        int count = Preferences.Default.Get(CountKey, 0);

        Preferences.Default.Set(CountKey, count + 1);

        Preferences.Default.Set(
            LastActivityKey,
            $"Incoming call: {callerInfo} at " +
            DateTime.Now.ToString("dd MMM, hh:mm tt"));
    }
}
