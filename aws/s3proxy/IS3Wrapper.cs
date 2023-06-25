namespace MorningstarAWD.Common.Amazon
{
    public interface IS3Wrapper
    {
        byte[] GetObject(AWDEnvironment env, string key, string type);

        byte[] GetObjectPartial(AWDEnvironment env, string key, string type, long start, long end);

        void PutObject(AWDEnvironment env, string key, string type, string content);

        void PutObject(AWDEnvironment env, string key, string type, byte[] content);

        void DeleteObject(AWDEnvironment env, string key, string type);
    }
}
