﻿using System.Web;

namespace PopForums.Models
{
	public class PostEdit
	{
		public PostEdit() {}

		public PostEdit(Post post)
		{
			Title = post.Title;
			FullText = post.FullText;
			ShowSig = post.ShowSig;
		}
        public int Id { get; set; }
		public string Title { get; set; }
		public string FullText { get; set; }
		public bool ShowSig { get; set; }
		public string Comment { get; set; }
		public bool IsPlainText { get; set; }
        public bool IsAnonymous { get; set; }
        public HttpPostedFile UploadFile { get; set; }
        public string FileUrl { get; set; }
        public bool IsFirstTopic { get; set; }
    }
}
