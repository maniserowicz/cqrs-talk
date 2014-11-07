#region helpers

using System;
using System.Security.Principal;
using System.Collections.Generic;

#region NHibernate

public interface ISession
{
    object QueryOver<T>();
}

public static class SessionFactory
{
    public static ISession OpenSession()
    {
        throw new NotImplementedException();
    }
}

#endregion

#region MVC

public class ActionResult
{
}

public class Controller
{
    protected ActionResult View(object o)
    {
        throw new NotImplementedException();
    }
}

#endregion

public static class IPrincipalExtensions
{
    public static int Id(this IPrincipal @this)
    {
        return 2;
    }
}

public class WILL_NOT_BE_ImplementedException : Exception
{

}

#endregion


//// INTRO

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
}

public class Advertisement
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsArchived { get; set; }
    public byte[] Picture { get; set; }
}

public class AdView
{
    public Advertisement ParentAd { get; set; }
    public DateTime ViewDate { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<User> Customers { get; set; }
    public List<Advertisement> Ads { get; set; }
}

//// READ

public partial class AdsController : Controller
{
    // AS a user
    // WHEN I enter ads page
    // I WANT to see ads from companies that I am a customer of
    // SO THAT I can choose to buy more stuff
    // AND I ALSO WANT these ads to be current
    // SO THAT I don't waste my time on reading archived ads
    // AH AH AND I ALSO WANT to see their views count
    // SO THAT I know if this might be trendy soon
    // OH AND OH AND OH AND OH AND OH AND......
    // (...)
    public ActionResult GetAds(IPrincipal currentUser)
    {
        int userId = currentUser.Id();

        var repository = new AdsRepository();
        IEnumerable<Advertisement> ads = repository
            .Get_NonArchived_News_WithViewsCount_WithoutPictures_ForUser(userId);

        return View(ads);
    }
}

public class AdsRepository
{
    public IEnumerable<Advertisement> Get_NonArchived_News_WithViewsCount_WithoutPictures_ForUser(int userId)
    {
        //  select n.Id, n.Title, n.Content
        //      , (select count(*) from AdViews v where v.ParentAdId = a.Id)
        //  from Advertisements a
        //      join Companies c on a.CompanyId = c.Id
        //      join Users u on u.CompanyId = c.Id
        //  where
        //      a.IsArchived = 0
        //      and u.Id = <userId>

        ISession session = SessionFactory.OpenSession();

        var results = session.QueryOver<Advertisement>()

            // joins...

            // conditions...

            // count ad views...

            // do NOT select Picture column...

            // ........
            // ........
            // ........
            // ........
            // ........
            // ........
            ;
            
        throw new WILL_NOT_BE_ImplementedException();
    }
}

/*

CREATE VIEW advertisements_for_ads_page AS
    select u.Id, n.Id, n.Title, n.Content
        , (select count(*) from AdViews v where v.ParentAdId = a.Id)
    from Advertisements a
        join Companies c on a.CompanyId = c.Id
        join Users u on u.CompanyId = c.Id
    where
        a.IsArchived = 0

*/

public partial class AdsController
{
    private readonly dynamic _simpleData;

    public AdsController(dynamic simpleData)
    {
        _simpleData = simpleData;
    }

    public ActionResult GetAds_WithoutSuicides_InITDepartment(IPrincipal currentUser)
    {
        int userId = currentUser.Id();

        var ads = _simpleData.advertisements_for_ads_page
            .FindAllByUserId(userId);

        return View(ads);
    }
}
