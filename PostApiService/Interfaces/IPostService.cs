﻿using PostApiService.Dto;
using PostApiService.Models;

namespace PostApiService.Interfaces
{
    public interface IPostService
    {
        Task<List<PostDto>> GetAllPostsAsync();

        Task<Post> GetPostByIdAsync(int postId);
        Task AddPostAsync(Post post);
        Task EditPostAsync(Post post);
        Task DeletePostAsync(int postId);
    }
}
