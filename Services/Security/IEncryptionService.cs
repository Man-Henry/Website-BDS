namespace Website_QLPT.Services.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string clearText);
        string Decrypt(string cipherText);
    }
}