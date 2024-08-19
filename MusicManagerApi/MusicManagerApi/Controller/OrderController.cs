using Domain;
using Microsoft.AspNetCore.Mvc;
using Services.Services;

namespace MusicManagerApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly string _musicFolderPath;
    private readonly IMusicOrderService _musicOrderService;
    private readonly IMusicService _musicService;
    private readonly IPlaylistService _playlistService;
    public OrderController(IMusicOrderService musicOrderService, 
        IMusicService musicService, 
        IPlaylistService playlistService, 
        IConfiguration configuration)
    {
        _musicOrderService = musicOrderService;
        _musicService = musicService;
        _playlistService = playlistService;
        
        _musicFolderPath = configuration["MusicAddress"];
    }
    [HttpGet("Test")]
    public IActionResult Test()
    {
        return Ok("Controller is working.");
    }

    [HttpPost("UploadMusic")]
    public async Task<IActionResult> UploadMusic(IFormFile musicFile, string musicName, string author)
    {
        if (musicFile == null)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _musicService.PushMusicAsync(musicFile,musicName,author, _musicFolderPath);

        if (result)
        {
            return Ok("File uploaded successfully.");
        }
    
        return StatusCode(500, "An error occurred while uploading the file.");
    }

    [HttpGet("GetListMusic")]
    public async Task<List<MusicSerchResponse>> GetListMusic(string request)
    {
            var listOfMusic = _musicService.GetPotentialMusic(request);
            return await listOfMusic;
    }
        
        
    [HttpGet("PlayMusic")]
    public async Task<IActionResult> PlayMusic(string musicName)
    {
        try
        {
            var fileStream = _musicService.PlaySelectedMusicAsync(musicName);
            var mimeType = "audio/mpeg";
            return File(fileStream.Result, mimeType, enableRangeProcessing: true);
        }
        catch (FileNotFoundException)
        {
            return NotFound("Music file not found.");
        }
        
        
    }
}