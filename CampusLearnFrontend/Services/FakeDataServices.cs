using System.Collections.Generic;
using System.Linq;

namespace CampusLearnFrontend.Services
{
    public record Subject(string Id, string Title);
    public record Topic(string Id, string SubjectId, string Title, string Content);

    public class FakeDataService
    {
        public List<Subject> Subjects { get; } = new()
        {
            new("s1", "Programming"),
            new("s2", "Databases"),
            new("s3", "Machine Learning"),
        };

        public List<Topic> Topics { get; } = new()
        {
            new("t1", "s1", "JavaScript Functions", "Details about JS functions..."),
            new("t2", "s2", "NoSQL Basics", "Details about NoSQL..."),
            new("t3", "s3", "Naive Bayes", "Details about Naive Bayes..."),
        };

        public IEnumerable<Topic> GetTopicsForSubject(string subjectId) =>
            Topics.Where(t => t.SubjectId == subjectId);

        public Topic GetTopic(string id) =>
            Topics.FirstOrDefault(t => t.Id == id);
    }
}

