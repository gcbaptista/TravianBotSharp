﻿namespace MainCore.Commands.Queries
{
    public class GetAccess
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogService _logService;

        private readonly GetSetting _getSetting = new();

        public GetAccess(IDbContextFactory<AppDbContext> contextFactory = null, ILogService logService = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _logService = logService ?? Locator.Current.GetService<ILogService>();
        }

        public Result<AccessDto> Execute(AccountId accountId, bool ignoreSleepTime = false)
        {
            var accesses = GetAccesses(accountId);
            var logger = _logService.GetLogger(accountId);

            var access = GetValidAccess(accesses, logger);

            if (access is null) return Stop.AllAccessNotWorking;

            UpdateAccessLastUsed(access.Id);

            if (accesses.Count == 1) return access;
            if (ignoreSleepTime) return access;

            var minSleep = _getSetting.ByName(accountId, AccountSettingEnums.SleepTimeMin);

            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Stop.LackOfAccess;
            return access;
        }

        private static AccessDto GetValidAccess(List<AccessDto> accesses, ILogger logger)
        {
            if (accesses.Count == 0) return null;
            var access = accesses[0];
            logger.Information("Using connection {Proxy}", access.Proxy);
            return access;
        }

        private List<AccessDto> GetAccesses(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var accessess = context.Accesses
               .Where(x => x.AccountId == accountId.Value)
               .OrderBy(x => x.LastUsed) // get oldest one
               .ToDto()
               .ToList();
            return accessess;
        }

        private void UpdateAccessLastUsed(AccessId accessId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Accesses
               .Where(x => x.Id == accessId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.LastUsed, x => DateTime.Now));
        }
    }
}