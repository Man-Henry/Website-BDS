using FluentAssertions;
using NetArchTest.Rules;

namespace Website_QLPT.Tests.Architecture
{
    /// <summary>
    /// NetArchTest — Kiểm tra cấu trúc kiến trúc dự án
    /// Ít nhất 5 architecture rules
    /// </summary>
    public class ArchitectureTests
    {
        private const string ModelsNamespace = "Website_QLPT.Models";
        private const string ControllersNamespace = "Website_QLPT.Controllers";
        private const string ApiControllersNamespace = "Website_QLPT.Controllers.Api";
        private const string ServicesNamespace = "Website_QLPT.Services";
        private const string DataNamespace = "Website_QLPT.Data";

        /// <summary>
        /// Rule 1: Models không được reference Controllers
        /// </summary>
        [Fact]
        public void Models_ShouldNot_DependOnControllers()
        {
            var result = Types.InAssembly(typeof(Website_QLPT.Models.Room).Assembly)
                .That()
                .ResideInNamespace(ModelsNamespace)
                .ShouldNot()
                .HaveDependencyOn(ControllersNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "Models should not depend on Controllers — " +
                $"Violations: {(result.FailingTypes != null ? string.Join(", ", result.FailingTypes.Select(t => t.Name)) : "none")}");
        }

        /// <summary>
        /// Rule 2: Models không được reference Services
        /// </summary>
        [Fact]
        public void Models_ShouldNot_DependOnServices()
        {
            var result = Types.InAssembly(typeof(Website_QLPT.Models.Room).Assembly)
                .That()
                .ResideInNamespace(ModelsNamespace)
                .ShouldNot()
                .HaveDependencyOn(ServicesNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "Models should not depend on Services");
        }

        /// <summary>
        /// Rule 3: API Controllers phải kế thừa ControllerBase
        /// </summary>
        [Fact]
        public void ApiControllers_ShouldInherit_ControllerBase()
        {
            // All API controllers with [ApiController] attribute should inherit from ControllerBase
            var result = Types.InAssembly(typeof(Website_QLPT.Controllers.Api.AuthApiController).Assembly)
                .That()
                .ResideInNamespace(ApiControllersNamespace)
                .And()
                .HaveCustomAttribute(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute))
                .Should()
                .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "All API controllers with [ApiController] should inherit from ControllerBase");
        }

        /// <summary>
        /// Rule 4: MVC Controllers phải kế thừa Controller
        /// </summary>
        [Fact]
        public void MvcControllers_ShouldInherit_Controller()
        {
            var result = Types.InAssembly(typeof(Website_QLPT.Controllers.HomeController).Assembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .DoNotResideInNamespace(ApiControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .Should()
                .Inherit(typeof(Microsoft.AspNetCore.Mvc.Controller))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "All MVC controllers should inherit from Controller");
        }

        /// <summary>
        /// Rule 5: Data namespace chỉ chứa DbContext và SeedData
        /// </summary>
        [Fact]
        public void DataLayer_ShouldNot_DependOnControllers()
        {
            var result = Types.InAssembly(typeof(Website_QLPT.Data.ApplicationDbContext).Assembly)
                .That()
                .ResideInNamespace(DataNamespace)
                .ShouldNot()
                .HaveDependencyOn(ControllersNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "Data layer should not depend on Controllers");
        }

        /// <summary>
        /// Rule 6: Services không được reference Controllers
        /// </summary>
        [Fact]
        public void Services_ShouldNot_DependOnControllers()
        {
            var result = Types.InAssembly(typeof(Website_QLPT.Models.Room).Assembly)
                .That()
                .ResideInNamespace(ServicesNamespace)
                .ShouldNot()
                .HaveDependencyOn(ControllersNamespace)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "Services should not depend on Controllers");
        }

        /// <summary>
        /// Rule 7: Tất cả API Controllers phải có [ApiController] attribute
        /// </summary>
        [Fact]
        public void ApiControllers_ShouldHave_ApiControllerAttribute()
        {
            var result = Types.InAssembly(typeof(Website_QLPT.Controllers.Api.AuthApiController).Assembly)
                .That()
                .ResideInNamespace(ApiControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .Should()
                .HaveCustomAttribute(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "All API controllers should have [ApiController] attribute");
        }
    }
}
