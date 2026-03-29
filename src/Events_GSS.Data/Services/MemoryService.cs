using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;

namespace Events_GSS.Data.Services
{
    public class MemoryService : IMemoryService
    {
        private readonly IMemoryRepository _memoryRepo;

        public MemoryService(IMemoryRepository memoryRepo)
        {
            _memoryRepo = memoryRepo;
        }


        public async Task<List<string>> GetOnlyPhotosAsync(int eventId)
        {
            var memories = await _memoryRepo.GetByEventAsync(eventId);
            return memories
                .Where(m => m.PhotoPath != null)
                .Select(m => m.PhotoPath!)
                .ToList();
        }

        public async Task<List<Memory>> FilterByMyMemoriesAsync(int eventId, int userId)
        {
            var memories = await GetByEventAsync(eventId, userId);
            return memories.Where(m => m.Author.UserId == userId).ToList();
        }

        public async Task<List<Memory>> OrderByDateAsync(int eventId, int currentUserId, bool ascending)
        {
            var memories = await GetByEventAsync(eventId, currentUserId);
            return ascending
                ? memories.OrderBy(m => m.CreatedAt).ToList()
                : memories.OrderByDescending(m => m.CreatedAt).ToList();
        }

        public async Task AddAsync(int eventId, int userId, string? photoPath, string? text)
        {
            bool hasPhoto = !string.IsNullOrWhiteSpace(photoPath);
            bool hasText = !string.IsNullOrWhiteSpace(text);

            if (!hasPhoto && !hasText)
                throw new InvalidOperationException("A memory must have at least a photo or text.");

            var memory = new Memory
            {
                PhotoPath = hasPhoto ? photoPath : null,
                Text = hasText ? text : null,
                CreatedAt = DateTime.UtcNow,
                Event = new Event { EventId = eventId },
                Author = new User { UserId = userId }
            };

            await _memoryRepo.AddAsync(memory);
        }

        public async Task DeleteAsync(int memoryId, int requestingUserId)
        {
            var memory = await _memoryRepo.GetByIdAsync(memoryId);

            if (memory == null)
                throw new Exception("Memory not found.");

            bool isAdmin = memory.Event.Admin.UserId == requestingUserId;
            bool isOwner = memory.Author.UserId == requestingUserId;

            if (!isAdmin && !isOwner)
                throw new UnauthorizedAccessException("You can only delete your own memories.");

            await _memoryRepo.DeleteAsync(memoryId);
        }

        public async Task ToggleLikeAsync(int memoryId, int userId)
        {
            var memory = await _memoryRepo.GetByIdAsync(memoryId);

            if (memory == null)
                throw new Exception("Memory not found.");

            if (memory.Author.UserId == userId)
                throw new InvalidOperationException("You cannot like your own memory.");

            bool alreadyLiked = await _memoryRepo.HasLikedAsync(memoryId, userId);
            if (alreadyLiked)
                await _memoryRepo.RemoveLikeAsync(memoryId, userId);
            else
                await _memoryRepo.AddLikeAsync(memoryId, userId);
        }
        public async Task<List<Memory>> GetByEventAsync(int eventId, int currentUserId)
        {
            var memories = await _memoryRepo.GetByEventAsync(eventId);

            foreach (var memory in memories)
            {
                memory.LikesCount = await _memoryRepo.GetLikesCountAsync(memory.MemoryId);
                memory.IsLikedByCurrentUser = await _memoryRepo.HasLikedAsync(memory.MemoryId, currentUserId);
            }

            return memories;
        }
    }
}