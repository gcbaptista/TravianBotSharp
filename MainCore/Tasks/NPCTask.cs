﻿using MainCore.Commands.Features.NpcResource;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient<NpcTask>]
    public class NpcTask : VillageTask
    {
        private readonly IMediator _mediator;

        public NpcTask(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var toNpcResourcePageCommand = scoped.ServiceProvider.GetRequiredService<ToNpcResourcePageCommand>();
            result = await toNpcResourcePageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var npcResourceCommand = scoped.ServiceProvider.GetRequiredService<NpcResourceCommand>();
            result = await npcResourceCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateStorageCommand = scoped.ServiceProvider.GetRequiredService<UpdateStorageCommand>();
            result = await updateStorageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Publish(new StorageUpdated(AccountId, VillageId), cancellationToken);
            return Result.Ok();
        }

        protected override string TaskName => "NPC";
    }
}