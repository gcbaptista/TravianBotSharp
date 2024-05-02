﻿using MainCore.Infrasturecture.Persistence;
using MainCore.Tasks.Base;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartFarmListTask : FarmListTask
    {
        private readonly IAccountSettingRepository _accountSettingRepository;
        private readonly IFarmRepository _farmRepository;
        private readonly ITaskManager _taskManager;

        public StartFarmListTask(IChromeManager chromeManager, IMediator mediator, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand, IFarmParser farmParser, IAccountSettingRepository accountSettingRepository, IFarmRepository farmRepository, ITaskManager taskManager) : base(chromeManager, mediator, contextFactory, delayClickCommand, farmParser)
        {
            _accountSettingRepository = accountSettingRepository;
            _farmRepository = farmRepository;
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            var chromeBrowser = _chromeManager.Get(AccountId);

            result = await ToFarmListPage(chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var html = chromeBrowser.Html;

            var useStartAllButton = _accountSettingRepository.GetBooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                var startAllButton = _farmParser.GetStartAllButton(html);
                if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

                result = await chromeBrowser.Click(By.XPath(startAllButton.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                var farmLists = _farmRepository.GetActive(AccountId);
                if (farmLists.Count == 0) return Result.Fail(Skip.NoActiveFarmlist);

                foreach (var farmList in farmLists)
                {
                    var startButton = _farmParser.GetStartButton(html, farmList);
                    if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                    result = await chromeBrowser.Click(By.XPath(startButton.XPath));
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                    await _delayClickCommand.Execute(AccountId);
                }
            }
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = _accountSettingRepository.GetByName(AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Start farm list";
        }
    }
}