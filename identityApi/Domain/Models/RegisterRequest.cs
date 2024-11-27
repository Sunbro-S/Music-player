namespace Domain;

public class RegisterRequest
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public string RolesCommaDelimited { get; set; }
}