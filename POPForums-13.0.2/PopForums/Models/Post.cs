using System;

namespace PopForums.Models
{
	public class Post
	{
		public Post(int postID)
		{
			PostID = postID;
		}

		public int PostID { get; set; }
		public int TopicID { get; set; }
		public int ParentPostID { get; set; }
		public string IP { get; set; }
		public bool IsFirstInTopic { get; set; }
		public bool ShowSig { get; set; }
		public int UserID { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
		public string FullText { get; set; }
		public DateTime PostTime { get; set; }
		public bool IsEdited { get; set; }
		public string LastEditName { get; set; }
		public DateTime? LastEditTime { get; set; }
		public bool IsDeleted { get; set; }
		public int Votes { get; set; }
        public bool? IsAnonymous { get; set; }
        public string FileUrl { get; set; }
        public string UrlName { get; set; }
        public bool IsUp { get; set; }
        public int? DownVotes { get; set; }
        public int? ViewCount { get; set; }
        public CountVote countvote { get; set; }
        public int UserType { get; set; }
    }

    public class CountVote
    {
        public int Votes { get; set; }
        public int DownVotes { get; set; }
        public int PostID { get; set; }
        public bool IsUp { get; set; }
    }

    public class ClosureDateClass
    {
        public DateTime ClosureDate { get; set; }
        public DateTime FinalClosureDate { get; set; }
    }
}
