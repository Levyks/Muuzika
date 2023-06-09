﻿using Muuzika.Server.Dtos.Hub;
using Muuzika.Server.Dtos.Hub.Responses;
using Muuzika.Server.Models;

namespace Muuzika.Server.Mappers.Interfaces;

public interface IPlayerMapper
{
    PlayerDto ToDto(Player player);
}