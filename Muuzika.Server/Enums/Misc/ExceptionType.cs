﻿namespace Muuzika.Server.Enums.Misc;

public enum ExceptionType
{
    #region Endpoints
    InvalidCaptcha,
    OutOfAvailableRoomCodes,
    RoomIsFull,
    UsernameAlreadyTaken,
    #endregion
    
    #region Hub
    NoTokenProvided,
    InvalidToken,
    PlayerNotFound,
    LeaderOnlyAction,
    CannotKickLeader,
    InvalidArguments,
    #endregion

    #region Both
    RoomNotFound,
    Unknown,
    #endregion
}