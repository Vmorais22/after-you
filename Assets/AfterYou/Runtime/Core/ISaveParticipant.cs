namespace AfterYou.Core
{
    public interface ISaveParticipant
    {
        string SaveKey { get; }
        string CaptureJson();
        void RestoreJson(string json);
    }
}
