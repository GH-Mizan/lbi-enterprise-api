using LbI.Models.TokenAuth;
using LbI.Web.Controllers;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace LbI.Web.Tests.Controllers;

public class HomeController_Tests : LbIWebTestBase
{
    [Fact]
    public async Task Index_Test()
    {
        await AuthenticateAsync(null, new AuthenticateModel
        {
            UserNameOrEmailAddress = "admin",
            Password = "123qwe"
        });

        //Act
        var response = await GetResponseAsStringAsync(
            GetUrl<HomeController>(nameof(HomeController.Index))
        );

        //Assert
        response.ShouldNotBeNullOrEmpty();
    }
}