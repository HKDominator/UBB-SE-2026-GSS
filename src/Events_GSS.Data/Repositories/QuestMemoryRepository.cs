using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Database;
using Events_GSS.Data.Models;
using Events_GSS.Data.Repositories.Interfaces;

using Microsoft.Data.SqlClient;

namespace Events_GSS.Data.Repositories;

internal class QuestMemoryRepository : IQuestMemoryRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public QuestMemoryRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> AddMemoryAsync(Memory proofMemory)
    {
        using SqlConnection connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync();

            const string addQuery = @"
                INSERT INTO Memories (EventId, UserId, PhotoPath, Text, CreatedAt)
                OUTPUT INSERTED.MemoryId
                VALUES (@EventId, @UserId, @PhotoPath, @Text, @CreatedAt)";

            using SqlCommand cmd = new SqlCommand(addQuery, connection);

            cmd.Parameters.AddWithValue("@EventId", proofMemory.Event.EventId);
            cmd.Parameters.AddWithValue("@UserId", proofMemory.Author.UserId);
            cmd.Parameters.AddWithValue("@PhotoPath", (object?)proofMemory.PhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Text", (object?)proofMemory.Text ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", proofMemory.CreatedAt);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while creating the memory as proof.", ex);
        }
    }

    public async Task SubmitProofAsync(Quest quest, Memory proof)
    {
        var connection = _connectionFactory.CreateConnection();

        try
        {
            await connection.OpenAsync();

            const string insertQuery = @"
                    INSERT INTO QuestProofs (QuestId, MemoryId)
                    VALUES (@QuestId, @MemoryId)";

            using SqlCommand cmd = new SqlCommand(insertQuery, connection);
            cmd.Parameters.AddWithValue("@QuestId", quest.Id);
            cmd.Parameters.AddWithValue("@MemoryId", proof.MemoryId);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while submitting the proof.", ex);
        }

    }

    public async Task<List<QuestMemory>> GetSubmissionsStatusForUser(List<Quest> quests, User user)
    {
        var proofs = new List<QuestMemory>();
        using SqlConnection connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync();
            const string query = @"
                (
                SELECT qp.QuestId, qp.Status, qp.MemoryId
                FROM  QuestMemories qp
                WHERE qp.QuestId IN @Quests
                )
                INNER JOIN 
                (
                SELECT m.MemoryId
                FROM Memories m
                WHERE m.UserId = @UserId
                ) 
                ON qp.MemoryId = m.MemoryId                            
                ";
            using SqlCommand cmd = new SqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@Quests", quests);
            cmd.Parameters.AddWithValue("@UserId", user.UserId);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                proofs.Add(new QuestMemory
                {
                    ForQuest = quests.Find(q => q.Id == (int)reader["QuestId"]),
                    Proof = null,
                    ProofStatus = (QuestMemoryStatus)reader["Status"]
                });
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while retrieving the proofs.", ex);
        }

        return proofs;
    }


    public async Task<List<QuestMemory>> GetProofsForQuestAsync(Quest quest)
    {
        var proofs = new List<QuestMemory>();
        using SqlConnection connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync();
            const string query = @"
                SELECT qp.QuestMemoryId, qp.QuestId, qp.MemoryId,
                       m.UserId, m.PhotoPath, m.Text, m.CreatedAt
                FROM QuestProofs qp
                JOIN Memories m ON qp.MemoryId = m.MemoryId
                WHERE qp.QuestId = @QuestId";
            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@QuestId", quest.Id);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                proofs.Add(new QuestMemory
                {
                    ForQuest = quest,
                    Proof = new Memory
                    {
                        MemoryId = (int)reader["MemoryId"],
                        Author = new User { UserId = (int)reader["UserId"] },
                        PhotoPath = reader["PhotoPath"] == DBNull.Value ? null : (string)reader["PhotoPath"],
                        Text = reader["Text"] == DBNull.Value ? null : (string)reader["Text"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    },
                    ProofStatus = QuestMemoryStatus.Submitted
                });
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while retrieving the proofs.", ex);
        }

        return proofs;
    }

    public async Task ChangeProofStatusAsync(QuestMemory proof)
    {
        var connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync();
            const string updateQuery = @"
                UPDATE QuestProofs
                SET ProofStatus = @Status
                WHERE QuestId = @QuestId AND MemoryId = @MemoryId";

            using SqlCommand cmd = new SqlCommand(updateQuery, connection);
            cmd.Parameters.AddWithValue("@Status", proof.ProofStatus.ToString());
            cmd.Parameters.AddWithValue("@QuestId", proof.ForQuest.Id);
            cmd.Parameters.AddWithValue("@MemoryId", proof.Proof.MemoryId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while changing proof status.", ex);
        }
    }

    public async Task DeleteProofAsync(QuestMemory proof)
    {
        var connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync();
            const string deleteQuery = @"
                DELETE FROM QuestMemories
                WHERE QuestId = @QuestId AND MemoryId = @MemoryId
            ";
            using SqlCommand cmd = new SqlCommand(deleteQuery, connection);

            cmd.Parameters.AddWithValue("@QuestId", proof.ForQuest.Id);
            cmd.Parameters.AddWithValue("@MemoryId", proof.Proof.MemoryId);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"SQL Exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while deleting the proof.", ex);
        }
    }


}