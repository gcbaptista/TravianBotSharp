﻿using MainCore.Common.Models;

namespace MainCore.Commands.Misc
{
    public class OpenBrowserCommand
    {
        private readonly GetAccount _getAccount = new();
        private readonly GetSetting _getSetting = new();

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, AccessDto access, CancellationToken cancellationToken)
        {
            var account = _getAccount.Execute(accountId);
            var uri = new Uri(account.Server);

            var serverFolderName = uri.Host.Replace(".", "_");
            var accountFolderName = account.Username;

            var headlessChrome = _getSetting.BooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
            var profilePath = Path.Combine(serverFolderName, accountFolderName);
            var chromeSetting = new ChromeSetting()
            {
                UserAgent = access.Useragent,
                ProfilePath = profilePath,
                ProxyHost = access.ProxyHost,
                ProxyPort = access.ProxyPort,
                ProxyUsername = access.ProxyUsername,
                ProxyPassword = access.ProxyPassword,
                IsHeadless = headlessChrome,
            };
            var result = await chromeBrowser.Setup(chromeSetting);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.Navigate($"{account.Server}/dorf1.php", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}