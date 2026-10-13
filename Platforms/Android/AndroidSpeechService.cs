
#if ANDROID
using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Speech;

namespace AIPhoneAssistant.Mobile.Platforms.Android
{
    public class AndroidSpeechService
    {
        public Task<string> ListenOnceAsync()
        {
            var completion = new TaskCompletionSource<string>();

            try
            {
                var activity = Platform.CurrentActivity;

                if (activity == null)
                {
                    completion.TrySetResult(
                        "Android activity is not available.");
                    return completion.Task;
                }

                if (!SpeechRecognizer.IsRecognitionAvailable(activity))
                {
                    completion.TrySetResult(
                        "Speech recognition is not available on this device.");
                    return completion.Task;
                }

                var recognizer =
                    SpeechRecognizer.CreateSpeechRecognizer(activity);

                var listener = new SpeechListener(
                    result => completion.TrySetResult(result),
                    () =>
                    {
                        recognizer.Destroy();
                        completion.TrySetResult(
                            "No speech was recognized. Please try again.");
                    });

                recognizer.SetRecognitionListener(listener);

                var intent = new Intent(
                    RecognizerIntent.ActionRecognizeSpeech);

                intent.PutExtra(
                    RecognizerIntent.ExtraLanguageModel,
                    RecognizerIntent.LanguageModelFreeForm);

                intent.PutExtra(
                    RecognizerIntent.ExtraLanguage,
                    "en-IN");

                recognizer.StartListening(intent);
            }
            catch (Exception ex)
            {
                completion.TrySetResult(
                    $"Speech recognition error: {ex.Message}");
            }

            return completion.Task;
        }

        private sealed class SpeechListener : Java.Lang.Object,
            IRecognitionListener
        {
            private readonly Action<string> _onResult;
            private readonly Action _onError;

            public SpeechListener(
                Action<string> onResult,
                Action onError)
            {
                _onResult = onResult;
                _onError = onError;
            }

            public void OnResults(Bundle? results)
            {
                var matches = results?.GetStringArrayList(
                    SpeechRecognizer.ResultsRecognition);

                if (matches != null && matches.Count > 0)
                    _onResult(matches[0]);
                else
                    _onError();
            }

            public void OnError(SpeechRecognizerError error)
            {
                _onResult($"Speech recognition failed: {error}");
            }

            public void OnReadyForSpeech(Bundle? parameters) { }
            public void OnBeginningOfSpeech() { }
            public void OnRmsChanged(float rmsdB) { }
            public void OnBufferReceived(byte[]? buffer) { }
            public void OnEndOfSpeech() { }
            public void OnPartialResults(Bundle? partialResults) { }
            public void OnEvent(int eventType, Bundle? parameters) { }
        }
    }
}
#endif
