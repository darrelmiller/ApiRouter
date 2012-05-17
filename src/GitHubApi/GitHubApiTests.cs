using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GitHubApi
{
    public class GitHubApiTests
    {


        [Fact]
        public void UriGeneration()
        {
            var router =
                new GitHubApiRouter(new Uri("http://localhost/"));

            var url = router.GetUrlForController(typeof(RepoLabelsController));

            Assert.Equal("http://localhost/repos/{userid}/{repoid}/labels", url.OriginalString);
        }

        [Fact]
        public void UriGenerationForDuplicateController()
        {
            var router =
                new GitHubApiRouter(new Uri("http://localhost/"));

            var url = router.GetUrlForController(typeof(GistController),"star");

            Assert.Equal("http://localhost/gists/{gistid}/star", url.OriginalString);
        }
    }
}
