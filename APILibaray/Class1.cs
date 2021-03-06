﻿using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;

namespace APILibaray
{
    public abstract class Response<T>
    {
        public abstract T Get();
    }
    public enum ResponseType { Xml, Json, PlainText };
    public enum ResponseCode { Ok, Error };
    public class User
    {
        public User()
        {

        }
        public override string ToString()
        {
            return $"{Nickname}#{Uid}";
        }
        public uint Uid { get; set; }
        public String Nickname { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
    }
    public class CommentList
    {
        private String etag;
        private String jsonResult;
        private JsonSerializerSettings settings;
        private Thread thread;
        public CommentList(Thread thread)
        {
            this.thread = thread;
            etag = null;

            jsonResult = null;
            settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }
        public Task<List<Comment>> Get()
        {
            return Task.Factory.StartNew<List<Comment>>(() =>
            {
                var url = new Uri($"https://board.hehehee.net/threads/{thread.Uid}/comments");

                var request =
                (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Accept = "application/json";
                if (etag != null && this.jsonResult != null)
                {
                    request.Headers.Add("If-None-Match", etag);
                }
                System.Net.HttpWebResponse response = null;
                try
                {
                    response = (System.Net.HttpWebResponse)request.GetResponse();

                }
                catch (WebException e)
                {
                    
                    var res = e.Response as HttpWebResponse;
                    if(res.StatusCode == HttpStatusCode.NotModified)
                    {   
                        return JsonConvert.DeserializeObject<List<Comment>>(jsonResult, settings);
                    }
                    else
                    {
                        Console.WriteLine(e);
                        jsonResult = null;
                        etag = null;
                        return null;
                    }
                }
                etag = response.Headers["ETag"];
                var reader =
                    new System.IO.StreamReader(response.GetResponseStream());
                jsonResult = reader.ReadToEnd();
                List<Comment> comments = null;
                try
                {
                    comments =
                    JsonConvert.DeserializeObject<List<Comment>>(jsonResult, settings);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    jsonResult = null;
                    etag = null;
                    return null;
                }
                return comments;
            });
        }
    }
    public class Comment
    {
        public Comment()
        {

        }
        public uint Uid { get; set; }
        public User User { get; set; }
        public DateTime write_datetime{get;set;}
        public String Content { get; set; }
        public static Task<bool> Upload(String token, uint threadUid, String content)
        {
            return Task.Factory.StartNew<bool>(() =>
            {
                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                   { "token", token},
                   { "content", content }
                };
                var body = new FormUrlEncodedContent(values);
                var s = client.PostAsync($"https://board.hehehee.net/threads/{threadUid}/comments", body).Result;
                return s.StatusCode == System.Net.HttpStatusCode.OK;
            });
        }
        public Task<bool> Delete(String token)
        {
            return Task.Factory.StartNew<bool>(() =>
            {
                HttpClient client = new HttpClient();
                var body = new FormUrlEncodedContent(new Dictionary<String, String>(){ { "token", token } });
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Content = body,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"https://board.hehehee.net/comments/{this.Uid}")
                };
                var s = client.SendAsync(request); 
                return s.Result.StatusCode == System.Net.HttpStatusCode.OK;
            });
        }
    }

    public class Thread
    {
        public Thread()
        {

        }
        public uint Uid { get; set; }
        public String Subject { get; set; }
        public User Opener { get; set; }
        public DateTime RecentUpdateDatetime { get; set; }
        public DateTime OpenDatetime { get; set; }
        public Task<bool> Delete(String token)
        {
            return Task.Factory.StartNew<bool>(() =>
            {
                HttpClient client = new HttpClient();
                var body = new FormUrlEncodedContent(new Dictionary<String, String>() { { "token", token } });
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Content = body,
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"https://board.hehehee.net/threads/{this.Uid}")
                };
                var s = client.SendAsync(request);
                return s.Result.StatusCode == System.Net.HttpStatusCode.OK;
            });
        }
        public Task<bool> Upload(String token, String content, String tags)
        {
            return Task.Factory.StartNew<bool>(() =>
            {
                HttpClient client = new HttpClient();
                var body = new FormUrlEncodedContent(new Dictionary<String, String>()
                {
                    { "token", token },
                    { "subject", Subject},
                    { "tags", tags},
                    {"comment", content }
                });
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Content = body,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://board.hehehee.net/threads"),
                    
                };
                request.Headers.Add("Accept","application/json");
                var s = client.SendAsync(request);
                return s.Result.StatusCode == System.Net.HttpStatusCode.OK;
            });
        }
    }
    public class ThreadListResponse : Response<List<Thread>>
    {
        List<Thread> m_res;
        internal ThreadListResponse(String text)
        {
            var setting = new Newtonsoft.Json.JsonSerializerSettings();
            setting.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            m_res = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Thread>>(text, setting);
        }

        public override List<Thread> Get()
        {
            return m_res;
        }
    }

    public class Token
    {
        class Res { public String Token { get; set; } public String Nickname { get; set; } public String Gravatar { get; set; } }
        public static Task<String> Get(String id, String password)
        {
            return Task.Factory.StartNew< String > (() =>
            {
                var email = System.Web.HttpUtility.UrlEncode(id);
                var pass = System.Web.HttpUtility.UrlEncode(password);
                var url = new Uri($"https://board.hehehee.net/login?email={email}&password={pass}");

                var request =
                (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Accept = "application/json";
                try
                {
                    var response =
                    (System.Net.HttpWebResponse)request.GetResponse();
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return null;
                    }
                    var reader =
                        new System.IO.StreamReader(response.GetResponseStream());
                    var setting = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        }
                    };
                    var text = reader.ReadToEnd();

                    Res comments =
                    JsonConvert.DeserializeObject<Res>(text, setting);
                    return comments.Token;
                }
                catch(Exception)
                {
                    return null;
                }
            });
        }
    }
    public abstract class API<T>
    {
        public abstract ResponseCode Call();
        public ResponseType ResponseType { get; set; }
        public abstract Response<T> GetResponse();
    }
    public class ThreadList
    {
       // private String etag;
        //private String jsonResult;
        private JsonSerializerSettings settings;
        public ThreadList()
        {
           // etag = null;
            //jsonResult = null;
            settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }
        public Task<List<Thread>> Get(uint offset, uint count)
        {
            return Task.Factory.StartNew<List<Thread>>(() =>
            {
                var url = new Uri("https://board.hehehee.net/threads");
                
                var request =
                (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Accept = "application/json";
                //if (etag != null && jsonResult != null)
                //{
                //    request.Headers.Add("If-None-Match", etag);
                //}
                System.Net.HttpWebResponse response = null;
                try
                {
                    response = (System.Net.HttpWebResponse)request.GetResponse();
                    
                }
                catch(WebException e)
                {
                    Console.WriteLine(e);
                    return null;
                }
                var reader =
                    new System.IO.StreamReader(response.GetResponseStream());
                var jsonResult = reader.ReadToEnd();
                List<Thread> threads = null;
                try
                {
                    threads =
                    JsonConvert.DeserializeObject<List<Thread>>(jsonResult, settings);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
                return threads;
            });
        }
    }
    public class ThreadListAPI : API<List<Thread>>
    {
        private System.Net.HttpWebResponse m_response;
        public override Response<List<Thread>> GetResponse()
        {
            var reader = new System.IO.StreamReader(m_response.GetResponseStream());
            return new ThreadListResponse(reader.ReadToEnd());
        }
        public override ResponseCode Call()
        {
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://board.hehehee.net/threads");
            switch(this.ResponseType)
            {
                case ResponseType.Json:
                    request.Accept = "application/json";
                    break;
                case ResponseType.Xml:
                    request.Accept = "application/xml";
                    break;
                case ResponseType.PlainText:
                    request.Accept = "text/html";
                    break;
            }
            var response = (System.Net.HttpWebResponse)request.GetResponse();
            m_response = response;
            switch(response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return ResponseCode.Ok;
                default:
                    return ResponseCode.Error;
            }
        }
    }
}
