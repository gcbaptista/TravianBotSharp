namespace MainCore.Commands.Misc
{
    public class DelayClickCommand
    {
        private readonly GetSetting _getSetting = new();

        public async Task Execute(AccountId accountId)
        {
            var delay = _getSetting.ByName(accountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}