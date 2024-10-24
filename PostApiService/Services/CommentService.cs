﻿using PostApiService.Interfaces;
using PostApiService.Models;

namespace PostApiService.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(int postId, Comment comment)
        {
            comment.PostId = postId;
            comment.CreatedAt = DateTime.Now;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task EditCommentAsync(Comment comment)
        {
            var existingComment = await _context.Comments.FindAsync(comment.CommentId);
            if (existingComment != null)
            {
                existingComment.Content = comment.Content; 
                existingComment.PostId = comment.PostId; 
                await _context.SaveChangesAsync();
            }
        }
    }
}
