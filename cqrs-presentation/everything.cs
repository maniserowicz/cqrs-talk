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

public partial class AdsController : Controller
{
    public ActionResult GetAds(IPrincipal currentUser)
    {
        int userId = currentUser.Id();

        var repository = new AdsRepository();
        IEnumerable<Advertisement> ads = repository
            .Get_NonArchived_Ads_WithViewsCount_WithoutPictures_ForUser(userId);

        return View(ads);
    }
}

public class AdsRepository
{
    public IEnumerable<Advertisement> Get_NonArchived_Ads_WithViewsCount_WithoutPictures_ForUser(int userId)
    {
        //  select a.Id, a.Title, a.Content
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
    select u.Id as UserId, a.Id, a.Title, a.Content
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

        IEnumerable<dynamic> ads = _simpleData
            .advertisements_for_ads_page
                .FindAllByUserId(userId);

        return View(ads);
    }
}






//// WRITE

// AS a user
// WHEN I read an ad
// I WANT to mark it as "interesting"
// SO THAT I can easily find it again later
// AND see more ads similar to this one
// (...)

public partial class AdsController
{
    public ActionResult MarkAsInteresting(IPrincipal currentUser, int adId)
    {
        using (ISession session = SessionFactory.OpenSession())
        {
            using (ITransaction tx = session.BeginTransaction())
            {
                var user = session.Get<User>(currentUser.Id());
                var ad = session.Get<Advertisement>(adId);
                user.InterestingAds.Add(ad);
                session.Save(user);

                tx.Commit();
            }
        }

        // or use "Ads Service"...

        return OK();
    }

    // many many similar actions...
}

public partial class User
{
    public List<Advertisement> InterestingAds { get; set; }
}

// ...recall READ side before modifications...
// ...we would now need to generate this:
//    select 1 from InterestingAds ia
//    where ia.UserId = ... and ia.AdvertisementId = ....
//          as IsMarkedAsInteresting
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
        // authz
        // tx
        // validate
        // measure time
        // error handling
        // ...

        var handler = (IHandleCommand<T>)_handlersFactory(typeof(T));
        handler.Handle(cmd);
    }
}

public class MarkAdAsInterestingCommand : ICommand
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

    public ActionResult MarkAsInteresting_UsingCommands(MarkAdAsInterestingCommand command)
    {
        _commandBus.SendCommand(command);
        return OK();
    }
}

// every command handler is crafted for the job,
// so this might be OK instead of "full blown domain model"
public class MarkAdAsInterestingHandler : IHandleCommand<MarkAdAsInterestingCommand>
{
    private readonly dynamic _simpleData;

    public MarkAdAsInterestingHandler(dynamic simpleData)
    {
        _simpleData = simpleData;
    }

    public void Handle(MarkAdAsInterestingCommand command)
    {
        _simpleData.InterestingAds.Insert(
            UserId: command.UserId, AdvertisementId: command.AdId
        );

        // enter EVENTS
        _eventsBus.PublishEvent(new AdMarkedAsInteresting
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

public class AdMarkedAsInteresting : IEvent
{
    public int UserId { get; set; }
    public int AdId { get; set; }
}

public class WhenAdMarkedAsInteresting_NotifyCompany : IHandleEvent<AdMarkedAsInteresting>
{
    public void Handle(AdMarkedAsInteresting @event)     {    }
}

public class WhenAdMarkedAsInteresting_TargetMoreAdsTowardsUser : IHandleEvent<AdMarkedAsInteresting>
{
    public void Handle(AdMarkedAsInteresting @event)    {    }
}

public class WhenAdMarkedAsInteresting_PrepareEmailCampaignForUser : IHandleEvent<AdMarkedAsInteresting>
{
    public void Handle(AdMarkedAsInteresting @event)    {    }
}



// NEXT STEPS...



// single controller for all write-operations
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
public class MessageBusIntegration :
    IHandleCommand<ICommand>
    , IHandleEvent<IEvent>
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