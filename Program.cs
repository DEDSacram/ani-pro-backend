using System;
using System.Text;
using System.Text.Json;
using System.Collections;
using System.Collections.Generic;

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
        


		Postres res = new Postres();
        switch (Convert.ToInt32(data?.Cipher))
        {
            case 1:
                res = CaesarCipher.Encipher(data.Text, Convert.ToInt32(data.Key));
				res.Display = new char[]{'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
                break;
            case 2:
                res = CaesarCipher.Encipher(data.Text, Convert.ToInt32(data.Key));
				res.Display = new char[]{'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
                break;
            case 3:
                res = PlayfairCipher.Encipher(data.Text, data.Key);
                break;
			case 4:
				string format = HomoCipher.RemoveSpecialCharacters((data.Text).ToLower());
				dynamic k = HomoCipher.CreateKey(format);
                res.TextBefore = format;
				res.TextNow = HomoCipher.Encipher(format, k);
                res.Display = k;
				break;
            default:
                //
                break;
        }
		res.Cipher = data?.Cipher;
		
 		return JsonSerializer.Serialize(res);
    }
});

// string text = "Lakoma Lokomotiva.";
// string ready = Homo.RemoveSpecialCharacters(text.ToLower());
// dynamic k = Homo.CreateKey(ready);
// string encrypted = Homo.Encipher(k,ready);
// string decrypted = Homo.Decipher(k,encrypted);

app.MapPost("/decrypt", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
              string jsonstring = await reader.ReadToEndAsync();
        Postreq? data =
       JsonSerializer.Deserialize<Postreq>(jsonstring);

		Postres res = new Postres();
        switch (Convert.ToInt32(data?.Cipher))
        {
            case 1:
                res = CaesarCipher.Decipher(data.Text, Convert.ToInt32(data.Key));
				res.Display = new char[]{'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
                break;
            case 2:
                res = CaesarCipher.Decipher(data.Text, Convert.ToInt32(data.Key));
				res.Display = new char[]{'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
                break;
            case 3:
                res = PlayfairCipher.Decipher(data.Text, data.Key);
                break;
			case 4:
				// Deserialize then pass
				// res = HomoCipher.Decipher(data.Text, data.Key);
				break;
            default:
                //
                break;
        }
		res.Cipher = data?.Cipher;

    
        return JsonSerializer.Serialize(res);
    }
});

app.Run();




class HomoCipher
{
    
    public static string Encipher(string ready,Dictionary<char,string[]> key){

        //helper
        Dictionary<char,int> count_per_char = new Dictionary<char,int>();
        foreach(var letter in key)
        {
            count_per_char.Add(letter.Key,0);
        }
        //

        StringBuilder sb = new StringBuilder();
        foreach(char letter in ready){
            sb.Append(key[letter][count_per_char[letter]]);
            count_per_char[letter] += 1;
        }
        return sb.ToString();
    }

    public static string Decipher(string ready,Dictionary<char,string[]> key){
            //get first same for every single one
        var e = key.GetEnumerator();
        e.MoveNext();
        var anElement = e.Current;
        //Get Depth
        int digits = anElement.Value[0].Length;
        StringBuilder sb = new StringBuilder();
        string code = "";
        for (int i = 0; i < ready.Length; i++)
        {
            code += ready[i];
                if ((i-1) % digits == 0){
                foreach(var pair in key){
                    int check = Array.IndexOf(pair.Value,code);
                    if(check == -1){
                        continue;
                    }else{
                        code = "";
                    }
                    sb.Append(pair.Key);
                }
            }
        }
        return sb.ToString();
        }
    public static Dictionary<char,string[]> CreateKey(string ready){
    //get Frequency
    Dictionary<char,int> frequency = prCharWithFreq(ready);
    // Generated Codes
    List<String> code = new List<String>();
    // Dynamic
    int digits = countDigit(ready.Length);
    string fmt = new String('0', digits);

    // codes from string to 000# format
    int upto = IntPow(10, digits);

    //Create codes
    for(int i = 0; i < upto; i++){
        code.Add(i.ToString(fmt));
    }
    // Initialize actual key
    Dictionary<char,string[]> key = new Dictionary<char,string[]>();
    // for easier decryption instead of combinatorics
    // Dictionary<char,int> count_per_char = new Dictionary<char,int>();

    // Get random from codes
    Random rnd = new Random();
    foreach(var pair in frequency)
    {
    // Creating array for codes per char
        string[] c_values = new string[pair.Value];
        for(int i = 0; i < pair.Value; i++){
            int rand = rnd.Next(0, code.Count());
            c_values[i] = code[rand];
            // Remove so its Unique
            code.RemoveAt(rand);
        }
    //add to key
        key.Add(pair.Key,c_values);
    //add to countperchar
    //count_per_char.Add(pair.Key,0);
    }
    return key;
    }

    static int IntPow(int x, int pow)
    {
        int ret = 1;
        while ( pow != 0 )
        {
            if ( (pow & 1) == 1 )
                ret = ret * x;
            x = x * x;
            pow >>= 1;
        }
        return ret;
    }


    static int countDigit(int n)
    {
        int count = 0;
        while (n != 0) {
            n = n / 10;
            ++count;
        }
        return count;
    }


