using System;

namespace PopForums.Models
{
	public class Category
	{
		public Category(int categoryID)
		{
			CategoryID = categoryID;
		}

		public int CategoryID { get; private set; }
		public string Title { get; set; }
        public string UserName { get; set; }
        public DateTime ClosureDate { get; set; }
		public int SortOrder { get; set; }
        public int TotalPost { get; set; }
        public int TotalTopic { get; set; }
    }
}
