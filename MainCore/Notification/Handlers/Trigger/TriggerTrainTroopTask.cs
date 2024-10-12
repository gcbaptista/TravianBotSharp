﻿using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerTrainTroopTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;
        private readonly IGetSetting _getSetting;
        private readonly GetVillage _getVillage;

        public TriggerTrainTroopTask(ITaskManager taskManager, IGetSetting getSetting, GetVillage getVillage)
        {
            _taskManager = taskManager;
            _getSetting = getSetting;
            _getVillage = getVillage;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var villages = _getVillage.All(notification.AccountId);
            foreach (var village in villages)
            {
                await Trigger(notification.AccountId, village);
            }
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var trainTroopEnable = _getSetting.BooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (trainTroopEnable)
            {
                if (_taskManager.IsExist<TrainTroopTask>(accountId, villageId)) return;
                await _taskManager.Add<TrainTroopTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<TrainTroopTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}