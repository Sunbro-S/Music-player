using Infrastructure.Data.Models;

namespace Domain.Interfaces
{
    public interface IMusicRepository
    {
        public Guid AddMusicAsync(Music music);
    }
}
