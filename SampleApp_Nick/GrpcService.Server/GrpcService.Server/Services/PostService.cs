using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Server.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrpcService.Server.Services
{
    public class PostService: UserPost.UserPostBase
    {
        private IHttpClientFactory _httpClientFactory;

        public PostService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async override Task<PostResponse> GetPost(PostRequest request, ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient
                .GetStringAsync($"https://jsonplaceholder.typicode.com/posts/{request.Id}");

            var post = JsonConvert.DeserializeObject<Post>(response); 

                //JsonSerializer.Deserialize<Post>(response);

            return new PostResponse
            {
                 UserId = post.UserId,
                 Title = post.Title,
                 Body = post.Body
            };
        }

        public async override Task GetPostStream(
            PostRequest request, IServerStreamWriter<PostResponseStream> responseStream, 
            ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();

            for (int i = 0; i < 20; i++)
            {
                if(context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var response = await httpClient
                         .GetStringAsync($"https://jsonplaceholder.typicode.com/posts/{request.Id}");

                var post = JsonConvert.DeserializeObject<Post>(response);
                await responseStream.WriteAsync(new PostResponseStream
                {
                    UserId = post.UserId,
                    Title = post.Title,
                    Body = post.Body,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                });

                await Task.Delay(1000);
            }
        }

        public override async Task<MultiPostResponse> GetMultiPostStream(
            IAsyncStreamReader<PostRequest> requestStream, ServerCallContext context)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = new MultiPostResponse
            {
                 Posts = { }
            };

            await foreach (var request in requestStream.ReadAllAsync())
            {
                var responseApi = await httpClient
                         .GetStringAsync($"https://jsonplaceholder.typicode.com/posts/{request.Id}");

                var post = JsonConvert.DeserializeObject<Post>(responseApi);

                response.Posts.Add(new PostResponse
                {
                    UserId = post.UserId,
                    Title = post.Title,
                    Body = post.Body,
                });
            }

            return response;
        }

        public override async Task<Empty> PrintStream(
            IAsyncStreamReader<PrintRequest> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {

            }

            return new();
        }
    }
}
