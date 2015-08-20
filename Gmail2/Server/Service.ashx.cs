using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Reflection;
using System.IO;

namespace Gmail2.Server
{
    public class Service : IHttpHandler
    {
        private HttpContext _context;

        public void ProcessRequest(HttpContext context)
        {
            _context = context;
            string operation = context.Request["op"];

            if (string.IsNullOrEmpty(operation))
            {
                ErrorResponse("Missing parameter op");
                return;
            }

            var method = typeof(Service).GetMethod(operation, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);

            if (method == null)
            {
                ErrorResponse("Invalid parameter op: " + operation);
                return;
            }

            try
            {
                method.Invoke(this, null);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null)
                    ErrorResponse(ex.ToString());
                else if (ex.InnerException != null && ex.InnerException is InvalidArgException)
                    ErrorResponse(ex.InnerException.Message);
                else
                    ErrorResponse(ex.ToString());
            }
            catch (Exception ex)
            {
                ErrorResponse(ex.ToString());
            }
        }

        public void GetPostById()
        {
            int id = GetIntParam("id");
            var post = Database.GetData("", "").Posts.FirstOrDefault(p => p.Id == id);
            SuccessResponse(post);
        }

        public void UpdatePost()
        {
            var post = GetBodyAs<Post>();
            Database.UpdatePost(post);
        }

        public void DeletePosts()
        {
            var ids = GetBodyAs<int[]>();
            Database.DeletePosts(ids);
        }

        public void CreatePost()
        {
            string title = GetStringParam("title");
            string body = GetStringParamOptional("body");
            string to = GetStringParam("to");
            Database.CreatePost(title, body, to);
        }

        public void GetAttachment()
        {
            int postId = GetIntParam("postId");
            string attachmentId = GetStringParam("attachmentId");
            var attachment = Database.GetAttachment(postId, attachmentId);

            string fullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Server/attachments"), attachment.Title);

            _context.Response.ContentType = "image/jpeg";
            _context.Response.WriteFile(fullPath);
        }

        public void MoveItem()
        {
            int id = GetIntParam("id");
            string tag = GetStringParam("tag");
            tag = tag.ToLowerInvariant();
            Database.MovePost(id, tag);
        }

        private T GetBodyAs<T>()
        {
            using (var stream = _context.Request.InputStream)
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                return new JavaScriptSerializer().Deserialize<T>(json);
            }
        }

        public void GetPosts()
        {
            int page = GetIntParam("page");
            string realm = GetStringParam("realm");
            string tag = GetStringParamOptional("tag");
            var posts = Database.GetData(realm, tag).Posts;
            var limited = posts.Skip(page * 20).Take(20).ToList();
            SuccessResponse(limited, page: page, total: posts.Count);
        }

        public void SearchPosts()
        {
            int page = GetIntParam("page");
            string query = GetStringParam("query");
            var posts = Database.SearchPosts(query);
            var limited = posts.Skip(page * 20).Take(20).ToList();
            SuccessResponse(limited, page: page, total: posts.Count);
        }

        public void RegenerateData()
        {
            Database.DeleteAllData();
        }

        private int GetIntParam(string paramName)
        {
            string val = _context.Request[paramName];

            if (string.IsNullOrEmpty(val))
                throw new InvalidArgException("Missing param " + paramName);

            return int.Parse(val);
        }

        private string GetStringParam(string paramName)
        {
            string val = _context.Request[paramName];
            if (string.IsNullOrEmpty(val))
                throw new InvalidArgException("Missing param " + paramName);

            return val;
        }

        private string GetStringParamOptional(string paramName)
        {
            string val = _context.Request[paramName];
            return val;
        }

        private class InvalidArgException : Exception
        {
            public InvalidArgException(string message)
                : base(message) { }
        }

        private void SuccessResponse(object data, int? page = null, int? total = null)
        {
            var response = new Response { Success = true, Data = data, Page = page, Total = total };
            _context.Response.ContentType = "application/json";
            string json = new JavaScriptSerializer().Serialize(response);
            _context.Response.Write(json);
        }

        private void ErrorResponse(string errorMessage)
        {
            _context.Response.ContentType = "application/json";
            string json = new JavaScriptSerializer().Serialize(new Response { Success = false, ErrorMessage = errorMessage });
            _context.Response.Write(json);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int? Total { get; set; }
        public object Data { get; set; }
        public int? Page { get; set; }
    }

    public class Post
    {
        public Post()
        {
            Attachments = new List<Attachment>();
            Recepients = new List<AppUser>();
            CC = new List<AppUser>();
            BCC = new List<AppUser>();
            Tags = new List<string>();
        }

        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime DateSent { get; set; }
        public AppUser Sender { get; set; }
        public List<AppUser> Recepients { get; set; }
        public List<AppUser> CC { get; set; }
        public List<AppUser> BCC { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<string> Tags { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsRead { get; set; }
        public bool IsChecked { get; set; }
    }

    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class Attachment
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public double Size { get; set; }
    }
}