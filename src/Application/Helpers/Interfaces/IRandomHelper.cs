using Muuzika.Domain.Models.Playlist;

namespace Muuzika.Application.Helpers.Interfaces;

public interface IRandomHelper
{
    T TakeRandom<T>(ICollection<T> collection);
    ICollection<T> TakeRandom<T>(ICollection<T> collection, int count); 
}