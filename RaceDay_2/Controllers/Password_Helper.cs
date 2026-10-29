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

        //checks a plain text password againt the stored hash
        public static bool verify_password (string password, string stored_hash)
        {
            //start of method

            try
            {
                //start of try

                //split the stored value into 3 parts
                string[] parts = stored_hash.Split('.');
                if (parts.Length != 3 )
                {
                    return false;
                }

                int hash_iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] stored_key = Convert.FromBase64String(parts[2]);

                //get a hash from the supplied password using the same salt
                byte[] computed_key = Rfc2898DeriveBytes.Pbkdf2(password, salt, hash_iterations, HashAlgorithmName.SHA256, stored_key.Length);

                //compare the 2 hashes
                return CryptographicOperations.FixedTimeEquals(computed_key, stored_key);

                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                return false;

                //end of catch
            }
        }
        //end of class
    }

    //end of namespace
}
