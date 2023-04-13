using Moq;

namespace Muuzika.ServerTests.Helpers.Extensions;

public static class RandomMockExtensions
{
    public static void SetupNextSequence(this Mock<Random> randomMock, params int[] sequence)
    {
        _ = sequence.Aggregate(
            randomMock.SetupSequence(x => x.Next(It.IsAny<int>())), 
            (current, randomNumber) => current.Returns(randomNumber)
        );
    }
}