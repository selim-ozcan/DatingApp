using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper.Internal;
using CloudinaryDotNet.Actions;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);

    }
}