
using Microsoft.AspNetCore.Http;

namespace RaceDay_MVC.Services
{
    //start of namespace

    public class ProfileImageStore
    {
        //start of class

        private readonly string _folder;

        //maximum image size is 2MB
        private const long max_size = 2 * 1024 * 1024;

        public ProfileImageStore(IWebHostEnvironment environment)
        {
            //start of constructor

            string web_root = environment.WebRootPath
                ?? Path.Combine(environment.ContentRootPath, "wwwroot");

            _folder = Path.Combine(
                web_root,
                "uploads",
                "profiles"
            );

            //end of constructor
        }

        //find the current user's saved profile picture
        public (string path, string content_type)? FindImage(int user_id)
        {
            //start of method

            if (user_id <= 0 || !Directory.Exists(_folder))
            {
                return null;
            }

            string prefix = $"user-{user_id}-";

            //find only images belonging to this user
            var files = Directory.EnumerateFiles(_folder)
                .Where(path =>
                    Path.GetFileName(path).StartsWith(
                        prefix,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .OrderByDescending(File.GetLastWriteTimeUtc);

            foreach (string path in files)
            {
                //start of foreach

                string extension = Path.GetExtension(path).ToLowerInvariant();

                string? content_type = extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => null
                };

                if (content_type != null)
                {
                    return (path, content_type);
                }

                //end of foreach
            }

            return null;

            //end of method
        }

        //validate and save a profile picture
        public async Task SaveImage(
            int user_id,
            IFormFile image,
            CancellationToken cancellation_token)
        {
            //start of method

            if (user_id <= 0)
            {
                throw new InvalidDataException("Invalid user.");
            }

            if (image.Length == 0 || image.Length > max_size)
            {
                throw new InvalidDataException(
                    "Choose an image smaller than 2 MB."
                );
            }

            //read the uploaded image
            using var memory = new MemoryStream();

            await image.CopyToAsync(memory, cancellation_token);

            byte[] bytes = memory.ToArray();

            //identify supported image signatures
            bool is_jpeg =
                bytes.Length >= 3 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xD8 &&
                bytes[2] == 0xFF;

            bool is_png =
                bytes.Length >= 8 &&
                bytes.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }
                );

            bool is_webp =
                bytes.Length >= 12 &&
                System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&
                System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";

            string extension;
            string expected_type;

            if (is_jpeg)
            {
                extension = ".jpg";
                expected_type = "image/jpeg";
            }
            else if (is_png)
            {
                extension = ".png";
                expected_type = "image/png";
            }
            else if (is_webp)
            {
                extension = ".webp";
                expected_type = "image/webp";
            }
            else
            {
                throw new InvalidDataException(
                    "Only JPG, PNG and WebP images are supported."
                );
            }

            //check the declared file type
            if (!string.Equals(
                image.ContentType,
                expected_type,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The image type does not match its contents."
                );
            }

            //create profile image folder if missing
            Directory.CreateDirectory(_folder);

            //generate a safe filename
            string filename =
                $"user-{user_id}-{Guid.NewGuid():N}{extension}";

            string new_path = Path.Combine(_folder, filename);

            //save the new image
            await File.WriteAllBytesAsync(
                new_path,
                bytes,
                cancellation_token
            );

            //remove previous profile pictures for this user
            string prefix = $"user-{user_id}-";

            foreach (string old_path in Directory.EnumerateFiles(_folder))
            {
                //start of foreach

                if (old_path == new_path)
                {
                    continue;
                }

                if (Path.GetFileName(old_path).StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(old_path);
                }

                //end of foreach
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}
