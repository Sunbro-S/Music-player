using Infrastructure;

namespace Services.Services;

public class MusicOrderService : IMusicOrderService
{
    private readonly ContextDb _context;
    public MusicOrderService(ContextDb context)
    {
        _context = context;
    }
    
}