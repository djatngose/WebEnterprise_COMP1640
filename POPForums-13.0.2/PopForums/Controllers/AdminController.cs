using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PopForums.Configuration;
using PopForums.Configuration.DependencyResolution;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.ScoringGame;
using PopForums.Services;
using PopForums.Web;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PopForums.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        public AdminController()
        {
            var serviceLocator = StructuremapMvc.StructureMapDependencyScope;
            _userService = serviceLocator.GetInstance<IUserService>();
            _profileService = serviceLocator.GetInstance<IProfileService>();
            _settingsManager = serviceLocator.GetInstance<ISettingsManager>();
            _categoryService = serviceLocator.GetInstance<ICategoryService>();
            _forumService = serviceLocator.GetInstance<IForumService>();
            _searchService = serviceLocator.GetInstance<ISearchService>();
            _securityLogService = serviceLocator.GetInstance<ISecurityLogService>();
            _errorLogService = serviceLocator.GetInstance<IErrorLog>();
            _banService = serviceLocator.GetInstance<IBanService>();
            _moderationLogService = serviceLocator.GetInstance<IModerationLogService>();
            _ipHistoryService = serviceLocator.GetInstance<IIPHistoryService>();
            _imageService = serviceLocator.GetInstance<IImageService>();
            _mailingListService = serviceLocator.GetInstance<IMailingListService>();
            _eventDefinitionService = serviceLocator.GetInstance<IEventDefinitionService>();
            _awardDefinitionService = serviceLocator.GetInstance<IAwardDefinitionService>();
            _eventPublisher = serviceLocator.GetInstance<IEventPublisher>();
            _topicService = serviceLocator.GetInstance<ITopicService>();
        }

        protected internal AdminController(IUserService userService, IProfileService profileService, ISettingsManager settingsManager, ICategoryService categoryService, IForumService forumService, ISearchService searchService, ISecurityLogService securityLogService, IErrorLog errorLog, IBanService banService, IModerationLogService modLogService, IIPHistoryService ipHistoryService, IImageService imageService, IMailingListService mailingListService, IEventDefinitionService eventDefinitionService, IAwardDefinitionService awardDefinitionService, IEventPublisher eventPublisher, ITopicService topicService)
        {
            _userService = userService;
            _profileService = profileService;
            _settingsManager = settingsManager;
            _categoryService = categoryService;
            _forumService = forumService;
            _searchService = searchService;
            _securityLogService = securityLogService;
            _errorLogService = errorLog;
            _banService = banService;
            _moderationLogService = modLogService;
            _ipHistoryService = ipHistoryService;
            _imageService = imageService;
            _mailingListService = mailingListService;
            _eventDefinitionService = eventDefinitionService;
            _awardDefinitionService = awardDefinitionService;
            _eventPublisher = eventPublisher;
            _topicService = topicService;
        }

        public static string Name = "Admin";
        public static string TimeZonesKey = "TimeZonesKey";

        private readonly IUserService _userService;
        private readonly IProfileService _profileService;
        private readonly ISettingsManager _settingsManager;
        private readonly ICategoryService _categoryService;
        private readonly IForumService _forumService;
        private readonly ISearchService _searchService;
        private readonly ISecurityLogService _securityLogService;
        private readonly IErrorLog _errorLogService;
        private readonly IBanService _banService;
        private readonly IModerationLogService _moderationLogService;
        private readonly IIPHistoryService _ipHistoryService;
        private readonly IImageService _imageService;
        private readonly IMailingListService _mailingListService;
        private readonly IEventDefinitionService _eventDefinitionService;
        private readonly IAwardDefinitionService _awardDefinitionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITopicService _topicService;
        private void SaveFormValuesToSettings(FormCollection collection)
        {
            ViewData["PostResult"] = Resources.SettingsSaved;
            var dictionary = new Dictionary<string, object>();
            collection.CopyTo(dictionary);
            _settingsManager.SaveCurrent(dictionary);
        }

        public ViewResult Index()
        {
            return View(_settingsManager.Current);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ViewResult Index(FormCollection collection)
        {
            SaveFormValuesToSettings(collection);
            ViewData[TimeZonesKey] = DataCollections.TimeZones();
            return View(_settingsManager.Current);
        }

        public ViewResult ExternalLogins()
        {
            return View(_settingsManager.Current);
        }

        [HttpPost]
        public ViewResult ExternalLogins(FormCollection collection)
        {
            SaveFormValuesToSettings(collection);
            HttpRuntime.UnloadAppDomain();
            return View(_settingsManager.Current);
        }

        public ViewResult Email()
        {
            return View(_settingsManager.Current);
        }

        [HttpPost]
        public ViewResult Email(FormCollection collection)
        {
            SaveFormValuesToSettings(collection);
            return View(_settingsManager.Current);
        }

        public ViewResult Categories()
        {
            return View(_categoryService.GetAll());
        }
        public ViewResult ClosureDate()
        {
            var closuredate = _categoryService.GetValue();
            return View(closuredate);
        }
        [HttpPost]
        public RedirectToRouteResult AddClosureDate( string closureDate,string finalClosureDate)
        {
            DateTime parsedDate;

            //if (string.IsNullOrEmpty(newCategoryTitle) || string.IsNullOrEmpty(closureDate) || !DateTime.TryParse(closureDate, out parsedDate))
            //{
            //    TempData["errorName"] = "Please enter the name !!";
            //}
            //else
            //{
            //    _categoryService.Create(newCategoryTitle, Convert.ToDateTime(closureDate));
            //}
            _categoryService.UpdateClosureDate(Convert.ToDateTime(closureDate), Convert.ToDateTime(finalClosureDate));
            return RedirectToAction("ClosureDate");
        }
        [HttpPost]
        public RedirectToRouteResult AddCategory(string newCategoryTitle, string closureDate)
        {
            DateTime parsedDate;

            if (string.IsNullOrEmpty(newCategoryTitle) || string.IsNullOrEmpty(closureDate) || !DateTime.TryParse(closureDate, out parsedDate))
            {
                TempData["errorName"] = "Please enter the name !!";
            }
            else
            {
                _categoryService.Create(newCategoryTitle, Convert.ToDateTime(closureDate));
            }
            return RedirectToAction("Categories");
        }

        public ViewResult CategoryList()
        {
            return View(_categoryService.GetAll());
        }

        [HttpPost]
        public RedirectToRouteResult DeleteCategory(int categoryID)
        {
            var category = _categoryService.Get(categoryID);
            if (category == null)
                throw new Exception(String.Format("Category with ID {0} does not exist.", categoryID));
            var checkPosts = _topicService.GetTopicsInCategory(categoryID);
            if(checkPosts>0)
                throw new Exception("There are still posts in this department");
            _categoryService.Delete(category);
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public JsonResult MoveCategoryUp(int categoryID)
        {
            var category = _categoryService.Get(categoryID);
            if (category == null)
                return Json(new BasicJsonMessage { Result = false, Message = "That category doesn't exist" });
            _categoryService.MoveCategoryUp(category);
            return Json(new BasicJsonMessage { Result = true });
        }

        [HttpPost]
        public JsonResult MoveCategoryDown(int categoryID)
        {
            var category = _categoryService.Get(categoryID);
            if (category == null)
                return Json(new BasicJsonMessage { Result = false, Message = "That category doesn't exist" });
            _categoryService.MoveCategoryDown(category);
            return Json(new BasicJsonMessage { Result = true });
        }

        public ViewResult EditCategory(int id)
        {
            var category = _categoryService.Get(id);
            if (category == null)
                throw new Exception(String.Format("Category with ID {0} does not exist.", id));
            return View(category);
        }

        [HttpPost]
        public RedirectToRouteResult EditCategory(int categoryID, string newTitle, string closureDate)
        {
            var category = _categoryService.Get(categoryID);
            if (category == null)
                throw new Exception(String.Format("Category with ID {0} does not exist.", categoryID));
            _categoryService.UpdateTitle(category, newTitle, Convert.ToDateTime(closureDate));
            return RedirectToAction("Categories");
        }

        public ViewResult Forums()
        {
            return View(_forumService.GetCategorizedForumContainer());
        }

        public ViewResult AddForum()
        {
            SetupCategoryDropDown();
            return View();
        }

        private void SetupCategoryDropDown(int categoryID = 0)
        {
            var categories = _categoryService.GetAll();
            //categories.Insert(0, new Category(0) {Title = "Uncategorized"});
            var selectList = new SelectList(categories, "CategoryID", "Title", categoryID);
            ViewData["categoryID"] = selectList;
        }

        [HttpPost]
        public ActionResult AddForum(int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum)
        {
            if (string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError("title", "Please enter your title!");
                SetupCategoryDropDown();
                return View();
            }
            else
            {
                _forumService.Create(categoryID, title, description, isVisible, isArchived, -2, forumAdapterName, isQAForum);
                return RedirectToAction("Forums");
            }

        }

        public ViewResult EditForum(int id)
        {
            var forum = _forumService.Get(id);
            SetupCategoryDropDown(forum.CategoryID.HasValue ? forum.CategoryID.Value : 0);
            return View(forum);
        }
        public ActionResult DeleteForum(int id)
        {
            var obj = _topicService.GetTopicsInForum(id);
            if (obj != null)
            {

                TempData["CustomError"] = "There are still posts in this category";
            }
            else
            {
                _forumService.Delete(id);

            }
            return RedirectToAction("Forums");
        }
        [HttpPost]
        public RedirectToRouteResult EditForum(int id, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum)
        {
            var forum = _forumService.Get(id);
            _forumService.Update(forum, categoryID, title, description, isVisible, isArchived, forumAdapterName, isQAForum);
            return RedirectToAction("Forums");
        }

        public ViewResult CategorizedForums()
        {
            return View(_forumService.GetCategorizedForumContainer());
        }

        [HttpPost]
        public JsonResult MoveForumUp(int forumID)
        {
            var forum = _forumService.Get(forumID);
            if (forum == null)
                return Json(new BasicJsonMessage { Result = false, Message = "That forum doesn't exist" });
            _forumService.MoveForumUp(forum);
            return Json(new BasicJsonMessage { Result = true });
        }

        [HttpPost]
        public JsonResult MoveForumDown(int forumID)
        {
            var forum = _forumService.Get(forumID);
            if (forum == null)
                return Json(new BasicJsonMessage { Result = false, Message = "That forum doesn't exist" });
            _forumService.MoveForumDown(forum);
            return Json(new BasicJsonMessage { Result = true });
        }

        public ViewResult ForumPermissions()
        {
            return View(_forumService.GetCategorizedForumContainer());
        }

        public JsonResult ForumRoles(int id)
        {
            var forum = _forumService.Get(id);
            if (forum == null)
                throw new Exception(String.Format("ForumID {0} not found.", id));
            var container = new ForumPermissionContainer
            {
                ForumID = forum.ForumID,
                AllRoles = _userService.GetAllRoles(),
                PostRoles = _forumService.GetForumPostRoles(forum),
                ViewRoles = _forumService.GetForumViewRoles(forum)
            };
            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public enum ModifyForumRolesType
        {
            AddView, RemoveView, AddPost, RemovePost, RemoveAllView, RemoveAllPost
        }

        public EmptyResult ModifyForumRoles(int forumID, ModifyForumRolesType modifyType, string role = null)
        {
            var forum = _forumService.Get(forumID);
            if (forum == null)
                throw new Exception(String.Format("ForumID {0} not found.", forumID));
            switch (modifyType)
            {
                case ModifyForumRolesType.AddPost:
                    _forumService.AddPostRole(forum, role);
                    break;
                case ModifyForumRolesType.RemovePost:
                    _forumService.RemovePostRole(forum, role);
                    break;
                case ModifyForumRolesType.AddView:
                    _forumService.AddViewRole(forum, role);
                    break;
                case ModifyForumRolesType.RemoveView:
                    _forumService.RemoveViewRole(forum, role);
                    break;
                case ModifyForumRolesType.RemoveAllPost:
                    _forumService.RemoveAllPostRoles(forum);
                    break;
                case ModifyForumRolesType.RemoveAllView:
                    _forumService.RemoveAllViewRoles(forum);
                    break;
                default:
                    throw new Exception("ModifyForumRoles doesn't know what to do.");
            }
            return new EmptyResult();
        }

        public ViewResult UserRoles()
        {
            var roles = _userService.GetAllRoles();
            roles.Remove("Admin");
            roles.Remove("Moderator");
            return View(roles);
        }

        [HttpPost]
        public RedirectToRouteResult CreateRole(string newRole)
        {
            var user = this.CurrentUser();
            _userService.CreateRole(newRole, user, HttpContext.Request.UserHostAddress);
            return RedirectToAction("UserRoles");
        }

        [HttpPost]
        public RedirectToRouteResult DeleteRole(string roleToDelete)
        {
            var user = this.CurrentUser();
            _userService.DeleteRole(roleToDelete, user, HttpContext.Request.UserHostAddress);
            return RedirectToAction("UserRoles");
        }

        public ViewResult Search()
        {
            ViewData["Interval"] = _settingsManager.Current.SearchIndexingInterval;
            ViewData["JunkWords"] = _searchService.GetJunkWords();
            return View();
        }

        [HttpPost]
        public ViewResult Search(FormCollection collection)
        {
            SaveFormValuesToSettings(collection);
            ViewData["Interval"] = _settingsManager.Current.SearchIndexingInterval;
            ViewData["JunkWords"] = _searchService.GetJunkWords();
            return View();
        }

        public RedirectToRouteResult CreateJunkWord(string newWord)
        {
            _searchService.CreateJunkWord(newWord);
            return RedirectToAction("Search");
        }

        public RedirectToRouteResult DeleteJunkWord(string deleteWord)
        {
            _searchService.DeleteJunkWord(deleteWord);
            return RedirectToAction("Search");
        }

        public ViewResult EditUserSearch()
        {
            return View();
        }

        [HttpPost]
        public ViewResult EditUserSearch(UserSearch userSearch)
        {
            ViewBag.SearchText = userSearch.SearchText;
            ViewBag.UserSearchType = userSearch.SearchType;
            switch (userSearch.SearchType)
            {
                case UserSearch.UserSearchType.Email:
                    return View(_userService.SearchByEmail(userSearch.SearchText));
                case UserSearch.UserSearchType.Name:
                    return View(_userService.SearchByName(userSearch.SearchText));
                case UserSearch.UserSearchType.Role:
                    return View(_userService.SearchByRole(userSearch.SearchText));
                default:
                    throw new ArgumentOutOfRangeException("userSearch");
            }
        }

        public ActionResult EditUser(int id)
        {
            var user = _userService.GetUser(id);
            if (user == null)
                throw new Exception(String.Format("UserID {0} not found.", id));
            var profile = _profileService.GetProfileForEdit(user);
            var model = new UserEdit(user, profile);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(int id, UserEdit userEdit)
        {
            var user = this.CurrentUser();
            var targetUser = _userService.GetUser(id);
            if (targetUser == null)
                throw new Exception(String.Format("UserID {0} not found.", id));
            var avatarFile = Request.Files["avatarFile"];
            var photoFile = Request.Files["photoFile"];
            _userService.EditUser(targetUser, userEdit, userEdit.DeleteAvatar, userEdit.DeleteImage, avatarFile, photoFile, HttpContext.Request.UserHostAddress, user);
            return RedirectToAction("EditUserSearch");
        }

        [HttpPost]
        public RedirectToRouteResult DeleteUser(int id)
        {
            return DeleteUser(id, false);
        }

        [HttpPost]
        public RedirectToRouteResult DeleteAndBanUser(int id)
        {
            return DeleteUser(id, true);
        }

        private RedirectToRouteResult DeleteUser(int id, bool ban)
        {
            var targetUser = _userService.GetUser(id);
            if (targetUser == null)
                throw new Exception(String.Format("UserID {0} not found.", id));
            _userService.DeleteUser(targetUser, this.CurrentUser(), HttpContext.Request.UserHostAddress, ban);
            return RedirectToAction("EditUserSearch");
        }

        public ViewResult SecurityLog()
        {
            return View();
        }

        [HttpPost]
        public ViewResult SecurityLog(DateTime startDate, DateTime endDate, string searchType, string searchTerm)
        {
            List<SecurityLogEntry> list;
            switch (searchType.ToLower())
            {
                case "userid":
                    list = _securityLogService.GetLogEntriesByUserID(Convert.ToInt32(searchTerm), startDate, endDate);
                    break;
                case "name":
                    list = _securityLogService.GetLogEntriesByUserName(searchTerm, startDate, endDate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("searchTerm");
            }
            return View(list);
        }

        public ViewResult ErrorLog(int page = 1)
        {
            PagerContext pagerContext;
            var errors = _errorLogService.GetErrors(page, 20, out pagerContext);
            ViewBag.PagerContext = pagerContext;
            return View(errors);
        }

        [HttpPost]
        public RedirectToRouteResult DeleteAllErrorLog()
        {
            _errorLogService.DeleteAllErrors();
            return RedirectToAction("ErrorLog");
        }

        [HttpPost]
        public RedirectToRouteResult DeleteErrorLog(int id)
        {
            _errorLogService.DeleteError(id);
            return RedirectToAction("ErrorLog");
        }

        public ViewResult Ban()
        {
            ViewBag.EmailList = _banService.GetEmailBans();
            ViewBag.IPList = _banService.GetIPBans();
            return View();
        }

        [HttpPost]
        public RedirectToRouteResult BanIP(string ip)
        {
            _banService.BanIP(ip);
            return RedirectToAction("Ban");
        }

        [HttpPost]
        public RedirectToRouteResult RemoveIPBan(string ip)
        {
            _banService.RemoveIPBan(ip);
            return RedirectToAction("Ban");
        }

        [HttpPost]
        public RedirectToRouteResult BanEmail(string email)
        {
            _banService.BanEmail(email);
            return RedirectToAction("Ban");
        }

        [HttpPost]
        public RedirectToRouteResult RemoveEmailBan(string email)
        {
            _banService.RemoveEmailBan(email);
            return RedirectToAction("Ban");
        }

        public ViewResult Services()
        {
            var services = PopForumsActivation.ApplicationServices;
            return View(services);
        }

        public ViewResult ModerationLog()
        {
            return View();
        }

        [HttpPost]
        public ViewResult ModerationLog(DateTime start, DateTime end)
        {
            var list = _moderationLogService.GetLog(start, end);
            return View(list);
        }

        public ViewResult IPHistory()
        {
            return View();
        }

        [HttpPost]
        public ViewResult IPHistory(string ip, DateTime start, DateTime end)
        {
            var list = _ipHistoryService.GetHistory(ip, start, end);
            return View(list);
        }

        public ViewResult UserImageApprove()
        {
            ViewBag.IsNewUserImageApproved = _settingsManager.Current.IsNewUserImageApproved;
            var dictionary = new Dictionary<UserImage, User>();
            var unapprovedImages = _imageService.GetUnapprovedUserImages();
            var users = _userService.GetUsersFromIDs(unapprovedImages.Select(i => i.UserID).ToList());
            foreach (var image in unapprovedImages)
            {
                dictionary.Add(image, users.Single(u => u.UserID == image.UserID));
            }
            return View(dictionary);
        }

        [HttpPost]
        public RedirectToRouteResult ApproveUserImage(int id)
        {
            _imageService.ApproveUserImage(id);
            return RedirectToAction("UserImageApprove");
        }

        [HttpPost]
        public RedirectToRouteResult DeleteUserImage(int id)
        {
            _imageService.DeleteUserImage(id);
            return RedirectToAction("UserImageApprove");
        }

        public ViewResult EmailUsers()
        {
    

            return View(_userService.GetAll());
        }
        public ViewResult ShowUserForm()
        {
            var user = new List<UserType>();
            user.Add(new UserType() { Id = 2, Name = "Staff" });
            user.Add(new UserType() { Id = 1, Name = "Student" });
            //categories.Insert(0, new Category(0) {Title = "Uncategorized"});
            var selectList = new SelectList(user, "Id", "Name");
            ViewData["userType"] = selectList;
            var categories = _categoryService.GetAll();
            var categoryList = new SelectList(categories, "CategoryID", "Title", 0);
            ViewData["department"] = categoryList;
            return View("UserForm");
        }
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UserForm(string name, string email, string password, string repassword, int usertype, int department)
        {
            var user = new SignupData
            {
                Name = name,
                Email = email,
                Password = password,
                PasswordRetype = repassword,
                UserType = usertype,
                DepartmentId=department
            };
            ViewData["name"] = name;
            ViewData["email"] = email;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email?? "");

            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("title", "Please enter your name!");            
            }
            else if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "Please enter the email");
            }
            else if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "Please enter your password!");
            }
            else if (string.IsNullOrEmpty(repassword))
            {
                ModelState.AddModelError("repassword", "Please enter your repassword!");
            }
            else if (password != repassword)
            {
                ModelState.AddModelError("repassword", "Repassword is not the same as password");
            }

            else if (match != null && !match.Success)
            {
                ModelState.AddModelError("email", "Email is invalid");
            }
            if (ViewData.ModelState.IsValid)
            {
                var user2 = _userService.CreateUser(user, HttpContext.Request.UserHostAddress);
                _profileService.Create(user2, user);
                return RedirectToAction("EmailUsers");
            }

            //if (String.IsNullOrWhiteSpace(subject) || String.IsNullOrWhiteSpace(body))
            //{
            //    ViewBag.Result = Resources.SubjectAndBodyNotEmpty;
            //    return View();
            //}
            //var baseString = this.FullUrlHelper("Unsubscribe", AccountController.Name, new { id = "--id--", key = "--key--" });
            //baseString = baseString.Replace("--id--", "{0}").Replace("--key--", "{1}");
            //Func<User, string> unsubscribeLinkGenerator =
            //    user => String.Format(baseString, user.UserID, _profileService.GetUnsubscribeHash(user));
            //_mailingListService.MailUsers(subject, body, htmlBody, unsubscribeLinkGenerator);
            var user1 = new List<UserType>();
            user1.Add(new UserType() { Id = 2, Name = "Staff" });
            user1.Add(new UserType() { Id = 1, Name = "Student" });
            //categories.Insert(0, new Category(0) {Title = "Uncategorized"});
            var selectList = new SelectList(user1, "Id", "Name");
            ViewData["userType"] = selectList;
            var categories = _categoryService.GetAll();
            var categoryList = new SelectList(categories, "CategoryID", "Title", 0);
            ViewData["department"] = categoryList;
            return View();
        }
        public ViewResult EditUserForm(int id)
        {
            var user = _userService.GetUser(id);
            if (user == null)
                throw new Exception(String.Format("User with ID {0} does not exist.", id));
            var user1 = new List<UserType>();
            user1.Add(new UserType() { Id = 2, Name = "Staff" });
            user1.Add(new UserType() { Id = 1, Name = "Student" });
            //categories.Insert(0, new Category(0) {Title = "Uncategorized"});
            var selectList = new SelectList(user1, "Id", "Name",user.UserType);
            ViewData["userType"] = selectList;
            var categories = _categoryService.GetAll();
            var categoryList = new SelectList(categories, "CategoryID", "Title", user.DepartmentId);
            ViewData["department"] = categoryList;
            return View(user);
        }

        [HttpPost]
        public ActionResult EditUserForm(int userId, string name, string password,string repassword, int userType, int department)
        {
            var user = _userService.GetUser(userId);
            if (user == null)
                throw new Exception(String.Format("User with ID {0} does not exist.", userId));
            ViewData["name"] = user.Name;
            ViewData["id"] = userId;
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "Please enter your password!");
            }
            else if (string.IsNullOrEmpty(repassword))
            {
                ModelState.AddModelError("repassword", "Please enter your repassword!");
            }
            else if (password != repassword)
            {
                ModelState.AddModelError("repassword", "Repassword is not the same as password");
            }
            if (ViewData.ModelState.IsValid)
            {
                _userService.UpdateUser(userId, name, password,true, userType, department);
                return RedirectToAction("EmailUsers");
            }
            var user1 = new List<UserType>();
            user1.Add(new UserType() { Id = 2, Name = "Staff" });
            user1.Add(new UserType() { Id = 1, Name = "Student" });
            //categories.Insert(0, new Category(0) {Title = "Uncategorized"});
            var selectList = new SelectList(user1, "Id", "Name", user.UserType);
            ViewData["userType"] = selectList;
            var categories = _categoryService.GetAll();
            var categoryList = new SelectList(categories, "CategoryID", "Title", user.DepartmentId);
            ViewData["department"] = categoryList;
            return View();
        }
        [HttpPost]
        public RedirectToRouteResult DeleteUserForm(int userID)
        {
            var category = _userService.GetUser(userID);
            if (category == null)
                throw new Exception(String.Format("User with ID {0} does not exist.", userID));
            _userService.DeleteUser(category, category,"", false);
            return RedirectToAction("EmailUsers");
        }
        public ViewResult EventDefinitions()
        {
            var model = _eventDefinitionService.GetAll();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddEvent(EventDefinition eventDefinition)
        {
            _eventDefinitionService.Create(eventDefinition);
            return RedirectToAction("EventDefinitions");
        }

        [HttpPost]
        public ActionResult DeleteEvent(string id)
        {
            _eventDefinitionService.Delete(id);
            return RedirectToAction("EventDefinitions");
        }

        public ViewResult AwardDefinitions()
        {
            var model = _awardDefinitionService.GetAll();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddAward(AwardDefinition awardDefinition)
        {
            _awardDefinitionService.Create(awardDefinition);
            return RedirectToAction("Award", new { id = awardDefinition.AwardDefinitionID });
        }

        [HttpPost]
        public ActionResult DeleteAward(string id)
        {
            _awardDefinitionService.Delete(id);
            return RedirectToAction("AwardDefinitions");
        }

        public ViewResult Award(string id)
        {
            var award = _awardDefinitionService.Get(id);
            if (award == null)
                return this.NotFound("NotFound", null);
            var selectList = new SelectList(_eventDefinitionService.GetAll(), "EventDefinitionID", "EventDefinitionID");
            ViewBag.EventList = selectList;
            ViewBag.Conditions = _awardDefinitionService.GetConditions(id);
            return View(award);
        }

        [HttpPost]
        public ActionResult DeleteAwardCondition(string awardDefinitionID, string eventDefinitionID)
        {
            _awardDefinitionService.DeleteCondition(awardDefinitionID, eventDefinitionID);
            return RedirectToAction("Award", new { id = awardDefinitionID });
        }

        [HttpPost]
        public ActionResult AddAwardCondition(AwardCondition awardCondition)
        {
            _awardDefinitionService.AddCondition(awardCondition);
            return RedirectToAction("Award", new { id = awardCondition.AwardDefinitionID });
        }

        public ViewResult ManualEvent()
        {
            var selectList = new SelectList(_eventDefinitionService.GetAll(), "EventDefinitionID", "EventDefinitionID");
            ViewBag.EventList = selectList;
            return View();
        }

        [HttpPost]
        public ActionResult ManualEvent(int userID, string feedMessage, int points)
        {
            var user = _userService.GetUser(userID);
            if (user != null)
                _eventPublisher.ProcessManualEvent(feedMessage, user, points);
            return RedirectToAction("ManualEvent");
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ManualExistingEvent(int userID, string feedMessage, string eventDefinitionID)
        {
            var user = _userService.GetUser(userID);
            var eventDefinition = _eventDefinitionService.GetEventDefinition(eventDefinitionID);
            if (user != null && eventDefinition != null)
                _eventPublisher.ProcessEvent(feedMessage, user, eventDefinition.EventDefinitionID, false);
            return RedirectToAction("ManualEvent");
        }

        public JsonResult GetNames(string id)
        {
            var users = _userService.SearchByName(id);
            var projection = users.Select(u => new { u.UserID, value = u.Name });
            return Json(projection, JsonRequestBehavior.AllowGet);
        }

        public ViewResult ScoringGame()
        {
            ViewData["Interval"] = _settingsManager.Current.ScoringGameCalculatorInterval;
            return View();
        }

        [HttpPost]
        public ViewResult ScoringGame(FormCollection collection)
        {
            SaveFormValuesToSettings(collection);
            ViewData["Interval"] = _settingsManager.Current.ScoringGameCalculatorInterval;
            return View();
        }
    }
}
