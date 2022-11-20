using System.Text;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//services cors
builder.Services.AddCors(p => p.AddPolicy("usecors", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
var app = builder.Build();

//make app use cors
app.UseCors("usecors");

app.MapGet("/", () => "Hello World!");
app.MapPost("/encrypt", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        string jsonstring = await reader.ReadToEndAsync();
        Postreq? data =
       JsonSerializer.Deserialize<Postreq>(jsonstring);
        string encryptedtext = "";
        switch (Convert.ToInt32(data?.Cipher))
        {
            case 1:
                encryptedtext = CaesarEncrypt(data.Text, Convert.ToInt32(data.Key));
                break;
            case 2:
                encryptedtext = CaesarEncrypt(data.Text, Convert.ToInt32(data.Key));
                break;
            case 3:
                //
                break;
            default:
                //
                break;
        }

        Postres res = new Postres(data.Text, encryptedtext);
        return JsonSerializer.Serialize(res);
    }
});
app.MapPost("/decrypt", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
              string jsonstring = await reader.ReadToEndAsync();
        Postreq? data =
       JsonSerializer.Deserialize<Postreq>(jsonstring);
        string encryptedtext = "";
        switch (Convert.ToInt32(data?.Cipher))
        {
            case 1:
                encryptedtext = CaesarDecrypt(data.Text, Convert.ToInt32(data.Key));
                break;
            case 2:
                encryptedtext = CaesarDecrypt(data.Text, Convert.ToInt32(data.Key));
                break;
            case 3:
                //
                break;
            default:
                //
                break;
        }

        Postres res = new Postres(data.Text, encryptedtext);
        return JsonSerializer.Serialize(res);
    }
});

app.Run();

// bad caesar implementation
static char Caesar(char ch, int key)
{
    if (!char.IsLetter(ch))
    {
        return ch;
    }
    char d = char.IsUpper(ch) ? 'A' : 'a';
    return (char)((((ch + key) - d) % 26) + d);
}

static string CaesarEncrypt(string input, int key)
{
    string output = string.Empty;
    foreach (char ch in input)
        output += Caesar(ch, key);
    return output;
}

static string CaesarDecrypt(string input, int key)
{
    return CaesarEncrypt(input, 26 - key);
}

public class Postreq
{
    public string Cipher { get; set; }
    public string Text { get; set; }
    public string Key { get; set; }
    public Postreq(string cipher, string text, string key)
    {
        this.Cipher = cipher;
        this.Text = text;
        this.Key = key;
    }
}

public class Postres
{
    public string TextBefore { get; set; }
    public string TextNow { get; set; }
    public Postres(string textbefore, string textnow)
    {
        this.TextBefore = textbefore;
        this.TextNow = textnow;
    }
}
