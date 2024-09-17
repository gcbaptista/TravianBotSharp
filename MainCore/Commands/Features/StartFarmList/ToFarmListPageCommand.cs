using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    public class ToFarmListPageCommand : FarmListCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        private readonly SwitchVillageCommand _switchVillageCommand = new();
        private readonly ToDorfCommand _toDorfCommand = new();
        private readonly UpdateBuildingCommand _updateBuildingCommand = new();
        private readonly ToBuildingCommand _toBuildingCommand = new();
        private readonly SwitchTabCommand _switchTabCommand = new();

        public ToFarmListPageCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToPage(chromeBrowser, accountId, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ToPage(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            Result result;

            var rallypointVillageId = GetVillageHasRallypoint(accountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            result = await _switchVillageCommand.Execute(chromeBrowser, rallypointVillageId, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _toDorfCommand.Execute(chromeBrowser, 2, false, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _updateBuildingCommand.Execute(chromeBrowser, accountId, rallypointVillageId, cancellationToken);

            result = await _toBuildingCommand.Execute(chromeBrowser, 39, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _switchTabCommand.Execute(chromeBrowser, 4, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private VillageId GetVillageHasRallypoint(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Include(x => x.Buildings.Where(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
                .Where(x => x.Buildings.Count > 0)
                .OrderByDescending(x => x.IsActive)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .FirstOrDefault();
            return village;
        }
    }
}