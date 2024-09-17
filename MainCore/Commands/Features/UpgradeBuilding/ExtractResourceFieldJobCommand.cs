﻿using MainCore.Common.Models;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class ExtractResourceFieldJobCommand
    {
        private readonly IMediator _mediator;

        private readonly DeleteJobCommand _deleteJobCommand = new();
        private readonly AddJobCommand _addJobCommand = new();

        public ExtractResourceFieldJobCommand(IMediator mediator = null)
        {
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId, JobDto job, CancellationToken cancellationToken)
        {
            var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);

            var normalBuildPlan = GetNormalBuildPlan(villageId, resourceBuildPlan);
            if (normalBuildPlan is null)
            {
                _deleteJobCommand.ByJobId(job.Id);
            }
            else
            {
                _addJobCommand.ToTop(villageId, normalBuildPlan);
            }
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
            return Result.Ok();
        }

        private static readonly Dictionary<ResourcePlanEnums, List<BuildingEnums>> _fieldList = new()
        {
            {ResourcePlanEnums.AllResources, new(){
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland,}},
            {ResourcePlanEnums.ExcludeCrop, new() {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,}},
            {ResourcePlanEnums.OnlyCrop, new() {
                BuildingEnums.Cropland,}},
        };

        private static NormalBuildPlan GetNormalBuildPlan(VillageId villageId, ResourceBuildPlan plan)
        {
            var resourceTypes = _fieldList[plan.Plan];

            var buildings = new GetBuildings().Execute(villageId, true);

            buildings = buildings
                .Where(x => resourceTypes.Contains(x.Type))
                .Where(x => x.Level < plan.Level)
                .ToList();

            if (!buildings.Any()) return null;

            var minLevel = buildings
                .Select(x => x.Level)
                .Min();

            var chosenOne = buildings
                .Where(x => x.Level == minLevel)
                .OrderBy(x => x.Id.Value + Random.Shared.Next())
                .FirstOrDefault();

            if (chosenOne is null) return null;

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
        }
    }
}