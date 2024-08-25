using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RPC.Interface;
using Services.Services;

namespace MusicManagerApi.Controller;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly string _musicFolderPath;
    private readonly IMusicOrderService _musicOrderService;
    private readonly IMusicService _musicService;
    private readonly IRabbitMqServiceSender _mqServiceSender;
    private readonly IPlaylistService _playlistService;
    public OrderController(IMusicOrderService musicOrderService, 
        IMusicService musicService, 
        IPlaylistService playlistService, 
        IConfiguration configuration, IRabbitMqServiceSender mqServiceSender)
    {
        _musicOrderService = musicOrderService;
        _musicService = musicService;
        _playlistService = playlistService;
        _mqServiceSender = mqServiceSender;

        _musicFolderPath = configuration["MusicAddress"];
    }

    [HttpPost("sent")]
    public IActionResult SendMessage(object message)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var chanel = connection.CreateModel())
        {
            chanel.QueueDeclare(queue: "Test",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            
            chanel.BasicPublish(exchange:"",
                routingKey:"Test",
                basicProperties:null,
                body:body);
        }

        return Ok();
    }
    [HttpPost("AddFavorite")]
    public IActionResult SendMessage(MusicToAdd message)
    {
        string authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }
        var content = new RPCMusic()
        {
            UserId = authHeader,
            MusicId = message.MusicId,
            Status = message.Status
        };
        var result = _mqServiceSender.SendMessage(content);
        if (result!=null)
            return Ok("Песня добавлена");
        return BadRequest("Песня уже есть в избранном");
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