using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Tavis;

namespace GitHubApi
{
    public class GitHubApiRouter : ApiRouter
    {

        public GitHubApiRouter() : base("")
        {
            DispatchTo<RootController>();

            Add(new ApiRouter("issues").DispatchTo<IssuesController>());
            Add(new ApiRouter("events").DispatchTo<EventsController>());
            Add(new ApiRouter("networks").Add(new ApiRouter("{userid}").Add(new ApiRouter("{repoid}"))).DispatchTo<NetworksController>());

            Add(new ApiRouter("gists").DispatchTo<GistsController>()
                .Add(new ApiRouter("public").DispatchTo<GistsController>(new {gistfilter = "public"}))
                .Add(new ApiRouter("starred").DispatchTo<GistsController>(new {gistfilter = "starred"}))
                .Add(new ApiRouter("{gistid}").DispatchTo<GistController>()
                         .Add(new ApiRouter("comments").DispatchTo<GistCommentsController>())
                         .Add(new ApiRouter("star").DispatchTo<GistController>(new {action = "Star"}))
                         .Add(new ApiRouter("fork").DispatchTo<GistController>(new { action = "Fork" }))
                ));

            Add(new ApiRouter("repos").Add(new ApiRouter("{userid}").Add(new ApiRouter("{repoid}")).DispatchTo<RepoController>())
                .Add(new ApiRouter("labels").DispatchTo<RepoLabelsController>())
                .Add(new ApiRouter("milestones").DispatchTo<RepoMilestonesController>())
                .Add(new ApiRouter("contributors").DispatchTo<RepoContributorsController>())
                .Add(new ApiRouter("languages").DispatchTo<RepoLanguagesController>())
                .Add(new ApiRouter("teams").DispatchTo<RepoTeamsController>())
                .Add(new ApiRouter("tags").DispatchTo<RepoTagsController>())
                .Add(new ApiRouter("branches").DispatchTo<RepoBranchesController>())
                .Add(new ApiRouter("events").DispatchTo<RepoEventsController>())
                .Add(new ApiRouter("watchers").DispatchTo<RepoWatchersController>())
                .Add(new ApiRouter("collaborators").DispatchTo<RepoCollaboratorsController>()
                        .Add(new ApiRouter("{userid}").DispatchTo<RepoCollaboratorsController>())
                    )
                .Add(new ApiRouter("downloads").DispatchTo<RepoDownloadsController>()
                        .Add(new ApiRouter("{downloadid}").DispatchTo<RepoDownloadsController>())
                    )
                .Add(new ApiRouter("keys").DispatchTo<RepoKeyController>()
                        .Add(new ApiRouter("{keyid}").DispatchTo<RepoKeyController>())
                    )
                .Add(new ApiRouter("commits").DispatchTo<RepoCommitsController>()
                        .Add(new ApiRouter("{shaid}").DispatchTo<RepoCommitsController>()
                                .Add(new ApiRouter("comments").DispatchTo<RepoCommitsCommentsController>())
                            )
                    )
                .Add(new ApiRouter("compare").Add(new ApiRouter(@"{baseid}\.\.\.{headid}")).DispatchTo<RepoCompareController>())
                .Add(new ApiRouter("pulls")
                    .Add(new ApiRouter("comments").Add(new ApiRouter("{number}").DispatchTo<RepoPullsController>()))
                    .Add(new ApiRouter(@"{number}").Add(new ApiRouter("comments").DispatchTo<RepoPullsController>()))
                    )

                .Add(new ApiRouter("hooks").DispatchTo<RepoHooksController>()
                        .Add(new ApiRouter("{hookid}").DispatchTo<RepoHooksController>()
                                .Add(new ApiRouter("test").DispatchTo<RepoHooksTestController>())
                            )
                    )                
                );

            Add(new ApiRouter("orgs").Add(new ApiRouter("{orgid}").DispatchTo<OrgController>()
                .Add(new ApiRouter("events").DispatchTo<OrgEventsController>())
                .Add(new ApiRouter("public_members").DispatchTo<OrgMembersController>(new { orgmemberfilter = "public" })
                        .Add(new ApiRouter("{userid}").DispatchTo<OrgMembersController>())
                    )
                .Add(new ApiRouter("members").DispatchTo<OrgMembersController>()
                        .Add(new ApiRouter("{userid}").DispatchTo<OrgMembersController>())
                    )
                ));

        }
    }


    public class RootController : DummyController { }
    public class IssuesController : DummyController { }
    public class EventsController : DummyController { }
    public class NetworksController : DummyController { }
    public class GistsController : DummyController { }
    public class GistController : DummyController { }
    public class GistCommentsController : DummyController { }
    public class RepoController : DummyController { }
    public class RepoLabelsController : DummyController { }
    public class RepoMilestonesController : DummyController { }
    public class RepoContributorsController : DummyController { }
    public class RepoLanguagesController : DummyController { }
    public class RepoTeamsController : DummyController { }
    public class RepoTagsController : DummyController { }
    public class RepoBranchesController : DummyController { }
    public class RepoEventsController : DummyController { }
    public class RepoWatchersController : DummyController { }
    public class RepoCollaboratorsController : DummyController { }
    public class RepoDownloadsController : DummyController { }
    public class RepoKeyController : DummyController { }
    public class RepoCommitsController : DummyController { }
    public class RepoCommitsCommentsController : DummyController { }
    public class RepoCompareController : DummyController { }
    public class RepoPullsController : DummyController { }
    public class OrgController : DummyController { }
    public class OrgEventsController : DummyController { }
    public class OrgMembersController : DummyController { }
    public class RepoHooksController : DummyController { }
    public class RepoHooksTestController : DummyController { }

    public class DummyController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var httpRouteData = Request.GetRouteData();

            var paramvalues =new StringBuilder();

            foreach (KeyValuePair<string, object> keyValuePair in httpRouteData.Values)
            {
                paramvalues.Append(keyValuePair.Key);
                paramvalues.Append(" = ");
                paramvalues.Append(keyValuePair.Value.ToString());
                paramvalues.Append(Environment.NewLine);
            }
            return new HttpResponseMessage()
                       {
                           Content = new StringContent("Response from " + this.GetType().Name  + Environment.NewLine + "Parameters: " + Environment.NewLine + paramvalues.ToString())
                       };
        }
    }

}
