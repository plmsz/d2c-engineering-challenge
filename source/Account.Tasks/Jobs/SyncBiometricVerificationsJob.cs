using System.Threading.Tasks;

namespace Account.Tasks.Jobs
{
    public class SyncBiometricVerificationsJob : IScheduledJob
    {
        public string Schedule => "0 */1 * * * *";

        public Task DoWorkAsync()
        {
            return Task.CompletedTask;
        }
    }
}
