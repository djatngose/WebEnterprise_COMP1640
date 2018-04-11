using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		public CategoryRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public Category Get(int categoryID)
		{
			Category category = null;
			_sqlObjectFactory.GetConnection().Using(c => c.Command("SELECT CategoryID, Title, SortOrder,ClosureDate FROM pf_Category WHERE CategoryID = @CategoryID")
				.AddParameter("@CategoryID", categoryID)
				.ExecuteReader()
				.ReadOne(r => category = new Category(r.GetInt32(0)) { Title = r.GetString(1), SortOrder = r.GetInt32(2),ClosureDate=r.GetDateTime(3) }));
			return category;
		}

	    public ClosureDateClass GetValue()
	    {
            ClosureDateClass category = null;
            _sqlObjectFactory.GetConnection().Using(c => c.Command("SELECT ClosureDate,FinalClosureDate from pf_ClosureDate")
                .ExecuteReader()
                .ReadOne(r => category = new ClosureDateClass() { ClosureDate = r.GetDateTime(0),FinalClosureDate = r.GetDateTime(1)}));
            return category;
        }
        public void UpdateClosureDate(DateTime value,DateTime value1)
        {
            var result = 0;
            _sqlObjectFactory.GetConnection().Using(c => result = c.Command("UPDATE pf_ClosureDate SET ClosureDate = @ClosureDate, FinalClosureDate = @FinalClosureDate")

                .AddParameter("@ClosureDate", value)
                .AddParameter("@FinalClosureDate", value1)
                .ExecuteNonQuery());
            if (result != 1)
                throw new Exception(String.Format("Can't update category with ID {0} because it does not exist.", value));
        }
        public List<Category> GetAll()
		{
			var categories = new List<Category>();
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("SELECT a.CategoryID, a.Title, a.SortOrder,a.ClosureDate FROM pf_Category a ORDER BY SortOrder")
					.ExecuteReader()
					.ReadAll(r => categories.Add(new Category(r.GetInt32(0)) {Title = r.GetString(1), SortOrder = r.GetInt32(2),ClosureDate = r.GetDateTime(3)})));
			return categories;
		}
        public Category Create(string newTitle, int sortOrder,DateTime closureDate)
		{
			var categoryID = 0;
			_sqlObjectFactory.GetConnection().Using(c => categoryID = Convert.ToInt32(c.Command("INSERT INTO pf_Category (Title, SortOrder,ClosureDate) VALUES (@Title, @SortOrder,@ClosureDate)")
				.AddParameter("@Title", newTitle)
				.AddParameter("@SortOrder", sortOrder)
                .AddParameter("@ClosureDate", closureDate)
                .ExecuteAndReturnIdentity()));
			var category = new Category(categoryID) { Title = newTitle, SortOrder = sortOrder };
			return category;
		}

		public void Delete(int categoryID)
		{
			var result = 0;
			_sqlObjectFactory.GetConnection().Using(c => result = c.Command("DELETE FROM pf_Category WHERE CategoryID = @CategoryID")
				.AddParameter("@CategoryID", categoryID)
				.ExecuteNonQuery());
			if (result != 1)
				throw new Exception(String.Format("Can't delete category with ID {0} because it does not exist.", categoryID));
		}

		public void Update(Category category)
		{
			var result = 0;
			_sqlObjectFactory.GetConnection().Using(c => result = c.Command("UPDATE pf_Category SET Title = @Title, SortOrder = @SortOrder,ClosureDate=@ClosureDate WHERE CategoryID = @CategoryID")
				.AddParameter("@Title", category.Title)
				.AddParameter("@SortOrder", category.SortOrder)
                .AddParameter("@ClosureDate", category.ClosureDate)
                .AddParameter("@CategoryID", category.CategoryID)
				.ExecuteNonQuery());
			if (result != 1)
				throw new Exception(String.Format("Can't update category with ID {0} because it does not exist.", category.CategoryID));
		}
	}
}