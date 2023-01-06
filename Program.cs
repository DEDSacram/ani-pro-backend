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
                encryptedtext = CaesarCipher.Encipher(data.Text, Convert.ToInt32(data.Key));
                break;
            case 2:
                encryptedtext = CaesarCipher.Encipher(data.Text, Convert.ToInt32(data.Key));
                break;
            case 3:
                encryptedtext = PlayfairCipher.Encipher(data.Text, data.Key);
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
                encryptedtext = CaesarCipher.Decipher(data.Text, Convert.ToInt32(data.Key));
                break;
            case 2:
                encryptedtext = CaesarCipher.Decipher(data.Text, Convert.ToInt32(data.Key));
                break;
            case 3:
                encryptedtext = PlayfairCipher.Decipher(data.Text, data.Key);
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

public class CaesarCipher{
static char Caesar(char ch, int key)
{
    if (!char.IsLetter(ch))
    {
        return ch;
    }
    char d = char.IsUpper(ch) ? 'A' : 'a';
    return (char)((((ch + key) - d) % 26) + d);
}

public static string Encipher(string input, int key)
{
    string output = string.Empty;
    foreach (char ch in input)
        output += Caesar(ch, key);
    return output;
}

public static string Decipher(string input, int key)
{
    return Encipher(input, 26 - key);
}

}

public class PlayfairCipher{

private static int Mod(int a, int b)
{
	return (a % b + b) % b;
}

private static List<int> FindAllOccurrences(string str, char value)
{
	List<int> indexes = new List<int>();

	int index = 0;
	while ((index = str.IndexOf(value, index)) != -1)
		indexes.Add(index++);

	return indexes;
}

private static string RemoveAllDuplicates(string str, List<int> indexes)
{
	string retVal = str;

	for (int i = indexes.Count - 1; i >= 1; i--)
		retVal = retVal.Remove(indexes[i], 1);

	return retVal;
}

private static char[,] GenerateKeySquare(string key)
{
	char[,] keySquare = new char[5, 5];
	string defaultKeySquare = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
	string tempKey = string.IsNullOrEmpty(key) ? "CIPHER" : key.ToUpper();

	tempKey = tempKey.Replace("J", "");
	tempKey += defaultKeySquare;

	for (int i = 0; i < 25; ++i)
	{
		List<int> indexes = FindAllOccurrences(tempKey, defaultKeySquare[i]);
		tempKey = RemoveAllDuplicates(tempKey, indexes);
	}

	tempKey = tempKey.Substring(0, 25);

	for (int i = 0; i < 25; ++i)
		keySquare[(i / 5), (i % 5)] = tempKey[i];

	return keySquare;
}

private static void GetPosition(ref char[,] keySquare, char ch, ref int row, ref int col)
{
	if (ch == 'J')
		GetPosition(ref keySquare, 'I', ref row, ref col);

	for (int i = 0; i < 5; ++i)
		for (int j = 0; j < 5; ++j)
			if (keySquare[i, j] == ch)
			{
				row = i;
				col = j;
			}
}

private static char[] SameRow(ref char[,] keySquare, int row, int col1, int col2, int encipher)
{
	return new char[] { keySquare[row, Mod((col1 + encipher), 5)], keySquare[row, Mod((col2 + encipher), 5)] };
}

private static char[] SameColumn(ref char[,] keySquare, int col, int row1, int row2, int encipher)
{
	return new char[] { keySquare[Mod((row1 + encipher), 5), col], keySquare[Mod((row2 + encipher), 5), col] };
}

private static char[] SameRowColumn(ref char[,] keySquare, int row, int col, int encipher)
{
	return new char[] { keySquare[Mod((row + encipher), 5), Mod((col + encipher), 5)], keySquare[Mod((row + encipher), 5), Mod((col + encipher), 5)] };
}

private static char[] DifferentRowColumn(ref char[,] keySquare, int row1, int col1, int row2, int col2)
{
	return new char[] { keySquare[row1, col2], keySquare[row2, col1] };
}

private static string RemoveOtherChars(string input)
{
	string output = input;

	for (int i = 0; i < output.Length; ++i)
		if (!char.IsLetter(output[i]))
			output = output.Remove(i, 1);

	return output;
}

private static string AdjustOutput(string input, string output)
{
	StringBuilder retVal = new StringBuilder(output);

	for (int i = 0; i < input.Length; ++i)
	{
		if (!char.IsLetter(input[i]))
			retVal = retVal.Insert(i, input[i].ToString());

		if (char.IsLower(input[i]))
			retVal[i] = char.ToLower(retVal[i]);
	}

	return retVal.ToString();
}

private static string Cipher(string input, string key, bool encipher)
{
	string retVal = string.Empty;
	char[,] keySquare = GenerateKeySquare(key);
	string tempInput = RemoveOtherChars(input);
	int e = encipher ? 1 : -1;

	if ((tempInput.Length % 2) != 0)
		tempInput += "X";

	for (int i = 0; i < tempInput.Length; i += 2)
	{
		int row1 = 0;
		int col1 = 0;
		int row2 = 0;
		int col2 = 0;

		GetPosition(ref keySquare, char.ToUpper(tempInput[i]), ref row1, ref col1);
		GetPosition(ref keySquare, char.ToUpper(tempInput[i + 1]), ref row2, ref col2);

		if (row1 == row2 && col1 == col2)
		{
			retVal += new string(SameRowColumn(ref keySquare, row1, col1, e));
		}
		else if (row1 == row2)
		{
			retVal += new string(SameRow(ref keySquare, row1, col1, col2, e));
		}
		else if(col1 == col2)
		{
			retVal += new string(SameColumn(ref keySquare, col1, row1, row2, e));
		}
		else
		{
			retVal += new string(DifferentRowColumn(ref keySquare, row1, col1, row2, col2));
		}
	}

	retVal = AdjustOutput(input, retVal);

	return retVal;
}

public static string Encipher(string input, string key)
{
	return Cipher(input, key, true);
}

public static string Decipher(string input, string key)
{
	return Cipher(input, key, false);
}

}


//usage
// string text = "Hello World";
// string cipherText = Encipher(text, "cipher");
// string plainText = Decipher(cipherText, "cipher");


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