    static Dictionary<char,int> prCharWithFreq(string s)
    {
    
    // Store all characters and
    // their frequencies in dictionary
    Dictionary<char,int> d = new Dictionary<char,int>();
    
    foreach(char i in s)
    {
        if(d.ContainsKey(i))
        {
            d[i]++;
        }
        else
        {
            d[i]=1; 
        }
    }
    return d;
    }

    public static string RemoveSpecialCharacters(string str) {
    StringBuilder sb = new StringBuilder();
    foreach (char c in str) {
        if ((c >= 'a' && c <= 'z')) {
            sb.Append(c);
        }
    }
    return sb.ToString();
    }
}



public class CaesarCipher{
static char Caesar(char ch, int key, char d)
{
    return (char)((((ch + key) - d) % 26) + d);
}

public static Postres Encipher(string input, int key)
{
	
    string output = string.Empty;
	int[][][][] ani = new int[input.Length][][][];
	for(int i = 0; i < input.Length; i++){
		char ch = input[i];
		char d =  char.IsUpper(ch) ? 'A' : 'a';
		if (!char.IsLetter(ch)) continue;
		output += Caesar(input[i], key, d);
		int[] from = new int[2] {(((ch) - d) % 26),0};
		int[] to = new int[2]{ ((((ch + key) - d) % 26)),0};
		ani[i] = new int[][][] {new int[][]{from,to}};
	}
    return new Postres(input,output,ani);
}

public static Postres Decipher(string input, int key)
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

private static Postres Cipher(string input, string key, bool encipher)
{
	string retVal = string.Empty;
	char[,] keySquare = GenerateKeySquare(key);
	string tempInput = RemoveOtherChars(input);
	int[][][][] ani = new int[(tempInput.Length % 2 == 0) ? tempInput.Length/2 : (tempInput.Length+1)/2][][][];
	char[][] square = new char[5][];
	for(int i = 0; i<keySquare.GetLength(0);i++){
		square[i] = new char[5] {keySquare[i,0],keySquare[i,1],keySquare[i,2],keySquare[i,3],keySquare[i,4]};
	}

	int e = encipher ? 1 : -1;
	if ((tempInput.Length % 2) != 0)
		tempInput += "X";

	char[] fix = new char[tempInput.Length];
	fix = tempInput.ToCharArray();
	for(int i = 1; i < tempInput.Length;i+=2){
		if(tempInput[i-1] == tempInput[i]){
			fix[i] = 'X';
			continue;
		}
		fix[i] = tempInput[i];
	}
	tempInput = new string(fix);
	for (int i = 0; i < tempInput.Length; i += 2)
	{
		int row1 = 0;
		int col1 = 0;
		int row2 = 0;
		int col2 = 0;
		int[] point3 = new int[2];
		int[] point1 = new int[2]; 
		int[] point4 = new int[2];
		int[] point2 = new int[2];
		GetPosition(ref keySquare, char.ToUpper(tempInput[i]), ref row1, ref col1);
		point3[0] = row1;
		point3[1] = col1;
		GetPosition(ref keySquare, char.ToUpper(tempInput[i + 1]), ref row2, ref col2);
		point4[0] = row2;
		point4[1] = col2;
		if (row1 == row2 && col1 == col2)
		{
			point1[0] = Mod((row1 + e), 5);
			point1[1] = Mod((col1 + e), 5);
			point2[0] = Mod((row1 + e), 5);
			point2[1] = Mod((row1 + e), 5);
		}
		else if (row1 == row2)
		{
			point1[0] = row1;
			point1[1] = Mod((col1 + e), 5);
			point2[0] = row1;
			point2[1] = Mod((col2 + e), 5);
		}
		else if(col1 == col2)
		{
			point1[0] = Mod((row1 + e), 5);
			point1[1] = col1;
			point2[0] = Mod((row2 + e),5);
			point2[1] = col1;
		}
		else
		{
			point1[0] = row1;
			point1[1] = col2;
			point2[0] = row2;
			point2[1] = col1;
		}
		retVal += new string(new char[] { keySquare[point1[0], point1[1]], keySquare[point2[0], point2[1]] });
		
		
		int[] from1 = new int[2] {point3[0],point3[1]};
		int[] to1 =  new int[2] {point1[0],point1[1]};
		int[] from2 = new int[2] {point4[0],point4[1]};
		int[] to2 =  new int[2] {point2[0],point2[1]};
		ani[Convert.ToInt32(i/2)] = new int[][][]{new int[][]{from1,to1},new int[][]{from2,to2}};
	}

	retVal = AdjustOutput(input, retVal);

	Postres res = new Postres(tempInput,retVal,ani);
	res.Display = square;
	return res;
}

public static Postres Encipher(string input, string key)
{
	return Cipher(input, key, true);
}

public static Postres Decipher(string input, string key)
{
	return Cipher(input, key, false);
}

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
	public dynamic Ani{get;set;}
	public string? Cipher{get;set;}
	public dynamic? Display{get;set;}
    public Postres(string textbefore, string textnow, dynamic ani)
    {
        this.TextBefore = textbefore;
        this.TextNow = textnow;
		this.Ani = ani;
    }
	public Postres(){

	}
}


