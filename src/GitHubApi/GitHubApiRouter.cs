using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Tavis;

namespace GitHubApi
{
    public class GitHubApiRouter : ApiRouter
    {

        public GitHubApiRouter(Uri baseUrl)
            : base("", baseUrl)
        {

            To<RootController>();

            Add(new ApiRouter("issues").To<IssuesController>());
            Add(new ApiRouter("events").To<EventsController>());
            Add(new ApiRouter("networks").Add(new ApiRouter("{userid}").Add(new ApiRouter("{repoid}")))).To<NetworksController>();

            Add(new ApiRouter("gists").To<GistsController>()
                .Add(new ApiRouter("public").To<GistsController>(new { gistfilter = "public" }))
                .Add(new ApiRouter("starred").To<GistsController>(new { gistfilter = "starred" }))
                .Add(new ApiRouter("{gistid}").To<GistController>()
                         .Add("comments").To<GistCommentsController>()
                         .Add("star").To<GistController>(new { action = "Star" })
                         .Add("fork").To<GistController>(new { action = "Fork" })
                ));


            Add(new ApiRouter("repos").Add(new ApiRouter("{userid}").Add(new ApiRouter("{repoid}").To<RepoController>()
                .Add(new ApiRouter("labels").To<RepoLabelsController>())
                .Add(new ApiRouter("milestones").To<RepoMilestonesController>())
                .Add(new ApiRouter("contributors").To<RepoContributorsController>())
                .Add(new ApiRouter("languages").To<RepoLanguagesController>())
                .Add(new ApiRouter("teams").To<RepoTeamsController>())
                .Add(new ApiRouter("tags").To<RepoTagsController>())
                .Add(new ApiRouter("branches").To<RepoBranchesController>())
                .Add(new ApiRouter("events").To<RepoEventsController>())
                .Add(new ApiRouter("watchers").To<RepoWatchersController>())
                .Add(new ApiRouter("collaborators").To<RepoCollaboratorsController>()
                        .Add(new ApiRouter("{userid}").To<RepoCollaboratorsController>())
                    )
                .Add(new ApiRouter("downloads").To<RepoDownloadsController>()
                        .Add(new ApiRouter("{downloadid}").To<RepoDownloadsController>())
                    )
                .Add(new ApiRouter("keys").To<RepoKeyController>()
                        .Add(new ApiRouter("{keyid}").To<RepoKeyController>())
                    )
                .Add(new ApiRouter("commits").To<RepoCommitsController>()
                        .Add(new ApiRouter("{shaid}").To<RepoCommitsController>()
                                .Add(new ApiRouter("comments").To<RepoCommitsCommentsController>())
                            )
                    )
                .Add(new ApiRouter("compare").Add(new ApiRouter(@"{baseid}\.\.\.{headid}")).To<RepoCompareController>())
                .Add(new ApiRouter("pulls")
                    .Add(new ApiRouter("comments").Add(new ApiRouter("{number}").To<RepoPullsController>()))
                    .Add(new ApiRouter(@"{number}").Add(new ApiRouter("comments").To<RepoPullsController>()))
                    )

                .Add(new ApiRouter("hooks").To<RepoHooksController>()
                        .Add(new ApiRouter("{hookid}").To<RepoHooksController>()
                                .Add(new ApiRouter("test").To<RepoHooksTestController>())
                            )
                    )
                )));

            Add(new ApiRouter("orgs").Add(new ApiRouter("{orgid}").To<OrgController>()
                .Add(new ApiRouter("events").To<OrgEventsController>())
                .Add(new ApiRouter("public_members").To<OrgMembersController>(new { orgmemberfilter = "public" })
                        .Add(new ApiRouter("{userid}").To<OrgMembersController>())
                    )
                .Add(new ApiRouter("members").To<OrgMembersController>()
                        .Add(new ApiRouter("{userid}").To<OrgMembersController>())
                    )
                ));
            
            Add(new ApiRouter("favicon.ico").To<EmptyController>());
            Add(new ApiRouter("SiteMap").To<SiteMapController>());
            
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


    public class EmptyController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return  new HttpResponseMessage();
        }
    }

    public class SiteMapController : ApiController
    {
        public HttpResponseMessage Get()
        {
            
            var pathRouteData = (PathRouteData) Request.GetRouteData();
            var sb = new StringBuilder();
            RenderRouterHierarchy(sb, pathRouteData.RootRouter);
            return new HttpResponseMessage()
                       {
                           Content = new StringContent(sb.ToString())
                       };
        }

        private void RenderRouterHierarchy(StringBuilder sb,  ApiRouter router, int depth = 0 )
        {
                sb.Append("".PadLeft(depth));
                sb.Append('\\');
                sb.Append(router.SegmentTemplate);
                sb.Append(Environment.NewLine);
            foreach (var childrouter in router.ChildRouters.Values)
            {
                RenderRouterHierarchy(sb, childrouter, depth + 2);
            }
        }
    }

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
                paramvalues.Append(keyValuePair.Value);
                paramvalues.Append(Environment.NewLine);
            }
            return new HttpResponseMessage()
                       {
                           Content = new StringContent("Response from " + this.GetType().Name  + Environment.NewLine + "Parameters: " + Environment.NewLine + paramvalues.ToString())
                       };
        }
    }


    

}
