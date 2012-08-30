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

        public GitHubApiRouter(Uri baseUrl) : base("", baseUrl)
        {

            To<RootController>();

            Add("issues", ri=> ri.To<IssuesController>());
            Add("events", ri => ri.To<EventsController>());
            Add("networks", rn => rn.Add("{userid}", ru => ru.Add("{repoid}", rr => rr.To<NetworksController>())));

            Add("gists",rg => rg.To<GistsController>()
                    
                    .Add("public",rp => rp.To<GistsController>(new { gistfilter = "public" }))
                    .Add("starred", rs => rs.To<GistsController>(new { gistfilter = "starred" }))
                    .Add("{gistid}", rgi => rgi.To<GistController>("justid")
                         .Add("comments", rc => rc.To<GistCommentsController>())
                         .Add("star", rs=> rs.To<GistController>(new { action = "Star" }, "star"))
                         .Add("fork", rf => rf.To<GistController>(new { action = "Fork" }, "fork"))
                ));


            Add("repos",re => re.Add("{userid}", ru => ru.Add("{repoid}", rr => rr.To<RepoController>()
                .Add("labels", r => r.To<RepoLabelsController>())
                .Add("milestones", r => r.To<RepoMilestonesController>())
                .Add("contributors", r => r.To<RepoContributorsController>())
                .Add("languages", r => r.To<RepoLanguagesController>())
                .Add("teams", r => r.To<RepoTeamsController>())
                .Add("tags", r => r.To<RepoTagsController>())
                .Add("branches", r => r.To<RepoBranchesController>())
                .Add("events", r => r.To<RepoEventsController>())
                .Add("watchers", r => r.To<RepoWatchersController>())
                .Add("collaborators", r => r.To<RepoCollaboratorsController>()
                        .Add("{userid}", ruc => ruc.To<RepoCollaboratorsController>())
                    )
                .Add("downloads",rd => rd.To<RepoDownloadsController>()
                        .Add("{downloadid}", rdi => rdi.To<RepoDownloadsController>())
                    )
                .Add("keys",rk => rk.To<RepoKeyController>()
                        .Add("{keyid}", rki => rki.To<RepoKeyController>())
                    )
                .Add("commits", rc => rc.To<RepoCommitsController>()
                        .Add("{shaid}", rcs => rcs.To<RepoCommitsController>()
                                .Add("comments", rcc => rcc.To<RepoCommitsCommentsController>())
                            )
                    )
                .Add("compare", rc => rc.Add(@"{baseid}\.\.\.{headid}", rco => rco.To<RepoCompareController>()))
                .Add("pulls", rp => rp
                    .Add("comments",rc => rc.Add("{number}", r => r.To<RepoPullsController>()))
                    .Add(@"{number}", rn => rn.Add("comments", r => r.To<RepoPullsController>()))
                    )

                .Add("hooks", rh => rh.To<RepoHooksController>()
                        .Add("{hookid}", rhi => rhi.To<RepoHooksController>()
                                .Add("test", rht => rht.To<RepoHooksTestController>())
                            )
                    ))));

            Add("orgs", ro => ro.Add("{orgid}", roi => roi.To<OrgController>()
                .Add("events", r => r.To<OrgEventsController>())
                .Add("public_members", rpm => rpm.To<OrgMembersController>(new { orgmemberfilter = "public" })
                        .Add("{userid}", ru => ru.To<OrgMembersController>())
                    )
                .Add("members", r => r.To<OrgMembersController>()
                        .Add("{userid}", ru => ru.To<OrgMembersController>())
                    )
                ));
            
            Add("favicon.ico", r => r.To<EmptyController>());
            Add("SiteMap", r => r.To<SiteMapController>());
            
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
            var pathRouteData = (PathRouteData) Request.GetRouteData();

            var paramvalues =new StringBuilder();

            foreach (KeyValuePair<string, object> keyValuePair in pathRouteData.Values)
            {
                paramvalues.Append(keyValuePair.Key);
                paramvalues.Append(" = ");
                paramvalues.Append(keyValuePair.Value);
                paramvalues.Append(Environment.NewLine);
            }

            var url = pathRouteData.RootRouter.GetUrlForController(this.GetType());

            return new HttpResponseMessage()
                       {
                           Content = new StringContent("Response from " + this.GetType().Name  + Environment.NewLine
                                                     + "Url: " + url.AbsoluteUri
                                                     + "Parameters: " + Environment.NewLine 
                                                     + paramvalues.ToString())
                       };
        }
    }


    

}
