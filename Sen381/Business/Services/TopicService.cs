using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381.Business.Services
{
    public class TopicService
    {
        private readonly SupaBaseAuthService _supabase;

        public TopicService(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        /// <summary>
        /// Gets all topics for a specific subject
        /// </summary>
        public async Task<List<TopicDb>> GetTopicsBySubjectAsync(int subjectId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<TopicDb>()
                .Filter("subject_id", Supabase.Postgrest.Constants.Operator.Equals, subjectId)
                .Order("order_number", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models.ToList();
        }

        /// <summary>
        /// Gets a topic by ID
        /// </summary>
        public async Task<TopicDb?> GetTopicByIdAsync(int topicId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<TopicDb>()
                .Filter("topic_id", Supabase.Postgrest.Constants.Operator.Equals, topicId)
                .Get();

            return response.Models.FirstOrDefault();
        }

        /// <summary>
        /// Creates a new topic
        /// </summary>
        public async Task<TopicDb> CreateTopicAsync(TopicDb topic)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            // Set the order number to be the next available number
            var existingTopics = await GetTopicsBySubjectAsync(topic.SubjectId);
            topic.OrderNumber = existingTopics.Count + 1;

            // Ensure TopicId is 0 so Supabase auto-generates it
            topic.TopicId = 0;
            topic.CreatedAt = DateTime.UtcNow;
            topic.UpdatedAt = DateTime.UtcNow;

            Console.WriteLine($"[TopicService] Inserting topic: SubjectId={topic.SubjectId}, Title='{topic.Title}', OrderNumber={topic.OrderNumber}");

            var response = await client.From<TopicDb>().Insert(topic);
            
            Console.WriteLine($"[TopicService] Insert response status: {response.ResponseMessage?.StatusCode}");
            Console.WriteLine($"[TopicService] Insert response content: {response.Content}");
            Console.WriteLine($"[TopicService] Number of models returned: {response.Models?.Count ?? 0}");
            
            var result = response.Models.FirstOrDefault();
            Console.WriteLine($"[TopicService] Insert result: {(result != null ? $"TopicId={result.TopicId}, Title='{result.Title}'" : "null")}");
            
            // If the insert didn't return a proper ID, try to fetch the topic by other means
            if (result != null && result.TopicId <= 0)
            {
                Console.WriteLine($"[TopicService] TopicId is invalid ({result.TopicId}), attempting to fetch by title and subject");
                try
                {
                    var fetchedTopics = await GetTopicsBySubjectAsync(topic.SubjectId);
                    var matchingTopic = fetchedTopics.FirstOrDefault(t => t.Title == topic.Title);
                    if (matchingTopic != null && matchingTopic.TopicId > 0)
                    {
                        Console.WriteLine($"[TopicService] Found matching topic with valid ID: {matchingTopic.TopicId}");
                        return matchingTopic;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TopicService] Error fetching topics to find valid ID: {ex.Message}");
                }
            }
            
            return result ?? topic;
        }

        /// <summary>
        /// Updates an existing topic
        /// </summary>
        public async Task<TopicDb> UpdateTopicAsync(TopicDb topic)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            topic.UpdatedAt = DateTime.UtcNow;

            var response = await client
                .From<TopicDb>()
                .Filter("topic_id", Supabase.Postgrest.Constants.Operator.Equals, topic.TopicId)
                .Update(topic);

            return response.Models.FirstOrDefault() ?? topic;
        }

        /// <summary>
        /// Deletes a topic by ID
        /// </summary>
        public async Task DeleteTopicAsync(int topicId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            await client
                .From<TopicDb>()
                .Filter("topic_id", Supabase.Postgrest.Constants.Operator.Equals, topicId)
                .Delete();
        }

        /// <summary>
        /// Updates the order of topics within a subject
        /// </summary>
        public async Task UpdateTopicOrderAsync(int subjectId, List<int> topicIdsInOrder)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            for (int i = 0; i < topicIdsInOrder.Count; i++)
            {
                var topic = new TopicDb
                {
                    TopicId = topicIdsInOrder[i],
                    OrderNumber = i + 1,
                    UpdatedAt = DateTime.UtcNow
                };

                await client
                    .From<TopicDb>()
                    .Filter("topic_id", Supabase.Postgrest.Constants.Operator.Equals, topicIdsInOrder[i])
                    .Set(x => x.OrderNumber, i + 1)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow)
                    .Update();
            }
        }

        /// <summary>
        /// Checks if a topic title already exists within a subject
        /// </summary>
        public async Task<bool> TopicTitleExistsAsync(int subjectId, string title, int? excludeTopicId = null)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<TopicDb>()
                .Filter("subject_id", Supabase.Postgrest.Constants.Operator.Equals, subjectId)
                .Filter("title", Supabase.Postgrest.Constants.Operator.Equals, title)
                .Get();

            var existing = response.Models.FirstOrDefault();
            
            if (existing == null) return false;
            if (excludeTopicId.HasValue && existing.TopicId == excludeTopicId.Value) return false;
            
            return true;
        }
    }
}
