namespace MainCore.Commands.Misc
{
    public class DelayTaskCommand
    {
        private readonly GetSetting _getSetting = new();

        public async Task Execute(AccountId accountId)
        {
            var delay = _getSetting.ByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}