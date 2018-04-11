using System.Collections.Generic;
using PopForums.Models;
using System;

namespace PopForums.Services
{
	public interface ICategoryService
	{
		Category Get(int categoryID);
		List<Category> GetAll();
        ClosureDateClass GetValue();
        Category Create(string title, DateTime closureDate);
	    void UpdateClosureDate(DateTime closureDate, DateTime value);
        void Delete(Category category);
		void UpdateTitle(Category category, string newTitle,DateTime closureDate);
		void MoveCategoryUp(Category category);
		void MoveCategoryDown(Category category);
	}
}