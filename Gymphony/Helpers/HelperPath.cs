namespace Gymphony.Helpers
{
    public enum Folders { Images, Usuarios }
    public class HelperPath
    {
        private IWebHostEnvironment hostEnvironment;
        private IHttpContextAccessor httpContextAccessor;

        public HelperPath(IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            this.hostEnvironment = hostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        public string MapPath(string fileName, Folders folder)
        {
            string carpeta = "";
            if (folder == Folders.Images)
            {
                carpeta = "images";
            }
            else if (folder == Folders.Usuarios)
            {
                carpeta = "usuarios";
            }
            string rootPath = this.hostEnvironment.WebRootPath;
            string path = Path.Combine(rootPath, carpeta, fileName);
            return path;
        }

        public string MapUrlPath(string fileName, Folders folder)
        {
            string carpeta = "";
            if (folder == Folders.Images)
            {
                carpeta = "images";
            }
            else if (folder == Folders.Usuarios)
            {
                carpeta = "usuarios";
            }
            var request = this.httpContextAccessor.HttpContext.Request;
            string baseUrl = request.Scheme + "://" + request.Host + request.PathBase;
            return baseUrl + "/" + carpeta + "/" + fileName;
        }
    }
}
