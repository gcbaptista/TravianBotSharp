﻿using MainCore.Common.Enums;
using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class QueueBuildingDto
    {
        public int Position { get; set; }
        public int Location { get; set; } = -1;
        public string Type { get; set; }
        public int Level { get; set; }
        public DateTime CompleteTime { get; set; }
    }

    [Mapper]
    public static partial class QueueBuildingMapper
    {
        public static QueueBuilding ToEntity(this QueueBuildingDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;

            return entity;
        }

        public static partial void To(this QueueBuildingDto dto, QueueBuilding entity);

        private static partial QueueBuilding ToEntity(this QueueBuildingDto dto);

        public static partial IQueryable<QueueBuildingDto> ToDto(this IQueryable<QueueBuilding> entities);

        private static BuildingEnums ToBuildingEnums(string str) => Enum.Parse<BuildingEnums>(str);
    }
}