using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraCommon.FileUtility
{
    public class ImageFileManager
    {
        private readonly string _uploadDirectory;
        private readonly long _maxFileSizeInBytes;

        public ImageFileManager(string uploadDirectory, long maxFileSizeInBytes)
        {
            _uploadDirectory = uploadDirectory ?? throw new ArgumentNullException(nameof(uploadDirectory));
            _maxFileSizeInBytes = maxFileSizeInBytes;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No file was provided.");

            if (!IsValidImageFile(imageFile))
                throw new ArgumentException("The provided file is not a valid image file.");

            if (imageFile.Length > _maxFileSizeInBytes)
                throw new ArgumentException($"The file size exceeds the maximum allowed size of {_maxFileSizeInBytes} bytes.");

            var fileName = await GenerateUniqueFileNameAsync(imageFile.FileName);
            var filePath = Path.Combine(_uploadDirectory, fileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public void DeleteOrRenameFile(string fileName, string newFileName = null)
        {
            var filePath = Path.Combine(_uploadDirectory, fileName);

            if (File.Exists(filePath))
            {
                if (string.IsNullOrEmpty(newFileName))
                {
                    File.Delete(filePath);
                }
                else
                {
                    var newPath = Path.Combine(_uploadDirectory, newFileName);
                    File.Move(filePath, newPath);
                }
            }
        }

        public byte[] ReadFile(string fileName)
        {
            var filePath = Path.Combine(_uploadDirectory, fileName);

            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }

            return null;
        }

        private bool IsValidImageFile(IFormFile file)
        {
            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            return allowedImageTypes.Contains(file.ContentType);
        }

        private async Task<string> GenerateUniqueFileNameAsync(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string uniqueFileName;
            do
            {
                uniqueFileName = $"{fileNameWithoutExtension}_{Guid.NewGuid().ToString("N")}{fileExtension}";
            } while (File.Exists(Path.Combine(_uploadDirectory, uniqueFileName)));

            return uniqueFileName;
        }
    }

    public class ImageFileManager2
    {
        private readonly string _uploadDirectory;
        private readonly long _maxFileSizeInBytes;

        public ImageFileManager2(string uploadDirectory, long maxFileSizeInBytes)
        {
            _uploadDirectory = uploadDirectory;
            _maxFileSizeInBytes = maxFileSizeInBytes;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No file was provided.");

            if (!IsValidImageFile(imageFile))
                throw new ArgumentException("The provided file is not a valid image file.");

            if (imageFile.Length > _maxFileSizeInBytes)
                throw new ArgumentException($"The file size exceeds the maximum allowed size of {_maxFileSizeInBytes} bytes.");

            var fileName = await GenerateUniqueFileNameAsync(imageFile.FileName);
            var filePath = Path.Combine(_uploadDirectory, fileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private bool IsValidImageFile(IFormFile file)
        {
            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            return allowedImageTypes.Contains(file.ContentType);
        }

        private async Task<string> GenerateUniqueFileNameAsync(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string uniqueFileName;
            do
            {
                uniqueFileName = $"{fileNameWithoutExtension}_{Guid.NewGuid().ToString("N")}{fileExtension}";
            } while (File.Exists(Path.Combine(_uploadDirectory, uniqueFileName)));

            return uniqueFileName;
        }
    }

    public class ImageFileHandler
    {
        private readonly int _maxFileSizeInBytes;

        public ImageFileHandler(int maxFileSizeInBytes)
        {
            _maxFileSizeInBytes = maxFileSizeInBytes;
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            return allowedImageTypes.Contains(file.ContentType);
        }

        public async Task<string> SaveImageFileAsync(IFormFile file, string uploadPath)
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("File is not a valid image file.");

            if (file.Length > _maxFileSizeInBytes)
                throw new ArgumentException($"File size exceeds the maximum allowed size of {_maxFileSizeInBytes} bytes.");

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            if (File.Exists(filePath))
            {
                // 如果該位置有相同的檔案,可以選擇更名或刪除原檔案
                // 在這個範例中,我們將檔案更名
                fileName = $"{Guid.NewGuid().ToString()}_{file.FileName}";
                filePath = Path.Combine(uploadPath, fileName);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
