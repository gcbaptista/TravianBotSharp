﻿using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerClaimQuestTask : INotificationHandler<QuestUpdated>, INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public TriggerClaimQuestTask(ITaskManager taskManager, IVillageSettingRepository villageSettingRepository)
        {
            _taskManager = taskManager;
            _villageSettingRepository = villageSettingRepository;
        }

        public async Task Handle(QuestUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var autoClaimQuest = _villageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoClaimQuestEnable);
            if (autoClaimQuest)
            {
                if (_taskManager.IsExist<ClaimQuestTask>(accountId, villageId)) return;
                await _taskManager.Add<ClaimQuestTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<ClaimQuestTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}