using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using Gmail2.Server;

namespace Gmail2.Server
{
    public class Database
    {
        private static readonly List<string> personNames = new List<string> { 
            "Erlinda Priddy","Mellisa Eichhorn","Duncan Argo","Buster Bylsma","Louis Quezada","Otha Mccook","Leonida Aguas","Keturah Setton","John Maultsby",
            "Loretta Curry","Mariela Bartman","Veola Dunkle","Luetta Campagna","Jerome Vineyard","Rikki Jantzen","Alease Mccraw","Jenae Poirrier","Jeremy Everton",
            "Maris Holding","Colton Mcalexander","Beatrice Palacios","Ardath Goad","Stephani Lavallie","Nicholle Ivy","Calista Babineau","Palmer Katzer",
            "Agustina Scribner","Kaci Strain","Precious Bertone","Carla Harrold","Tova Tosh","Tricia Schriver","Andree Maxim","Letha Pfarr",
            "Annetta Salvaggio","Nichole Forest","Liana Poole","Harris Lapointe","Lauran Brien","Coleen Garofalo","Yahaira Elliot","Carlota Loux",
            "Mandi Cuthbertson","Adam Frazier","Florencio Singleton","Viviana Kall","Jena Kreitzer","Sun Mckamie","Nicolas Gerke","Kiley Maguire"
        };

        private static readonly List<string> defaultTags = new List<string> { 
            "inbox", "spam", "recyclebin", "draft", "personal", "me", "vip", "important", "travel", "friends", "social", "work", "family"
        };

        public class AppData
        {
            public List<Post> Posts;
            public AppUser CurrentUser;
            public List<AppUser> Users;
        }

        public static List<string> GetAvailableUsers()
        {
            return new List<string>(personNames);
        }

        public static void DeleteAllData()
        {
            string path = GetPath();
            if (File.Exists(path))
                File.Delete(path);
        }

        public static void UpdatePost(Post post)
        {
            var data = GetDataFromDisk();
            var existing = data.Posts.FirstOrDefault(p => p.Id == post.Id);
            if (existing != null)
            {
                int index = data.Posts.IndexOf(existing);
                data.Posts.Remove(existing);
                data.Posts.Insert(index, post);
                SaveDataToDisk(data);
            }
        }

        public static bool IsLoggedIn()
        {
            return DataExists();
        }

        public static AppUser GetCurrentUser()
        {
            var data = GetData("", "");

            return data.CurrentUser;
        }

        public static AppData GetData(string realm, string tag)
        {
            if (!DataExists())
                throw new InvalidOperationException("No data on server");

            var data = GetDataFromDisk();

            if (realm == "starred")
                data.Posts = data.Posts.Where(p => p.IsFavorite).ToList();
            else if (realm == "unread")
                data.Posts = data.Posts.Where(p => !p.IsRead).ToList();
            else if (realm == "sent")
                data.Posts = data.Posts.Where(p => p.Sender.Id == data.CurrentUser.Id).ToList();
            else if (realm == "draft")
                data.Posts = data.Posts.Where(p => p.Tags.Contains("draft")).ToList();
            else if (realm == "inbox" || realm == "default")
                data.Posts = data.Posts.Where(p => p.Tags.Contains("inbox")).ToList();
            else if (realm == "recyclebin")
                data.Posts = data.Posts.Where(p => p.Tags.Contains("recyclebin")).ToList();
            else if (realm == "spam")
                data.Posts = data.Posts.Where(p => p.Tags.Contains("spam")).ToList();
            else if (realm == "tag")
                data.Posts = data.Posts.Where(p => p.Tags.Contains(tag)).ToList();

            foreach (var post in data.Posts)
                post.IsChecked = false;

            return data;
        }

