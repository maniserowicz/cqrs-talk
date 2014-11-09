#region helpers

using System;
using System.Linq;
using System.Security.Principal;
using System.Collections.Generic;

#region NHibernate

public interface ITransaction : IDisposable
{
    void Commit();
    void Rollback();
}

public interface ISession : IDisposable
{
    object QueryOver<T>();
    ITransaction BeginTransaction();
    T Get<T>(int id);
    void Save(object o);
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

    protected ActionResult OK()
    {
        throw new NotImplementedException();
    }
}

#endregion

#region NServiceBus

public interface IBus
{
    void Send(object o);
    void Publish(object o);
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

public partial class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
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

    public ActionResult GetAds_TheEasyWay(IPrincipal currentUser)
    {
        int userId = currentUser.Id();

        var ads = _simpleData.advertisements_for_ads_page
            .FindAllByUserId(userId);

        return View(ads);
    }
}






//// WRITE

public partial class AdsController
{
    public ActionResult MarkAsSeen(IPrincipal currentUser, int adId)
    {
        using (ISession session = SessionFactory.OpenSession())
        {
            using (ITransaction tx = session.BeginTransaction())
            {
                var user = session.Get<User>(currentUser.Id());
                var ad = session.Get<Advertisement>(adId);
                user.SeenAds.Add(ad);
                session.Save(user);

                tx.Commit();
            }
        }

        // or use "Ads Service"

        return OK();
    }

    // many many similar actions...
}

public partial class User
{
    public List<Advertisement> SeenAds { get; set; }
}

// !!! JUST a reminder...
// ...recall READ side before modifications...
// ...we would need to generate this:
//      and not exists
//      (select 1 from SeenAds sa where sa.UserId = ... AND sa.AdvertisementId = ...)
// using ORM - DIFFICULT,
// now we only add it in view - SIMPLE

// enter COMMANDS...

public interface ICommand
{

}

public interface IHandleCommand
{

}

public interface IHandleCommand<TCommand> : IHandleCommand
    where TCommand : ICommand
{
    void Handle(TCommand command);
}

public interface ICommandBus
{
    void SendCommand<T>(T cmd) where T : ICommand;
}

// implementation detail
public class CommandBus : ICommandBus
{
    private readonly Func<Type, IHandleCommand> _handlersFactory;

    public CommandBus(Func<Type, IHandleCommand> handlersFactory)
    {
        _handlersFactory = handlersFactory;
    }

    public void SendCommand<T>(T cmd) where T : ICommand
    {
        // log
        // validate
        // measure time
        // ...

        var handler = (IHandleCommand<T>)_handlersFactory(typeof(T));
        handler.Handle(cmd);
    }
}

public class MarkAdAsSeenCommand : ICommand
{
    public int UserId { get; set; }
    public int AdId { get; set; }
}

public partial class AdsController
{
    private readonly ICommandBus _commandBus;

    public AdsController(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public ActionResult MarkAsSeen_UsingCommands(MarkAdAsSeenCommand command)
    {
        _commandBus.SendCommand(command);
        return OK();
    }
}

// every command handler is crafted for the job,
// so this might be OK instead of "full blown domain model"
public class MarkAdAsSeenHandler : IHandleCommand<MarkAdAsSeenCommand>
{
    private readonly dynamic _simpleData;

    public MarkAdAsSeenHandler(dynamic simpleData)
    {
        _simpleData = simpleData;
    }

    // binding takes care of validation
    public void Handle(MarkAdAsSeenCommand command)
    {
        _simpleData.SeenAds.Insert(
            UserId: command.UserId, AdvertisementId: command.AdId
        );

        // enter EVENTS
        _eventsBus.PublishEvent(new AdMarkedAsSeen
        {
            UserId = command.UserId,
            AdId = command.AdId,
        });
    }

    private readonly IEventsBus _eventsBus;
}


public interface IEvent
{

}

public interface IHandleEvent
{

}

public interface IHandleEvent<TEvent> : IHandleEvent
    where TEvent : IEvent
{
    void Handle(TEvent @event);
}

public interface IEventsBus
{
    void PublishEvent<T>(T cmd) where T : IEvent;
}

public class EventsBus : IEventsBus
{
    private readonly Func<Type, IEnumerable<IHandleEvent>> _handlersFactory;

    public EventsBus(Func<Type, IEnumerable<IHandleEvent>> handlersFactory)
    {
        _handlersFactory = handlersFactory;
    }

    public void PublishEvent<T>(T e) where T : IEvent
    {
        var handlers = _handlersFactory(typeof(T))
            .Cast<IHandleEvent<T>>()
            ;

        foreach (var handler in handlers)
        {
            handler.Handle(e);
        }
    }
}

public class AdMarkedAsSeen : IEvent
{
    public int UserId { get; set; }
    public int AdId { get; set; }
}

public class WhenAdMarkedAsSeen_NotifyCompany : IHandleEvent<AdMarkedAsSeen>
{
    public void Handle(AdMarkedAsSeen @event)     {    }
}

public class WhenAdMarkedAsSeen_TargetMoreAdsTowardsUser : IHandleEvent<AdMarkedAsSeen>
{
    public void Handle(AdMarkedAsSeen @event)    {    }
}

public class WhenAdMarkedAsSeen_PrepareEmailCampaignForUser : IHandleEvent<AdMarkedAsSeen>
{
    public void Handle(AdMarkedAsSeen @event)    {    }
}



// NEXT STEPS...



// hardcore "catch all commands controller" implementation:
public class CommandsController : Controller
{
    private readonly ICommandBus _commandBus;

    public CommandsController(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    // would require type detection during binding...
    public ActionResult Send(ICommand command)
    {
        _commandBus.SendCommand(command);
        return OK();
    }
}

// implement Event Sourcing
public class EventSourcingEventHandler : IHandleEvent<IEvent>
{
    public void Handle(IEvent @event)
    {
        // _eventStore.Save(@event);
    }
}

// distribute commands and events via message bus
public class MessageBusIntegration : IHandleCommand<ICommand>, IHandleEvent<IEvent>
{
    private readonly IBus _bus;

    public MessageBusIntegration(IBus bus)
    {
        _bus = bus;
    }

    public void Handle(ICommand command)
    {
        _bus.Send(command);
    }

    public void Handle(IEvent @event)
    {
        _bus.Publish(@event);
    }
}