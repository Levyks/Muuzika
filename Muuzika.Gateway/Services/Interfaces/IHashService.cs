﻿namespace Muuzika.Gateway.Services.Interfaces;

public interface IHashService
{
    string Hash(string input);
    bool Verify(string input, string hash);
}