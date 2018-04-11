using System.Collections.Generic;
using PopForums.Models;
using System;

namespace PopForums.Repositories
{
	public interface ICategoryRepository
	{
		Category Get(int categoryID);
		List<Category> GetAll();
        Category Create(string newTitle, int sortOrder,  DateTime closureDate);
	    void UpdateClosureDate(DateTime value,DateTime value1);
        void Delete(int categoryID);
		void Update(Category category);
	    ClosureDateClass GetValue();

	}
}