        public static List<Post> SearchPosts(string query)
        {
            if (!DataExists())
                throw new InvalidOperationException("No data on server");

            var data = GetDataFromDisk();

            var result = data.Posts.Where(p =>
                    p.Subject.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.Body.Contains(query, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            return result;
        }

        private static string GetPath()
        {
            return HttpContext.Current.Server.MapPath("~/data.json");
        }

        private static bool DataExists()
        {
            return File.Exists(GetPath());
        }

        private static AppData GetDataFromDisk()
        {
            string json = File.ReadAllText(GetPath());
            return new JavaScriptSerializer().Deserialize<AppData>(json);
        }

        private static void SaveDataToDisk(AppData data)
        {
            string json = new JavaScriptSerializer().Serialize(data);
            File.WriteAllText(GetPath(), json);
        }

        public static Attachment GetAttachment(int postId, string attachmentId)
        {
            var data = GetDataFromDisk();
            var post = data.Posts.FirstOrDefault(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Cannot find post with id " + postId);

            var attachment = post.Attachments.FirstOrDefault(p => p.Id == attachmentId);

            if (attachment == null)
                throw new InvalidOperationException("Cannot find attachment with id " + attachmentId);

            return attachment;
        }

        public static void GenerateRandomData(string personName)
        {
            var random = new RandomGenerator();

            string rootPath = HttpContext.Current.Server.MapPath("~/Server");

            var attachmentFiles = System.IO.Directory.GetFiles(Path.Combine(rootPath, "attachments"));
            var lipsumParagraphs = System.IO.File.ReadAllLines(Path.Combine(rootPath, "LipsumParagraphs.txt")).Where(l => !string.IsNullOrEmpty(l)).ToList();
            var lipsumTitles = System.IO.File.ReadAllLines(Path.Combine(rootPath, "LipsumTitles.txt")).Where(l => !string.IsNullOrEmpty(l)).ToList();
            var people = new List<AppUser>();

            for (int i = 0; i < 100; i++)
            {
                var person = new AppUser();
                person.Name = random.GetRandom(personNames);
                person.Email = person.Name.ToLowerInvariant() + "@gmail.com";
                person.Id = random.GetRandomNumber(1000000, 9999999);
                people.Add(person);
            }

            var currentUser = people.First(p => p.Name == personName);

            var posts = new List<Post>();
            for (int i = 0; i < random.GetRandomNumber(800, 1200); i++)
            {
                var post = new Post();

                post.Id = random.GetRandomNumber(100000, 999999);
                post.IsFavorite = random.GetRandomDouble() < 0.2;
                post.IsRead = random.GetRandomDouble() < 0.6;
                post.Body = random.GetRandom(lipsumParagraphs);
                post.Subject = random.GetRandom(lipsumTitles);
                post.Sender = random.GetRandom(people);
                post.DateSent = random.GetRandomDate();

                for (int j = 0; j < random.GetRandomNumber(1, 10); j++)
                    post.Recepients.Add(random.GetRandom(people));

                if (random.GetRandomBool())
                {
                    for (int j = 0; j < random.GetRandomNumber(0, 5); j++)
                        post.CC.Add(random.GetRandom(people));
                }

                if (random.GetRandomBool())
                {
                    for (int j = 0; j < random.GetRandomNumber(0, 5); j++)
                        post.BCC.Add(random.GetRandom(people));
                }

                if (random.GetRandomBool())
                {
                    for (int j = 0; j < random.GetRandomNumber(0, 3); j++)
                    {
                        var attachmentFile = new FileInfo(random.GetRandom(attachmentFiles));

                        post.Attachments.Add(new Attachment { Id = Guid.NewGuid().ToString(), Size = random.GetRandomNumber(10000, 10000000), Title = attachmentFile.Name });
                    }
                }

                if (random.GetRandomBool())
                {
                    for (int j = 0; j < random.GetRandomNumber(1, 5); j++)
                    {
                        string randomTag = random.GetRandom(defaultTags);
                        if (!post.Tags.Contains(randomTag))
                            post.Tags.Add(randomTag);
                    }
                }

                posts.Add(post);
            }

            var data = new AppData
            {
                Posts = posts,
                Users = people,
                CurrentUser = currentUser
            };

            SaveDataToDisk(data);
        }

        public static void CreatePost(string title, string body, string to)
        {
            var random = new RandomGenerator();
            var post = new Post();
            var data = GetDataFromDisk();

            post.Id = random.GetRandomNumber(100000, 999999);
            post.Body = body;
            post.Subject = title;
            post.Recepients.Add(new AppUser { Email = to });
            post.DateSent = DateTime.Now;

            data.Posts.Add(post);
            SaveDataToDisk(data);
        }

        public static void DeletePosts(int[] ids)
        {
            var data = GetDataFromDisk();
            data.Posts = data.Posts.Where(p => !ids.Contains(p.Id)).ToList();
            SaveDataToDisk(data);
        }

        public static void MovePost(int id, string tag)
        {
            var data = GetDataFromDisk();
            var post = data.Posts.FirstOrDefault(p => p.Id == id);
            if (!post.Tags.Contains(tag))
            {
                post.Tags.Add(tag);
                SaveDataToDisk(data);
            }
        }
    }
}