using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace RaceDay_2.Controllers
{
    //start of namesoace
    
    //helper class used to hash and vheck the users passwords
    // keeps the raw password out of the database 

    public static class Password_Helper
    {
        //start of class

        //number if iteractions used when hashing
        private const int iterations = 100000;

        //size of the salt resulting hash
        private const int salt_size = 16;
        private const int hash_size = 32;

        //hashes the plain text password with a random salt
        //returns a single that stores the iterations
        public static string hash_password(string password)
        {
            //start of method 

            //genrate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(salt_size);

            //get the hash from the password and salt
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash_size);

            //combine iteractions, salt and hash into one stored string
            return string.Format("{0}.{1}.{2}", iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));

            //end of method
        }
        //end of class
    }

    //end of namespace
}
