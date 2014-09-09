using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavis;

namespace TreeRoutingTests
{
    public class TavisRouteTests
    {

        public void CreateTMServerRoute()
        {


            var route = new TreeRoute("",
                new TreeRoute("images").To<FakeController>(),
                CreateDesktopRoute().To<FakeController>(),
                new TreeRoute("portal").To<FakeController>(),
                new TreeRoute("reports").To<FakeController>());

            

        }

        public TreeRoute CreateDesktopRoute()
        {
            return new TreeRoute("desktop",
                            new TreeRoute("datasets").To<FakeController>(),
                            new TreeRoute("{dataset}", 
                                new TreeRoute("shell").To<FakeController>(),
                                CreateDesktopDocumentsController(),
                                new TreeRoute("setup").To<FakeController>(),
                                new TreeRoute("resources").To<FakeController>(),
                                new TreeRoute("tools").To<FakeController>(),
                                new TreeRoute("dataset").To<FakeController>(),
                                new TreeRoute("help").To<FakeController>()
                                         ).To<FakeController>()
                                );

        }

        public TreeRoute CreateDesktopDocumentsController()
        {
            return new TreeRoute("documents",
                            new TreeRoute("salesorder").To<FakeController>()
                            ).To<FakeController>();
        }

        public TreeRoute CreateSalesOrderController()
        {

            return new TreeRoute("salesorder",
                                new TreeRoute("{id}", new TreeRoute("salesitem").To<FakeController>()).To<FakeController>(),
                                new TreeRoute("detaillayout").To<FakeController>()
                                ).To<FakeController>();
        }

        public TreeRoute CreateReportsController()
        {
            return new TreeRoute("desktop",
                            new TreeRoute("datasets").To<FakeController>(),
                            new TreeRoute("{dataset}",
                                new TreeRoute("shell").To<FakeController>(),
                                new TreeRoute("documents").To<FakeController>(),
                                new TreeRoute("setup").To<FakeController>(),
                                new TreeRoute("resources").To<FakeController>(),
                                new TreeRoute("tools").To<FakeController>(),
                                new TreeRoute("dataset").To<FakeController>(),
                                new TreeRoute("help").To<FakeController>()
                                         ).To<FakeController>()
                                );

        }

      

    }
}
