using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories;
using Events_GSS.Data.Services.Interfaces;

namespace Events_GSS.Data.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IMemoryRepository _memoryRepo;
        private readonly IDiscussionRepository _discussionRepo;
        private readonly IQuestRepository _questRepo;
        private readonly IQuestMemoryRepository _questMemoryRepository;

        public StatisticsService(IMemoryRepository memoryRepo, IDiscussionRepository discussionRepo, IQuestRepository questRepo, IQuestMemoryRepository questMemoryRepository)
        {
            _memoryRepo = memoryRepo;
            _discussionRepo = discussionRepo;
            _questRepo = questRepo;
            _questMemoryRepository = questMemoryRepository;
        }

       

        public async Task<ParticipantOverview> GetParticipantOverviewAsync(int eventId)
        {
            var participants = await _discussionRepo.GetEventParticipantsAsync(eventId);
            var memories = await _memoryRepo.GetByEventAsync(eventId);
            var discussions = await _discussionRepo.GetByEventAsync(eventId, 0);
            var submissions = await _questMemoryRepository.GetSubmissionsByEventAsync(eventId);

            int total = participants.Count;

            var activeUserIds = new HashSet<int>();
            foreach (var m in memories)
                activeUserIds.Add(m.Author.UserId);
            foreach (var d in discussions)
                activeUserIds.Add(d.Author.UserId);
            foreach (var s in submissions)
                activeUserIds.Add(s.Proof.Author.UserId);

            // only count active users that are actually participants
            var participantIds = participants.Select(p => p.UserId).ToHashSet();
            int active = activeUserIds.Count(id => participantIds.Contains(id));

            double engagementRate = total > 0
                ? Math.Round((double)active / total * 100, 2)
                : 0;

            return new ParticipantOverview
            {
                TotalParticipants = total,
                ActiveParticipants = active,
                EngagementRate = engagementRate
            };
        }

        // Engagement Breakdown 

        public async Task<EngagementBreakdown> GetEngagementBreakdownAsync(int eventId)
        {
            var memories = await _memoryRepo.GetByEventAsync(eventId);
            var discussions = await _discussionRepo.GetByEventAsync(eventId, 0);
            var submissions = await _questMemoryRepository.GetSubmissionsByEventAsync(eventId);

            int totalMessages = discussions.Count;
            int totalMemories = memories.Count;
            int totalSubmissions = submissions.Count;

            int approved = submissions.Count(s => s.ProofStatus == QuestMemoryStatus.Approved);
            int denied = submissions.Count(s => s.ProofStatus == QuestMemoryStatus.Rejected);

            double approvedRate = totalSubmissions > 0
                ? Math.Round((double)approved / totalSubmissions * 100, 2)
                : 0;
            double deniedRate = totalSubmissions > 0
                ? Math.Round(100 - approvedRate, 2)
                : 0;

            return new EngagementBreakdown
            {
                TotalMessages = totalMessages,
                TotalMemories = totalMemories,
                TotalQuestSubmissions = totalSubmissions,
                ApprovedQuestsRate = approvedRate,
                DeniedQuestsRate = deniedRate
            };
        }

        //Participant Leaderboard 

        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int eventId)
        {
            var participants = await _discussionRepo.GetEventParticipantsAsync(eventId);
            var memories = await _memoryRepo.GetByEventAsync(eventId);
            var discussions = await _discussionRepo.GetByEventAsync(eventId, 0);
            var submissions = await _questMemoryRepository.GetSubmissionsByEventAsync(eventId);

            var memoriesByUser = memories
                .GroupBy(m => m.Author.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            var discussionsByUser = discussions
                .GroupBy(d => d.Author.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            var approvedByUser = submissions
                .Where(s => s.ProofStatus == QuestMemoryStatus.Approved)
                .GroupBy(s => s.Proof.Author.UserId)
                .ToDictionary(g => g.Key, g => g.Count());

            return participants
                .Select(u =>
                {
                    int msg = discussionsByUser.GetValueOrDefault(u.UserId, 0);
                    int mem = memoriesByUser.GetValueOrDefault(u.UserId, 0);
                    int qst = approvedByUser.GetValueOrDefault(u.UserId, 0);
                    return new LeaderboardEntry
                    {
                        Participant = u,
                        MessagesCount = msg,
                        MemoriesCount = mem,
                        QuestsCompleted = qst,
                        TotalScore = msg + mem * 2 + qst * 3
                    };
                })
                .OrderByDescending(e => e.TotalScore)
                .Take(100)
                .ToList();
        }

        //Quest Analytics 

        public async Task<List<QuestAnalyticsEntry>> GetQuestAnalyticsAsync(int eventId)
        {
            var forEvent = new Event { EventId = eventId };
            var quests = await _questRepo.GetQuestsAsync(forEvent);
            var submissions = await _questMemoryRepository.GetSubmissionsByEventAsync(eventId);

            var approvedByQuest = submissions
                .Where(s => s.ProofStatus == QuestMemoryStatus.Approved)
                .GroupBy(s => s.ForQuest.Id)
                .ToDictionary(g => g.Key, g => g.Count());

            return quests
                .Select(q => new QuestAnalyticsEntry
                {
                    QuestName = q.Name,
                    CompletedCount = approvedByQuest.GetValueOrDefault(q.Id, 0)
                })
                .ToList();
        }
    }
}