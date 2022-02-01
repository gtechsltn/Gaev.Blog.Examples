using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Gaev.Blog.Examples.PiiManagement.PiiSerializers;

public class Aes128WithRandomIv : IPiiSerializer
{
    private readonly string _key;

    public Aes128WithRandomIv(string key)
    {
        _key = key;
    }

    public string ToString(PiiString piiString)
    {
        var stringToEncrypt = piiString.ToString();
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_key);
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var buffer = Encoding.UTF8.GetBytes(stringToEncrypt);
        var encryptedBuffer = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
        return Convert.ToBase64String(aes.IV.Concat(encryptedBuffer).ToArray());
    }

    public PiiString FromString(string str)
    {
        var dataToDecrypt = Convert.FromBase64String(str);
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_key);
        var ivSize = aes.BlockSize / 8;
        aes.IV = dataToDecrypt.Take(ivSize).ToArray();
        using var decryptor = aes.CreateDecryptor();
        var encryptedBuffer = dataToDecrypt.Skip(ivSize).ToArray();
        var decryptedBuffer = decryptor.TransformFinalBlock(encryptedBuffer, 0, encryptedBuffer.Length);
        return new PiiString(Encoding.UTF8.GetString(decryptedBuffer));
    }
}