using System.Diagnostics;
using System.Text;

namespace vkteams.Services
{
    public class LogService : IDisposable
    {
        public StreamWriter LogWriter { get; set; } = new StreamWriter($"/logs/log_{DateTime.Now:g}", Encoding.UTF8, new FileStreamOptions()
        {
            Access = FileAccess.Write,
            Share = FileShare.Write,
            Mode = FileMode.Append,
            Options = FileOptions.Asynchronous,
        });

        public void Dispose()
        {
            LogWriter.Dispose();
        }

        ~LogService()
        {
            Dispose();
        }

        public void Log(string message)
        {
            LogWriter.WriteLine(message.Replace("***", "%*%*%*") + "\r\n***");
        }

        public void Log(Exception exception)
        {
            Log(exception.Message +
                "\r\n\r\n[Stack trace]:" +
                $"\r\n{exception.StackTrace}");
        }
    }
}
